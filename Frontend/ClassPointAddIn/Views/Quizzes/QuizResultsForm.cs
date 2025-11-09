using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ClassPointAddIn.Views.Quizzes
{
    public class QuizResultsForm : Form
    {
        private Chart resultsChart;
        private Button btnShowCorrectAnswer;
        private int[] percentages;
        private List<int> correctIndices;
        private List<string> labels;

        public QuizResultsForm(int[] percentages, List<int> correctIndices = null, List<string> labels = null)
        {
            this.percentages = percentages ?? new int[0];
            this.correctIndices = correctIndices ?? new List<int>();
            this.labels = labels ?? Enumerable.Range(0, this.percentages.Length).Select(i => $"Choice {(char)('A' + i)}").ToList();

            InitializeComponent();
            PopulateChart();
        }

        private void InitializeComponent()
        {
            Text = "Quiz Results";
            Size = new Size(520, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            resultsChart = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300,
                BackColor = Color.White
            };

            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            chartArea.AxisY.Title = "Percentage";
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 100;
            resultsChart.ChartAreas.Add(chartArea);

            var series = new Series("Responses")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            resultsChart.Series.Add(series);

            btnShowCorrectAnswer = new Button
            {
                Text = "Show Correct Answer",
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShowCorrectAnswer.FlatAppearance.BorderSize = 0;
            btnShowCorrectAnswer.Click += BtnShowCorrectAnswer_Click;

            Controls.Add(resultsChart);
            Controls.Add(btnShowCorrectAnswer);
        }

        private void PopulateChart()
        {
            var series = resultsChart.Series["Responses"];
            series.Points.Clear();

            for (int i = 0; i < percentages.Length; i++)
            {
                int pointIndex = series.Points.AddY(percentages[i]);
                DataPoint dp = series.Points[pointIndex];

                dp.AxisLabel = labels.Count > i ? labels[i] : $"Choice {(char)('A' + i)}";
                dp.Label = $"{percentages[i]}%";
                dp.Color = Color.FromArgb(0, 120, 215); // default blue
                dp.Tag = i;
            }
        }

        private void BtnShowCorrectAnswer_Click(object sender, EventArgs e)
        {
            var series = resultsChart.Series["Responses"];

            if (series == null || series.Points.Count == 0)
            {
                MessageBox.Show("No results to show.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataPoint pt in series.Points)
            {
                var value = pt.YValues != null && pt.YValues.Length > 0 ? (int)Math.Round(pt.YValues[0]) : 0;
                pt.Color = Color.LightGray;
                pt.Label = $"{value}%";
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
                        pt.Label = $"? {val}%";
                    }
                }
            }
            else
            {
                MessageBox.Show("No correct answer configured.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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