using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassPointAddIn.Views.Modern
{
    public abstract class ModernFormBase : Form
    {
        #region Private Fields
        private Panel _headerPanel;
        private Panel _contentPanel;
        private Panel _footerPanel;
        private Label _titleLabel;
        private Label _closeButton;
        private Timer _fadeTimer;
        private bool _isClosing = false;
        private bool _isDragging = false;
        private Point _lastMousePos;
        #endregion

        #region Public Properties
        [Browsable(true)]
        [Category("Modern Form")]
        [Description("The title text displayed in the header")]
        public string FormTitle { get; set; } = "Modern Form";

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("The icon/emoji displayed before the title")]
        public string TitleIcon { get; set; } = "📋";

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Whether the form can be dragged by the header")]
        public bool AllowDragging { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Whether to show the close button in header")]
        public bool ShowCloseButton { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Whether to show the header panel")]
        public bool ShowHeader { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Whether to show the footer panel")]
        public bool ShowFooter { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("The header background color")]
        public Color HeaderColor { get; set; } = Color.FromArgb(67, 56, 202);

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("The content background color")]
        public Color ContentColor { get; set; } = Color.White;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("The footer background color")]
        public Color FooterColor { get; set; } = Color.FromArgb(249, 250, 251);

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Corner radius for rounded corners")]
        public int CornerRadius { get; set; } = 8;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Whether to use fade animations")]
        public bool UseAnimations { get; set; } = true;

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Header height in pixels")]
        public int HeaderHeight { get; set; } = 36; // Reduced from 60

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Footer height in pixels")]
        public int FooterHeight { get; set; } = 50; // Reduced from 80

        [Browsable(true)]
        [Category("Modern Form")]
        [Description("Animation speed (higher = faster)")]
        public double AnimationSpeed { get; set; } = 0.15; // Increased from 0.05

        // Access to main panels for derived classes
        protected Panel HeaderPanel => _headerPanel;
        protected Panel ContentPanel => _contentPanel;
        protected Panel FooterPanel => _footerPanel;
        #endregion

        #region Constructor
        protected ModernFormBase()
        {
            InitializeModernForm();
        }
        #endregion

        #region Initialization
        private void InitializeModernForm()
        {
            // Form setup
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 244, 248);
            Font = new Font("Segoe UI", 9F);

            // Enable modern rendering
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            CreatePanels();
            SetupFadeAnimation();
        }

        private void CreatePanels()
        {
            CreateHeaderPanel();
            CreateContentPanel();
            CreateFooterPanel();
        }

        private void CreateHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight, // Now configurable and smaller (36px)
                BackColor = HeaderColor,
                Padding = new Padding(15, 5, 15, 5), // Reduced padding
                Visible = ShowHeader
            };

            _titleLabel = new Label
            {
                Text = $"{TitleIcon} {FormTitle}",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular), // Reduced from 14F
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            if (ShowCloseButton)
            {
                _closeButton = new Label
                {
                    Text = "✕",
                    Size = new Size(24, 24), // Reduced from 30x30
                    Font = new Font("Segoe UI", 10F), // Reduced from 12F
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                _closeButton.Click += OnCloseButtonClick;
                _closeButton.MouseEnter += (s, e) => _closeButton.BackColor = Color.FromArgb(100, 255, 255, 255);
                _closeButton.MouseLeave += (s, e) => _closeButton.BackColor = Color.Transparent;

                _headerPanel.Controls.Add(_closeButton);
            }

            if (AllowDragging)
            {
                _headerPanel.MouseDown += OnHeaderMouseDown;
                _headerPanel.MouseMove += OnHeaderMouseMove;
                _headerPanel.MouseUp += (s, e) => _isDragging = false;
                _titleLabel.MouseDown += OnHeaderMouseDown;
                _titleLabel.MouseMove += OnHeaderMouseMove;
                _titleLabel.MouseUp += (s, e) => _isDragging = false;
            }

            _headerPanel.Controls.Add(_titleLabel);
            Controls.Add(_headerPanel);
        }

        private void CreateContentPanel()
        {
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentColor,
                Padding = new Padding(15) // Reduced from 20
            };

            Controls.Add(_contentPanel);
        }

        private void CreateFooterPanel()
        {
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = FooterHeight, // Now configurable and smaller (50px)
                BackColor = FooterColor,
                Padding = new Padding(15, 10, 15, 10), // Reduced padding
                Visible = ShowFooter
            };

            Controls.Add(_footerPanel);
        }

        private void SetupFadeAnimation()
        {
            if (!UseAnimations) return;

            Opacity = 0;
            _fadeTimer = new Timer { Interval = 10 }; // Reduced from 20ms for faster animation
            _fadeTimer.Tick += OnFadeTimerTick;
            _fadeTimer.Start();
        }
        #endregion

        #region Event Handlers
        private void OnFadeTimerTick(object sender, EventArgs e)
        {
            if (_isClosing)
            {
                Opacity -= AnimationSpeed; // Now configurable (0.15 vs 0.05)
                if (Opacity <= 0)
                {
                    _fadeTimer.Stop();
                    base.Close();
                }
            }
            else
            {
                Opacity += AnimationSpeed; // Now configurable (0.15 vs 0.05)
                if (Opacity >= 1)
                {
                    _fadeTimer.Stop();
                }
            }
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            OnCloseRequested();
        }

        private void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && AllowDragging)
            {
                _isDragging = true;
                _lastMousePos = e.Location;
            }
        }

        private void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Location = new Point(
                    Location.X + e.X - _lastMousePos.X,
                    Location.Y + e.Y - _lastMousePos.Y);
            }
        }
        #endregion

        #region Virtual Methods
        protected virtual void OnCloseRequested()
        {
            CloseWithAnimation();
        }

        protected virtual void OnInitializeContent()
        {
            // Override in derived classes to add content
        }
        #endregion

        #region Public Methods
        public void UpdateTitle(string title, string icon = null)
        {
            FormTitle = title;
            if (icon != null) TitleIcon = icon;
            _titleLabel.Text = $"{TitleIcon} {FormTitle}";
        }

        public void CloseWithAnimation()
        {
            if (UseAnimations)
            {
                _isClosing = true;
                _fadeTimer?.Start();
            }
            else
            {
                Close();
            }
        }

        public void ShowTemporaryMessage(string message, Color? backgroundColor = null, int durationMs = 2000)
        {
            var messageLabel = new Label
            {
                Text = message,
                AutoSize = true,
                BackColor = backgroundColor ?? Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(8, 4, 8, 4), // Reduced padding
                Location = new Point(15, ContentPanel.Height - 40) // Adjusted positioning
            };

            ContentPanel.Controls.Add(messageLabel);
            messageLabel.BringToFront();

            var messageTimer = new Timer { Interval = durationMs };
            messageTimer.Tick += (s, e) =>
            {
                ContentPanel.Controls.Remove(messageLabel);
                messageTimer.Stop();
                messageTimer.Dispose();
            };
            messageTimer.Start();
        }
        #endregion

        #region Overrides
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Position close button after form is loaded
            if (_closeButton != null)
            {
                _closeButton.Location = new Point(_headerPanel.Width - 30, 6); // Adjusted for smaller header
            }

            OnInitializeContent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isClosing && UseAnimations)
            {
                e.Cancel = true;
                CloseWithAnimation();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw subtle drop shadow
            var shadowRect = new Rectangle(3, 3, Width - 6, Height - 6); // Reduced shadow offset
            using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0))) // Lighter shadow
            {
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }

            // Draw main form with rounded corners
            using (var path = GetRoundedRectanglePath(ClientRectangle, CornerRadius))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fadeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Helper Methods
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
        #endregion
    }
}

