using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public class SimpleClassCodeDisplay : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public string ClassCode { get; }
        public string CourseName { get; }
        private bool _allowClose = false;

        public SimpleClassCodeDisplay(string classCode, string courseName)
        {
            ClassCode = classCode ?? "ERROR";
            CourseName = courseName ?? "Unknown";

            // Minimal setup
            Size = new Size(280, 120);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            BackColor = Color.FromArgb(40, 40, 40);
            ForeColor = Color.White;
            TopMost = true;
            ShowInTaskbar = false;
            Text = "Class Code";

            // Position top-right
            var screen = Screen.PrimaryScreen.Bounds;
            Location = new Point(screen.Right - Width - 20, 20);

            // Create simple label
            var label = new Label
            {
                Text = $"Class Code\n\n{ClassCode}\n\nStudents join with this code",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 14F, FontStyle.Bold),
                ForeColor = Color.Cyan,
                BackColor = Color.Transparent
            };

            Controls.Add(label);

            // Make it stay on top
            Shown += (s, e) => SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void ForceClose()
        {
            _allowClose = true;
            Close();
        }
    }
}
