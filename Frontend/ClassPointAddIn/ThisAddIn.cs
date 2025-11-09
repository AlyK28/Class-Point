using ClassPointAddIn.Api.Services.ClassService;
using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Views;
using ClassPointAddIn.Views.Quizzes;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClassPointAddIn
{
    public partial class ThisAddIn
    {
        public bool IsTeacherLoggedIn { get; private set; } = false;
        public int? CurrentCourseId { get; private set; }
        public string CurrentCourseName { get; private set; }

        // Track active class session
        private SimpleClassCodeDisplay _activeSimpleDisplay;
        private ClassCodeDisplayForm _activeClassForm;
        private PowerPointCodeDisplay _activePowerPointDisplay;
        private int? _currentClassId;
        private string _currentClassCode; // Store the actual class code
        private ClassApiService _classApiService;
        private CustomTaskPane _quizTaskPane;
        private QuizTaskPane _quizTaskPaneControl;

        public void ShowQuizTaskPane()
        {
            try
            {
                if (_quizTaskPane == null)
                {
                    _quizTaskPaneControl = new QuizTaskPane();

                    _quizTaskPane = CustomTaskPanes.Add(_quizTaskPaneControl, "Quiz Library");
                    _quizTaskPane.Width = 350;
                    _quizTaskPane.DockPosition = Microsoft.Office.Core.MsoCTPDockPosition.msoCTPDockPositionRight;
                    _quizTaskPane.DockPositionRestrict = Microsoft.Office.Core.MsoCTPDockPositionRestrict.msoCTPDockPositionRestrictNoChange;
                }

                // Update course ID if available
                if (CurrentCourseId.HasValue)
                {
                    _quizTaskPaneControl.SetCourseId(CurrentCourseId.Value);
                }

                _quizTaskPane.Visible = true;

                Debug.WriteLine("Quiz Task Pane shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowQuizTaskPane Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error showing quiz pane: {ex.Message}", "Error");
            }
        }
        private void Application_WindowSelectionChange(Microsoft.Office.Interop.PowerPoint.Selection Sel)
        {
            try
            {
                // Check if the selection contains a quiz button
                if (Sel.Type == Microsoft.Office.Interop.PowerPoint.PpSelectionType.ppSelectionShapes)
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in Sel.ShapeRange)
                    {
                        // Check if this is a quiz button
                        bool isQuizButton = false;
                        try
                        {
                            for (int i = 1; i <= shape.Tags.Count; i++)
                            {
                                if (shape.Tags.Name(i) == "QuizButton" && shape.Tags.Value(i) == "MultiChoiceQuiz")
                                {
                                    isQuizButton = true;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore tag access errors
                        }

                        if (isQuizButton)
                        {
                            System.Diagnostics.Debug.WriteLine("Quiz button selected - showing task pane");

                            // Show the quiz task pane (existing behavior)
                            ShowQuizTaskPane();

                            // Prepare data for the results dialog.
                            // Prefer real configured settings from the task pane if available; otherwise use sample data.
                            int numberOfChoices = 4;
                            List<int> correctIndices = new List<int> { 0 };

                            try
                            {
                                if (_quizTaskPaneControl != null)
                                {
                                    numberOfChoices = _quizTaskPaneControl.GetNumberOfChoices();
                                    var configuredCorrect = _quizTaskPaneControl.GetCorrectAnswerIndices();
                                    if (configuredCorrect != null && configuredCorrect.Count > 0)
                                        correctIndices = configuredCorrect;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading task pane settings for results dialog: {ex.Message}");
                            }

                            // Build sample percentages (even split) — replace with real API data if available
                            var percentages = new int[numberOfChoices];
                            var basePercent = 100 / numberOfChoices;
                            var remainder = 100 % numberOfChoices;
                            for (int i = 0; i < numberOfChoices; i++)
                            {
                                percentages[i] = basePercent + (i < remainder ? 1 : 0);
                            }

                            // Show results dialog (modal)
                            try
                            {
                                using (var dlg = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(percentages, correctIndices))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to show quiz results dialog: {ex.Message}");
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application_WindowSelectionChange Error: {ex.Message}");
            }
        }
        public void HideQuizTaskPane()
        {
            try
            {
                if (_quizTaskPane != null)
                {
                    _quizTaskPane.Visible = false;
                }
                Debug.WriteLine("Quiz Task Pane hidden");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HideQuizTaskPane Error: {ex.Message}");
            }
        }

        public void ToggleQuizTaskPane()
        {
            try
            {
                if (_quizTaskPane == null || !_quizTaskPane.Visible)
                {
                    ShowQuizTaskPane();
                }
                else
                {
                    HideQuizTaskPane();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ToggleQuizTaskPane Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error toggling quiz pane: {ex.Message}", "Error");
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try
            {
                Debug.WriteLine("ThisAddIn_Startup: Starting up...");

                // Hook into PowerPoint events
                Application.PresentationOpen += Application_PresentationOpen;
                Application.PresentationClose += Application_PresentationClose;

                // Hook into slideshow events
                Application.SlideShowBegin += Application_SlideShowBegin;
                Application.SlideShowEnd += Application_SlideShowEnd;
                Application.SlideShowNextClick += Application_SlideShowNextClick; // <--- add this line
                Application.WindowSelectionChange += Application_WindowSelectionChange;
                // Initialize class API service
                _classApiService = new ClassApiService();

                Debug.WriteLine("ThisAddIn_Startup: Events hooked up successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ThisAddIn_Startup Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Add-in startup error: {ex.Message}", "Error");
            }
        }
        private void Application_SlideShowOnNext(
    Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn,
    Microsoft.Office.Interop.PowerPoint.Effect Effect)
        {
            try
            {
                // Make sure a shape was clicked
                var clickedShape = Effect?.Shape;
                if (clickedShape == null)
                    return;

                // Detect quiz shape by tag
                for (int i = 1; i <= clickedShape.Tags.Count; i++)
                {
                    if (clickedShape.Tags.Name(i) == "QuizButton" &&
                        clickedShape.Tags.Value(i) == "MultiChoiceQuiz")
                    {
                        ShowQuizResultFromTaskPane();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SlideShowOnNext Error: {ex.Message}");
            }
        }
        private void ShowQuizResultFromTaskPane()
        {
            int numberOfChoices = 4;
            List<int> correctIndices = new List<int> { 0 };

            if (_quizTaskPaneControl != null)
            {
                numberOfChoices = _quizTaskPaneControl.GetNumberOfChoices();
                var configured = _quizTaskPaneControl.GetCorrectAnswerIndices();
                if (configured != null && configured.Count > 0)
                    correctIndices = configured;
            }

            var percentages = new int[numberOfChoices];
            var basePercent = 100 / numberOfChoices;
            var remainder = 100 % numberOfChoices;

            for (int i = 0; i < numberOfChoices; i++)
                percentages[i] = basePercent + (i < remainder ? 1 : 0);

            using (var dlg = new QuizResultsForm(percentages, correctIndices))
                dlg.ShowDialog();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Clean up any active displays
            CleanupActiveDisplays();
        }
        // Add this method to ThisAddIn.cs to refresh the ribbon
        public void RefreshRibbon()
        {
            try
            {
                var ribbonExtensibility = CreateRibbonExtensibilityObject() as DynamicRibbon;
                // Unfortunately, we need a reference to the ribbon UI to invalidate it
                // The ribbon will be refreshed when Office checks the callback methods
                System.Diagnostics.Debug.WriteLine("RefreshRibbon called - ribbon state will update on next Office check");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshRibbon Error: {ex.Message}");
            }
        }

        // Update the ShowClassCodeForm method to refresh ribbon
        public void ShowClassCodeForm(string classCode, string courseName)
        {
            try
            {
                if (_activeClassForm != null)
                {
                    _activeClassForm.FormClosed -= ActiveClassForm_FormClosed; // Remove old handler
                    _activeClassForm.Close();
                }

                _activeClassForm = new ClassCodeDisplayForm(classCode, courseName);
                _activeClassForm.FormClosed += ActiveClassForm_FormClosed;
                _activeClassForm.Show();

                Debug.WriteLine($"Showing class code form with code: {classCode}");

                // Force ribbon refresh by simulating a state change
                // Office will call GetStartSlideshowEnabled to check button state
                RefreshRibbon();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowClassCodeForm Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error showing class form: {ex.Message}", "Error");
            }
        }


        // Update the ActiveClassForm_FormClosed method to refresh ribbon
        private async void ActiveClassForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            var form = sender as ClassCodeDisplayForm;
            Debug.WriteLine($"ActiveClassForm_FormClosed: ShouldEndClass={form?.ShouldEndClass}, HasActiveClass={_currentClassId.HasValue}");

            if (form?.ShouldEndClass == true && _currentClassId.HasValue)
            {
                try
                {
                    Debug.WriteLine("User requested to end class via form - calling EndManualClassAsync");
                    await EndManualClassAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error ending class from form: {ex.Message}");
                    System.Windows.Forms.MessageBox.Show($"Error ending class: {ex.Message}", "Error");
                }
            }

            _activeClassForm = null;

            // Force ribbon refresh when form closes
            RefreshRibbon();
        }


        private async void CleanupActiveDisplays()
        {
            try
            {
                // End active class first if it exists
                if (_currentClassId.HasValue)
                {
                    Debug.WriteLine($"Cleanup: Ending active class {_currentClassId}");
                    await _classApiService.EndClassAsync(_currentClassId.Value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ending class during cleanup: {ex.Message}");
            }
            finally
            {
                // Clean up displays regardless
                if (_activeSimpleDisplay != null)
                {
                    _activeSimpleDisplay.ForceClose();
                    _activeSimpleDisplay = null;
                }
                if (_activeClassForm != null)
                {
                    _activeClassForm.Close();
                    _activeClassForm = null;
                }
                if (_activePowerPointDisplay != null)
                {
                    _activePowerPointDisplay.Hide();
                    _activePowerPointDisplay = null;
                }

                // Hide quiz task pane
                HideQuizTaskPane();

                _currentClassId = null;
                _currentClassCode = null;
            }
        }

        #region Slideshow Event Handlers

        private async void Application_SlideShowBegin(SlideShowWindow Wn)
        {
            Debug.WriteLine("Application_SlideShowBegin: Slideshow started!");

            try
            {
                if (!IsTeacherLoggedIn) return;

                // Switch from ClassCodeDisplayForm to PowerPoint native display if needed
                if (_activeClassForm != null)
                {
                    _activeClassForm.Close();
                    _activeClassForm = null;
                }

                // Hide existing PowerPoint display
                if (_activePowerPointDisplay != null)
                {
                    _activePowerPointDisplay.Hide();
                }

                // Ensure we have a course
                if (!CurrentCourseId.HasValue)
                {
                    var presentation = Wn.Presentation;
                    await CreateCourseForPresentation(presentation);
                }

                if (CurrentCourseId.HasValue)
                {
                    // Create class session if not already created
                    if (!_currentClassId.HasValue)
                    {
                        var classResponse = await _classApiService.CreateClassFromPowerPointAsync(CurrentCourseId.Value);
                        _currentClassId = classResponse.Id;
                        _currentClassCode = classResponse.Code; // Store the actual code
                        Debug.WriteLine($"Created new class session: ID={_currentClassId}, Code={_currentClassCode}");
                    }

                    // Show code using PowerPoint native display
                    if (!string.IsNullOrEmpty(_currentClassCode))
                    {
                        _activePowerPointDisplay = new PowerPointCodeDisplay(_currentClassCode, CurrentCourseName);
                        _activePowerPointDisplay.Show();
                        Debug.WriteLine("PowerPoint code display is now visible");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Application_SlideShowBegin Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error starting slideshow class: {ex.Message}", "Error");
            }
        }

        private async void Application_SlideShowEnd(Presentation Pres)
        {
            Debug.WriteLine("Application_SlideShowEnd: Slideshow ended!");

            try
            {
                // Hide the PowerPoint display
                if (_activePowerPointDisplay != null)
                {
                    _activePowerPointDisplay.Hide();
                    _activePowerPointDisplay = null;
                }

                // Auto-end class session when slideshow ends
                if (_currentClassId.HasValue)
                {
                    Debug.WriteLine($"Auto-ending class session: {_currentClassId}");
                    await _classApiService.EndClassAsync(_currentClassId.Value);
                    _currentClassId = null;
                    _currentClassCode = null;
                    Debug.WriteLine("Class session ended automatically");

                    System.Windows.Forms.MessageBox.Show(
                        "Class session ended when slideshow stopped.",
                        "Class Ended",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Application_SlideShowEnd Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error ending class: {ex.Message}", "Error");
            }
        }

        #endregion

        #region Public Methods for Ribbon/External Use

        // Method for ribbon to start a manual class (non-slideshow)
        public async System.Threading.Tasks.Task<(string classCode, int classId)> StartManualClassAsync()
        {
            try
            {
                if (!IsTeacherLoggedIn || !CurrentCourseId.HasValue)
                    throw new InvalidOperationException("Teacher must be logged in and have an active presentation");

                if (_currentClassId.HasValue)
                    throw new InvalidOperationException("Class session already active");

                // Create class session
                var classResponse = await _classApiService.CreateClassFromPowerPointAsync(CurrentCourseId.Value);
                _currentClassId = classResponse.Id;
                _currentClassCode = classResponse.Code; // Store the actual code

                Debug.WriteLine($"Manual class created: ID={_currentClassId}, Code={_currentClassCode}");
                return (classResponse.Code, classResponse.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartManualClassAsync Error: {ex.Message}");
                throw;
            }
        }

        // Method for ribbon to end a class manually
        public async System.Threading.Tasks.Task EndManualClassAsync()
        {
            try
            {
                if (_currentClassId.HasValue)
                {
                    Debug.WriteLine($"Ending manual class: {_currentClassId}");
                    await _classApiService.EndClassAsync(_currentClassId.Value);
                    _currentClassId = null;
                    _currentClassCode = null;
                    Debug.WriteLine("Manual class ended successfully");

                    // Clean up any active displays (but don't call API again)
                    if (_activeSimpleDisplay != null)
                    {
                        _activeSimpleDisplay.ForceClose();
                        _activeSimpleDisplay = null;
                    }
                    if (_activeClassForm != null)
                    {
                        _activeClassForm.Close();
                        _activeClassForm = null;
                    }
                    if (_activePowerPointDisplay != null)
                    {
                        _activePowerPointDisplay.Hide();
                        _activePowerPointDisplay = null;
                    }

                    System.Windows.Forms.MessageBox.Show(
                        "Class session ended successfully.",
                        "Class Ended",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                }
                else
                {
                    Debug.WriteLine("EndManualClassAsync: No active class to end");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EndManualClassAsync Error: {ex.Message}");
                throw;
            }
        }
        // Add this property to ThisAddIn.cs
        public bool HasActiveClassForm => _activeClassForm != null && !_activeClassForm.IsDisposed;



        #endregion

        #region Existing Methods

        private async void Application_PresentationOpen(Microsoft.Office.Interop.PowerPoint.Presentation Pres)
        {
            Debug.WriteLine($"Application_PresentationOpen: {Pres.Name}");
            if (IsTeacherLoggedIn)
            {
                await CreateCourseForPresentation(Pres);
            }
        }

        private void Application_PresentationClose(Microsoft.Office.Interop.PowerPoint.Presentation Pres)
        {
            Debug.WriteLine($"Application_PresentationClose: {Pres.Name}");
            // Clear current course when presentation is closed
            CurrentCourseId = null;
            CurrentCourseName = null;

            // Clean up any active displays and end class
            CleanupActiveDisplays();
        }

        private async System.Threading.Tasks.Task CreateCourseForPresentation(Microsoft.Office.Interop.PowerPoint.Presentation presentation)
        {
            try
            {
                Debug.WriteLine($"CreateCourseForPresentation: Creating course for {presentation.Name}");
                var courseService = new CourseApiService();
                var courseName = GetCourseNameFromPresentation(presentation);

                var courseResponse = await courseService.CreateCourseAsync(courseName);
                CurrentCourseId = courseResponse.Id;
                CurrentCourseName = courseResponse.Name;

                Debug.WriteLine($"Course created: ID={CurrentCourseId}, Name={CurrentCourseName}");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"CreateCourseForPresentation Error: {ex.Message}");
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
            Debug.WriteLine("LoadTeacherRibbon: Teacher logging in...");
            IsTeacherLoggedIn = true;

            try
            {
                // If there's already a presentation open, create a course for it
                if (Application.Presentations.Count > 0)
                {
                    var activePresentation = Application.ActivePresentation;
                    if (activePresentation != null)
                    {
                        Debug.WriteLine($"Creating course for presentation: {activePresentation.Name}");
                        await CreateCourseForPresentation(activePresentation);

                        // Update quiz task pane with new course
                        if (_quizTaskPane != null && _quizTaskPane.Visible && CurrentCourseId.HasValue)
                        {
                            _quizTaskPaneControl.SetCourseId(CurrentCourseId.Value);
                        }

                        Debug.WriteLine($"Course created: ID={CurrentCourseId}, Name={CurrentCourseName}");
                    }
                    else
                    {
                        Debug.WriteLine("Active presentation is null");
                    }
                }
                else
                {
                    Debug.WriteLine("No presentations are currently open");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadTeacherRibbon course creation error: {ex.Message}");
                // Don't show error to user, just log it - they can create course later
            }

            Debug.WriteLine($"LoadTeacherRibbon: Completed. IsTeacherLoggedIn={IsTeacherLoggedIn}, CurrentCourseId={CurrentCourseId}");
        }

        public void UnloadTeacherRibbon()
        {
            Debug.WriteLine("UnloadTeacherRibbon: Teacher logging out...");
            IsTeacherLoggedIn = false;
            CurrentCourseId = null;
            CurrentCourseName = null;

            // Clean up any active displays
            CleanupActiveDisplays();
        }

        public bool HasActivePresentation()
        {
            try
            {
                var hasActive = Application.Presentations.Count > 0 && Application.ActivePresentation != null;
                Debug.WriteLine($"HasActivePresentation: {hasActive}");
                return hasActive;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HasActivePresentation Error: {ex.Message}");
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

        // Public properties to check class status
        public bool HasActiveClass => _currentClassId.HasValue;
        public int? CurrentClassId => _currentClassId;
        public string CurrentClassCode => _currentClassCode; // Expose the actual class code

        #endregion

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

        private void Application_SlideShowNextClick(Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn, Microsoft.Office.Interop.PowerPoint.Effect n)
        {
            try
            {
                if (n == null) return;

                var clickedShape = n.Shape;
                if (clickedShape == null) return;

                bool isQuizButton = false;
                try
                {
                    for (int i = 1; i <= clickedShape.Tags.Count; i++)
                    {
                        if (clickedShape.Tags.Name(i) == "QuizButton" && clickedShape.Tags.Value(i) == "MultiChoiceQuiz")
                        {
                            isQuizButton = true;
                            break;
                        }
                    }
                }
                catch
                {
                    // ignore tag access errors
                }

                if (!isQuizButton) return;

                // Prepare data from task pane if available
                int numberOfChoices = 4;
                List<int> correctIndices = new List<int> { 0 };

                try
                {
                    if (_quizTaskPaneControl != null)
                    {
                        numberOfChoices = _quizTaskPaneControl.GetNumberOfChoices();
                        var configuredCorrect = _quizTaskPaneControl.GetCorrectAnswerIndices();
                        if (configuredCorrect != null && configuredCorrect.Count > 0)
                            correctIndices = configuredCorrect;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading task pane settings for results dialog: {ex.Message}");
                }

                // Build sample percentages (even split) — replace with real API call if you have live data
                var percentages = new int[numberOfChoices];
                var basePercent = 100 / Math.Max(1, numberOfChoices);
                var remainder = 100 % Math.Max(1, numberOfChoices);
                for (int i = 0; i < numberOfChoices; i++)
                {
                    percentages[i] = basePercent + (i < remainder ? 1 : 0);
                }

                try
                {
                    using (var dlg = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(percentages, correctIndices))
                    {
                        // Modal dialog is appropriate in slideshow to ensure user sees results
                        dlg.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to show quiz results dialog: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application_SlideShowNextClick Error: {ex.Message}");
            }
        }
    }
}
