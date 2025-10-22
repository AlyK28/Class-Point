using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Drawing;

namespace ClassPointAddIn.Views
{
    public class PowerPointCodeDisplay
    {
        private Shape _codeShape;
        private Presentation _presentation;
        private bool _isActive = false;

        public string ClassCode { get; private set; }
        public string CourseName { get; private set; }

        public PowerPointCodeDisplay(string classCode, string courseName)
        {
            ClassCode = classCode;
            CourseName = courseName;
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing PowerPoint code display: {ex.Message}");
            }
        }

        private void CreateCodeTextBox(Slide slide)
        {
            // Create a text box in the top-right corner
            _codeShape = slide.Shapes.AddTextbox(
                Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                slide.Master.Width - 200, // X position (right side)
                20,  // Y position (top)
                180, // Width
                80   // Height
            );

            // Style the text box
            var textFrame = _codeShape.TextFrame;
            textFrame.MarginLeft = 10;
            textFrame.MarginRight = 10;
            textFrame.MarginTop = 5;
            textFrame.MarginBottom = 5;

            // Set the text
            var textRange = textFrame.TextRange;
            textRange.Text = $"Class Code\n{ClassCode}\nStudents join with this code";

            // Format the text
            textRange.Font.Name = "Consolas";
            textRange.Font.Size = 12;
            textRange.Font.Color.RGB = ColorTranslator.ToOle(Color.White);
            textRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignCenter;

            // Style the shape background
            _codeShape.Fill.ForeColor.RGB = ColorTranslator.ToOle(Color.FromArgb(40, 40, 40));
            _codeShape.Fill.Transparency = 0.2f; // 20% transparent
            _codeShape.Line.ForeColor.RGB = ColorTranslator.ToOle(Color.Gray);
            _codeShape.Line.Weight = 1;

            // Make it appear on top
            _codeShape.ZOrder(Microsoft.Office.Core.MsoZOrderCmd.msoBringToFront);
        }

        public void Hide()
        {
            try
            {
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
                textRange.Text = $"Class Code\n{ClassCode}\nStudents join with this code";
            }
        }
    }
}
