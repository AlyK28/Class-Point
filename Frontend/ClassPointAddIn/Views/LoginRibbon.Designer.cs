

using Microsoft.Office.Tools.Ribbon;

namespace ClassPointAddIn.Views
{
    partial class ConnectRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ConnectRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectRibbon));
            this.ConnectTab = this.Factory.CreateRibbonTab();
            this.ConnectGroup = this.Factory.CreateRibbonGroup();
            this.Connect = this.Factory.CreateRibbonButton();
            this.ConnectTab.SuspendLayout();
            this.ConnectGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnectTab
            // 
            this.ConnectTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.ConnectTab.Groups.Add(this.ConnectGroup);
            this.ConnectTab.Label = "ClassPoint";
            this.ConnectTab.Name = "ConnectTab";
            // 
            // ConnectGroup
            // 
            this.ConnectGroup.Items.Add(this.Connect);
            this.ConnectGroup.Name = "ConnectGroup";
            // 
            // Connect
            // 
            this.Connect.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Connect.Description = "Login Or Register to Class Api";
            this.Connect.Image = ((System.Drawing.Image)(resources.GetObject("Connect.Image")));
            this.Connect.ImageName = "Connect";
            this.Connect.Label = "Connect";
            this.Connect.Name = "Connect";
            this.Connect.ShowImage = true;
            this.Connect.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Connect_Click);
            // 
            // ConnectRibbon
            // 
            this.Name = "ConnectRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.ConnectTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ConnectRibbon_Load);
            this.ConnectTab.ResumeLayout(false);
            this.ConnectTab.PerformLayout();
            this.ConnectGroup.ResumeLayout(false);
            this.ConnectGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ConnectTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ConnectGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton Connect;
    }

    partial class ThisRibbonCollection
    {
        internal ConnectRibbon LoginRibbon
        {
            get { return this.GetRibbon<ConnectRibbon>(); }
        }

        private T GetRibbon<T>() where T : RibbonBase
        {
            return Globals.Ribbons.GetRibbon<T>();
        }
    }
}
