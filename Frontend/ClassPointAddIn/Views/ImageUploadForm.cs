using ClassPointAddIn.Api.Responses;
using ClassPointAddIn.Api.Service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ImageUploadForm : Form
    {
        private readonly IImageUploadApiClient _imageUploadClient;
        private List<SessionResponse> _sessions;
        private List<SubmissionResponse> _submissions;
        private string _selectedSessionCode;

        public ImageUploadForm(IImageUploadApiClient imageUploadClient)
        {
            InitializeComponent();
            _imageUploadClient = imageUploadClient;
            _sessions = new List<SessionResponse>();
            _submissions = new List<SubmissionResponse>();
        }

        private async void ImageUploadForm_Load(object sender, EventArgs e)
        {
            await LoadSessions();
        }

        private async Task LoadSessions()
        {
            try
            {
                _sessions = await _imageUploadClient.GetTeacherSessionsAsync();
                UpdateSessionsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load sessions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSessionsList()
        {
            lstSessions.Items.Clear();
            foreach (var session in _sessions.Where(s => s.Status == "active"))
            {
                var item = new ListViewItem(session.Name);
                item.SubItems.Add(session.SessionCode);
                item.SubItems.Add(session.SubmissionCount.ToString());
                item.SubItems.Add(session.CreatedAt.ToString("MM/dd/yyyy HH:mm"));
                item.Tag = session;
                lstSessions.Items.Add(item);
            }
        }

        private void lstSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSessions.SelectedItems.Count > 0)
            {
                var session = (SessionResponse)lstSessions.SelectedItems[0].Tag;
                _selectedSessionCode = session.SessionCode;
                lblSelectedSession.Text = $"Selected: {session.Name} ({session.SessionCode})";
                btnUpload.Enabled = true;
                btnViewSubmissions.Enabled = true;
            }
            else
            {
                _selectedSessionCode = null;
                lblSelectedSession.Text = "No session selected";
                btnUpload.Enabled = false;
                btnViewSubmissions.Enabled = false;
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtImagePath.Text = openFileDialog.FileName;
                    
                    // Show image preview
                    try
                    {
                        var image = Image.FromFile(openFileDialog.FileName);
                        var thumbnail = new Bitmap(image, new Size(150, 150));
                        picPreview.Image = thumbnail;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load image preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedSessionCode))
            {
                MessageBox.Show("Please select a session first.", "No Session Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtImagePath.Text) || !File.Exists(txtImagePath.Text))
            {
                MessageBox.Show("Please select a valid image file.", "No Image Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnUpload.Enabled = false;
                btnUpload.Text = "Uploading...";

                var studentName = txtStudentName.Text.Trim();
                var submission = await _imageUploadClient.UploadImageAsync(_selectedSessionCode, txtImagePath.Text, studentName);

                MessageBox.Show($"Image uploaded successfully!\nSubmission ID: {submission.Id}", "Upload Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                txtImagePath.Text = "";
                txtStudentName.Text = "";
                picPreview.Image = null;

                // Refresh sessions list
                await LoadSessions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload image: {ex.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpload.Enabled = true;
                btnUpload.Text = "Upload Image";
            }
        }

        private async void btnViewSubmissions_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedSessionCode))
            {
                MessageBox.Show("Please select a session first.", "No Session Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _submissions = await _imageUploadClient.GetSessionSubmissionsAsync(_selectedSessionCode);
                ShowSubmissionsDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load submissions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSubmissionsDialog()
        {
            var submissionsForm = new SubmissionsViewerForm(_submissions, _imageUploadClient);
            submissionsForm.ShowDialog();
        }

        private async void btnCreateSession_Click(object sender, EventArgs e)
        {
            var sessionName = Microsoft.VisualBasic.Interaction.InputBox("Enter session name:", "Create New Session", "");
            if (string.IsNullOrEmpty(sessionName)) return;

            var question = Microsoft.VisualBasic.Interaction.InputBox("Enter optional question/prompt for students:", "Session Question", "");
            
            try
            {
                var session = await _imageUploadClient.CreateSessionAsync(sessionName, question);
                MessageBox.Show($"Session created successfully!\nSession Code: {session.SessionCode}", "Session Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadSessions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create session: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadSessions();
        }
    }
}
