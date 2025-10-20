namespace ClassPointAddIn.Views
{
    partial class TestForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnTestLogin = new System.Windows.Forms.Button();
            this.btnOpenImageUpload = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ClassPoint Image Upload Test";
            
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = true;
            this.lblInstructions.Location = new System.Drawing.Point(12, 50);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(400, 39);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.Text = "1. Make sure the Django backend is running on http://localhost:8000\r\n2. Click 'Test Login' to authenticate\r\n3. Click 'Open Image Upload' to test the functionality";
            
            // 
            // btnTestLogin
            // 
            this.btnTestLogin.Location = new System.Drawing.Point(12, 110);
            this.btnTestLogin.Name = "btnTestLogin";
            this.btnTestLogin.Size = new System.Drawing.Size(120, 40);
            this.btnTestLogin.TabIndex = 2;
            this.btnTestLogin.Text = "Test Login\r\n(admin/admin123)";
            this.btnTestLogin.UseVisualStyleBackColor = true;
            this.btnTestLogin.Click += new System.EventHandler(this.btnTestLogin_Click);
            
            // 
            // btnOpenImageUpload
            // 
            this.btnOpenImageUpload.Enabled = false;
            this.btnOpenImageUpload.Location = new System.Drawing.Point(150, 110);
            this.btnOpenImageUpload.Name = "btnOpenImageUpload";
            this.btnOpenImageUpload.Size = new System.Drawing.Size(120, 40);
            this.btnOpenImageUpload.TabIndex = 3;
            this.btnOpenImageUpload.Text = "Open Image\r\nUpload Form";
            this.btnOpenImageUpload.UseVisualStyleBackColor = true;
            this.btnOpenImageUpload.Click += new System.EventHandler(this.btnOpenImageUpload_Click);
            
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(288, 110);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 40);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 170);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpenImageUpload);
            this.Controls.Add(this.btnTestLogin);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.lblTitle);
            this.Name = "TestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClassPoint Test";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Button btnTestLogin;
        private System.Windows.Forms.Button btnOpenImageUpload;
        private System.Windows.Forms.Button btnClose;
    }
}
