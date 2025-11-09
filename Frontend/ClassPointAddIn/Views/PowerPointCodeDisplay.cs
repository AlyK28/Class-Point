using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointAddIn.Views
{
    public class PowerPointCodeDisplay
    {
        private Shape _codeShape;
        private Presentation _presentation;
        private bool _isActive = false;
        private int _studentCount = 0;
        private Timer _updateTimer;
        private int _classId;

        public string ClassCode { get; private set; }
        public string CourseName { get; private set; }

        public PowerPointCodeDisplay(string classCode, string courseName, int initialStudentCount = 0, int classId = 0)
        {
            ClassCode = classCode;
            CourseName = courseName;
            _studentCount = initialStudentCount;
            _classId = classId;
        }

        public void Show()
        {
            try
            {
                if (_isActive) return;

                _presentation = Globals.ThisAddIn.Application.ActivePresentation;
                if (_presentation?.SlideShowWindow?.View?.Slide != null)
                {
                    var slide = _presentation.SlideShowWindow.View.Slide;
                    CreateCodeTextBox(slide);
                    _isActive = true;
                    
                    // Start polling for student count updates if class ID is available
                    if (_classId > 0)
                    {
                        StartStudentCountPolling();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing PowerPoint code display: {ex.Message}");
            }
        }

        private void StartStudentCountPolling()
        {
            _updateTimer = new Timer();
            _updateTimer.Interval = 3000; // Poll every 3 seconds
            _updateTimer.Tick += async (s, e) => await UpdateStudentCountFromApi();
            _updateTimer.Start();
        }

        private async System.Threading.Tasks.Task UpdateStudentCountFromApi()
        {
            try
            {
                if (_classId <= 0) return;

                var classService = new Api.Services.ClassService.ClassApiService();
                var classInfo = await classService.GetClassAsync(_classId);
                
                if (classInfo != null && classInfo.StudentCount != _studentCount)
                {
                    UpdateStudentCount(classInfo.StudentCount);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating student count: {ex.Message}");
            }
        }

        private void CreateCodeTextBox(Slide slide)
        {
            // Create a text box in the top-right corner
            _codeShape = slide.Shapes.AddTextbox(
                Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                slide.Master.Width - 220, // X position (right side)
                20,  // Y position (top)
                200, // Width
                60   // Height
            );

            // Style the text box
            var textFrame = _codeShape.TextFrame;
            textFrame.MarginLeft = 10;
            textFrame.MarginRight = 10;
            textFrame.MarginTop = 5;
            textFrame.MarginBottom = 5;

            // Set the text - just code and student count
            var textRange = textFrame.TextRange;
            textRange.Text = $"{ClassCode}\n👥 {_studentCount} student{(_studentCount != 1 ? "s" : "")} connected";

            // Format the text
            textRange.Font.Name = "Segoe UI";
            textRange.Font.Size = 14;
            textRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
            textRange.Font.Color.RGB = ColorTranslator.ToOle(Color.White);
            textRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignCenter;

            // Style the shape background
            _codeShape.Fill.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 120, 215));
            _codeShape.Fill.Transparency = 0.1f; // 10% transparent
            _codeShape.Line.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(0, 100, 180));
            _codeShape.Line.Weight = 2;

            // Make it appear on top
            _codeShape.ZOrder(Microsoft.Office.Core.MsoZOrderCmd.msoBringToFront);
        }

        public void Hide()
        {
            try
            {
                // Stop the polling timer
                if (_updateTimer != null)
                {
                    _updateTimer.Stop();
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                if (_codeShape != null && _isActive)
                {
                    _codeShape.Delete();
                    _codeShape = null;
                    _isActive = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding PowerPoint code display: {ex.Message}");
            }
        }

        public void UpdateCode(string newCode)
        {
            if (_codeShape != null && _isActive)
            {
                ClassCode = newCode;
                var textRange = _codeShape.TextFrame.TextRange;
                textRange.Text = $"{ClassCode}\n👥 {_studentCount} student{(_studentCount != 1 ? "s" : "")} connected";
            }
        }

        public void UpdateStudentCount(int count)
        {
            _studentCount = count;
            if (_codeShape != null && _isActive)
            {
                var textRange = _codeShape.TextFrame.TextRange;
                textRange.Text = $"{ClassCode}\n👥 {_studentCount} student{(_studentCount != 1 ? "s" : "")} connected";
            }
        }
    }
}
