using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ClassCodeDisplayForm : Form
    {
        private Label lblTitle;
        private Label lblCode;
        private Label lblInstructions;
        private Button btnClose;
        private Button btnEndClass;

        public string ClassCode { get; }
        public string CourseName { get; }
        public bool ShouldEndClass { get; private set; }

        public ClassCodeDisplayForm(string classCode, string courseName)
        {
            ClassCode = classCode ?? throw new ArgumentNullException(nameof(classCode));
            CourseName = courseName ?? throw new ArgumentNullException(nameof(courseName));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form setup
            Text = "Class Session Active";
            Size = new Size(450, 300);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            BackColor = Color.White;

            // Title Label
            lblTitle = new Label
            {
                Text = $"Class Session: {CourseName}",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Code Label (Large and prominent)
            lblCode = new Label
            {
                Text = ClassCode,
                Location = new Point(20, 70),
                Size = new Size(400, 80),
                Font = new Font("Microsoft Sans Serif", 48F, FontStyle.Bold),
                ForeColor = Color.Green,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Instructions Label
            lblInstructions = new Label
            {
                Text = "Students can join this class using the code above.\nKeep this window open during your presentation.",
                Location = new Point(20, 170),
                Size = new Size(400, 40),
                Font = new Font("Microsoft Sans Serif", 10F),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Buttons
            btnClose = new Button
            {
                Text = "Minimize",
                Location = new Point(250, 220),
                Size = new Size(80, 30),
                BackColor = Color.LightBlue
            };

            btnEndClass = new Button
            {
                Text = "End Class",
                Location = new Point(340, 220),
                Size = new Size(80, 30),
                BackColor = Color.LightCoral,
                DialogResult = DialogResult.OK
            };

            // Event handlers
            btnClose.Click += BtnClose_Click;
            btnEndClass.Click += BtnEndClass_Click;

            // Add controls
            Controls.AddRange(new Control[] { lblTitle, lblCode, lblInstructions, btnClose, btnEndClass });
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void BtnEndClass_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to end this class session?",
                "End Class",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ShouldEndClass = true;
                DialogResult = DialogResult.OK;
                Close();
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
    }
}

