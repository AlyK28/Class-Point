using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Users.Auth;
using System;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class TestForm : Form
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IImageUploadApiClient _imageUploadClient;
        private readonly AuthenticationService _authenticationService;

        public TestForm()
        {
            InitializeComponent();
            _userApiClient = new UserApiClient();
            _imageUploadClient = new ImageUploadApiClient();
            _authenticationService = new AuthenticationService(_userApiClient);
        }

        private async void btnTestLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var user = await _authenticationService.LoginAsync("admin", "admin123");
                _imageUploadClient.SetAuthToken(_authenticationService.GetCurrentToken());
                MessageBox.Show($"Login successful! Welcome {user.Username}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnOpenImageUpload.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenImageUpload_Click(object sender, EventArgs e)
        {
            using (var imageUploadForm = new ImageUploadForm(_imageUploadClient))
            {
                imageUploadForm.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
