using ClassPointAddIn.Api.Services.QuizService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views.Quizzes
{
    public partial class QuizTaskPane : UserControl
    {
        // Main sections
        private Panel pnlHeader;
        private Label lblTitle;

        // Quiz Configuration Controls
        private Label lblQuestionType;
        private ComboBox cmbQuestionType;

        // New: question text input
        private Label lblQuestionText;
        private TextBox txtQuestionText;

        private Label lblNumberOfChoices;
        private Panel pnlChoiceNumbers;
        private Button[] choiceNumberButtons;
        private int selectedChoiceCount = 4;

        private CheckBox chkAllowMultiple;
        private CheckBox chkHasCorrectAnswer;
        private CheckedListBox clbCorrectAnswers; // Multi-select dropdown for correct answers
        private CheckBox chkCompetitionMode;

        private Label lblPlayOptions;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimizeResult;
        private CheckBox chkCloseSubmission;

        private NumericUpDown numCloseAfter;
        private Label lblCloseAfterUnit;

        // New: Save button
        private Button btnSaveQuiz;

        private int _courseId;

        public QuizTaskPane()
        {
            InitializeComponent();
        }

        public void SetCourseId(int courseId)
        {
            _courseId = courseId;
        }

        private void InitializeComponent()
        {
            Size = new Size(350, 600);
            BackColor = Color.White;
            Padding = new Padding(10);
            AutoScroll = true;

            CreateMainLayout();
        }

        private void CreateMainLayout()
        {
            // Header
            pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(330, 50),
                BackColor = Color.FromArgb(0, 120, 215)
            };

            lblTitle = new Label
            {
                Text = "Quiz Settings",
                Location = new Point(10, 10),
                Size = new Size(310, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.Add(lblTitle);
            Controls.Add(pnlHeader);

            CreateQuizConfiguration();
        }

        private void CreateQuizConfiguration()
        {
            var y = 60; // Start below header

            // Question Type
            lblQuestionType = new Label
            {
                Text = "Question Type",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblQuestionType);

            y += 25;

            cmbQuestionType = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(320, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbQuestionType.Items.AddRange(new string[] { "Multiple Choice", "True/False", "Short Answer" });
            cmbQuestionType.SelectedIndex = 0;
            Controls.Add(cmbQuestionType);

            y += 30;

            // QUESTION TEXT input (new)
            lblQuestionText = new Label
            {
                Text = "Question Text",
                Location = new Point(10, y),
                Size = new Size(320, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblQuestionText);

            y += 20;

            txtQuestionText = new TextBox
            {
                Location = new Point(10, y),
                Size = new Size(320, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 9F),
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(txtQuestionText);

            y += 70;

            // Number of choices
            lblNumberOfChoices = new Label
            {
                Text = "Number of choices",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblNumberOfChoices);

            y += 25;

            CreateChoiceNumberButtons(y);

            y += 50;

            // Checkboxes for quiz options
            chkAllowMultiple = new CheckBox
            {
                Text = "Allow selecting multiple choices",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkAllowMultiple);

            y += 30;

            // Has correct answer with multi-select dropdown
            CreateCorrectAnswerSection(y);

            // Dynamic spacing based on dropdown height
            y += Math.Max(80, CalculateDropdownHeight(selectedChoiceCount) + 30);

            chkCompetitionMode = new CheckBox
            {
                Text = "Competition mode",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkCompetitionMode);

            y += 50;

            // Play Options section
            lblPlayOptions = new Label
            {
                Text = "Play Options",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(lblPlayOptions);

            y += 30;

            chkStartWithSlide = new CheckBox
            {
                Text = "Start question with slide",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkStartWithSlide);

            y += 30;

            chkMinimizeResult = new CheckBox
            {
                Text = "Minimize result window after question starts",
                Location = new Point(10, y),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            Controls.Add(chkMinimizeResult);

            y += 30;

            // Close submission option with time input
            var pnlCloseSubmission = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(320, 25),
                BackColor = Color.Transparent
            };

            chkCloseSubmission = new CheckBox
            {
                Text = "Close submission after",
                Location = new Point(0, 2),
                Size = new Size(140, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };

            numCloseAfter = new NumericUpDown
            {
                Location = new Point(145, 0),
                Size = new Size(50, 23),
                Minimum = 1,
                Maximum = 60,
                Value = 1,
                Font = new Font("Segoe UI", 9F)
            };

            lblCloseAfterUnit = new Label
            {
                Text = "minute",
                Location = new Point(200, 2),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68)
            };

            pnlCloseSubmission.Controls.AddRange(new Control[] { chkCloseSubmission, numCloseAfter, lblCloseAfterUnit });
            Controls.Add(pnlCloseSubmission);

            y += 40;

            // SAVE button (new)
            btnSaveQuiz = new Button
            {
                Text = "Save Quiz",
                Location = new Point(10, y),
                Size = new Size(320, 36),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveQuiz.FlatAppearance.BorderSize = 0;
            btnSaveQuiz.Click += BtnSaveQuiz_Click;
            Controls.Add(btnSaveQuiz);
        }

        private void CreateCorrectAnswerSection(int yPosition)
        {
            chkHasCorrectAnswer = new CheckBox
            {
                Text = "Has correct answer(s)",
                Location = new Point(10, yPosition),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(68, 68, 68),
                Checked = true
            };
            chkHasCorrectAnswer.CheckedChanged += ChkHasCorrectAnswer_CheckedChanged;
            Controls.Add(chkHasCorrectAnswer);

            // Multi-select dropdown for correct answers
            clbCorrectAnswers = new CheckedListBox
            {
                Location = new Point(175, yPosition - 2),
                Size = new Size(155, CalculateDropdownHeight(selectedChoiceCount)), // Dynamic height
                Font = new Font("Segoe UI", 8F),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                IntegralHeight = false // Allows custom height
            };

            // Initialize with default 4 choices
            UpdateCorrectAnswerChoices();
            Controls.Add(clbCorrectAnswers);
        }

        // Helper method to calculate the appropriate height for the dropdown
        private int CalculateDropdownHeight(int choiceCount)
        {
            // Each item is approximately 16 pixels high, plus some padding
            var itemHeight = 16;
            var padding = 4;
            var minHeight = 20; // Minimum height
            var maxHeight = 120; // Maximum height to prevent it from getting too large

            var calculatedHeight = (choiceCount * itemHeight) + padding;

            // Ensure height is within reasonable bounds
            if (calculatedHeight < minHeight)
                calculatedHeight = minHeight;
            else if (calculatedHeight > maxHeight)
                calculatedHeight = maxHeight;

            return calculatedHeight;
        }
        private void ChkHasCorrectAnswer_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable the correct answers dropdown based on checkbox
            clbCorrectAnswers.Enabled = chkHasCorrectAnswer.Checked;

            if (!chkHasCorrectAnswer.Checked)
            {
                // Uncheck all items when disabled
                for (int i = 0; i < clbCorrectAnswers.Items.Count; i++)
                {
                    clbCorrectAnswers.SetItemChecked(i, false);
                }
            }
        }

        private void UpdateCorrectAnswerChoices()
        {
            if (clbCorrectAnswers == null) return;
            //hello
            // Store currently selected items
            var selectedItems = new List<string>();
            for (int i = 0; i < clbCorrectAnswers.CheckedItems.Count; i++)
            {
                selectedItems.Add(clbCorrectAnswers.CheckedItems[i].ToString());
            }

            // Clear and repopulate based on selected choice count
            clbCorrectAnswers.Items.Clear();

            for (int i = 0; i < selectedChoiceCount; i++)
            {
                var choiceLabel = $"Choice {(char)('A' + i)}";
                clbCorrectAnswers.Items.Add(choiceLabel);

                // Restore selection if it was previously selected
                if (selectedItems.Contains(choiceLabel))
                {
                    clbCorrectAnswers.SetItemChecked(i, true);
                }
            }

            // If no items are checked and "Has correct answer" is enabled, check the first one by default
            if (chkHasCorrectAnswer.Checked && clbCorrectAnswers.CheckedItems.Count == 0 && clbCorrectAnswers.Items.Count > 0)
            {
                clbCorrectAnswers.SetItemChecked(0, true);
            }
        }

        private void CreateChoiceNumberButtons(int yPosition)
        {
            pnlChoiceNumbers = new Panel
            {
                Location = new Point(10, yPosition),
                Size = new Size(320, 35),
                BackColor = Color.Transparent
            };

            choiceNumberButtons = new Button[5]; // 2, 3, 4, 5, 6

            for (int i = 0; i < 5; i++)
            {
                var choiceNumber = i + 2; // Start from 2
                var button = new Button
                {
                    Text = choiceNumber.ToString(),
                    Location = new Point(i * 50, 5),
                    Size = new Size(40, 30),
                    Font = new Font("Segoe UI", 9F),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = choiceNumber == 4 ? Color.FromArgb(0, 120, 215) : Color.FromArgb(240, 240, 240),
                    ForeColor = choiceNumber == 4 ? Color.White : Color.Black,
                    Cursor = Cursors.Hand,
                    Tag = choiceNumber
                };

                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                button.Click += ChoiceNumberButton_Click;

                choiceNumberButtons[i] = button;
                pnlChoiceNumbers.Controls.Add(button);
            }

            Controls.Add(pnlChoiceNumbers);
        }

        private void ChoiceNumberButton_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as Button;
            var newChoiceCount = (int)clickedButton.Tag;

            // Update selected choice count
            selectedChoiceCount = newChoiceCount;

            // Update button appearances
            foreach (var button in choiceNumberButtons)
            {
                var choiceNumber = (int)button.Tag;
                if (choiceNumber == selectedChoiceCount)
                {
                    button.BackColor = Color.FromArgb(0, 120, 215);
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = Color.FromArgb(240, 240, 240);
                    button.ForeColor = Color.Black;
                }
            }

            // Update the correct answer choices dropdown
            UpdateCorrectAnswerChoices();

            System.Diagnostics.Debug.WriteLine($"Selected choice count: {selectedChoiceCount}");
        }

        // Public methods to get current settings
        public string GetQuestionType()
        {
            return cmbQuestionType.SelectedItem?.ToString() ?? "Multiple Choice";
        }

        public int GetNumberOfChoices()
        {
            return selectedChoiceCount;
        }

        public bool GetAllowMultipleChoices()
        {
            return chkAllowMultiple.Checked;
        }

        public bool GetHasCorrectAnswer()
        {
            return chkHasCorrectAnswer.Checked;
        }

        public List<int> GetCorrectAnswerIndices()
        {
            var correctIndices = new List<int>();
            if (chkHasCorrectAnswer.Checked)
            {
                for (int i = 0; i < clbCorrectAnswers.CheckedItems.Count; i++)
                {
                    var checkedItem = clbCorrectAnswers.CheckedItems[i].ToString();
                    var index = clbCorrectAnswers.Items.IndexOf(checkedItem);
                    if (index >= 0)
                    {
                        correctIndices.Add(index);
                    }
                }
            }
            return correctIndices;
        }

        public List<string> GetCorrectAnswerChoices()
        {
            var correctChoices = new List<string>();
            if (chkHasCorrectAnswer.Checked)
            {
                foreach (var item in clbCorrectAnswers.CheckedItems)
                {
                    correctChoices.Add(item.ToString());
                }
            }
            return correctChoices;
        }

        public bool GetCompetitionMode()
        {
            return chkCompetitionMode.Checked;
        }

        public bool GetStartWithSlide()
        {
            return chkStartWithSlide.Checked;
        }

        public bool GetMinimizeResult()
        {
            return chkMinimizeResult.Checked;
        }

        public bool GetCloseSubmissionEnabled()
        {
            return chkCloseSubmission.Checked;
        }

        public int GetCloseSubmissionTime()
        {
            return (int)numCloseAfter.Value;
        }

        // Method to apply settings to a quiz (can be called when creating or updating quiz)
        public void ApplySettingsToQuiz()
        {
            var settings = new
            {
                QuestionType = GetQuestionType(),
                NumberOfChoices = GetNumberOfChoices(),
                AllowMultipleChoices = GetAllowMultipleChoices(),
                HasCorrectAnswer = GetHasCorrectAnswer(),
                CorrectAnswerIndices = GetCorrectAnswerIndices(),
                CorrectAnswerChoices = GetCorrectAnswerChoices(),
                CompetitionMode = GetCompetitionMode(),
                StartWithSlide = GetStartWithSlide(),
                MinimizeResult = GetMinimizeResult(),
                CloseSubmissionEnabled = GetCloseSubmissionEnabled(),
                CloseSubmissionTime = GetCloseSubmissionTime()
            };

            System.Diagnostics.Debug.WriteLine($"Quiz Settings Applied: {settings}");

            // Here you can integrate with your quiz creation logic
            // For example, call a method on the selected quiz button in the slide
            // or store these settings for when a quiz is actually created
        }

        // Save button handler - sends POST to QuizApiService
        private async void BtnSaveQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                if (_courseId <= 0)
                {
                    MessageBox.Show("Course ID not set. Ensure a course is created/selected before saving the quiz.", "Missing Course", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var svc = new QuizApiService();

                var request = new CreateMultipleChoiceQuizRequest
                {
                    Course = _courseId,
                    Title = string.IsNullOrWhiteSpace(txtQuestionText?.Text) ? "Untitled Quiz" : txtQuestionText.Text,
                    Properties = new QuizProperties
                    {
                        NumberOfChoices = selectedChoiceCount,
                        AllowMultipleChoices = chkAllowMultiple.Checked,
                        HasCorrectAnswer = chkHasCorrectAnswer.Checked,
                        CompetitionMode = chkCompetitionMode.Checked,
                        RandomizeChoiceOrder = false,
                        PointsPerCorrect = 1,
                        PenaltyPerWrong = 0
                    }
                };

                // Build choices. If the UI doesn't have custom choice text, use default labels.
                request.Properties.Choices = new List<QuizChoice>();
                for (int i = 0; i < selectedChoiceCount; i++)
                {
                    var label = $"Choice {(char)('A' + i)}";
                    bool isCorrect = false;

                    if (chkHasCorrectAnswer.Checked && clbCorrectAnswers != null)
                    {
                        var idx = clbCorrectAnswers.Items.IndexOf(label);
                        if (idx >= 0 && clbCorrectAnswers.GetItemChecked(idx))
                            isCorrect = true;
                    }

                    request.Properties.Choices.Add(new QuizChoice
                    {
                        Text = label,
                        IsCorrect = isCorrect
                    });
                }

                // Optional: set additional properties from UI (close time, points, etc.) if you extend the UI later.

                // Send request
                var response = await svc.CreateMultipleChoiceQuizAsync(request);

                if (response != null)
                {
                    // Add the quiz button to the slide with the quiz ID directly without dialog
                    Globals.ThisAddIn.AddQuizButtonToSlideWithQuizId(response.Id, response.Title);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save quiz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}