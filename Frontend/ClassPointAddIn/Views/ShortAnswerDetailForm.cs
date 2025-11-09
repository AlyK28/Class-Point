using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ShortAnswerDetailForm : Form
    {
        private Panel pnlHeader;
        private Label lblStudentName;
        private Label lblSubmittedAt;
        private Panel pnlAnswerContent;
        private Label lblAnswerText;
        private Button btnClose;
        
        private ShortAnswerSubmission submission;

        public ShortAnswerDetailForm(ShortAnswerSubmission submission)
        {
            this.submission = submission ?? throw new ArgumentNullException(nameof(submission));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "View Answer";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            
            // Header Panel with Gradient Background
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            
            // Add subtle gradient effect via paint
            pnlHeader.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    Color.FromArgb(0, 120, 215),
                    Color.FromArgb(0, 90, 180),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
                }
            };
            
            // Student Name
            lblStudentName = new Label
            {
                Text = submission.StudentName,
                Location = new System.Drawing.Point(30, 20),
                Size = new Size(540, 40),
                Font = new System.Drawing.Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblStudentName);
            
            // Submitted Time
            lblSubmittedAt = new Label
            {
                Text = $"Submitted: {submission.SubmittedAt:MMM dd, yyyy • h:mm tt}",
                Location = new System.Drawing.Point(30, 65),
                Size = new Size(540, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(230, 240, 255),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblSubmittedAt);
            
            Controls.Add(pnlHeader);
            
            // Answer Content Panel with decorative border
            pnlAnswerContent = new Panel
            {
                Location = new System.Drawing.Point(30, 130),
                Size = new Size(540, 280),
                BackColor = Color.FromArgb(250, 250, 255),
                Padding = new Padding(30)
            };
            
            // Add border with quote styling
            pnlAnswerContent.Paint += (s, e) =>
            {
                // Draw outer border
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 3))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlAnswerContent.Width - 1, pnlAnswerContent.Height - 1);
                }
                
                // Draw opening quote mark
                using (var font = new System.Drawing.Font("Georgia", 48F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(100, 180, 255)))
                {
                    e.Graphics.DrawString("\u201C", font, brush, new PointF(10, -5));
                }
            };
            
            // Answer Text with quote marks
            lblAnswerText = new Label
            {
                Text = $"\"{submission.AnswerText}\"",
                Location = new System.Drawing.Point(40, 50),
                Size = new Size(460, 180),
                Font = new System.Drawing.Font("Georgia", 11F, FontStyle.Italic),
                ForeColor = Color.FromArgb(60, 60, 60),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };
            
            // If text is too long, enable scrolling
            if (submission.AnswerText.Length > 400)
            {
                var scrollPanel = new Panel
                {
                    Location = new System.Drawing.Point(40, 50),
                    Size = new Size(460, 180),
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };
                
                lblAnswerText.Location = new System.Drawing.Point(0, 0);
                lblAnswerText.AutoSize = true;
                lblAnswerText.MaximumSize = new Size(440, 0);
                
                scrollPanel.Controls.Add(lblAnswerText);
                pnlAnswerContent.Controls.Add(scrollPanel);
            }
            else
            {
                pnlAnswerContent.Controls.Add(lblAnswerText);
            }
            
            Controls.Add(pnlAnswerContent);
            
            // Close Button
            btnClose = new Button
            {
                Text = "Close",
                Location = new System.Drawing.Point(245, 425),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 190);
            btnClose.Click += (s, e) => this.Close();
            
            Controls.Add(btnClose);
            
            // Set default button
            this.AcceptButton = btnClose;
        }
    }
}
