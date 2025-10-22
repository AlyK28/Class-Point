using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Views;

namespace ClassPointAddIn
{
    public partial class ThisAddIn
    {
        public bool IsTeacherLoggedIn { get; private set; } = false;
        public int? CurrentCourseId { get; private set; }
        public string CurrentCourseName { get; private set; }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // Hook into PowerPoint events
            Application.PresentationOpen += Application_PresentationOpen;
            Application.PresentationClose += Application_PresentationClose;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        private async void Application_PresentationOpen(Microsoft.Office.Interop.PowerPoint.Presentation Pres)
        {
            if (IsTeacherLoggedIn)
            {
                await CreateCourseForPresentation(Pres);
            }
        }

        private void Application_PresentationClose(Microsoft.Office.Interop.PowerPoint.Presentation Pres)
        {
            // Clear current course when presentation is closed
            CurrentCourseId = null;
            CurrentCourseName = null;
        }

        private async System.Threading.Tasks.Task CreateCourseForPresentation(Microsoft.Office.Interop.PowerPoint.Presentation presentation)
        {
            try
            {
                var courseService = new CourseApiService();
                var courseName = GetCourseNameFromPresentation(presentation);

                var courseResponse = await courseService.CreateCourseAsync(courseName);
                CurrentCourseId = courseResponse.Id;
                CurrentCourseName = courseResponse.Name;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to create course: {ex.Message}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        private string GetCourseNameFromPresentation(Microsoft.Office.Interop.PowerPoint.Presentation presentation)
        {
            var presentationName = System.IO.Path.GetFileNameWithoutExtension(presentation.Name);
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{presentationName}_{timestamp}";
        }

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new DynamicRibbon();
        }

        public async void LoadTeacherRibbon()
        {
            IsTeacherLoggedIn = true;

            // If there's already a presentation open, create a course for it
            if (Application.Presentations.Count > 0)
            {
                var activePresentation = Application.ActivePresentation;
                if (activePresentation != null)
                {
                    await CreateCourseForPresentation(activePresentation);
                }
            }
        }

        public void UnloadTeacherRibbon()
        {
            IsTeacherLoggedIn = false;
            CurrentCourseId = null;
            CurrentCourseName = null;
        }

        public bool HasActivePresentation()
        {
            try
            {
                return Application.Presentations.Count > 0 && Application.ActivePresentation != null;
            }
            catch
            {
                return false;
            }
        }

        public async System.Threading.Tasks.Task<(int courseId, string courseName)> EnsureCourseForCurrentPresentation()
        {
            if (CurrentCourseId.HasValue)
            {
                return (CurrentCourseId.Value, CurrentCourseName);
            }

            if (!HasActivePresentation())
            {
                throw new System.InvalidOperationException("No active PowerPoint presentation found. Please open a presentation first.");
            }

            await CreateCourseForPresentation(Application.ActivePresentation);

            if (!CurrentCourseId.HasValue)
            {
                throw new System.InvalidOperationException("Failed to create course for the current presentation.");
            }

            return (CurrentCourseId.Value, CurrentCourseName);
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


