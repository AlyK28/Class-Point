using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassPointAddIn.Views.Modern
{
    #region ModernButton
    public class ModernButton : Button
    {
        private bool _isHovered = false;
        private bool _isPressed = false;

        [Browsable(true)]
        [Category("Modern Button")]
        public Color HoverColor { get; set; } = Color.FromArgb(200, 200, 200);

        [Browsable(true)]
        [Category("Modern Button")]
        public Color PressedColor { get; set; } = Color.FromArgb(180, 180, 180);

        [Browsable(true)]
        [Category("Modern Button")]
        public Color TextColor { get; set; } = Color.Black;

        [Browsable(true)]
        [Category("Modern Button")]
        public int CornerRadius { get; set; } = 6;

        [Browsable(true)]
        [Category("Modern Button")]
        public bool ShowShadow { get; set; } = true;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            Cursor = Cursors.Hand;
            Size = new Size(100, 35);

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;

            // Draw shadow
            if (ShowShadow && !_isPressed)
            {
                var shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 2, rect.Height - 2);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                using (var shadowPath = GetRoundedRectanglePath(shadowRect, CornerRadius))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            // Determine color
            Color currentColor = BackColor;
            if (_isPressed) currentColor = PressedColor;
            else if (_isHovered) currentColor = HoverColor;

            // Draw button
            var buttonRect = _isPressed ? new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2) : rect;
            using (var brush = new SolidBrush(currentColor))
            using (var path = GetRoundedRectanglePath(buttonRect, CornerRadius))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw text
            var textRect = buttonRect;
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, TextColor, flags);
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            return path;
        }
    }
    #endregion

    #region ModernPanel
    public class ModernPanel : Panel
    {
        [Browsable(true)]
        [Category("Modern Panel")]
        public int CornerRadius { get; set; } = 8;

        [Browsable(true)]
        [Category("Modern Panel")]
        public bool ShowShadow { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Panel")]
        public Color BorderColor { get; set; } = Color.FromArgb(229, 231, 235);

        [Browsable(true)]
        [Category("Modern Panel")]
        public int BorderWidth { get; set; } = 1;

        public ModernPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow
            if (ShowShadow)
            {
                var shadowRect = new Rectangle(3, 3, Width - 6, Height - 6);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                using (var shadowPath = GetRoundedRectanglePath(shadowRect, CornerRadius))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            // Draw main panel
            var mainRect = new Rectangle(0, 0, Width - 3, Height - 3);
            using (var brush = new SolidBrush(BackColor))
            using (var path = GetRoundedRectanglePath(mainRect, CornerRadius))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border
            if (BorderWidth > 0)
            {
                using (var pen = new Pen(BorderColor, BorderWidth))
                using (var path = GetRoundedRectanglePath(mainRect, CornerRadius))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            base.OnPaint(e);
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            return path;
        }
    }
    #endregion

    #region ModernTextBox
    public class ModernTextBox : TextBox
    {
        private bool _isFocused = false;

        [Browsable(true)]
        [Category("Modern TextBox")]
        public Color FocusedBorderColor { get; set; } = Color.FromArgb(67, 56, 202);

        [Browsable(true)]
        [Category("Modern TextBox")]
        public Color BorderColor { get; set; } = Color.FromArgb(229, 231, 235);

        [Browsable(true)]
        [Category("Modern TextBox")]
        public int CornerRadius { get; set; } = 6;

        [Browsable(true)]
        [Category("Modern TextBox")]
        public string PlaceholderText { get; set; } = "";

        public ModernTextBox()
        {
            BorderStyle = BorderStyle.None;
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(10, 8, 10, 8);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer, true);
        }

        protected override void OnEnter(EventArgs e)
        {
            _isFocused = true;
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            _isFocused = false;
            Invalidate();
            base.OnLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // This is a simplified version - full implementation would require more work
            // for proper text rendering in a custom paint scenario
            base.OnPaint(e);
        }
    }
    #endregion

    #region ModernLabel
    public class ModernLabel : Label
    {
        private bool _isTitle = false;
        private bool _isSubtitle = false;
        private bool _isCaption = false;

        [Browsable(true)]
        [Category("Modern Label")]
        public bool IsTitle
        {
            get => _isTitle;
            set
            {
                if (_isTitle != value)
                {
                    _isTitle = value;
                    if (value)
                    {
                        _isSubtitle = false;
                        _isCaption = false;
                    }
                    UpdateStyle();
                }
            }
        }

        [Browsable(true)]
        [Category("Modern Label")]
        public bool IsSubtitle
        {
            get => _isSubtitle;
            set
            {
                if (_isSubtitle != value)
                {
                    _isSubtitle = value;
                    if (value)
                    {
                        _isTitle = false;
                        _isCaption = false;
                    }
                    UpdateStyle();
                }
            }
        }

        [Browsable(true)]
        [Category("Modern Label")]
        public bool IsCaption
        {
            get => _isCaption;
            set
            {
                if (_isCaption != value)
                {
                    _isCaption = value;
                    if (value)
                    {
                        _isTitle = false;
                        _isSubtitle = false;
                    }
                    UpdateStyle();
                }
            }
        }

        public ModernLabel()
        {
            Font = new Font("Segoe UI", 9F);
            UpdateStyle();
        }

        private void UpdateStyle()
        {
            if (_isTitle)
            {
                Font = new Font("Segoe UI", 16F, FontStyle.Bold);
                ForeColor = Color.FromArgb(17, 24, 39);
            }
            else if (_isSubtitle)
            {
                Font = new Font("Segoe UI", 12F, FontStyle.Regular);
                ForeColor = Color.FromArgb(55, 65, 81);
            }
            else if (_isCaption)
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                ForeColor = Color.FromArgb(107, 114, 128);
            }
            else
            {
                // Default style
                Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                ForeColor = Color.FromArgb(55, 65, 81);
            }

            // Refresh the control
            Invalidate();
        }
    }
    #endregion
}