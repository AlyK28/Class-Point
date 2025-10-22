using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Users.Auth;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new DynamicRibbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace ClassPointAddIn.Views
{
    [ComVisible(true)]
    public class DynamicRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        public DynamicRibbon()
        {
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("ClassPointAddIn.Views.DynamicRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226
        public bool GetConnectVisible(Office.IRibbonControl control)
         => !Globals.ThisAddIn.IsTeacherLoggedIn;

        public bool GetTeacherVisible(Office.IRibbonControl control)
            => Globals.ThisAddIn.IsTeacherLoggedIn;
        public void OnConnectClick(Office.IRibbonControl control)
        {
            try
            {
                // create API client and auth service
                var userApiClient = new UserApiService();
                var authService = new AuthenticationService(userApiClient);
                var courseService = new CourseApiService();

                using (var loginForm = new LoginForm(authService, courseService))
                {
                    var result = loginForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        Globals.ThisAddIn.LoadTeacherRibbon();
                        ribbon?.Invalidate();
                        ribbon?.ActivateTab(RibbonConstants.TabTeacher);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnLogoutClick(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.UnloadTeacherRibbon();
            ribbon?.Invalidate();
            ribbon?.ActivateTab(RibbonConstants.TabConnect);
        }
        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }


        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
