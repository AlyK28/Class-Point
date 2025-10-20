using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Users.Auth;
using ClassPointAddIn.Views;
using System.Windows.Forms;

namespace ClassPointAddIn
{
    public partial class ThisAddIn
    {
        private IUserApiClient _userApiClient;
        private IImageUploadApiClient _imageUploadClient;
        private AuthenticationService _authenticationService;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // Initialize API clients
            _userApiClient = new UserApiClient();
            _imageUploadClient = new ImageUploadApiClient();
            _authenticationService = new AuthenticationService(_userApiClient);

            // Add custom ribbon
            this.Application.AfterNewPresentation += Application_AfterNewPresentation;
        }

        private void Application_AfterNewPresentation(PowerPoint.Presentation Pres)
        {
            // Add image upload button to the ribbon
            AddImageUploadButton();
        }

        private void AddImageUploadButton()
        {
            try
            {
                // This would typically be done through a custom ribbon XML
                // For now, we'll add it programmatically
                MessageBox.Show("ClassPoint Image Upload is ready! Use the login form to access image upload features.", 
                    "ClassPoint Add-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing ClassPoint: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowImageUploadForm()
        {
            try
            {
                // Check if user is authenticated
                if (string.IsNullOrEmpty(_authenticationService.GetCurrentToken()))
                {
                    // Show login form first
                    using (var loginForm = new LoginForm(_authenticationService))
                    {
                        if (loginForm.ShowDialog() == DialogResult.OK)
                        {
                            // Set auth token for image upload client
                            _imageUploadClient.SetAuthToken(_authenticationService.GetCurrentToken());
                            
                            // Show image upload form
                            using (var imageUploadForm = new ImageUploadForm(_imageUploadClient))
                            {
                                imageUploadForm.ShowDialog();
                            }
                        }
                    }
                }
                else
                {
                    // User is already authenticated, show image upload form directly
                    _imageUploadClient.SetAuthToken(_authenticationService.GetCurrentToken());
                    using (var imageUploadForm = new ImageUploadForm(_imageUploadClient))
                    {
                        imageUploadForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening image upload form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
