using ClassPointAddIn.Api.Service;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ConnectRibbon
    {
        private void ConnectRibbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private async void Connect_Click(object sender, RibbonControlEventArgs e)
        {
            var userApiClient = new UserApiClient();
            var authService = new Users.Auth.AuthenticationService(userApiClient);

            using (var loginForm = new LoginForm(authService))
            {
                var result = loginForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                }
            }
        }
    }
}
