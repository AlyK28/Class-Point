using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views.Modern
{
    public static class ModernDialogs
    {
        public static DialogResult ShowConfirmation(
            IWin32Window owner,
            string title,
            string message,
            string confirmText = "OK",
            string cancelText = "Cancel",
            MessageBoxIcon icon = MessageBoxIcon.Question)
        {
            using (var dialog = new ModernConfirmDialog(title, message, confirmText, cancelText, icon))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static void ShowSuccess(IWin32Window owner, string message, string title = "Success")
        {
            using (var dialog = new ModernMessageDialog(title, message, MessageBoxIcon.Information))
            {
                dialog.ShowDialog(owner);
            }
        }

        public static void ShowError(IWin32Window owner, string message, string title = "Error")
        {
            using (var dialog = new ModernMessageDialog(title, message, MessageBoxIcon.Error))
            {
                dialog.ShowDialog(owner);
            }
        }

        public static void ShowWarning(IWin32Window owner, string message, string title = "Warning")
        {
            using (var dialog = new ModernMessageDialog(title, message, MessageBoxIcon.Warning))
            {
                dialog.ShowDialog(owner);
            }
        }

        public static void ShowInfo(IWin32Window owner, string message, string title = "Information")
        {
            using (var dialog = new ModernMessageDialog(title, message, MessageBoxIcon.Information))
            {
                dialog.ShowDialog(owner);
            }
        }
    }

    public class ModernConfirmDialog : ModernFormBase
    {
        private ModernLabel lblMessage;
        private ModernButton btnConfirm;
        private ModernButton btnCancel;

        public ModernConfirmDialog(string title, string message, string confirmText, string cancelText, MessageBoxIcon icon)
        {
            FormTitle = title;
            TitleIcon = GetIconForMessageBox(icon);
            ShowFooter = false;
            Size = new Size(420, 200);

            InitializeDialog(message, confirmText, cancelText, icon);
        }

        private void InitializeDialog(string message, string confirmText, string cancelText, MessageBoxIcon icon)
        {
            lblMessage = new ModernLabel
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(360, 60),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            btnConfirm = new ModernButton
            {
                Text = confirmText,
                Size = new Size(100, 35),
                Location = new Point(200, 100),
                BackColor = GetColorForIcon(icon),
                HoverColor = GetHoverColorForIcon(icon),
                TextColor = Color.White,
                DialogResult = DialogResult.OK
            };

            btnCancel = new ModernButton
            {
                Text = cancelText,
                Size = new Size(100, 35),
                Location = new Point(310, 100),
                BackColor = Color.FromArgb(156, 163, 175),
                HoverColor = Color.FromArgb(107, 114, 128),
                TextColor = Color.White,
                DialogResult = DialogResult.Cancel
            };

            ContentPanel.Controls.AddRange(new Control[] { lblMessage, btnConfirm, btnCancel });
            AcceptButton = btnConfirm;
            CancelButton = btnCancel;
        }

        private string GetIconForMessageBox(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Question: return "❓";
                case MessageBoxIcon.Information: return "ℹ️";
                case MessageBoxIcon.Warning: return "⚠️";
                case MessageBoxIcon.Error: return "❌";
                default: return "💬";
            }
        }

        private Color GetColorForIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error: return Color.FromArgb(239, 68, 68);
                case MessageBoxIcon.Warning: return Color.FromArgb(245, 158, 11);
                case MessageBoxIcon.Information: return Color.FromArgb(59, 130, 246);
                default: return Color.FromArgb(67, 56, 202);
            }
        }

        private Color GetHoverColorForIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error: return Color.FromArgb(220, 38, 38);
                case MessageBoxIcon.Warning: return Color.FromArgb(217, 119, 6);
                case MessageBoxIcon.Information: return Color.FromArgb(37, 99, 235);
                default: return Color.FromArgb(55, 48, 163);
            }
        }
    }

    public class ModernMessageDialog : ModernFormBase
    {
        public ModernMessageDialog(string title, string message, MessageBoxIcon icon)
        {
            FormTitle = title;
            TitleIcon = GetIconForMessageBox(icon);
            ShowFooter = false;
            Size = new Size(400, 180);

            InitializeDialog(message);
        }

        private void InitializeDialog(string message)
        {
            var lblMessage = new ModernLabel
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(340, 60),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            var btnOK = new ModernButton
            {
                Text = "OK",
                Size = new Size(80, 35),
                Location = new Point(290, 90),
                BackColor = Color.FromArgb(67, 56, 202),
                HoverColor = Color.FromArgb(55, 48, 163),
                TextColor = Color.White,
                DialogResult = DialogResult.OK
            };

            ContentPanel.Controls.AddRange(new Control[] { lblMessage, btnOK });
            AcceptButton = btnOK;
        }

        private string GetIconForMessageBox(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information: return "ℹ️";
                case MessageBoxIcon.Warning: return "⚠️";
                case MessageBoxIcon.Error: return "❌";
                default: return "💬";
            }
        }
    }
}