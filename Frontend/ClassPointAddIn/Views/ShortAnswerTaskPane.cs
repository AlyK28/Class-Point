using ClassPointAddIn.Api.Services.QuizService;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public partial class ShortAnswerTaskPane : UserControl
    {
        // Main sections
        private Panel pnlHeader;
        private Label lblTitle;

        // Question Configuration
        private Label lblQuestionText;
        private TextBox txtQuestionText;

        // Grading Criteria (Optional)
        private Label lblGradingCriteria;
        private Label lblCorrectAnswer;
        private TextBox txtCorrectAnswer;
        private Label lblKeywords;
        private TextBox txtKeywords;
        private CheckBox chkCaseSensitive;

        // Play Options
        private Label lblPlayOptions;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimizeResult;
        private CheckBox chkShowTimer;
        private CheckBox chkAllowLateSubmissions;

        // Time limit
        private CheckBox chkAutoClose;
        private NumericUpDown numCloseAfter;
        private Label lblCloseAfterUnit;

        // Action Buttons
        private Button btnSaveQuiz;
        private Button btnCancel;

        private int _courseId;
        private QuizApiService _quizService;

        public ShortAnswerTaskPane()
        {
            _quizService = new QuizApiService();
            InitializeComponent();
        }

        public void SetCourseId(int courseId)
        {
            _courseId = courseId;
            System.Diagnostics.Debug.WriteLine($"[ShortAnswerTaskPane] Course ID set to: {courseId}");
        }

        private void InitializeComponent()
        {
            Size = new Size(350, 700);
            BackColor = Color.White;
            Padding = new Padding(0);  // Remove padding to prevent layout issues
            AutoScroll = true;

            CreateMainLayout();
        }

        private void CreateMainLayout()
        {
            var y = 0;
            var leftMargin = 15;
            var controlWidth = Width - (leftMargin * 2) - 20;  // Account for scrollbar

            // Header
            pnlHeader = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(Width, 60),
                BackColor = Color.FromArgb(0, 120, 215)
            };

            lblTitle = new Label
            {
                Text = "Short Answer Quiz",
                Location = new Point(10, 15),
                Size = new Size(Width - 20, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.Add(lblTitle);
            Controls.Add(pnlHeader);

            y += 70;

            // Question Text Section
            lblQuestionText = new Label
            {
                Text = "Question *",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblQuestionText);

            y += 25;

            txtQuestionText = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5F),
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(txtQuestionText);

            y += 90;

            // Grading Criteria Section
            lblGradingCriteria = new Label
            {
                Text = "Grading Criteria (Optional)",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblGradingCriteria);

            y += 25;

            lblCorrectAnswer = new Label
            {
                Text = "Expected Answer",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 18),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            Controls.Add(lblCorrectAnswer);

            y += 20;

            txtCorrectAnswer = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 50),
                Multiline = true,
                Font = new Font("Segoe UI", 9F),
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(txtCorrectAnswer);

            y += 60;

            lblKeywords = new Label
            {
                Text = "Key Terms/Keywords (comma separated)",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 18),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            Controls.Add(lblKeywords);

            y += 20;

            txtKeywords = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 50),
                Multiline = true,
                Font = new Font("Segoe UI", 9F),
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(txtKeywords);

            y += 60;

            chkCaseSensitive = new CheckBox
            {
                Text = "Case sensitive",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkCaseSensitive);

            y += 35;

            // Play Options Section
            lblPlayOptions = new Label
            {
                Text = "Play Options",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblPlayOptions);

            y += 25;

            chkStartWithSlide = new CheckBox
            {
                Text = "Start with slide",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68),
                Checked = true
            };
            Controls.Add(chkStartWithSlide);

            y += 30;

            chkMinimizeResult = new CheckBox
            {
                Text = "Minimize results window on start",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkMinimizeResult);

            y += 30;

            chkShowTimer = new CheckBox
            {
                Text = "Show timer",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68),
                Checked = true
            };
            Controls.Add(chkShowTimer);

            y += 30;

            chkAllowLateSubmissions = new CheckBox
            {
                Text = "Allow late submissions",
                Location = new Point(leftMargin, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkAllowLateSubmissions);

            y += 30;

            chkAutoClose = new CheckBox
            {
                Text = "Auto-close submissions after:",
                Location = new Point(leftMargin, y),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            chkAutoClose.CheckedChanged += ChkAutoClose_CheckedChanged;
            Controls.Add(chkAutoClose);

            numCloseAfter = new NumericUpDown
            {
                Location = new Point(200, y),
                Size = new Size(70, 25),
                Minimum = 10,
                Maximum = 600,
                Value = 60,
                Enabled = false,
                Font = new Font("Segoe UI", 9F)
            };
            Controls.Add(numCloseAfter);

            lblCloseAfterUnit = new Label
            {
                Text = "sec",
                Location = new Point(275, y + 3),
                Size = new Size(40, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            Controls.Add(lblCloseAfterUnit);

            y += 45;  // Increased spacing before buttons

            // Action Buttons
            btnSaveQuiz = new Button
            {
                Text = "Create Quiz",
                Location = new Point(leftMargin, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveQuiz.FlatAppearance.BorderSize = 0;
            btnSaveQuiz.Click += BtnSaveQuiz_Click;
            Controls.Add(btnSaveQuiz);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(175, y),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(68, 68, 68),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            Controls.Add(btnCancel);

            y += 50;

            // Set minimum height to ensure scrolling works
            this.MinimumSize = new Size(350, y);
        }

        private void ChkAutoClose_CheckedChanged(object sender, EventArgs e)
        {
            numCloseAfter.Enabled = chkAutoClose.Checked;
        }

        private async void BtnSaveQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate question text
                if (string.IsNullOrWhiteSpace(txtQuestionText.Text))
                {
                    MessageBox.Show("Please enter a question.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuestionText.Focus();
                    return;
                }

                if (_courseId <= 0)
                {
                    MessageBox.Show("No course selected. Please open a presentation first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Disable button to prevent double-clicks
                btnSaveQuiz.Enabled = false;
                btnSaveQuiz.Text = "Creating...";

                // Prepare quiz data
                var quizData = new CreateShortAnswerQuizRequest
                {
                    Course = _courseId,
                    Title = $"Short Answer - {DateTime.Now:yyyy-MM-dd HH:mm}",
                    Properties = new ShortAnswerQuizProperties
                    {
                        QuestionText = txtQuestionText.Text.Trim(),
                        CorrectAnswer = string.IsNullOrWhiteSpace(txtCorrectAnswer.Text) ? null : txtCorrectAnswer.Text.Trim(),
                        ExpectedKeywords = string.IsNullOrWhiteSpace(txtKeywords.Text) ? null : txtKeywords.Text.Trim(),
                        CaseSensitive = chkCaseSensitive.Checked
                    }
                };

                System.Diagnostics.Debug.WriteLine($"[ShortAnswerTaskPane] Creating quiz with title: {quizData.Title}");

                // Call API to create quiz
                var response = await _quizService.CreateShortAnswerQuizAsync(quizData);

                // Ensure we're back on the UI thread for all UI operations
                if (InvokeRequired)
                {
                    Invoke(new Action(() => HandleQuizCreationResponse(response)));
                }
                else
                {
                    HandleQuizCreationResponse(response);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShortAnswerTaskPane] Error creating quiz: {ex.Message}");
                
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Error creating quiz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnSaveQuiz.Enabled = true;
                        btnSaveQuiz.Text = "Create Quiz";
                    }));
                }
                else
                {
                    MessageBox.Show($"Error creating quiz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSaveQuiz.Enabled = true;
                    btnSaveQuiz.Text = "Create Quiz";
                }
            }
        }

        private void HandleQuizCreationResponse(QuizResponse response)
        {
            try
            {
                if (response != null && response.Id > 0)
                {
                    MessageBox.Show($"Short answer quiz created successfully!\nQuiz ID: {response.Id}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Add quiz button to slide with ShortAnswer type
                    var addIn = Globals.ThisAddIn;
                    addIn.AddQuizButtonToSlideWithQuizId(response.Id, response.Title, "ShortAnswer");

                    // Clear form
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to create quiz. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnSaveQuiz.Enabled = true;
                btnSaveQuiz.Text = "Create Quiz";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtQuestionText.Clear();
            txtCorrectAnswer.Clear();
            txtKeywords.Clear();
            chkCaseSensitive.Checked = false;
            chkStartWithSlide.Checked = true;
            chkMinimizeResult.Checked = false;
            chkShowTimer.Checked = true;
            chkAllowLateSubmissions.Checked = false;
            chkAutoClose.Checked = false;
            numCloseAfter.Value = 60;
        }
    }
}
