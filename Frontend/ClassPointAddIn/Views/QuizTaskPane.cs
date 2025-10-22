using ClassPointAddIn.Api.Services.QuizService;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Font = System.Drawing.Font;
using Point = System.Drawing.Point;

namespace ClassPointAddIn.Views.Quizzes
{
    public partial class QuizTaskPane : UserControl
    {
        // Main sections
        private TabControl tabContainer;
        private TabPage tabQuizList;
        private TabPage tabCreateQuiz;

        // Quiz List Tab
        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnRefresh;
        private ListBox lstQuizzes;
        private Button btnInsertQuiz;
        private Panel pnlQuizDetails;
        private Label lblQuizTitle;
        private Label lblQuizQuestion;
        private Label lblQuizChoices;

        // Create Quiz Tab
        private PlaceholderTextBox txtTitle;
        private PlaceholderTextBox txtQuestion;
        private CheckBox chkAllowMultiple;
        private CheckBox chkCompetitionMode;
        private CheckBox chkRandomizeOrder;
        private NumericUpDown numPointsCorrect;
        private NumericUpDown numPenaltyWrong;
        private Panel pnlChoices;
        private Button btnAddChoice;
        private Button btnCreateQuiz;
        private Button btnCancelCreate;

        private int _courseId;
        private List<QuizResponse> _quizzes;
        private QuizApiService _quizService;
        private List<ChoiceControl> _choiceControls;

        public QuizTaskPane()
        {
            _quizzes = new List<QuizResponse>();
            _quizService = new QuizApiService();
            _choiceControls = new List<ChoiceControl>();
            InitializeComponent();
        }

        public void SetCourseId(int courseId)
        {
            _courseId = courseId;
            LoadQuizzes();
        }

        private void InitializeComponent()
        {
            Size = new Size(350, 700);
            BackColor = Color.White;
            Padding = new Padding(5);

            CreateMainLayout();
        }

        private void CreateMainLayout()
        {
            // Header
            pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(340, 50),
                BackColor = Color.FromArgb(0, 120, 215)
            };

            lblTitle = new Label
            {
                Text = "Quiz Manager",
                Location = new Point(10, 10),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.Add(lblTitle);
            Controls.Add(pnlHeader);

            // Tab Container
            tabContainer = new TabControl
            {
                Location = new Point(5, 55),
                Size = new Size(340, 640),
                Font = new Font("Segoe UI", 9F)
            };

            CreateQuizListTab();
            CreateQuizCreationTab();

            tabContainer.TabPages.Add(tabQuizList);
            tabContainer.TabPages.Add(tabCreateQuiz);

            Controls.Add(tabContainer);
        }

        #region Quiz List Tab

        private void CreateQuizListTab()
        {
            tabQuizList = new TabPage("Quiz Library")
            {
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var y = 10;

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "🔄 Refresh List",
                Location = new Point(10, y),
                Size = new Size(300, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;
            tabQuizList.Controls.Add(btnRefresh);

            y += 40;

            // Quiz List
            var lblQuizList = new Label
            {
                Text = "Available Quizzes:",
                Location = new Point(10, y),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            tabQuizList.Controls.Add(lblQuizList);

            y += 25;

            var pnlQuizList = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(300, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            lstQuizzes = new ListBox
            {
                Dock = DockStyle.Fill,
                DisplayMember = "Title",
                ValueMember = "Id",
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.None
            };
            lstQuizzes.SelectedIndexChanged += LstQuizzes_SelectedIndexChanged;

            pnlQuizList.Controls.Add(lstQuizzes);
            tabQuizList.Controls.Add(pnlQuizList);

            y += 210;

            // Insert Quiz Button
            btnInsertQuiz = new Button
            {
                Text = "📋 Insert Quiz into Slide",
                Location = new Point(10, y),
                Size = new Size(300, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnInsertQuiz.FlatAppearance.BorderSize = 0;
            btnInsertQuiz.Click += BtnInsertQuiz_Click;
            tabQuizList.Controls.Add(btnInsertQuiz);

            y += 50;

            // Quiz Preview
            var lblDetails = new Label
            {
                Text = "Quiz Preview:",
                Location = new Point(10, y),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68)
            };
            tabQuizList.Controls.Add(lblDetails);

            y += 25;

            pnlQuizDetails = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(300, 250),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                AutoScroll = true
            };

            lblQuizTitle = new Label
            {
                Text = "Select a quiz to preview",
                Location = new Point(10, 10),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68),
                BackColor = Color.Transparent
            };

            lblQuizQuestion = new Label
            {
                Location = new Point(10, 35),
                Size = new Size(280, 60),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                Text = ""
            };

            lblQuizChoices = new Label
            {
                Location = new Point(10, 100),
                Size = new Size(280, 120),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(108, 117, 125),
                BackColor = Color.Transparent,
                Text = ""
            };

            pnlQuizDetails.Controls.AddRange(new Control[] { lblQuizTitle, lblQuizQuestion, lblQuizChoices });
            tabQuizList.Controls.Add(pnlQuizDetails);
        }

        #endregion

        #region Quiz Creation Tab

        private void CreateQuizCreationTab()
        {
            tabCreateQuiz = new TabPage("Create Quiz")
            {
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoScroll = true
            };

            var y = 10;

            // Title
            var lblTitleLabel = new Label
            {
                Text = "Quiz Title:",
                Location = new Point(10, y),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            txtTitle = new PlaceholderTextBox
            {
                Location = new Point(10, y + 25),
                Size = new Size(300, 23),
                Placeholder = "Enter quiz title..."
            };
            tabCreateQuiz.Controls.AddRange(new Control[] { lblTitleLabel, txtTitle });

            y += 60;

            // Question
            var lblQuestionLabel = new Label
            {
                Text = "Question:",
                Location = new Point(10, y),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            txtQuestion = new PlaceholderTextBox
            {
                Location = new Point(10, y + 25),
                Size = new Size(300, 60),
                Multiline = true,
                Placeholder = "Enter your question..."
            };
            tabCreateQuiz.Controls.AddRange(new Control[] { lblQuestionLabel, txtQuestion });

            y += 100;

            // Options
            chkAllowMultiple = new CheckBox
            {
                Text = "Allow multiple correct answers",
                Location = new Point(10, y),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 8F)
            };

            chkCompetitionMode = new CheckBox
            {
                Text = "Competition mode",
                Location = new Point(10, y + 25),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 8F)
            };

            chkRandomizeOrder = new CheckBox
            {
                Text = "Randomize choice order",
                Location = new Point(10, y + 50),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 8F)
            };

            tabCreateQuiz.Controls.AddRange(new Control[] { chkAllowMultiple, chkCompetitionMode, chkRandomizeOrder });

            y += 85;

            // Scoring
            var lblPoints = new Label
            {
                Text = "Points per correct:",
                Location = new Point(10, y),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };
            numPointsCorrect = new NumericUpDown
            {
                Location = new Point(140, y),
                Size = new Size(60, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 1
            };

            var lblPenalty = new Label
            {
                Text = "Penalty per wrong:",
                Location = new Point(10, y + 30),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };
            numPenaltyWrong = new NumericUpDown
            {
                Location = new Point(140, y + 30),
                Size = new Size(60, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            tabCreateQuiz.Controls.AddRange(new Control[] { lblPoints, numPointsCorrect, lblPenalty, numPenaltyWrong });

            y += 70;

            // Choices section
            var lblChoicesLabel = new Label
            {
                Text = "Answer Choices:",
                Location = new Point(10, y),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            tabCreateQuiz.Controls.Add(lblChoicesLabel);

            y += 25;

            pnlChoices = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(300, 200),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.White
            };
            tabCreateQuiz.Controls.Add(pnlChoices);

            y += 210;

            btnAddChoice = new Button
            {
                Text = "➕ Add Choice",
                Location = new Point(10, y),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F)
            };
            btnAddChoice.FlatAppearance.BorderSize = 0;
            btnAddChoice.Click += BtnAddChoice_Click;
            tabCreateQuiz.Controls.Add(btnAddChoice);

            y += 40;

            // Action buttons
            btnCreateQuiz = new Button
            {
                Text = "✅ Create Quiz",
                Location = new Point(10, y),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCreateQuiz.FlatAppearance.BorderSize = 0;
            btnCreateQuiz.Click += BtnCreateQuiz_Click;

            btnCancelCreate = new Button
            {
                Text = "❌ Clear Form",
                Location = new Point(160, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            btnCancelCreate.FlatAppearance.BorderSize = 0;
            btnCancelCreate.Click += BtnCancelCreate_Click;

            tabCreateQuiz.Controls.AddRange(new Control[] { btnCreateQuiz, btnCancelCreate });

            // Add default choices
            AddDefaultChoices();
        }

        private void AddDefaultChoices()
        {
            // Add 3 default choices
            for (int i = 0; i < 3; i++)
            {
                AddChoice($"Choice {i + 1}", false);
            }
        }

        private void BtnAddChoice_Click(object sender, EventArgs e)
        {
            AddChoice("", false);
        }

        private void AddChoice(string text, bool isCorrect)
        {
            var choiceControl = new ChoiceControl(text, isCorrect, _choiceControls.Count + 1);
            choiceControl.Location = new Point(5, _choiceControls.Count * 35 + 5);
            choiceControl.Size = new Size(280, 30);
            choiceControl.RemoveRequested += (s, e) => RemoveChoice(choiceControl);

            _choiceControls.Add(choiceControl);
            pnlChoices.Controls.Add(choiceControl);

            UpdateRemoveButtons();
        }

        private void RemoveChoice(ChoiceControl choiceControl)
        {
            if (_choiceControls.Count <= 2) return;

            _choiceControls.Remove(choiceControl);
            pnlChoices.Controls.Remove(choiceControl);

            // Reposition remaining choices
            for (int i = 0; i < _choiceControls.Count; i++)
            {
                _choiceControls[i].Location = new Point(5, i * 35 + 5);
                _choiceControls[i].UpdateNumber(i + 1);
            }

            UpdateRemoveButtons();
        }

        private void UpdateRemoveButtons()
        {
            bool canRemove = _choiceControls.Count > 2;
            foreach (var choice in _choiceControls)
            {
                choice.CanRemove = canRemove;
            }
        }

        private async void BtnCreateQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                btnCreateQuiz.Enabled = false;
                btnCreateQuiz.Text = "Creating...";

                var request = new CreateMultipleChoiceQuizRequest
                {
                    Course = _courseId,
                    Title = txtTitle.Text.Trim(),
                    Properties = new QuizProperties
                    {
                        QuestionText = txtQuestion.Text.Trim(),
                        AllowMultipleChoices = chkAllowMultiple.Checked,
                        CompetitionMode = chkCompetitionMode.Checked,
                        RandomizeChoiceOrder = chkRandomizeOrder.Checked,
                        PointsPerCorrect = (int)numPointsCorrect.Value,
                        PenaltyPerWrong = (int)numPenaltyWrong.Value,
                        HasCorrectAnswer = _choiceControls.Any(c => c.IsCorrect),
                        Choices = _choiceControls.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText))
                                                .Select(c => new QuizChoice
                                                {
                                                    Text = c.ChoiceText,
                                                    IsCorrect = c.IsCorrect
                                                }).ToList()
                    }
                };

                request.Properties.NumberOfChoices = request.Properties.Choices.Count;

                var createdQuiz = await _quizService.CreateMultipleChoiceQuizAsync(request);

                MessageBox.Show($"Quiz '{createdQuiz.Title}' created successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearCreateForm();
                LoadQuizzes();
                tabContainer.SelectedTab = tabQuizList; // Switch back to list tab
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating quiz: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreateQuiz.Enabled = true;
                btnCreateQuiz.Text = "✅ Create Quiz";
            }
        }

        private void BtnCancelCreate_Click(object sender, EventArgs e)
        {
            ClearCreateForm();
        }

        private void ClearCreateForm()
        {
            txtTitle.Text = "";
            txtQuestion.Text = "";
            chkAllowMultiple.Checked = false;
            chkCompetitionMode.Checked = false;
            chkRandomizeOrder.Checked = false;
            numPointsCorrect.Value = 1;
            numPenaltyWrong.Value = 0;

            // Clear choices
            _choiceControls.Clear();
            pnlChoices.Controls.Clear();
            AddDefaultChoices();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a quiz title.", "Validation Error");
                txtTitle.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuestion.Text))
            {
                MessageBox.Show("Please enter a question.", "Validation Error");
                txtQuestion.Focus();
                return false;
            }

            var validChoices = _choiceControls.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText)).ToList();
            if (validChoices.Count < 2)
            {
                MessageBox.Show("Please provide at least 2 answer choices.", "Validation Error");
                return false;
            }

            if (!validChoices.Any(c => c.IsCorrect))
            {
                MessageBox.Show("Please mark at least one choice as correct.", "Validation Error");
                return false;
            }

            return true;
        }

        #endregion

        #region Quiz List Functionality

        private async void LoadQuizzes()
        {
            try
            {
                btnRefresh.Text = "🔄 Loading...";
                btnRefresh.Enabled = false;

                if (_courseId > 0)
                {
                    _quizzes = await _quizService.GetQuizzesForCourseAsync(_courseId);

                    lstQuizzes.DataSource = null;
                    lstQuizzes.DataSource = _quizzes;
                    lstQuizzes.DisplayMember = "Title";
                    lstQuizzes.ValueMember = "Id";

                    if (_quizzes.Count == 0)
                    {
                        lstQuizzes.Items.Clear();
                        lstQuizzes.Items.Add("No quizzes found");
                    }
                }
                else
                {
                    lstQuizzes.Items.Clear();
                    lstQuizzes.Items.Add("No course selected");
                }
            }
            catch (Exception ex)
            {
                lstQuizzes.Items.Clear();
                lstQuizzes.Items.Add($"Error: {ex.Message}");
            }
            finally
            {
                btnRefresh.Text = "🔄 Refresh List";
                btnRefresh.Enabled = true;
            }
        }

        private void LstQuizzes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedQuiz = lstQuizzes.SelectedItem as QuizResponse;
            btnInsertQuiz.Enabled = selectedQuiz != null;

            if (selectedQuiz != null)
            {
                UpdateQuizPreview(selectedQuiz);
            }
            else
            {
                ClearQuizPreview();
            }
        }

        private void UpdateQuizPreview(QuizResponse quiz)
        {
            lblQuizTitle.Text = quiz.Title;
            lblQuizQuestion.Text = quiz.Properties.QuestionText;

            var choicesText = "Choices:\n";
            for (int i = 0; i < quiz.Properties.Choices.Count && i < 6; i++)
            {
                var choice = quiz.Properties.Choices[i];
                var prefix = choice.IsCorrect ? "✓" : "○";
                choicesText += $"{prefix} {(char)('A' + i)}. {choice.Text}\n";
            }

            choicesText += $"\nPoints: {quiz.Properties.PointsPerCorrect}";
            if (quiz.Properties.AllowMultipleChoices)
                choicesText += " | Multiple answers allowed";

            lblQuizChoices.Text = choicesText;
        }

        private void ClearQuizPreview()
        {
            lblQuizTitle.Text = "Select a quiz to preview";
            lblQuizQuestion.Text = "";
            lblQuizChoices.Text = "";
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadQuizzes();
        }

        private void BtnInsertQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedQuiz = lstQuizzes.SelectedItem as QuizResponse;
                if (selectedQuiz == null) return;

                InsertQuizIntoSlide(selectedQuiz);

                MessageBox.Show($"Quiz '{selectedQuiz.Title}' inserted into slide!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting quiz: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertQuizIntoSlide(QuizResponse quiz)
        {
            var application = Globals.ThisAddIn.Application;
            var presentation = application.ActivePresentation;
            var slide = application.ActiveWindow.View.Slide;

            // Create quiz container shape
            var containerShape = slide.Shapes.AddShape(
                Microsoft.Office.Core.MsoAutoShapeType.msoShapeRectangle,
                50, 100, 600, 400);

            // Style the container
            containerShape.Fill.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(248, 249, 250));
            containerShape.Line.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 120, 215));
            containerShape.Line.Weight = 3;

            // Add quiz title
            var titleShape = slide.Shapes.AddTextbox(
                Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                60, 110, 580, 50);

            var titleText = titleShape.TextFrame.TextRange;
            titleText.Text = quiz.Title;
            titleText.Font.Name = "Arial";
            titleText.Font.Size = 18;
            titleText.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
            titleText.Font.Color.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 120, 215));
            titleText.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignCenter;

            // Add question
            var questionShape = slide.Shapes.AddTextbox(
                Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                60, 170, 580, 60);

            var questionText = questionShape.TextFrame.TextRange;
            questionText.Text = quiz.Properties.QuestionText;
            questionText.Font.Name = "Arial";
            questionText.Font.Size = 14;
            questionText.Font.Color.RGB = ColorTranslator.ToOle(Color.Black);

            // Add choices
            var yPos = 240;
            for (int i = 0; i < quiz.Properties.Choices.Count && i < 6; i++)
            {
                var choice = quiz.Properties.Choices[i];
                var choiceShape = slide.Shapes.AddTextbox(
                    Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                    80, yPos, 540, 30);

                var choiceText = choiceShape.TextFrame.TextRange;
                choiceText.Text = $"{(char)('A' + i)}. {choice.Text}";
                choiceText.Font.Name = "Arial";
                choiceText.Font.Size = 12;
                choiceText.Font.Color.RGB = ColorTranslator.ToOle(
                    choice.IsCorrect ? Color.FromArgb(40, 167, 69) : Color.Black);

                if (choice.IsCorrect)
                {
                    choiceText.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                }

                yPos += 35;
            }

            // Add quiz info
            var infoShape = slide.Shapes.AddTextbox(
                Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                60, 460, 580, 30);

            var infoText = infoShape.TextFrame.TextRange;
            infoText.Text = $"Points: {quiz.Properties.PointsPerCorrect} | " +
                           $"Multiple Choice: {(quiz.Properties.AllowMultipleChoices ? "Yes" : "No")} | " +
                           $"Quiz ID: {quiz.Id}";
            infoText.Font.Name = "Arial";
            infoText.Font.Size = 10;
            infoText.Font.Color.RGB = ColorTranslator.ToOle(Color.Gray);
            infoText.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignCenter;
        }

        #endregion
    }

    // Custom TextBox with placeholder functionality for .NET Framework 4.8
    public class PlaceholderTextBox : TextBox
    {
        private string _placeholder = "";
        private bool _isPlaceholderMode = true;

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                if (_isPlaceholderMode)
                {
                    ShowPlaceholder();
                }
            }
        }

        public override string Text
        {
            get
            {
                return _isPlaceholderMode ? "" : base.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ShowPlaceholder();
                }
                else
                {
                    HidePlaceholder();
                    base.Text = value;
                }
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            if (_isPlaceholderMode)
            {
                HidePlaceholder();
            }
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                ShowPlaceholder();
            }
            base.OnLeave(e);
        }

        private void ShowPlaceholder()
        {
            _isPlaceholderMode = true;
            base.Text = _placeholder;
            ForeColor = Color.Gray;
            Font = new Font(Font, FontStyle.Italic);
        }

        private void HidePlaceholder()
        {
            _isPlaceholderMode = false;
            base.Text = "";
            ForeColor = SystemColors.WindowText;
            Font = new Font(Font, FontStyle.Regular);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (!_isPlaceholderMode)
            {
                base.OnTextChanged(e);
            }
        }
    }

    // Helper control for individual choices (updated for .NET Framework 4.8)
    public partial class ChoiceControl : UserControl
    {
        private CheckBox chkCorrect;
        private PlaceholderTextBox txtChoice;
        private Button btnRemove;
        private Label lblNumber;

        public string ChoiceText => txtChoice.Text.Trim();
        public bool IsCorrect => chkCorrect.Checked;
        public bool CanRemove { set => btnRemove.Enabled = value; }

        public event EventHandler RemoveRequested;

        public ChoiceControl(string text, bool isCorrect, int number)
        {
            InitializeComponent();
            txtChoice.Text = text;
            chkCorrect.Checked = isCorrect;
            lblNumber.Text = $"{number}.";
        }

        public void UpdateNumber(int number)
        {
            lblNumber.Text = $"{number}.";
        }

        private void InitializeComponent()
        {
            Size = new Size(280, 30);

            lblNumber = new Label
            {
                Location = new Point(0, 5),
                Size = new Size(15, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };

            chkCorrect = new CheckBox
            {
                Location = new Point(20, 5),
                Size = new Size(15, 20),
                Font = new Font("Segoe UI", 7F)
            };

            txtChoice = new PlaceholderTextBox
            {
                Location = new Point(40, 3),
                Size = new Size(200, 23),
                Placeholder = "Enter choice text...",
                Font = new Font("Segoe UI", 8F)
            };

            btnRemove = new Button
            {
                Text = "✕",
                Location = new Point(250, 2),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7F)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);

            Controls.AddRange(new Control[] { lblNumber, chkCorrect, txtChoice, btnRemove });
        }
    }
}
