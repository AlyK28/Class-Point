using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Users.Auth;
using Domain.Users.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authenticationService;
        private readonly CourseApiService _courseApiService;
        private CancellationTokenSource _cancellationTokenSource;

        public LoginForm(AuthenticationService authenticationService, CourseApiService courseApiService)
        {
            InitializeComponent();
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _courseApiService = courseApiService ?? throw new ArgumentNullException(nameof(courseApiService));

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Set form properties for better UX
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Wire up event handlers properly
            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;

            // Enable Enter key for login
            AcceptButton = btnLogin;

            // Clear any existing text and focus on username
            ClearForm();
            txtUsername.Focus();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            await HandleLoginAsync();
        }

        private async Task HandleLoginAsync()
        {
            if (!ValidateInput())
                return;

            // Cancel any previous operation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            SetUIState(isLoading: true);

            try
            {
                var username = txtUsername.Text.Trim();
                var password = txtPassword.Text.Trim();

                var user = await _authenticationService.LoginAsync(username, password);

                if (user == null)
                {
                    ShowError("Login failed. Please check your credentials.");
                    return;
                }

                await OnLoginSuccessAsync(user);
            }
            catch (OperationCanceledException)
            {
                // User cancelled the operation
                ShowInfo("Login operation was cancelled.");
            }
            catch (ArgumentException ex)
            {
                ShowError($"Invalid input: {ex.Message}");
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Invalid username or password.");
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {GetUserFriendlyErrorMessage(ex)}");
            }
            finally
            {
                SetUIState(isLoading: false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private bool ValidateInput()
        {
            var username = txtUsername.Text?.Trim();
            var password = txtPassword.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowValidationError("Please enter your username.");
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowValidationError("Please enter your password.");
                txtPassword.Focus();
                return false;
            }

            if (username.Length < 3)
            {
                ShowValidationError("Username must be at least 3 characters long.");
                txtUsername.Focus();
                return false;
            }

            return true;
        }

        private async Task OnLoginSuccessAsync(User user)
        {
            try
            {
                ShowSuccess($"Welcome, {user.Username}!");

                // Only create course if needed - this seems like test code that should be removed
                // Consider removing this line or making it conditional
                // await _courseApiService.CreateCourseAsync("course1" + Guid.NewGuid());

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                // Don't fail the login if course creation fails
                ShowWarning($"Login successful, but there was an issue: {ex.Message}");
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            // TODO: Implement proper registration form
            ShowInfo("Registration feature coming soon!");
        }

        private void SetUIState(bool isLoading)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetUIState), isLoading);
                return;
            }

            btnLogin.Enabled = !isLoading;
            btnRegister.Enabled = !isLoading;
            txtUsername.Enabled = !isLoading;
            txtPassword.Enabled = !isLoading;

            btnLogin.Text = isLoading ? "Logging in..." : "Login";
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            // Return user-friendly messages instead of technical details
            // Using switch expression syntax compatible with C# 7.3
            switch (ex)
            {
                case TimeoutException _:
                    return "The request timed out. Please try again.";
                case System.Net.Http.HttpRequestException _:
                    return "Unable to connect to the server. Please check your internet connection.";
                default:
                    return "An unexpected error occurred. Please try again.";
            }
        }

        #region Message Box Helpers

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(this, message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(this, message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(this, message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(this, message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Cancel any ongoing operations
            _cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }

    }
}
