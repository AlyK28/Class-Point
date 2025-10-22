using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Users.Auth;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authenticationService;
        private readonly CourseApiService _courseApiService;

        public LoginForm(AuthenticationService authenticationService, CourseApiService courseApiService)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
            _courseApiService = courseApiService;
            // Bind async event handler
            btnLogin.Click += async (s, e) => await BtnLogin_ClickAsync(s, e);
        }

        private async Task BtnLogin_ClickAsync(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            try
            {
                var username = txtUsername.Text.Trim();
                var password = txtPassword.Text.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both username and password.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = await _authenticationService.LoginAsync(username, password);

                MessageBox.Show($"Welcome, {user.Username}!", "Login Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await _courseApiService.CreateCourseAsync("course1");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Register functionality will be implemented here.",
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
