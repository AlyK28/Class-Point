using ClassPointAddIn.Api.Service;
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
        {
            var isEnabled = Globals.ThisAddIn.IsTeacherLoggedIn &&
                           Globals.ThisAddIn.HasActivePresentation() &&
                           !Globals.ThisAddIn.HasActiveClass &&
                           !Globals.ThisAddIn.HasActiveClassForm;

            System.Diagnostics.Debug.WriteLine($"GetStartSlideshowEnabled: " +
                                               $"IsTeacherLoggedIn={Globals.ThisAddIn.IsTeacherLoggedIn}, " +
                                               $"HasActivePresentation={Globals.ThisAddIn.HasActivePresentation()}, " +
                                               $"HasActiveClass={Globals.ThisAddIn.HasActiveClass}, " +
                                               $"HasActiveClassForm={Globals.ThisAddIn.HasActiveClassForm}, " +
                                               $"Result={isEnabled}");

            return isEnabled;
        }

        public bool GetQuizEnabled(Office.IRibbonControl control)
        {
            var isTeacherLoggedIn = Globals.ThisAddIn.IsTeacherLoggedIn;
            var hasActivePresentation = Globals.ThisAddIn.HasActivePresentation();
            var currentCourseId = Globals.ThisAddIn.CurrentCourseId;
            var hasCurrentCourse = currentCourseId.HasValue;

            var isEnabled = isTeacherLoggedIn && hasActivePresentation && hasCurrentCourse;

            // Enhanced debugging
            System.Diagnostics.Debug.WriteLine($"GetQuizEnabled: " +
                                               $"IsTeacherLoggedIn={isTeacherLoggedIn}, " +
                                               $"HasActivePresentation={hasActivePresentation}, " +
                                               $"CurrentCourseId={currentCourseId}, " +
                                               $"HasCurrentCourse={hasCurrentCourse}, " +
                                               $"Result={isEnabled}");

            return isEnabled;
        }

        public void OnConnectClick(Office.IRibbonControl control)
        {
            try
            {
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

                        // Force ribbon refresh after a short delay to ensure course creation completes
                        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
                        timer.Tick += (s, e) =>
                        {
                            timer.Stop();
                            timer.Dispose();
                            ribbon?.Invalidate();
                            System.Diagnostics.Debug.WriteLine("Ribbon refreshed after login delay");
                        };
                        timer.Start();
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
                System.Diagnostics.Debug.WriteLine("OnStartSlideshowClick: Starting manual class...");

                // Ensure we have a course before starting the class
                await Globals.ThisAddIn.EnsureCourseForCurrentPresentation();

                var (classCode, classId) = await Globals.ThisAddIn.StartManualClassAsync();
                Globals.ThisAddIn.ShowClassCodeForm(classCode, Globals.ThisAddIn.CurrentCourseName);

                ribbon?.Invalidate();

                System.Diagnostics.Debug.WriteLine($"OnStartSlideshowClick: Class started with code {classCode}");
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Cannot Start Class", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start class: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void OnQuizPanelClick(Office.IRibbonControl control)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"OnQuizPanelClick: CurrentCourseId={Globals.ThisAddIn.CurrentCourseId}");

                // Try to ensure we have a course
                if (!Globals.ThisAddIn.CurrentCourseId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("No current course, attempting to create one...");

                    try
                    {
                        await Globals.ThisAddIn.EnsureCourseForCurrentPresentation();
                        System.Diagnostics.Debug.WriteLine($"Course created/ensured: {Globals.ThisAddIn.CurrentCourseId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to create course: {ex.Message}");
                        MessageBox.Show("Please ensure you have an active presentation with a course.\n\n" +
                                       "If this problem persists, try:\n" +
                                       "1. Close and reopen your presentation\n" +
                                       "2. Log out and log back in\n\n" +
                                       $"Error: {ex.Message}",
                            "No Course", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Now try to show the quiz task pane
                Globals.ThisAddIn.ToggleQuizTaskPane();

                // Refresh ribbon to update button states
                ribbon?.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling quiz panel: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void OnLogoutClick(Office.IRibbonControl control)
        {
            try
            {
                if (Globals.ThisAddIn.HasActiveClass)
                {
                    var result = MessageBox.Show(
                        "You have an active class session. Do you want to end it before logging out?",
                        "Active Class Session",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                    {
                        await Globals.ThisAddIn.EndManualClassAsync();
                    }
                }

                Globals.ThisAddIn.UnloadTeacherRibbon();
                ribbon?.Invalidate();
                ribbon?.ActivateTab(RibbonConstants.TabConnect);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

