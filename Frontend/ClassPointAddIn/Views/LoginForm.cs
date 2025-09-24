using ClassPointAddIn.Users.Auth;
using System;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authenticationService;
        public LoginForm(AuthenticationService authenticationService)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
        }
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            btnLogin.Enabled = false;

            try
            {
                var user = await _authenticationService.LoginAsync(txtUsername.Text, textPassword.Text);
                MessageBox.Show($"Welcome {user.Username}!", "Login Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblUsername_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Register functionality will be implemented here.");
        }

    }
}
