using ClassPointAddIn.Api.Responses;
using ClassPointAddIn.Api.Service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class SubmissionsViewerForm : Form
    {
        private readonly List<SubmissionResponse> _submissions;
        private readonly IImageUploadApiClient _imageUploadClient;

        public SubmissionsViewerForm(List<SubmissionResponse> submissions, IImageUploadApiClient imageUploadClient)
        {
            InitializeComponent();
            _submissions = submissions;
            _imageUploadClient = imageUploadClient;
            LoadSubmissions();
        }

        private void LoadSubmissions()
        {
            flpSubmissions.Controls.Clear();
            
            if (_submissions.Count == 0)
            {
                var lblNoSubmissions = new Label
                {
                    Text = "No submissions found for this session.",
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                flpSubmissions.Controls.Add(lblNoSubmissions);
                return;
            }

            foreach (var submission in _submissions)
            {
                var submissionPanel = CreateSubmissionPanel(submission);
                flpSubmissions.Controls.Add(submissionPanel);
            }
        }

        private Panel CreateSubmissionPanel(SubmissionResponse submission)
        {
            var panel = new Panel
            {
                Size = new Size(200, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            // Image
            var picImage = new PictureBox
            {
                Size = new Size(180, 120),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Load image asynchronously
            LoadImageAsync(picImage, submission.ImageUrl);

            // Student name
            var lblStudentName = new Label
            {
                Text = string.IsNullOrEmpty(submission.StudentName) ? "Anonymous" : submission.StudentName,
                Location = new Point(10, 140),
                Size = new Size(180, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };

            // Upload time
            var lblUploadTime = new Label
            {
                Text = submission.UploadedAt.ToString("MM/dd/yyyy HH:mm"),
                Location = new Point(10, 160),
                Size = new Size(180, 15),
                Font = new Font("Microsoft Sans Serif", 8F),
                ForeColor = Color.Gray
            };

            // Likes
            var lblLikes = new Label
            {
                Text = $"❤️ {submission.Likes}",
                Location = new Point(10, 180),
                Size = new Size(60, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };

            // Buttons
            var btnLike = new Button
            {
                Text = submission.IsLiked ? "❤️ Liked" : "🤍 Like",
                Location = new Point(80, 175),
                Size = new Size(60, 25),
                Font = new Font("Microsoft Sans Serif", 8F),
                Tag = submission
            };
            btnLike.Click += BtnLike_Click;

            var btnDelete = new Button
            {
                Text = "🗑️",
                Location = new Point(150, 175),
                Size = new Size(30, 25),
                Font = new Font("Microsoft Sans Serif", 8F),
                Tag = submission
            };
            btnDelete.Click += BtnDelete_Click;

            // Add controls to panel
            panel.Controls.Add(picImage);
            panel.Controls.Add(lblStudentName);
            panel.Controls.Add(lblUploadTime);
            panel.Controls.Add(lblLikes);
            panel.Controls.Add(btnLike);
            panel.Controls.Add(btnDelete);

            return panel;
        }

        private async void LoadImageAsync(PictureBox pictureBox, string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                // Convert relative URL to absolute
                var fullUrl = imageUrl.StartsWith("http") ? imageUrl : $"http://localhost:8000{imageUrl}";

                using (var webClient = new WebClient())
                {
                    var imageData = await webClient.DownloadDataTaskAsync(fullUrl);
                    using (var ms = new MemoryStream(imageData))
                    {
                        var image = Image.FromStream(ms);
                        pictureBox.Image = new Bitmap(image);
                    }
                }
            }
            catch (Exception ex)
            {
                // Show placeholder image or error
                pictureBox.Image = CreateErrorImage();
            }
        }

        private Image CreateErrorImage()
        {
            var bitmap = new Bitmap(180, 120);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.LightGray);
                g.DrawString("Image not available", new Font("Arial", 10), Brushes.Gray, 10, 50);
            }
            return bitmap;
        }

        private async void BtnLike_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var submission = (SubmissionResponse)button.Tag;

            try
            {
                button.Enabled = false;
                var updatedSubmission = await _imageUploadClient.ToggleLikeAsync(submission.Id);
                
                // Update the button text
                button.Text = updatedSubmission.IsLiked ? "❤️ Liked" : "🤍 Like";
                
                // Update likes count
                var panel = button.Parent;
                foreach (Control control in panel.Controls)
                {
                    if (control is Label label && label.Text.Contains("❤️"))
                    {
                        label.Text = $"❤️ {updatedSubmission.Likes}";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to toggle like: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button.Enabled = true;
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var submission = (SubmissionResponse)button.Tag;

            var result = MessageBox.Show(
                $"Are you sure you want to delete this submission from {submission.StudentName ?? "Anonymous"}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    button.Enabled = false;
                    var success = await _imageUploadClient.DeleteSubmissionAsync(submission.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("Submission deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Remove the panel from the form
                        button.Parent.Parent.Controls.Remove(button.Parent);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete submission: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    button.Enabled = true;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
