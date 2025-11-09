using ClassPointAddIn.Api.Services.QuizService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ClassPointAddIn.Views
{
    public partial class ShortAnswerResultsForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblTotalSubmissions;
        private Label lblEnrolledStudents;
        private FlowLayoutPanel flowAnswers;
        private Button btnCloseSubmissions;
        private Button btnRefresh;
        private Timer refreshTimer;

        public int QuizId { get; private set; }
        private string questionText;
        private int totalSubmissions;
        private int enrolledStudents;
        private List<ShortAnswerSubmission> submissions;
        private bool submissionsClosed = false;
        private bool isRefreshing = false;
        private QuizApiService quizService;

        public ShortAnswerResultsForm(int quizId, string questionText, List<ShortAnswerSubmission> submissions, int totalSubmissions, int enrolledStudents)
        {
            System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Constructor called - QuizId: {quizId}, Total: {totalSubmissions}");

            this.QuizId = quizId;
            this.questionText = questionText ?? "Short Answer Question";
            this.submissions = submissions ?? new List<ShortAnswerSubmission>();
            this.totalSubmissions = totalSubmissions;
            this.enrolledStudents = enrolledStudents;
            this.quizService = new QuizApiService();

            InitializeComponent();

            this.Load += (s, e) =>
            {
                try
                {
                    this.Visible = true;
                    this.BringToFront();
                    this.Activate();

                    if (!this.IsHandleCreated)
                    {
                        var handle = this.Handle;
                        System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Form handle created in Load: {handle}");
                    }

                    PopulateAnswers();

                    if (QuizId > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Starting auto-refresh");
                        StartAutoRefresh();
                    }

                    this.Update();
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Form Load completed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Form Load error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Stack trace: {ex.StackTrace}");
                }
            };
        }

        private void InitializeComponent()
        {
            Text = "Short Answer Submissions";
            Size = new Size(700, 600);
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            TopMost = true;
            DoubleBuffered = true;
            ShowInTaskbar = true;

            this.Cursor = Cursors.Default;

            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new System.Drawing.Point(
           screen.Left + (screen.Width - this.Width) / 2,
        screen.Top + (screen.Height - this.Height) / 2
     );

            // Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(15)
            };

            lblTitle = new Label
            {
                Text = questionText,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            lblTotalSubmissions = new Label
            {
                Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTotalSubmissions);

            lblEnrolledStudents = new Label
            {
                Text = enrolledStudents > 0 ? $"Students in Class: {enrolledStudents}" : "",
                Dock = DockStyle.Top,
                Height = 20,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblEnrolledStudents);

            Controls.Add(pnlHeader);

            // Answers FlowLayout Panel
            flowAnswers = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(15, 120, 15, 15)  // Increased top padding to 70px to prevent cropping
            };
            Controls.Add(flowAnswers);

            // Bottom Button Panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnCloseSubmissions = new Button
            {
                Text = "Close Submissions",
                Dock = DockStyle.Right,
                Width = 150,
                Height = 35,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCloseSubmissions.FlatAppearance.BorderSize = 0;
            btnCloseSubmissions.Click += BtnCloseSubmissions_Click;
            buttonPanel.Controls.Add(btnCloseSubmissions);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Right,
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await RefreshSubmissions();
            buttonPanel.Controls.Add(btnRefresh);

            Controls.Add(buttonPanel);
        }

        private void PopulateAnswers()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] PopulateAnswers called with {submissions.Count} submissions");

                flowAnswers.Controls.Clear();

                if (submissions == null || submissions.Count == 0)
                {
                    var noDataLabel = new Label
                    {
                        Text = "No submissions yet. Waiting for students to answer...",
                        AutoSize = false,
                        Width = flowAnswers.Width - 40,
                        Height = 100,
                        Font = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    flowAnswers.Controls.Add(noDataLabel);
                    return;
                }

                foreach (var submission in submissions)
                {
                    var answerCard = CreateAnswerCard(submission);
                    flowAnswers.Controls.Add(answerCard);
                }

                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] PopulateAnswers completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] PopulateAnswers error: {ex.Message}");
            }
        }

        private Panel CreateAnswerCard(ShortAnswerSubmission submission)
        {
            var cardWidth = flowAnswers.Width - 50;

            var card = new Panel
            {
                Width = cardWidth,
                Height = 140,  // Increased height to accommodate buttons
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 10, 0, 15),
                Padding = new Padding(15)
            };

            // Add shadow effect using a border panel
            card.Paint += (s, e) =>
                {
                    ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
         Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
              Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                  Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid);
                };

            // Student name
            var lblStudent = new Label
            {
                Text = submission.StudentName,
                Location = new System.Drawing.Point(15, 10),
                Size = new Size(cardWidth - 200, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            card.Controls.Add(lblStudent);

            // Answer text (truncated)
            var answerPreview = submission.AnswerText.Length > 100
      ? submission.AnswerText.Substring(0, 100) + "..."
         : submission.AnswerText;

            var lblAnswer = new Label
            {
                Text = $"\"{answerPreview}\"",
                Location = new System.Drawing.Point(15, 40),
                Size = new Size(cardWidth - 30, 40),
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            card.Controls.Add(lblAnswer);

            // Action buttons in a row at the bottom
            var buttonY = 90;
            var buttonStartX = 15;
            var buttonSpacing = 5;

            var btnView = new Button
            {
                Text = "View",
                Location = new System.Drawing.Point(buttonStartX, buttonY),
                Size = new Size(75, 32),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand,
                Tag = submission
            };
            btnView.FlatAppearance.BorderSize = 0;
            btnView.Click += BtnView_Click;
            card.Controls.Add(btnView);
            buttonStartX += 75 + buttonSpacing;

            var btnLike = new Button
            {
                Text = submission.IsLiked ? "Liked" : "Like",
                Location = new System.Drawing.Point(buttonStartX, buttonY),
                Size = new Size(75, 32),
                BackColor = submission.IsLiked ? Color.FromArgb(220, 53, 69) : Color.FromArgb(240, 240, 240),
                ForeColor = submission.IsLiked ? Color.White : Color.FromArgb(68, 68, 68),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand,
                Tag = submission
            };
            btnLike.FlatAppearance.BorderSize = 0;
            btnLike.Click += BtnLike_Click;
            card.Controls.Add(btnLike);
            buttonStartX += 75 + buttonSpacing;

            var btnDownload = new Button
            {
                Text = "Add to Slide",
                Location = new System.Drawing.Point(buttonStartX, buttonY),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand,
                Tag = submission
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Click += BtnDownloadAsSlide_Click;
            card.Controls.Add(btnDownload);
            buttonStartX += 100 + buttonSpacing;

            var btnDelete = new Button
            {
                Text = "Delete",
                Location = new System.Drawing.Point(buttonStartX, buttonY),
                Size = new Size(75, 32),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand,
                Tag = submission
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            card.Controls.Add(btnDelete);

            return card;
        }

        private void BtnView_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var submission = button?.Tag as ShortAnswerSubmission;

            if (submission != null)
            {
                var detailForm = new ShortAnswerDetailForm(submission);
                detailForm.ShowDialog(this);
            }
        }

        private void BtnLike_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var submission = button?.Tag as ShortAnswerSubmission;

            if (submission != null)
            {
                submission.IsLiked = !submission.IsLiked;

                // Update button appearance
                button.Text = submission.IsLiked ? "Liked" : "Like";
                button.BackColor = submission.IsLiked ? Color.FromArgb(220, 53, 69) : Color.FromArgb(240, 240, 240);
                button.ForeColor = submission.IsLiked ? Color.White : Color.FromArgb(68, 68, 68);

                // TODO: Save like status to backend
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Toggled like for: {submission.StudentName}");
            }
        }

        private void BtnDownloadAsSlide_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var submission = button?.Tag as ShortAnswerSubmission;

            if (submission != null)
            {
                try
                {
                    CreateSlideFromAnswer(submission);
                    MessageBox.Show($"Slide created successfully for {submission.StudentName}'s answer!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating slide: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Error creating slide: {ex.Message}");
                }
            }
        }

        private void CreateSlideFromAnswer(ShortAnswerSubmission submission)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var presentation = app.ActivePresentation;

                // Add new slide at the end
                var slideIndex = presentation.Slides.Count + 1;
                var slide = presentation.Slides.Add(slideIndex, PowerPoint.PpSlideLayout.ppLayoutBlank);

                // Add question text at top
                var questionBox = slide.Shapes.AddTextbox(
                 Office.MsoTextOrientation.msoTextOrientationHorizontal,
         50, 50, 620, 100);
                questionBox.TextFrame.TextRange.Text = questionText;
                questionBox.TextFrame.TextRange.Font.Size = 20;
                questionBox.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                questionBox.TextFrame.TextRange.Font.Color.RGB = ColorTranslator.ToOle(Color.FromArgb(68, 68, 68));

                // Add student name
                var nameBox = slide.Shapes.AddTextbox(
            Office.MsoTextOrientation.msoTextOrientationHorizontal,
                     50, 180, 620, 40);
                nameBox.TextFrame.TextRange.Text = $"— {submission.StudentName}";
                nameBox.TextFrame.TextRange.Font.Size = 14;
                nameBox.TextFrame.TextRange.Font.Italic = Office.MsoTriState.msoTrue;
                nameBox.TextFrame.TextRange.Font.Color.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 120, 215));

                // Add answer with quote marks in a nice box
                var answerBox = slide.Shapes.AddTextbox(
              Office.MsoTextOrientation.msoTextOrientationHorizontal,
             80, 240, 560, 300);
                answerBox.TextFrame.TextRange.Text = $"\"{submission.AnswerText}\"";
                answerBox.TextFrame.TextRange.Font.Size = 16;
                answerBox.TextFrame.TextRange.Font.Italic = Office.MsoTriState.msoTrue;
                answerBox.TextFrame.TextRange.Font.Color.RGB = ColorTranslator.ToOle(Color.FromArgb(100, 100, 100));
                answerBox.Line.Visible = Office.MsoTriState.msoTrue;
                answerBox.Line.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 120, 215));
                answerBox.Line.Weight = 2;
                answerBox.Fill.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(245, 250, 255));
                answerBox.Fill.Visible = Office.MsoTriState.msoTrue;

                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Created slide for: {submission.StudentName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Error in CreateSlideFromAnswer: {ex.Message}");
                throw;
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var submission = button?.Tag as ShortAnswerSubmission;

            if (submission != null)
            {
                var result = MessageBox.Show(
                     $"Are you sure you want to delete the answer from {submission.StudentName}?",
                        "Confirm Delete",
                       MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Remove from list
                    submissions.Remove(submission);
                    totalSubmissions = submissions.Count;

                    // Update UI
                    lblTotalSubmissions.Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet";
                    PopulateAnswers();

                    // TODO: Delete from backend
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Deleted answer from: {submission.StudentName}");
                }
            }
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // 5 seconds
            refreshTimer.Tick += async (s, e) => await RefreshTimer_Tick();
            refreshTimer.Start();
            System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Auto-refresh started");
        }

        private async Task RefreshTimer_Tick()
        {
            if (isRefreshing || IsDisposed || !IsHandleCreated)
                return;

            await RefreshSubmissions();
        }

        private async Task RefreshSubmissions()
        {
            if (isRefreshing) return;

            isRefreshing = true;

            try
            {
                refreshTimer?.Stop();

                // Call API to get updated submissions
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Refreshing submissions for quiz {QuizId}");

                var stats = await quizService.GetShortAnswerStatsAsync(QuizId);

                if (stats != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Received {stats.Submissions?.Count ?? 0} submissions from API");

                    // Update submissions list
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
                }

                if (!IsDisposed && IsHandleCreated && InvokeRequired)
                {
                    Invoke(new Action(() =>
                           {
                               lblTotalSubmissions.Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet";
                               lblEnrolledStudents.Text = enrolledStudents > 0 ? $"Students in Class: {enrolledStudents}" : "";
                               PopulateAnswers();
                           }));
                }
                else if (!IsDisposed && IsHandleCreated)
                {
                    lblTotalSubmissions.Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet";
                    lblEnrolledStudents.Text = enrolledStudents > 0 ? $"Students in Class: {enrolledStudents}" : "";
                    PopulateAnswers();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Refresh error: {ex.Message}");
            }
            finally
            {
                isRefreshing = false;
                if (!IsDisposed && IsHandleCreated && refreshTimer != null)
                {
                    refreshTimer.Start();
                }
            }
        }

        private async void BtnCloseSubmissions_Click(object sender, EventArgs e)
        {
            if (QuizId <= 0)
            {
                MessageBox.Show("Cannot close submissions: Quiz ID not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (submissionsClosed)
                return;

            var result = MessageBox.Show(
            "Are you sure you want to close submissions for this quiz?\n\nStudents will no longer be able to submit answers.",
                          "Close Submissions",
                       MessageBoxButtons.YesNo,
               MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                await quizService.CloseQuizSubmissionsAsync(QuizId);

                // Ensure we're on UI thread for UI updates
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        submissionsClosed = true;
                        btnCloseSubmissions.Enabled = false;
                        btnCloseSubmissions.Text = "Submissions Closed";
                        btnCloseSubmissions.BackColor = Color.Gray;
                    }));
                }
                else
                {
                    submissionsClosed = true;
                    btnCloseSubmissions.Enabled = false;
                    btnCloseSubmissions.Text = "Submissions Closed";
                    btnCloseSubmissions.BackColor = Color.Gray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to close submissions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] Close submissions error: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                    refreshTimer = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerResultsForm] OnFormClosing error: {ex.Message}");
            }
            base.OnFormClosing(e);
        }
    }

    // Helper class to represent a short answer submission
    public class ShortAnswerSubmission
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; }
        public string AnswerText { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsLiked { get; set; }
    }
}
