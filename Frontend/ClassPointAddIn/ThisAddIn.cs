using ClassPointAddIn.Api.Services.ClassService;
using ClassPointAddIn.Api.Services.CourseService;
using ClassPointAddIn.Views;
using ClassPointAddIn.Views.Quizzes;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private CustomTaskPane _shortAnswerTaskPane;
        private ShortAnswerTaskPane _shortAnswerTaskPaneControl;
        private QuizResultsForm _activeQuizResultsForm; // Track active quiz results dialog
        private ShortAnswerResultsForm _activeShortAnswerResultsForm; // Track active short answer results dialog

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

        public void ShowShortAnswerTaskPane()
        {
            try
            {
                if (_shortAnswerTaskPane == null)
                {
                    _shortAnswerTaskPaneControl = new ShortAnswerTaskPane();

                    _shortAnswerTaskPane = CustomTaskPanes.Add(_shortAnswerTaskPaneControl, "Short Answer Quiz");
                    _shortAnswerTaskPane.Width = 350;
                    _shortAnswerTaskPane.DockPosition = Microsoft.Office.Core.MsoCTPDockPosition.msoCTPDockPositionRight;
                    _shortAnswerTaskPane.DockPositionRestrict = Microsoft.Office.Core.MsoCTPDockPositionRestrict.msoCTPDockPositionRestrictNoChange;
                }

                // Update course ID if available
                if (CurrentCourseId.HasValue)
                {
                    _shortAnswerTaskPaneControl.SetCourseId(CurrentCourseId.Value);
                }

                _shortAnswerTaskPane.Visible = true;

                Debug.WriteLine("Short Answer Task Pane shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowShortAnswerTaskPane Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error showing short answer pane: {ex.Message}", "Error");
            }
        }

        private void Application_WindowSelectionChange(Microsoft.Office.Interop.PowerPoint.Selection Sel)
        {
            try
            {
                Debug.WriteLine($"WindowSelectionChange triggered. Selection type: {Sel.Type}");

                // Check if the selection contains a quiz button
                if (Sel.Type == Microsoft.Office.Interop.PowerPoint.PpSelectionType.ppSelectionShapes)
                {
                    Debug.WriteLine($"Selection is shapes. Shape count: {Sel.ShapeRange.Count}");

                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in Sel.ShapeRange)
                    {
                        Debug.WriteLine($"Checking shape: {shape.Name}");

                        // Check if this is a quiz button
                        bool isQuizButton = false;
                        int quizId = 0;
                        string quizType = null;

                        try
                        {
                            Debug.WriteLine($"Shape has {shape.Tags.Count} tags");

                            for (int i = 1; i <= shape.Tags.Count; i++)
                            {
                                var tagName = shape.Tags.Name(i);
                                var tagValue = shape.Tags.Value(i);
                                Debug.WriteLine($"Tag {i}: {tagName} = {tagValue}");

                                // Check if this is ANY quiz button (not just MultiChoiceQuiz)
                                if (tagName.Equals("QuizButton", StringComparison.OrdinalIgnoreCase))
                                {
                                    isQuizButton = true;
                                    quizType = tagValue;  // Store the quiz type (ShortAnswerQuiz or MultiChoiceQuiz)
                                    Debug.WriteLine($"Quiz button detected! Type: {tagValue}");
                                }
                                if (tagName.Equals("QuizId", StringComparison.OrdinalIgnoreCase))
                                {
                                    int.TryParse(tagValue, out quizId);
                                    Debug.WriteLine($"Quiz ID found: {quizId}");
                                }
                            }
                        }
                        catch (Exception tagEx)
                        {
                            Debug.WriteLine($"Error reading tags: {tagEx.Message}");
                        }

                        if (isQuizButton)
                        {
                            Debug.WriteLine($"Quiz button selected - Quiz ID: {quizId}, Type: {quizType}");

                            // Only show multiple choice task pane for multiple choice quizzes
                            if (quizType == "MultiChoiceQuiz")
                            {
                                Debug.WriteLine("Showing multiple choice task pane");
                                ShowQuizTaskPane();
                            }
                            else
                            {
                                Debug.WriteLine($"Skipping task pane for quiz type: {quizType}");
                            }

                            // Show quiz results dialog with real data if we have a quiz ID
                            Debug.WriteLine("Calling ShowQuizResultsDialogAsync...");
                            ShowQuizResultsDialogAsync(quizId);

                            break;
                        }
                        else
                        {
                            Debug.WriteLine("This shape is not a quiz button");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Selection is not shapes, it's: {Sel.Type}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Application_WindowSelectionChange Error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ShowAndActivateForm(QuizResultsForm form)
        {
            if (form == null || form.IsDisposed)
                return;

            Debug.WriteLine($"[ThisAddIn] ShowAndActivateForm called on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            // Use ShowDialog for modal behavior with proper message pump
            // This is how LoginForm works successfully
            form.ShowDialog();

            Debug.WriteLine($"[ThisAddIn] ShowDialog completed");
        }

        private void ShowAndActivateForm(ShortAnswerResultsForm form)
        {
            if (form == null || form.IsDisposed)
                return;

            Debug.WriteLine($"[ThisAddIn] ShowAndActivateForm (ShortAnswer) called on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            // Ensure we're on the UI thread before showing the dialog
            if (form.InvokeRequired)
            {
                Debug.WriteLine($"[ThisAddIn] InvokeRequired - marshaling to UI thread");
                form.Invoke(new System.Action(() =>
                {
                    Debug.WriteLine($"[ThisAddIn] Now on UI thread {System.Threading.Thread.CurrentThread.ManagedThreadId}, showing dialog");
                    form.ShowDialog();
                }));
            }
            else
            {
                Debug.WriteLine($"[ThisAddIn] Already on UI thread, showing dialog directly");
                // Use ShowDialog for modal behavior with proper message pump
                form.ShowDialog();
            }

            Debug.WriteLine($"[ThisAddIn] ShowDialog completed");
        }

        private async void ShowQuizResultsDialogAsync(int quizId)
        {
            try
            {
                Debug.WriteLine($"[ThisAddIn] ======================================");
                Debug.WriteLine($"[ThisAddIn] ShowQuizResultsDialogAsync START");
                Debug.WriteLine($"[ThisAddIn] QuizId: {quizId}");
                Debug.WriteLine($"[ThisAddIn] Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Debug.WriteLine($"[ThisAddIn] ======================================");

                if (quizId > 0)
                {
                    Debug.WriteLine("[ThisAddIn] Fetching quiz details to determine quiz type...");
                    var quizService = new Api.Services.QuizService.QuizApiService();
                    var quizDetails = await quizService.GetQuizAsync(quizId).ConfigureAwait(true);

                    Debug.WriteLine($"[ThisAddIn] Quiz details received: {quizDetails != null}");
                    if (quizDetails != null)
                    {
                        Debug.WriteLine($"[ThisAddIn] Quiz ID: {quizDetails.Id}");
                        Debug.WriteLine($"[ThisAddIn] Quiz Title: {quizDetails.Title}");
                        Debug.WriteLine($"[ThisAddIn] Quiz Type: '{quizDetails.QuizType}'");
                        Debug.WriteLine($"[ThisAddIn] Quiz Type Length: {quizDetails.QuizType?.Length ?? 0}");
                        Debug.WriteLine($"[ThisAddIn] Quiz Type == 'short_answer': {quizDetails.QuizType == "short_answer"}");
                    }

                    // Check if this is a short answer quiz
                    if (quizDetails != null && quizDetails.QuizType == "short_answer")
                    {
                        Debug.WriteLine("[ThisAddIn] *** ROUTING TO SHORT ANSWER RESULTS ***");
                        await ShowShortAnswerResultsAsync(quizId, quizDetails.Properties?.QuestionText ?? "Short Answer Question");
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("[ThisAddIn] *** ROUTING TO MULTIPLE CHOICE RESULTS ***");
                    }
                }

                // Check if dialog is already open for a DIFFERENT quiz - close it
                if (_activeQuizResultsForm != null && !_activeQuizResultsForm.IsDisposed)
                {
                    int currentQuizId = _activeQuizResultsForm.QuizId;

                    if (currentQuizId == quizId)
                    {
                        // Same quiz - just bring to front
                        Debug.WriteLine("[ThisAddIn] Quiz results dialog already open for same quiz - bringing to front");

                        if (_activeQuizResultsForm.InvokeRequired)
                        {
                            _activeQuizResultsForm.Invoke(new Action(() =>
                            {
                                if (!_activeQuizResultsForm.IsDisposed)
                                {
                                    _activeQuizResultsForm.BringToFront();
                                    _activeQuizResultsForm.Activate();
                                }
                            }));
                        }
                        else
                        {
                            _activeQuizResultsForm.BringToFront();
                            _activeQuizResultsForm.Activate();
                        }
                        return;
                    }
                    else
                    {
                        // Different quiz - close the old form
                        Debug.WriteLine($"[ThisAddIn] Different quiz detected (current: {currentQuizId}, new: {quizId}) - closing old form");

                        if (_activeQuizResultsForm.InvokeRequired)
                        {
                            _activeQuizResultsForm.Invoke(new Action(() =>
                            {
                                if (!_activeQuizResultsForm.IsDisposed)
                                {
                                    _activeQuizResultsForm.Close();
                                }
                            }));
                        }
                        else
                        {
                            _activeQuizResultsForm.Close();
                        }

                        _activeQuizResultsForm = null;
                    }
                }

                if (quizId > 0)
                {
                    Debug.WriteLine("[ThisAddIn] Fetching quiz submission stats from API...");
                    Debug.WriteLine($"[ThisAddIn] Before API call - thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");

                    // Fetch real submission data from API
                    var quizService = new Api.Services.QuizService.QuizApiService();
                    var stats = await quizService.GetQuizSubmissionStatsAsync(quizId).ConfigureAwait(true);

                    Debug.WriteLine($"[ThisAddIn] After API call - thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                    Debug.WriteLine($"[ThisAddIn] API response - Stats null: {stats == null}, ChoiceStats null: {stats?.ChoiceStats == null}");

                    if (stats != null && stats.ChoiceStats != null)
                    {
                        Debug.WriteLine($"[ThisAddIn] Processing stats - TotalSubmissions: {stats.TotalSubmissions}, ChoiceCount: {stats.ChoiceStats.Count}");

                        var percentagesData = stats.ChoiceStats.Select(c => c.Percentage).ToArray();
                        var submissionCounts = stats.ChoiceStats.Select(c => c.Count).ToArray();
                        var labels = stats.ChoiceStats.Select(c => c.Label).ToList();
                        var correctIndicesData = stats.ChoiceStats
                            .Where(c => c.IsCorrect)
                            .Select(c => c.Index)
                            .ToList();
                        var studentNamesPerChoice = stats.ChoiceStats.Select(c => c.Students ?? new List<string>()).ToList();

                        Debug.WriteLine($"[ThisAddIn] Creating QuizResultsForm with processed data...");

                        _activeQuizResultsForm = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(
                            percentagesData,
                            correctIndicesData,
                            labels,
                            submissionCounts,
                            stats.TotalSubmissions,
                            studentNamesPerChoice,
                            quizId,
                            stats.EnrolledStudents);

                        _activeQuizResultsForm.FormClosed += (s, e) => _activeQuizResultsForm = null;

                        Debug.WriteLine($"[ThisAddIn] Calling ShowDialog()...");
                        ShowAndActivateForm(_activeQuizResultsForm);
                        Debug.WriteLine($"[ThisAddIn] ShowDialog() completed successfully");
                        return;
                    }
                }

                // Fallback to sample data if no quiz ID or API call failed
                Debug.WriteLine("[ThisAddIn] Using fallback sample data (no quiz ID or API failed)");

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

                Debug.WriteLine($"[ThisAddIn] Creating QuizResultsForm with sample data - {numberOfChoices} choices");

                _activeQuizResultsForm = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(percentages, correctIndices);
                _activeQuizResultsForm.FormClosed += (s, e) => _activeQuizResultsForm = null;

                Debug.WriteLine("[ThisAddIn] Calling ShowDialog() for sample data...");
                ShowAndActivateForm(_activeQuizResultsForm);
                Debug.WriteLine("[ThisAddIn] ShowDialog() completed for sample data");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThisAddIn] ShowQuizResultsDialogAsync Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ThisAddIn] Stack trace: {ex.StackTrace}");
            }
        }

        private async System.Threading.Tasks.Task ShowShortAnswerResultsAsync(int quizId, string questionText = null)
        {
            try
            {
                Debug.WriteLine($"[ThisAddIn] ======================================");
                Debug.WriteLine($"[ThisAddIn] ShowShortAnswerResultsAsync START");
                Debug.WriteLine($"[ThisAddIn] QuizId: {quizId}");
                Debug.WriteLine($"[ThisAddIn] QuestionText: {questionText ?? "(null)"}");
                Debug.WriteLine($"[ThisAddIn] Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Debug.WriteLine($"[ThisAddIn] ======================================");

                // Check if dialog is already open for the same quiz
                if (_activeShortAnswerResultsForm != null && !_activeShortAnswerResultsForm.IsDisposed)
                {
                    if (_activeShortAnswerResultsForm.QuizId == quizId)
                    {
                        Debug.WriteLine("[ThisAddIn] Short answer results dialog already open - bringing to front");
                        _activeShortAnswerResultsForm.BringToFront();
                        _activeShortAnswerResultsForm.Activate();
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("[ThisAddIn] Closing old short answer results dialog");
                        _activeShortAnswerResultsForm.Close();
                        _activeShortAnswerResultsForm = null;
                    }
                }

                // Fetch quiz details and submissions from API
                Debug.WriteLine("[ThisAddIn] Creating QuizApiService instance");
                var quizService = new Api.Services.QuizService.QuizApiService();

                // Get quiz details if we don't have the question text
                if (string.IsNullOrEmpty(questionText))
                {
                    Debug.WriteLine("[ThisAddIn] QuestionText is empty, fetching quiz details from API");
                    var quizDetails = await quizService.GetQuizAsync(quizId);
                    Debug.WriteLine($"[ThisAddIn] Quiz details received: {quizDetails != null}");
                    questionText = quizDetails?.Properties?.QuestionText ?? "Short Answer Question";
                    Debug.WriteLine($"[ThisAddIn] QuestionText resolved to: {questionText}");
                }

                // Fetch actual submissions from API
                Debug.WriteLine("[ThisAddIn] Fetching short answer submissions from API");
                var stats = await quizService.GetShortAnswerStatsAsync(quizId);
                Debug.WriteLine($"[ThisAddIn] Stats received: {stats != null}");
                
                var submissions = new List<ShortAnswerSubmission>();
                int totalSubmissions = 0;
                int enrolledStudents = 0;

                if (stats != null)
                {
                    Debug.WriteLine($"[ThisAddIn] Got stats - Submissions: {stats.Submissions?.Count ?? 0}, Enrolled: {stats.EnrolledStudents}");
                    
                    // Map API response to ShortAnswerSubmission objects
                    submissions = stats.Submissions?.Select(s => new ShortAnswerSubmission
                    {
                        SubmissionId = s.Id,
                        StudentName = s.StudentName,
                        AnswerText = s.Answer,
                        SubmittedAt = DateTime.TryParse(s.SubmittedAt, out var submittedDate) ? submittedDate : DateTime.Now,
                        IsLiked = s.IsLiked
                    }).ToList() ?? new List<ShortAnswerSubmission>();
                    
                    totalSubmissions = stats.TotalSubmissions;
                    enrolledStudents = stats.EnrolledStudents;
                    
                    // Use question text from API if not provided
                    if (string.IsNullOrEmpty(questionText))
                    {
                        questionText = stats.QuestionText ?? "Short Answer Question";
                    }
                    
                    Debug.WriteLine($"[ThisAddIn] Mapped {submissions.Count} submissions");
                }
                else
                {
                    Debug.WriteLine("[ThisAddIn] No stats returned from API - using empty submission list");
                }

                Debug.WriteLine("[ThisAddIn] Creating ShortAnswerResultsForm instance");
                Debug.WriteLine($"[ThisAddIn] Parameters - QuizId: {quizId}, Question: {questionText}, Submissions: {submissions.Count}, Total: {totalSubmissions}, Enrolled: {enrolledStudents}");
                
                _activeShortAnswerResultsForm = new ShortAnswerResultsForm(
                   quizId,
                    questionText,
                 submissions,
                 totalSubmissions,
                   enrolledStudents);

                Debug.WriteLine("[ThisAddIn] Form created successfully");
                _activeShortAnswerResultsForm.FormClosed += (s, e) => 
                {
                    Debug.WriteLine("[ThisAddIn] Short answer results form closed event");
                    _activeShortAnswerResultsForm = null;
                };

                Debug.WriteLine("[ThisAddIn] Calling ShowAndActivateForm");
                ShowAndActivateForm(_activeShortAnswerResultsForm);
                Debug.WriteLine("[ThisAddIn] ShowAndActivateForm returned");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ThisAddIn] ======================================");
                Debug.WriteLine($"[ThisAddIn] ShowShortAnswerResultsAsync ERROR");
                Debug.WriteLine($"[ThisAddIn] Message: {ex.Message}");
                Debug.WriteLine($"[ThisAddIn] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ThisAddIn] Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"[ThisAddIn] Inner stack trace: {ex.InnerException.StackTrace}");
                }
                Debug.WriteLine($"[ThisAddIn] ======================================");
                System.Windows.Forms.MessageBox.Show($"Error showing short answer results: {ex.Message}\n\nSee debug output for details.", "Error");
            }
        }

        // Public method that can be called from the ribbon
        public void ShowQuizResultsForButton(int quizId)
        {
            Debug.WriteLine($"[ThisAddIn] ShowQuizResultsForButton called with Quiz ID: {quizId}");
            ShowQuizResultsDialogAsync(quizId);
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
                Debug.WriteLine($"[ThisAddIn] Startup - Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

                Debug.WriteLine("ThisAddIn_Startup: Starting up...");

                // Hook into PowerPoint events
                Application.PresentationOpen += Application_PresentationOpen;
                Application.PresentationClose += Application_PresentationClose;

                // Hook into slideshow events
                Application.SlideShowBegin += Application_SlideShowBegin;
                Application.SlideShowEnd += Application_SlideShowEnd;
                Application.SlideShowNextClick += Application_SlideShowNextClick;
                Application.WindowSelectionChange += Application_WindowSelectionChange;

                // Hook into window activation to detect clicks
                Application.WindowActivate += Application_WindowActivate;

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

        private void Application_WindowActivate(Microsoft.Office.Interop.PowerPoint.Presentation Pres, Microsoft.Office.Interop.PowerPoint.DocumentWindow Wn)
        {
            try
            {
                // Check if a shape is selected when window is activated (e.g., after clicking)
                if (Wn.Selection.Type == Microsoft.Office.Interop.PowerPoint.PpSelectionType.ppSelectionShapes)
                {
                    CheckAndShowQuizButtonDialog(Wn.Selection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Application_WindowActivate Error: {ex.Message}");
            }
        }

        private void CheckAndShowQuizButtonDialog(Microsoft.Office.Interop.PowerPoint.Selection selection)
        {
            try
            {
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in selection.ShapeRange)
                {
                    bool isQuizButton = false;
                    int quizId = 0;

                    try
                    {
                        for (int i = 1; i <= shape.Tags.Count; i++)
                        {
                            // Check for ANY quiz button type
                            if (shape.Tags.Name(i).Equals("QuizButton", StringComparison.OrdinalIgnoreCase))
                            {
                                isQuizButton = true;
                            }
                            if (shape.Tags.Name(i).Equals("QuizId", StringComparison.OrdinalIgnoreCase))
                            {
                                int.TryParse(shape.Tags.Value(i), out quizId);
                            }
                        }
                    }
                    catch { }

                    if (isQuizButton)
                    {
                        Debug.WriteLine($"Quiz button detected - Quiz ID: {quizId}");
                        ShowQuizResultsDialogAsync(quizId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckAndShowQuizButtonDialog Error: {ex.Message}");
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
                    // Check for ANY quiz button type
                    if (clickedShape.Tags.Name(i) == "QuizButton")
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
                        _activePowerPointDisplay = new PowerPointCodeDisplay(_currentClassCode, CurrentCourseName, 0, _currentClassId ?? 0);
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

        public void AddQuizButtonToSlideWithQuizId(int quizId, string quizTitle, string quizType = "MultipleChoice")
        {
            try
            {
                var application = Application;
                var presentation = application.ActivePresentation;
                var slide = (Microsoft.Office.Interop.PowerPoint.Slide)application.ActiveWindow.View.Slide;

                // Determine the tag value based on quiz type
                string quizTypeTag = quizType == "ShortAnswer" ? "ShortAnswerQuiz" : "MultiChoiceQuiz";

                // Check if a quiz button already exists on this slide
                Microsoft.Office.Interop.PowerPoint.Shape existingQuizButton = null;
                try
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        try
                        {
                            for (int i = 1; i <= shape.Tags.Count; i++)
                            {
                                if (shape.Tags.Name(i).Equals("QuizButton", StringComparison.OrdinalIgnoreCase))
                                {
                                    existingQuizButton = shape;
                                    break;
                                }
                            }
                            if (existingQuizButton != null) break;
                        }
                        catch { }
                    }
                }
                catch { }

                if (existingQuizButton != null)
                {
                    // Update the existing button with the quiz ID and type
                    existingQuizButton.Tags.Add("QuizId", quizId.ToString());
                    existingQuizButton.Tags.Add("QuizType", quizTypeTag);
                    existingQuizButton.TextFrame.TextRange.Text = $"📋 {quizTitle}";

                    // Ensure action setting is configured for slideshow clicks
                    try
                    {
                        existingQuizButton.ActionSettings[Microsoft.Office.Interop.PowerPoint.PpMouseActivation.ppMouseClick].Action =
                            Microsoft.Office.Interop.PowerPoint.PpActionType.ppActionNone;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to add action setting to existing button: {ex.Message}");
                    }

                    return;
                }

                // Create a new quiz button if none exists
                var quizButton = slide.Shapes.AddShape(
                    Microsoft.Office.Core.MsoAutoShapeType.msoShapeRoundedRectangle,
                    100, 50, 120, 40);

                quizButton.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 120, 215));
                quizButton.Line.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 100, 180));
                quizButton.Line.Weight = 2;
                quizButton.Shadow.Type = Microsoft.Office.Core.MsoShadowType.msoShadow6;

                quizButton.TextFrame.TextRange.Text = $"📋 {quizTitle}";
                quizButton.TextFrame.TextRange.Font.Name = "Segoe UI";
                quizButton.TextFrame.TextRange.Font.Size = 12;
                quizButton.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                quizButton.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                quizButton.TextFrame.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                quizButton.TextFrame.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;

                quizButton.Name = $"QuizButton_{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
                quizButton.Tags.Add("QuizButton", quizTypeTag);
                quizButton.Tags.Add("ButtonType", "QuizControl");
                quizButton.Tags.Add("QuizId", quizId.ToString());
                quizButton.Tags.Add("QuizType", quizTypeTag);

                Debug.WriteLine($"=== Quiz Button Created ===");
                Debug.WriteLine($"Button Name: {quizButton.Name}");
                Debug.WriteLine($"Tags added: QuizButton={quizTypeTag}, ButtonType=QuizControl, QuizId={quizId}, QuizType={quizTypeTag}");
                Debug.WriteLine($"Total tag count: {quizButton.Tags.Count}");

                // Verify tags were added
                for (int i = 1; i <= quizButton.Tags.Count; i++)
                {
                    Debug.WriteLine($"Tag {i}: {quizButton.Tags.Name(i)} = {quizButton.Tags.Value(i)}");
                }
                Debug.WriteLine($"=== End Quiz Button Creation ===");

                // Add double-click action to show statistics dialog
                try
                {
                    // Store reference for the click handler
                    var buttonName = quizButton.Name;
                    var buttonQuizId = quizId;

                    Debug.WriteLine($"Quiz button created with name: {buttonName}, Quiz ID: {buttonQuizId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to configure button actions: {ex.Message}");
                }

                // Add action setting to make button clickable in slideshow
                try
                {
                    quizButton.ActionSettings[Microsoft.Office.Interop.PowerPoint.PpMouseActivation.ppMouseClick].Action =
                        Microsoft.Office.Interop.PowerPoint.PpActionType.ppActionNone;

                    Debug.WriteLine($"Action setting added to quiz button");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to add action setting or animation: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                Debug.WriteLine($"Added quiz button with quiz ID: {quizId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddQuizButtonToSlideWithQuizId Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error adding quiz button: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Checks for quiz buttons that have been deleted from slides and removes the corresponding quizzes from the database.
        /// </summary>
        public async System.Threading.Tasks.Task CleanupDeletedQuizButtonsAsync()
        {
            try
            {
                if (!IsTeacherLoggedIn || !CurrentCourseId.HasValue)
                {
                    Debug.WriteLine("CleanupDeletedQuizButtons: No active course");
                    System.Windows.Forms.MessageBox.Show(
                        "No active course. Please open a presentation first.",
                        "Cleanup Quiz Buttons",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                if (!HasActivePresentation())
                {
                    Debug.WriteLine("CleanupDeletedQuizButtons: No active presentation");
                    System.Windows.Forms.MessageBox.Show(
                        "No active presentation. Please open a presentation first.",
                        "Cleanup Quiz Buttons",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                // Collect all quiz IDs from quiz buttons in the presentation
                var activeQuizIds = new HashSet<int>();
                var presentation = Application.ActivePresentation;

                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in presentation.Slides)
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        try
                        {
                            bool isQuizButton = false;
                            int quizId = 0;

                            for (int i = 1; i <= shape.Tags.Count; i++)
                            {
                                var tagName = shape.Tags.Name(i);
                                var tagValue = shape.Tags.Value(i);

                                // Check for ANY quiz button type
                                if (tagName.Equals("QuizButton", StringComparison.OrdinalIgnoreCase))
                                {
                                    isQuizButton = true;
                                }
                                if (tagName.Equals("QuizId", StringComparison.OrdinalIgnoreCase))
                                {
                                    int.TryParse(tagValue, out quizId);
                                }
                            }

                            if (isQuizButton && quizId > 0)
                            {
                                activeQuizIds.Add(quizId);
                                Debug.WriteLine($"Found quiz button with Quiz ID: {quizId} on slide {slide.SlideIndex}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error checking shape tags: {ex.Message}");
                        }
                    }
                }

                Debug.WriteLine($"Total active quiz buttons found: {activeQuizIds.Count}");

                // Fetch all quizzes for the current course
                var quizService = new Api.Services.QuizService.QuizApiService();
                var courseQuizzes = await quizService.GetQuizzesForCourseAsync(CurrentCourseId.Value);

                // Find quizzes that don't have corresponding buttons (orphaned quizzes)
                var orphanedQuizzes = courseQuizzes.Where(q => !activeQuizIds.Contains(q.Id)).ToList();

                Debug.WriteLine($"Found {orphanedQuizzes.Count} orphaned quizzes");

                if (orphanedQuizzes.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "No orphaned quizzes found. All quizzes have corresponding buttons in the presentation.",
                        "Cleanup Complete",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                }

                // Ask user for confirmation
                var orphanedTitles = string.Join("\n", orphanedQuizzes.Select(q => $"• {q.Title}"));
                var confirmResult = System.Windows.Forms.MessageBox.Show(
                    $"Found {orphanedQuizzes.Count} quiz(es) without buttons:\n\n{orphanedTitles}\n\nDo you want to delete these quizzes?",
                    "Delete Orphaned Quizzes",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (confirmResult != System.Windows.Forms.DialogResult.Yes)
                {
                    Debug.WriteLine("User cancelled cleanup");
                    return;
                }

                // Delete orphaned quizzes
                int successCount = 0;
                int failCount = 0;
                foreach (var quiz in orphanedQuizzes)
                {
                    try
                    {
                        var success = await quizService.DeleteQuizAsync(quiz.Id);
                        if (success)
                        {
                            successCount++;
                            Debug.WriteLine($"Deleted quiz: {quiz.Title} (ID: {quiz.Id})");
                        }
                        else
                        {
                            failCount++;
                            Debug.WriteLine($"Failed to delete quiz: {quiz.Title} (ID: {quiz.Id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Debug.WriteLine($"Error deleting quiz {quiz.Id}: {ex.Message}");
                    }
                }

                // Show results
                var message = $"Cleanup complete!\n\nDeleted: {successCount} quiz(es)";
                if (failCount > 0)
                {
                    message += $"\nFailed: {failCount} quiz(es)";
                }

                System.Windows.Forms.MessageBox.Show(
                    message,
                    "Cleanup Complete",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    failCount > 0 ? System.Windows.Forms.MessageBoxIcon.Warning : System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanupDeletedQuizButtons Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error cleaning up deleted quiz buttons: {ex.Message}", "Error");
            }
        }

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

        private async void Application_SlideShowNextClick(Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn, Microsoft.Office.Interop.PowerPoint.Effect n)
        {
            try
            {
                if (n == null) return;

                var clickedShape = n.Shape;
                if (clickedShape == null) return;

                bool isQuizButton = false;
                int quizId = 0;
                string quizType = null;

                try
                {
                    for (int i = 1; i <= clickedShape.Tags.Count; i++)
                    {
                        var tagName = clickedShape.Tags.Name(i);
                        var tagValue = clickedShape.Tags.Value(i);

                        if (tagName.Equals("QuizButton", StringComparison.OrdinalIgnoreCase))
                        {
                            isQuizButton = true;
                        }
                        if (tagName.Equals("QuizId", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(tagValue, out quizId);
                        }
                        if (tagName.Equals("QuizType", StringComparison.OrdinalIgnoreCase))
                        {
                            quizType = tagValue;
                        }
                    }
                }
                catch
                {
                    // ignore tag access errors
                }

                if (!isQuizButton) return;

                System.Diagnostics.Debug.WriteLine($"Quiz button clicked in slideshow - Quiz ID: {quizId}, Type: {quizType}");

                // Show quiz results with real data
                await ShowQuizResultsInSlideshowAsync(quizId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application_SlideShowNextClick Error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowQuizResultsInSlideshowAsync(int quizId)
        {
            try
            {
                Debug.WriteLine($"[ThisAddIn] ======================================");
                Debug.WriteLine($"[ThisAddIn] ShowQuizResultsInSlideshowAsync START");
                Debug.WriteLine($"[ThisAddIn] QuizId: {quizId}");
                Debug.WriteLine($"[ThisAddIn] ======================================");
                
                if (quizId > 0)
                {
                    // First, get quiz details to determine quiz type
                    Debug.WriteLine("[ThisAddIn] Fetching quiz details to determine quiz type in slideshow...");
                    var quizService = new Api.Services.QuizService.QuizApiService();
                    var quizDetails = await quizService.GetQuizAsync(quizId).ConfigureAwait(true);

                    Debug.WriteLine($"[ThisAddIn] Quiz details received: {quizDetails != null}");
                    Debug.WriteLine($"[ThisAddIn] Quiz type in slideshow: {quizDetails?.QuizType ?? "(null)"}");

                    // Check if this is a short answer quiz
                    if (quizDetails != null && quizDetails.QuizType == "short_answer")
                    {
                        Debug.WriteLine("[ThisAddIn] *** DETECTED SHORT ANSWER QUIZ ***");
                        Debug.WriteLine("[ThisAddIn] Calling ShowShortAnswerResultsAsync");
                        await ShowShortAnswerResultsAsync(quizId, quizDetails.Properties?.QuestionText ?? "Short Answer Question");
                        Debug.WriteLine("[ThisAddIn] ShowShortAnswerResultsAsync returned");
                        return;
                    }
                }

                // Check if dialog is already open - prevent multiple instances
                if (_activeQuizResultsForm != null && !_activeQuizResultsForm.IsDisposed)
                {
                    Debug.WriteLine("Quiz results dialog already open in slideshow - bringing to front");

                    // Use Invoke to ensure it happens on the correct thread
                    if (_activeQuizResultsForm.InvokeRequired)
                    {
                        _activeQuizResultsForm.Invoke(new Action(() =>
                        {
                            if (!_activeQuizResultsForm.IsDisposed)
                            {
                                _activeQuizResultsForm.BringToFront();
                                _activeQuizResultsForm.Activate();
                            }
                        }));
                    }
                    else
                    {
                        _activeQuizResultsForm.BringToFront();
                        _activeQuizResultsForm.Activate();
                    }
                    return;
                }

                if (quizId > 0)
                {
                    // Fetch real submission data from API (for multiple choice)
                    var quizService = new Api.Services.QuizService.QuizApiService();
                    var stats = await quizService.GetQuizSubmissionStatsAsync(quizId);

                    if (stats != null && stats.ChoiceStats != null)
                    {
                        var percentages = stats.ChoiceStats.Select(c => c.Percentage).ToArray();
                        var submissionCounts = stats.ChoiceStats.Select(c => c.Count).ToArray();
                        var labels = stats.ChoiceStats.Select(c => c.Label).ToList();
                        var correctIndicesData = stats.ChoiceStats
                            .Where(c => c.IsCorrect)
                            .Select(c => c.Index)
                            .ToList();
                        var studentNamesPerChoice = stats.ChoiceStats.Select(c => c.Students ?? new List<string>()).ToList();

                        _activeQuizResultsForm = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(
                            percentages,
                            correctIndicesData,
                            labels,
                            submissionCounts,
                            stats.TotalSubmissions,
                            studentNamesPerChoice,
                            quizId,
                            stats.EnrolledStudents);

                        _activeQuizResultsForm.FormClosed += (s, e) => _activeQuizResultsForm = null;
                        ShowAndActivateForm(_activeQuizResultsForm);

                        return;
                    }
                }

                // Fallback to sample data if no quiz ID or API call failed
                int numberOfChoices = 4;
                List<int> correctIndices = new List<int> { 0 };

                if (_quizTaskPaneControl != null)
                {
                    numberOfChoices = _quizTaskPaneControl.GetNumberOfChoices();
                    var configured = _quizTaskPaneControl.GetCorrectAnswerIndices();
                    if (configured != null && configured.Count > 0)
                        correctIndices = configured;
                }

                var percentagesData = new int[numberOfChoices];
                var basePercent = 100 / Math.Max(1, numberOfChoices);
                var remainder = 100 % Math.Max(1, numberOfChoices);
                for (int i = 0; i < numberOfChoices; i++)
                {
                    percentagesData[i] = basePercent + (i < remainder ? 1 : 0);
                }

                _activeQuizResultsForm = new ClassPointAddIn.Views.Quizzes.QuizResultsForm(percentagesData, correctIndices);
                _activeQuizResultsForm.FormClosed += (s, e) => _activeQuizResultsForm = null;
                ShowAndActivateForm(_activeQuizResultsForm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowQuizResultsInSlideshowAsync Error: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Error loading quiz results: {ex.Message}", "Error");
            }
        }
    }
}
