using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ClassPointAddIn.Api.Services.QuizService;

namespace ClassPointAddIn.Views.Quizzes
{
    public class QuizResultsForm : Form
    {
        private Chart resultsChart;
        private Button btnShowCorrectAnswer;
        private Button btnShowStudentNames;
        private Button btnCloseSubmissions;
        private Button btnRefresh;
        private Label lblTotalSubmissions;
        private Label lblEnrolledStudents;
        private int[] percentages;
        private int[] submissionCounts;
        private int totalSubmissions;
        private int enrolledStudents;
        private List<int> correctIndices;
        private List<string> labels;
        private List<List<string>> studentNamesPerChoice;  // Student names for each choice
        private Timer refreshTimer;
        public int QuizId { get; private set; } // Make quiz ID accessible from outside
        private bool showingCorrectAnswers = false;
        private bool submissionsClosed = false;
        private bool isRefreshing = false; // Prevent overlapping refresh operations

        public QuizResultsForm(int[] percentages, List<int> correctIndices = null, List<string> labels = null, int[] submissionCounts = null, int totalSubmissions = 0, List<List<string>> studentNamesPerChoice = null, int quizId = 0, int enrolledStudents = 0)
        {
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Constructor called - QuizId: {quizId}, TotalSubmissions: {totalSubmissions}");
            
            this.percentages = percentages ?? new int[0];
            this.submissionCounts = submissionCounts ?? new int[0];
            this.totalSubmissions = totalSubmissions;
            this.enrolledStudents = enrolledStudents;
            this.correctIndices = correctIndices ?? new List<int>();
            this.labels = labels ?? Enumerable.Range(0, this.percentages.Length).Select(i => $"Choice {(char)('A' + i)}").ToList();
            this.studentNamesPerChoice = studentNamesPerChoice ?? new List<List<string>>();
            this.QuizId = quizId; // Set the public property

            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] About to call InitializeComponent()");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] InitializeComponent() completed");
            
            // Populate chart after form is loaded to prevent freezing
            this.Load += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form Load event - populating chart");
                
                try
                {
                    // Force form to be visible and on top
                    this.Visible = true;
                    this.BringToFront();
                    this.Activate();
                    
                    // Ensure form handle is created
                    if (!this.IsHandleCreated)
                    {
                        var handle = this.Handle;
                        System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form handle created in Load: {handle}");
                    }
                    
                    // Ensure all controls are ready
                    System.Windows.Forms.Application.DoEvents();
                    
                    // Populate the chart
                    PopulateChart();
                    
                    // Start auto-refresh if quiz ID is provided
                    if (QuizId > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Starting auto-refresh");
                        StartAutoRefresh();
                    }
                    
                    // Force refresh
                    this.Update();
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form Load completed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form Load error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Stack trace: {ex.StackTrace}");
                }
            };
            
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Constructor completed successfully");
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Mouse clicked at ({e.X}, {e.Y}), Button: {e.Button}");
            base.OnMouseClick(e);
        }

        protected override void OnClick(EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form clicked");
            base.OnClick(e);
        }

        private void StartAutoRefresh()
        {
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] StartAutoRefresh() called");
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // Refresh every 5 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Auto-refresh timer started with 5 second interval");
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] RefreshTimer_Tick called - isRefreshing: {isRefreshing}, IsDisposed: {IsDisposed}, IsHandleCreated: {IsHandleCreated}");
            
            // Skip if already refreshing or form is disposed
            if (isRefreshing || IsDisposed || !IsHandleCreated)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Skipping refresh - already refreshing or form disposed");
                return;
            }

            isRefreshing = true;
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Starting refresh for QuizId: {QuizId}");

            try
            {
                // Temporarily stop the timer during refresh to prevent overlap
                refreshTimer?.Stop();
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Timer stopped, calling API...");
                
                var quizService = new QuizApiService();
                var stats = await quizService.GetQuizSubmissionStatsAsync(QuizId);

                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] API response received - Stats null: {stats == null}, ChoiceStats null: {stats?.ChoiceStats == null}");

                if (stats != null && stats.ChoiceStats != null && !IsDisposed && IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Updating data - TotalSubmissions: {stats.TotalSubmissions}, Choices: {stats.ChoiceStats.Count}");
                    
                    percentages = stats.ChoiceStats.Select(c => c.Percentage).ToArray();
                    submissionCounts = stats.ChoiceStats.Select(c => c.Count).ToArray();
                    totalSubmissions = stats.TotalSubmissions;
                    enrolledStudents = stats.EnrolledStudents;
                    labels = stats.ChoiceStats.Select(c => c.Label).ToList();
                    correctIndices = stats.ChoiceStats.Where(c => c.IsCorrect).Select(c => c.Index).ToList();
                    studentNamesPerChoice = stats.ChoiceStats.Select(c => c.Students ?? new List<string>()).ToList();

                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] About to call UpdateUI()");
                    UpdateUI();
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] UpdateUI() completed");
                }
                else if (IsDisposed || !IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Form disposed or handle destroyed during API call, stopping timer");
                    if (refreshTimer != null)
                    {
                        refreshTimer.Stop();
                        refreshTimer.Dispose();
                        refreshTimer = null;
                    }
                }
            }
            catch (ObjectDisposedException odex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] RefreshTimer_Tick - Form disposed: {odex.Message}");
                // Stop the timer if form is disposed
                if (refreshTimer != null)
                {
                    try
                    {
                        refreshTimer.Stop();
                        refreshTimer.Dispose();
                        refreshTimer = null;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] RefreshTimer_Tick error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Stack trace: {ex.StackTrace}");
            }
            finally
            {
                isRefreshing = false;
                // Restart the timer only if form is still alive
                if (!IsDisposed && IsHandleCreated && refreshTimer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Restarting timer");
                    try
                    {
                        refreshTimer.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Failed to restart timer: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Not restarting timer - form disposed or timer null");
                }
            }
        }

        private void UpdateUI()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] UpdateUI called - IsDisposed: {IsDisposed}, IsHandleCreated: {IsHandleCreated}");
                
                if (IsDisposed || !IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Form disposed or handle not created, skipping UI update");
                    return;
                }

                // Use Invoke if needed to ensure we're on the UI thread
                if (InvokeRequired)
                {
                    Invoke(new Action(UpdateUI));
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Updating labels - TotalSubmissions: {totalSubmissions}, EnrolledStudents: {enrolledStudents}");
                
                if (lblTotalSubmissions != null && !lblTotalSubmissions.IsDisposed)
                    lblTotalSubmissions.Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet";
                
                if (lblEnrolledStudents != null && !lblEnrolledStudents.IsDisposed)
                    lblEnrolledStudents.Text = enrolledStudents > 0 ? $"Students in Class: {enrolledStudents}" : "";
                
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] About to call PopulateChart from UpdateUI");
                PopulateChart();
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] PopulateChart completed from UpdateUI");
                
                // Re-apply correct answer highlighting if it was shown
                if (showingCorrectAnswers)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Re-applying correct answer highlighting");
                    HighlightCorrectAnswers();
                }
                
                // Force form refresh
                this.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] UpdateUI completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] UpdateUI error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer.Tick -= RefreshTimer_Tick;
                    refreshTimer.Dispose();
                    refreshTimer = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnFormClosing error: {ex.Message}");
            }
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            System.Diagnostics.Debug.WriteLine("[QuizResultsForm] InitializeComponent started");
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Current thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Is UI thread: {!InvokeRequired}");
            
            Text = "Quiz Results";
            Size = new Size(520, 570);
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            TopMost = true;  // Keep on top even during slideshow
            DoubleBuffered = true; // Reduce flickering during updates
            ShowInTaskbar = true; // Make sure it appears in taskbar
            
            // Set cursor to default to prevent loading cursor
            this.Cursor = Cursors.Default;
            
            // Position the form on screen (center of primary screen)
            this.StartPosition = FormStartPosition.Manual;
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                screen.Left + (screen.Width - this.Width) / 2,
                screen.Top + (screen.Height - this.Height) / 2
            );

            System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Creating labels...");
            
            // Total submissions label at the top
            lblTotalSubmissions = new Label
            {
                Text = totalSubmissions > 0 ? $"Total Submissions: {totalSubmissions}" : "No submissions yet",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(68, 68, 68)
            };

            // Enrolled students label
            lblEnrolledStudents = new Label
            {
                Text = enrolledStudents > 0 ? $"Students in Class: {enrolledStudents}" : "",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(230, 245, 255),
                ForeColor = Color.FromArgb(0, 100, 180)
            };

            resultsChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            chartArea.AxisY.Title = "Percentage";
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 100;
            // Add padding to prevent label cropping - increased Y position for more top space
            chartArea.Position.Auto = false;
            chartArea.Position.X = 5;
            chartArea.Position.Y = 12;  // Increased from 8 to 12
            chartArea.Position.Width = 90;
            chartArea.Position.Height = 82;  // Reduced from 85 to 82 to compensate
            chartArea.InnerPlotPosition.Auto = false;
            chartArea.InnerPlotPosition.X = 12;
            chartArea.InnerPlotPosition.Y = 8;  // Increased from 5 to 8 for more top padding
            chartArea.InnerPlotPosition.Width = 85;
            chartArea.InnerPlotPosition.Height = 82;  // Reduced from 85 to 82
            resultsChart.ChartAreas.Add(chartArea);

            var series = new Series("Responses")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            resultsChart.Series.Add(series);

            // Panel for buttons at the bottom
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };

            btnCloseSubmissions = new Button
            {
                Text = "Close Submissions",
                Dock = DockStyle.Right,
                Width = 140,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnCloseSubmissions.FlatAppearance.BorderSize = 0;
            btnCloseSubmissions.Click += BtnCloseSubmissions_Click;
            btnCloseSubmissions.Click += (s, e) => System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Close Submissions button clicked");

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Dock = DockStyle.Right,
                Width = 100,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0),
                Visible = false // Hide the manual refresh button since we have auto-refresh
            };
            btnRefresh.FlatAppearance.BorderSize = 0;

            btnShowStudentNames = new Button
            {
                Text = "Show Student Names",
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0)
            };
            btnShowStudentNames.FlatAppearance.BorderSize = 0;
            btnShowStudentNames.Click += BtnShowStudentNames_Click;
            btnShowStudentNames.Click += (s, e) => System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Show Student Names button clicked");

            btnShowCorrectAnswer = new Button
            {
                Text = "Show Correct Answer",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShowCorrectAnswer.FlatAppearance.BorderSize = 0;
            btnShowCorrectAnswer.Click += BtnShowCorrectAnswer_Click;
            btnShowCorrectAnswer.Click += (s, e) => System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Show Correct Answer button clicked");

            System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Adding controls to form...");
            
            buttonPanel.Controls.Add(btnShowCorrectAnswer);
            buttonPanel.Controls.Add(btnShowStudentNames);
            buttonPanel.Controls.Add(btnRefresh);
            buttonPanel.Controls.Add(btnCloseSubmissions);

            Controls.Add(lblTotalSubmissions);
            Controls.Add(lblEnrolledStudents);
            Controls.Add(resultsChart);
            Controls.Add(buttonPanel);
            
            System.Diagnostics.Debug.WriteLine("[QuizResultsForm] InitializeComponent completed successfully");
        }

        private void PopulateChart()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] PopulateChart called");
                
                // Wait for form and chart to be ready
                if (!this.IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Form handle not created yet, skipping PopulateChart");
                    return;
                }
                
                if (resultsChart == null || resultsChart.IsDisposed)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Chart is null or disposed, skipping PopulateChart");
                    return;
                }
                
                // Force chart handle creation if needed
                if (!resultsChart.IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Creating chart handle...");
                    var handle = resultsChart.Handle; // Force handle creation
                    System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Chart handle created: {handle}");
                }

                var series = resultsChart.Series["Responses"];
                if (series == null)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Series 'Responses' not found");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Clearing series and adding {percentages.Length} data points");
                series.Points.Clear();

                for (int i = 0; i < percentages.Length; i++)
                {
                    int pointIndex = series.Points.AddY(percentages[i]);
                    DataPoint dp = series.Points[pointIndex];

                    dp.AxisLabel = labels.Count > i ? labels[i] : $"Choice {(char)('A' + i)}";
                    
                    // Show count in label if available, otherwise just percentage
                    if (submissionCounts != null && submissionCounts.Length > i)
                    {
                        dp.Label = $"{submissionCounts[i]} ({percentages[i]}%)";
                    }
                    else
                    {
                        dp.Label = $"{percentages[i]}%";
                    }
                    
                    dp.Color = Color.FromArgb(0, 120, 215); // default blue
                    dp.Tag = i;
                }
                
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Invalidating chart to trigger redraw");
                resultsChart.Invalidate();
                resultsChart.Update(); // Force immediate update
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] PopulateChart completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] PopulateChart error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] Stack trace: {ex.StackTrace}");
            }
        }

        private void BtnShowCorrectAnswer_Click(object sender, EventArgs e)
        {
            try
            {
                showingCorrectAnswers = true;
                HighlightCorrectAnswers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] BtnShowCorrectAnswer_Click error: {ex.Message}");
                MessageBox.Show($"Error highlighting correct answers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HighlightCorrectAnswers()
        {
            try
            {
                if (resultsChart == null || resultsChart.IsDisposed)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Chart is null or disposed, cannot highlight");
                    return;
                }

                var series = resultsChart.Series["Responses"];

                if (series == null || series.Points.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[QuizResultsForm] Series is null or has no points");
                    return;
                }

                foreach (DataPoint pt in series.Points)
                {
                    var value = pt.YValues != null && pt.YValues.Length > 0 ? (int)Math.Round(pt.YValues[0]) : 0;
                    var idx = (int)pt.Tag;
                    pt.Color = Color.LightGray;
                    
                    // Update label to show count and percentage
                    if (submissionCounts != null && submissionCounts.Length > idx)
                    {
                        pt.Label = $"{submissionCounts[idx]} ({value}%)";
                    }
                    else
                    {
                        pt.Label = $"{value}%";
                    }
                }

                // Highlight correct indices
                if (correctIndices != null && correctIndices.Count > 0)
                {
                    foreach (var idx in correctIndices.Distinct())
                    {
                        if (idx >= 0 && idx < series.Points.Count)
                        {
                            var pt = series.Points[idx];
                            var val = pt.YValues != null && pt.YValues.Length > 0 ? (int)Math.Round(pt.YValues[0]) : 0;
                            pt.Color = Color.FromArgb(0, 180, 60); // green
                            
                            // Update label with checkmark for correct answer
                            if (submissionCounts != null && submissionCounts.Length > idx)
                            {
                                pt.Label = $"✓ {submissionCounts[idx]} ({val}%)";
                            }
                            else
                            {
                                pt.Label = $"✓ {val}%";
                            }
                        }
                    }
                }
                
                resultsChart.Invalidate();
                resultsChart.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuizResultsForm] HighlightCorrectAnswers error: {ex.Message}");
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
            {
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to close submissions for this quiz?\n\nStudents will no longer be able to submit answers.",
                "Close Submissions",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                var quizService = new QuizApiService();
                await quizService.CloseQuizSubmissionsAsync(QuizId);

                submissionsClosed = true;
                btnCloseSubmissions.Enabled = false;
                btnCloseSubmissions.Text = "Submissions Closed";
                btnCloseSubmissions.BackColor = Color.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to close submissions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Close submissions error: {ex.Message}");
            }
        }

        private void BtnShowStudentNames_Click(object sender, EventArgs e)
        {
            if (studentNamesPerChoice == null || studentNamesPerChoice.Count == 0)
            {
                return;
            }

            // Show in a dialog with better formatting
            var detailsForm = new Form
            {
                Text = "Student Submissions by Choice",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = false,
                TopMost = true
            };

            // Create a panel for each choice
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            int yPosition = 10;

            for (int i = 0; i < studentNamesPerChoice.Count; i++)
            {
                var choiceLabel = labels.Count > i ? labels[i] : $"Choice {(char)('A' + i)}";
                var students = studentNamesPerChoice[i];
                var count = students?.Count ?? 0;
                
                // Choice header panel
                var headerPanel = new Panel
                {
                    Location = new Point(10, yPosition),
                    Size = new Size(450, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    Padding = new Padding(10, 5, 10, 5)
                };

                var headerLabel = new Label
                {
                    Text = $"{choiceLabel} ({count} student{(count != 1 ? "s" : "")})",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                headerPanel.Controls.Add(headerLabel);
                mainPanel.Controls.Add(headerPanel);

                yPosition += 40;

                // Student list
                if (students != null && students.Count > 0)
                {
                    foreach (var studentName in students)
                    {
                        var studentPanel = new Panel
                        {
                            Location = new Point(10, yPosition),
                            Size = new Size(450, 30),
                            BackColor = Color.FromArgb(245, 245, 245)
                        };

                        var studentLabel = new Label
                        {
                            Text = $"  • {studentName}",
                            Location = new Point(15, 5),
                            Size = new Size(420, 20),
                            Font = new Font("Segoe UI", 9f),
                            ForeColor = Color.FromArgb(68, 68, 68),
                            AutoSize = false,
                            TextAlign = ContentAlignment.MiddleLeft
                        };

                        studentPanel.Controls.Add(studentLabel);
                        mainPanel.Controls.Add(studentPanel);

                        yPosition += 32;
                    }
                }
                else
                {
                    var noStudentPanel = new Panel
                    {
                        Location = new Point(10, yPosition),
                        Size = new Size(450, 30),
                        BackColor = Color.FromArgb(250, 250, 250)
                    };

                    var noStudentLabel = new Label
                    {
                        Text = "  (No submissions)",
                        Location = new Point(15, 5),
                        Size = new Size(420, 20),
                        Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    noStudentPanel.Controls.Add(noStudentLabel);
                    mainPanel.Controls.Add(noStudentPanel);

                    yPosition += 32;
                }

                yPosition += 10; // Space between choices
            }

            var closeButton = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, args) => detailsForm.Close();

            detailsForm.Controls.Add(mainPanel);
            detailsForm.Controls.Add(closeButton);
            detailsForm.ShowDialog(this);
        }

        // Helper static method for convenience
        public static void ShowDialogWithSampleData(IWin32Window owner, int numberOfChoices = 4, List<int> correct = null)
        {
            if (numberOfChoices <= 0) numberOfChoices = 4;
            var percentages = new int[numberOfChoices];
            var basePercent = 100 / numberOfChoices;
            var remainder = 100 % numberOfChoices;
            for (int i = 0; i < numberOfChoices; i++)
            {
                percentages[i] = basePercent + (i < remainder ? 1 : 0);
            }

            using (var dlg = new QuizResultsForm(percentages, correct ?? new List<int> { 0 }))
            {
                dlg.ShowDialog(owner);
            }
        }
    }
}