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

            return true;
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

                // Show the quiz task pane for settings control
                // Note: Quiz button will be added after saving the quiz with Save button
                Globals.ThisAddIn.ShowQuizTaskPane();

                // Refresh ribbon to update button states
                ribbon?.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding quiz button: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void OnShortAnswerQuizClick(Office.IRibbonControl control)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"OnShortAnswerQuizClick: CurrentCourseId={Globals.ThisAddIn.CurrentCourseId}");

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

                // Show the short answer task pane
                Globals.ThisAddIn.ShowShortAnswerTaskPane();

                // Refresh ribbon to update button states
                ribbon?.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening short answer quiz: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper: find an existing quiz button shape on a slide (returns null if none)
        private Microsoft.Office.Interop.PowerPoint.Shape FindExistingQuizButton(Microsoft.Office.Interop.PowerPoint.Slide slide)
        {
            try
            {
                if (slide == null) return null;

                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    try
                    {
                        // Tags collection is 1-based
                        for (int i = 1; i <= shape.Tags.Count; i++)
                        {
                            if (shape.Tags.Name(i).Equals("QuizButton", StringComparison.OrdinalIgnoreCase) && 
                                shape.Tags.Value(i).Equals("MultiChoiceQuiz", StringComparison.OrdinalIgnoreCase))
                            {
                                return shape;
                            }
                        }
                    }
                    catch
                    {
                        // ignore shape/tag access errors and continue searching other shapes
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindExistingQuizButton Error: {ex.Message}");
            }

            return null;
        }
        private void AddQuizButtonToSlide()
        {
            try
            {
                var application = Globals.ThisAddIn.Application;
                var presentation = application.ActivePresentation;
                var slide = (Microsoft.Office.Interop.PowerPoint.Slide)application.ActiveWindow.View.Slide;

                // Check if a quiz button already exists on this slide
                var existingQuizButton = FindExistingQuizButton(slide);
                if (existingQuizButton != null)
                {
                    MessageBox.Show("Quiz button already exists on this slide.\n\nTask pane reopened for configuration.",
                        "Quiz Button Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create a movable quiz button
                var quizButton = slide.Shapes.AddShape(
                    Microsoft.Office.Core.MsoAutoShapeType.msoShapeRoundedRectangle,
                    100, 50, 120, 40); // Position: x=100, y=50, width=120, height=40

                // Style the button
                quizButton.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 120, 215));
                quizButton.Line.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 100, 180));
                quizButton.Line.Weight = 2;
                quizButton.Shadow.Type = Microsoft.Office.Core.MsoShadowType.msoShadow6;

                // Add text to the button
                quizButton.TextFrame.TextRange.Text = "📋 Quiz";
                quizButton.TextFrame.TextRange.Font.Name = "Segoe UI";
                quizButton.TextFrame.TextRange.Font.Size = 12;
                quizButton.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                quizButton.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                quizButton.TextFrame.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                quizButton.TextFrame.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;

                // Set a unique name and tags to identify this as a quiz button
                quizButton.Name = $"QuizButton_{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
                quizButton.Tags.Add("QuizButton", "MultiChoiceQuiz");
                quizButton.Tags.Add("ButtonType", "QuizControl");

                // Add action setting to make button clickable in slideshow
                try
                {
                    quizButton.ActionSettings[Microsoft.Office.Interop.PowerPoint.PpMouseActivation.ppMouseClick].Action = 
                        Microsoft.Office.Interop.PowerPoint.PpActionType.ppActionNone;
                    
                    // Add an animation effect to trigger SlideShowNextClick event
                    var timeline = slide.TimeLine;
                    if (timeline != null)
                    {
                        var mainSequence = timeline.MainSequence;
                        if (mainSequence != null)
                        {
                            var effect = mainSequence.AddEffect(
                                quizButton,
                                Microsoft.Office.Interop.PowerPoint.MsoAnimEffect.msoAnimEffectAppear,
                                Microsoft.Office.Interop.PowerPoint.MsoAnimateByLevel.msoAnimateLevelNone,
                                Microsoft.Office.Interop.PowerPoint.MsoAnimTriggerType.msoAnimTriggerOnPageClick);
                            
                            if (effect != null)
                            {
                                effect.Timing.TriggerType = Microsoft.Office.Interop.PowerPoint.MsoAnimTriggerType.msoAnimTriggerOnShapeClick;
                                effect.Timing.TriggerShape = quizButton;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to add action setting or animation to quiz button: {ex.Message}");
                }

                // Add a click-triggered animation effect so SlideShowNextClick provides an Effect with .Shape
                try
                {
                    var seq = slide.TimeLine?.MainSequence;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to add animation effect to quiz button: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"Added quiz button with name: {quizButton.Name}");

                MessageBox.Show("Quiz button added to slide! You can move it to any position you want.\n\nStart the slideshow and click the button to open the Quiz Results dialog.",
                    "Quiz Button Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add quiz button to slide: {ex.Message}");
            }
        }

        // Helper: add click-triggered effect to all existing quiz buttons in the active presentation
        public void AddClickEffectToAllQuizButtonsInActivePresentation()
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                if (app == null || app.Presentations == null || app.Presentations.Count == 0)
                {
                    MessageBox.Show("No open presentation found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var pres = app.ActivePresentation;
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in pres.Slides)
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        bool isQuizButton = false;
                        try
                        {
                            for (int i = 1; i <= shape.Tags.Count; i++)
                            {
                                if (shape.Tags.Name(i).Equals("QuizButton", StringComparison.OrdinalIgnoreCase) && shape.Tags.Value(i) == "MultiChoiceQuiz")
                                {
                                    isQuizButton = true;
                                    break;
                                }
                            }
                        }
                        catch { /* ignore tag errors */ }

                        if (!isQuizButton) continue;

                        try
                        {
                            var seq = slide.TimeLine?.MainSequence;
                            if (seq != null)
                            {
                                // Check if this shape already has an effect
                                bool hasEffect = false;
                                foreach (Microsoft.Office.Interop.PowerPoint.Effect eff in seq)
                                {
                                    try
                                    {
                                        if (eff.Shape != null && eff.Shape.Name == shape.Name)
                                        {
                                            hasEffect = true;
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                                
                                if (!hasEffect)
                                {
                                    // Add animation effect to enable clicking
                                    var effect = seq.AddEffect(
                                        shape,
                                        Microsoft.Office.Interop.PowerPoint.MsoAnimEffect.msoAnimEffectAppear,
                                        Microsoft.Office.Interop.PowerPoint.MsoAnimateByLevel.msoAnimateLevelNone,
                                        Microsoft.Office.Interop.PowerPoint.MsoAnimTriggerType.msoAnimTriggerOnPageClick);
                                    
                                    if (effect != null)
                                    {
                                        effect.Timing.TriggerType = Microsoft.Office.Interop.PowerPoint.MsoAnimTriggerType.msoAnimTriggerOnShapeClick;
                                        effect.Timing.TriggerShape = shape;
                                    }
                                }
                                
                                // Ensure action setting is configured for slideshow clicks
                                try
                                {
                                    shape.ActionSettings[Microsoft.Office.Interop.PowerPoint.PpMouseActivation.ppMouseClick].Action = 
                                        Microsoft.Office.Interop.PowerPoint.PpActionType.ppActionNone;
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to ensure click effect on shape {shape.Name}: {ex.Message}");
                        }
                    }
                }

                MessageBox.Show("Ensured click effects on all quiz buttons in the active presentation.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add click effects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public void OnShowQuizStatsClick(Office.IRibbonControl control)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnShowQuizStatsClick Called ===");
                
                var application = Globals.ThisAddIn.Application;
                
                System.Diagnostics.Debug.WriteLine($"Selection Type: {application.ActiveWindow.Selection.Type}");
                
                // Check if a shape is selected
                if (application.ActiveWindow.Selection.Type != Microsoft.Office.Interop.PowerPoint.PpSelectionType.ppSelectionShapes)
                {
                    MessageBox.Show("Please select a quiz button first.", "No Quiz Button Selected", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Number of shapes selected: {application.ActiveWindow.Selection.ShapeRange.Count}");

                // Check each selected shape
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in application.ActiveWindow.Selection.ShapeRange)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Examining Shape ===");
                    System.Diagnostics.Debug.WriteLine($"Shape Name: {shape.Name}");
                    System.Diagnostics.Debug.WriteLine($"Shape Type: {shape.Type}");
                    System.Diagnostics.Debug.WriteLine($"Tag Count: {shape.Tags.Count}");
                    
                    bool isQuizButton = false;
                    int quizId = 0;

                    try
                    {
                        for (int i = 1; i <= shape.Tags.Count; i++)
                        {
                            var tagName = shape.Tags.Name(i);
                            var tagValue = shape.Tags.Value(i);
                            System.Diagnostics.Debug.WriteLine($"Tag {i}: {tagName} = {tagValue}");
                            
                            if (tagName.Equals("QuizButton", StringComparison.OrdinalIgnoreCase) && 
                                tagValue.Equals("MultiChoiceQuiz", StringComparison.OrdinalIgnoreCase))
                            {
                                isQuizButton = true;
                                System.Diagnostics.Debug.WriteLine(">>> QUIZ BUTTON DETECTED <<<");
                            }
                            if (tagName.Equals("QuizId", StringComparison.OrdinalIgnoreCase))
                            {
                                int.TryParse(tagValue, out quizId);
                                System.Diagnostics.Debug.WriteLine($">>> QUIZ ID FOUND: {quizId} <<<");
                            }
                        }
                    }
                    catch (Exception tagEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error reading tags: {tagEx.Message}");
                    }

                    if (isQuizButton)
                    {
                        System.Diagnostics.Debug.WriteLine($"Quiz button found - Quiz ID: {quizId}");
                        
                        // Call the method to show quiz results
                        Globals.ThisAddIn.ShowQuizResultsForButton(quizId);
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Shape is NOT a quiz button");
                    }
                }

                MessageBox.Show("The selected shape is not a quiz button.\n\nPlease select a quiz button to view statistics.", 
                    "Not a Quiz Button", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                System.Diagnostics.Debug.WriteLine("=== End OnShowQuizStatsClick ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnShowQuizStatsClick: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error showing quiz stats: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void OnCleanupQuizzesClick(Office.IRibbonControl control)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OnCleanupQuizzesClick: Starting cleanup...");
                await Globals.ThisAddIn.CleanupDeletedQuizButtonsAsync();
                System.Diagnostics.Debug.WriteLine("OnCleanupQuizzesClick: Cleanup completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnCleanupQuizzesClick: {ex.Message}");
                MessageBox.Show($"Error cleaning up quizzes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

