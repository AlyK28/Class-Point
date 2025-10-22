using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Api.Services.ClassService;
using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Users.Auth;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;

namespace ClassPointAddIn.Views
{
    [ComVisible(true)]
    public class DynamicRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;
        private ClassCodeDisplayForm _activeClassForm;
        private int? _currentClassId;

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

        public bool GetConnectVisible(Office.IRibbonControl control)
         => !Globals.ThisAddIn.IsTeacherLoggedIn;

        public bool GetTeacherVisible(Office.IRibbonControl control)
            => Globals.ThisAddIn.IsTeacherLoggedIn;

        public bool GetStartSlideshowEnabled(Office.IRibbonControl control)
            => Globals.ThisAddIn.IsTeacherLoggedIn &&
               Globals.ThisAddIn.HasActivePresentation() &&
               _activeClassForm == null;

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

        public async void OnStartSlideshowClick(Office.IRibbonControl control)
        {
            try
            {
                // Ensure we have a course for the current presentation
                var (courseId, courseName) = await Globals.ThisAddIn.EnsureCourseForCurrentPresentation();

                var classService = new ClassApiService();

                // Create a new class session using the current course
                var classResponse = await classService.CreateClassFromPowerPointAsync(courseId);
                _currentClassId = classResponse.Id;

                // Display the class code
                ShowClassCodeDialog(classResponse.Code, courseName, classService);

                // Refresh ribbon to disable the start slideshow button
                ribbon?.Invalidate();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Cannot Start Slideshow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start slideshow: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ShowClassCodeDialog(string classCode, string courseName, ClassApiService classService)
        {
            try
            {
                _activeClassForm = new ClassCodeDisplayForm(classCode, courseName);
                var result = _activeClassForm.ShowDialog();

                if (result == DialogResult.OK && _activeClassForm.ShouldEndClass && _currentClassId.HasValue)
                {
                    // End the class session
                    await classService.EndClassAsync(_currentClassId.Value);
                    MessageBox.Show("Class session ended successfully.",
                        "Class Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing class session: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _activeClassForm = null;
                _currentClassId = null;

                // Refresh ribbon to re-enable the start slideshow button
                ribbon?.Invalidate();
            }
        }

        public void OnLogoutClick(Office.IRibbonControl control)
        {
            // Close any active class session
            _activeClassForm?.Close();
            _activeClassForm = null;
            _currentClassId = null;

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


