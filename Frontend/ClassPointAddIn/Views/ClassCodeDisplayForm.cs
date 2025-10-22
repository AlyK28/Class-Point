using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ClassCodeDisplayForm : Form
    {
        private Label lblTitle;
        private Panel pnlCode;
        private Label lblCode;
        private Label lblInstructions;
        private Button btnCopy;
        private Button btnMinimize;
        private Button btnEndClass;

        // Win32 API for making window always on top
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public string ClassCode { get; }
        public string CourseName { get; }
        public bool ShouldEndClass { get; private set; }

        public ClassCodeDisplayForm(string classCode, string courseName)
        {
            ClassCode = classCode ?? throw new ArgumentNullException(nameof(classCode));
            CourseName = courseName ?? throw new ArgumentNullException(nameof(courseName));

            InitializeComponent();
            MakeAlwaysOnTop();
        }

        private void InitializeComponent()
        {
            // Optimized for slideshow overlay
            Text = $"Class Code - {CourseName}";
            Size = new Size(380, 220); // Smaller and more compact
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None; // Remove border for cleaner look
            BackColor = Color.FromArgb(240, 240, 240);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            TopMost = true;
            ShowInTaskbar = false;

            // Position in top-right corner with some margin
            var screen = Screen.PrimaryScreen.Bounds;
            Location = new Point(screen.Right - Width - 20, 20);

            CreateControls();
            AddBorderEffect();
        }

        private void AddBorderEffect()
        {
            // Add a subtle border and shadow effect
            Paint += (sender, e) =>
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);

                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, new Rectangle(2, 2, Width - 2, Height - 2));
                }

                // Draw main background
                using (var backBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, rect);
                }

                // Draw border
                using (var borderPen = new Pen(Color.FromArgb(100, 100, 100), 2))
                {
                    e.Graphics.DrawRectangle(borderPen, rect);
                }
            };
        }

        private void CreateControls()
        {
            // Compact title
            lblTitle = new Label
            {
                Text = $"Class: {CourseName}",
                Location = new Point(10, 10),
                Size = new Size(360, 18),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Compact code panel
            pnlCode = new Panel
            {
                Location = new Point(10, 35),
                Size = new Size(360, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            pnlCode.Click += CopyCodeToClipboard;

            // Code label - optimized size
            lblCode = new Label
            {
                Text = ClassCode,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 24F, FontStyle.Bold),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            lblCode.Click += CopyCodeToClipboard;

            // Compact instructions
            lblInstructions = new Label
            {
                Text = "Students join with code above. Click to copy.",
                Location = new Point(10, 105),
                Size = new Size(360, 30),
                Font = new Font("Microsoft Sans Serif", 8F),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };

            // Compact buttons
            btnCopy = new Button
            {
                Text = "Copy",
                Location = new Point(180, 145),
                Size = new Size(60, 22),
                UseVisualStyleBackColor = true,
                Font = new Font("Microsoft Sans Serif", 8F)
            };
            btnCopy.Click += CopyCodeToClipboard;

            btnMinimize = new Button
            {
                Text = "Hide",
                Location = new Point(250, 145),
                Size = new Size(50, 22),
                UseVisualStyleBackColor = true,
                Font = new Font("Microsoft Sans Serif", 8F)
            };
            btnMinimize.Click += BtnMinimize_Click;

            btnEndClass = new Button
            {
                Text = "End",
                Location = new Point(310, 145),
                Size = new Size(50, 22),
                UseVisualStyleBackColor = true,
                Font = new Font("Microsoft Sans Serif", 8F)
            };
            btnEndClass.Click += BtnEndClass_Click;

            // Add close button (X) in top-right
            var btnClose = new Button
            {
                Text = "✕",
                Location = new Point(350, 5),
                Size = new Size(20, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Red,
                Font = new Font("Microsoft Sans Serif", 8F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => WindowState = FormWindowState.Minimized;

            // Add code label to panel
            pnlCode.Controls.Add(lblCode);

            // Add all controls to form
            Controls.AddRange(new Control[]
            {
                lblTitle,
                pnlCode,
                lblInstructions,
                btnCopy,
                btnMinimize,
                btnEndClass,
                btnClose
            });

            // Set tab order
            btnCopy.TabIndex = 0;
            btnMinimize.TabIndex = 1;
            btnEndClass.TabIndex = 2;

            // Set default button
            AcceptButton = btnCopy;
        }

        private void MakeAlwaysOnTop()
        {
            // Ensure the form stays on top even during slideshow
            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            // Also set TopMost property
            TopMost = true;

            // Handle when window loses focus
            Deactivate += (s, e) =>
            {
                // Re-establish topmost when deactivated
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Ensure it's on top when shown
            MakeAlwaysOnTop();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Maintain topmost status
            TopMost = true;
        }

        private void CopyCodeToClipboard(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object, EventArgs>(CopyCodeToClipboard), sender, e);
                    return;
                }

                CopyToClipboardSTA(ClassCode);
                ShowBriefMessage("Copied!");
            }
            catch (Exception ex)
            {
                ShowBriefMessage("Failed");
            }
        }

        private void ShowBriefMessage(string message)
        {
            var originalText = btnCopy.Text;
            btnCopy.Text = message;
            btnCopy.Enabled = false;

            var timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            timer.Tick += (s, e) =>
            {
                btnCopy.Text = originalText;
                btnCopy.Enabled = true;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void CopyToClipboardSTA(string text)
        {
            try
            {
                Clipboard.SetText(text, TextDataFormat.Text);
            }
            catch (ThreadStateException)
            {
                Exception threadException = null;
                var thread = new Thread(() =>
                {
                    try
                    {
                        Clipboard.SetText(text, TextDataFormat.Text);
                    }
                    catch (Exception ex)
                    {
                        threadException = ex;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (threadException != null)
                {
                    throw threadException;
                }
            }
            catch (Exception)
            {
                try
                {
                    Clipboard.SetDataObject(text, true);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(100);
                    Clipboard.SetText(text);
                }
            }
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void BtnEndClass_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "End this class session?",
                    "End Class",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine("BtnEndClass_Click: User confirmed class end");
                    ShouldEndClass = true;
                    DialogResult = DialogResult.OK; // Set proper dialog result
                    Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BtnEndClass_Click: User cancelled class end");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnEndClass_Click Error: {ex.Message}");
                MessageBox.Show($"Error ending class: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !ShouldEndClass)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (value && IsHandleCreated)
            {
                MakeAlwaysOnTop();
            }
        }

        // Handle drag functionality for borderless form
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTCAPTION = 2;

            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = PointToClient(pos);
                if (pos.Y < 30) // Top 30 pixels act as title bar for dragging
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
