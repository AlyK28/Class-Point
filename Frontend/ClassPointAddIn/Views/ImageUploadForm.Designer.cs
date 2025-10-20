namespace ClassPointAddIn.Views
{
    partial class ImageUploadForm
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
            this.grpSessions = new System.Windows.Forms.GroupBox();
            this.lstSessions = new System.Windows.Forms.ListView();
            this.colSessionName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSessionCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSubmissions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCreated = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblSelectedSession = new System.Windows.Forms.Label();
            this.btnCreateSession = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.grpUpload = new System.Windows.Forms.GroupBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.txtStudentName = new System.Windows.Forms.TextBox();
            this.lblStudentName = new System.Windows.Forms.Label();
            this.btnSelectImage = new System.Windows.Forms.Button();
            this.txtImagePath = new System.Windows.Forms.TextBox();
            this.lblImagePath = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnViewSubmissions = new System.Windows.Forms.Button();
            this.grpSessions.SuspendLayout();
            this.grpUpload.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ClassPoint Image Upload";
            
            // 
            // grpSessions
            // 
            this.grpSessions.Controls.Add(this.btnViewSubmissions);
            this.grpSessions.Controls.Add(this.btnRefresh);
            this.grpSessions.Controls.Add(this.btnCreateSession);
            this.grpSessions.Controls.Add(this.lblSelectedSession);
            this.grpSessions.Controls.Add(this.lstSessions);
            this.grpSessions.Location = new System.Drawing.Point(12, 50);
            this.grpSessions.Name = "grpSessions";
            this.grpSessions.Size = new System.Drawing.Size(600, 300);
            this.grpSessions.TabIndex = 1;
            this.grpSessions.TabStop = false;
            this.grpSessions.Text = "Active Sessions";
            
            // 
            // lstSessions
            // 
            this.lstSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSessionName,
            this.colSessionCode,
            this.colSubmissions,
            this.colCreated});
            this.lstSessions.FullRowSelect = true;
            this.lstSessions.GridLines = true;
            this.lstSessions.Location = new System.Drawing.Point(6, 19);
            this.lstSessions.MultiSelect = false;
            this.lstSessions.Name = "lstSessions";
            this.lstSessions.Size = new System.Drawing.Size(588, 200);
            this.lstSessions.TabIndex = 0;
            this.lstSessions.UseCompatibleStateImageBehavior = false;
            this.lstSessions.View = System.Windows.Forms.View.Details;
            this.lstSessions.SelectedIndexChanged += new System.EventHandler(this.lstSessions_SelectedIndexChanged);
            
            // 
            // colSessionName
            // 
            this.colSessionName.Text = "Session Name";
            this.colSessionName.Width = 200;
            
            // 
            // colSessionCode
            // 
            this.colSessionCode.Text = "Code";
            this.colSessionCode.Width = 80;
            
            // 
            // colSubmissions
            // 
            this.colSubmissions.Text = "Submissions";
            this.colSubmissions.Width = 80;
            
            // 
            // colCreated
            // 
            this.colCreated.Text = "Created";
            this.colCreated.Width = 150;
            
            // 
            // lblSelectedSession
            // 
            this.lblSelectedSession.AutoSize = true;
            this.lblSelectedSession.Location = new System.Drawing.Point(6, 230);
            this.lblSelectedSession.Name = "lblSelectedSession";
            this.lblSelectedSession.Size = new System.Drawing.Size(100, 13);
            this.lblSelectedSession.TabIndex = 1;
            this.lblSelectedSession.Text = "No session selected";
            
            // 
            // btnCreateSession
            // 
            this.btnCreateSession.Location = new System.Drawing.Point(6, 250);
            this.btnCreateSession.Name = "btnCreateSession";
            this.btnCreateSession.Size = new System.Drawing.Size(100, 30);
            this.btnCreateSession.TabIndex = 2;
            this.btnCreateSession.Text = "Create Session";
            this.btnCreateSession.UseVisualStyleBackColor = true;
            this.btnCreateSession.Click += new System.EventHandler(this.btnCreateSession_Click);
            
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(112, 250);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 30);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // 
            // btnViewSubmissions
            // 
            this.btnViewSubmissions.Enabled = false;
            this.btnViewSubmissions.Location = new System.Drawing.Point(193, 250);
            this.btnViewSubmissions.Name = "btnViewSubmissions";
            this.btnViewSubmissions.Size = new System.Drawing.Size(120, 30);
            this.btnViewSubmissions.TabIndex = 4;
            this.btnViewSubmissions.Text = "View Submissions";
            this.btnViewSubmissions.UseVisualStyleBackColor = true;
            this.btnViewSubmissions.Click += new System.EventHandler(this.btnViewSubmissions_Click);
            
            // 
            // grpUpload
            // 
            this.grpUpload.Controls.Add(this.picPreview);
            this.grpUpload.Controls.Add(this.txtStudentName);
            this.grpUpload.Controls.Add(this.lblStudentName);
            this.grpUpload.Controls.Add(this.btnSelectImage);
            this.grpUpload.Controls.Add(this.txtImagePath);
            this.grpUpload.Controls.Add(this.lblImagePath);
            this.grpUpload.Controls.Add(this.btnUpload);
            this.grpUpload.Location = new System.Drawing.Point(12, 370);
            this.grpUpload.Name = "grpUpload";
            this.grpUpload.Size = new System.Drawing.Size(600, 200);
            this.grpUpload.TabIndex = 2;
            this.grpUpload.TabStop = false;
            this.grpUpload.Text = "Upload Image";
            
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(400, 50);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(150, 150);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 6;
            this.picPreview.TabStop = false;
            
            // 
            // txtStudentName
            // 
            this.txtStudentName.Location = new System.Drawing.Point(100, 80);
            this.txtStudentName.Name = "txtStudentName";
            this.txtStudentName.Size = new System.Drawing.Size(200, 20);
            this.txtStudentName.TabIndex = 5;
            
            // 
            // lblStudentName
            // 
            this.lblStudentName.AutoSize = true;
            this.lblStudentName.Location = new System.Drawing.Point(6, 83);
            this.lblStudentName.Name = "lblStudentName";
            this.lblStudentName.Size = new System.Drawing.Size(75, 13);
            this.lblStudentName.TabIndex = 4;
            this.lblStudentName.Text = "Student Name:";
            
            // 
            // btnSelectImage
            // 
            this.btnSelectImage.Location = new System.Drawing.Point(320, 50);
            this.btnSelectImage.Name = "btnSelectImage";
            this.btnSelectImage.Size = new System.Drawing.Size(75, 23);
            this.btnSelectImage.TabIndex = 3;
            this.btnSelectImage.Text = "Browse...";
            this.btnSelectImage.UseVisualStyleBackColor = true;
            this.btnSelectImage.Click += new System.EventHandler(this.btnSelectImage_Click);
            
            // 
            // txtImagePath
            // 
            this.txtImagePath.Location = new System.Drawing.Point(100, 52);
            this.txtImagePath.Name = "txtImagePath";
            this.txtImagePath.ReadOnly = true;
            this.txtImagePath.Size = new System.Drawing.Size(214, 20);
            this.txtImagePath.TabIndex = 2;
            
            // 
            // lblImagePath
            // 
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.Location = new System.Drawing.Point(6, 55);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new System.Drawing.Size(64, 13);
            this.lblImagePath.TabIndex = 1;
            this.lblImagePath.Text = "Image File:";
            
            // 
            // btnUpload
            // 
            this.btnUpload.Enabled = false;
            this.btnUpload.Location = new System.Drawing.Point(100, 120);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(100, 30);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload Image";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            
            // 
            // ImageUploadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 582);
            this.Controls.Add(this.grpUpload);
            this.Controls.Add(this.grpSessions);
            this.Controls.Add(this.lblTitle);
            this.Name = "ImageUploadForm";
            this.Text = "ClassPoint Image Upload";
            this.Load += new System.EventHandler(this.ImageUploadForm_Load);
            this.grpSessions.ResumeLayout(false);
            this.grpSessions.PerformLayout();
            this.grpUpload.ResumeLayout(false);
            this.grpUpload.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpSessions;
        private System.Windows.Forms.ListView lstSessions;
        private System.Windows.Forms.ColumnHeader colSessionName;
        private System.Windows.Forms.ColumnHeader colSessionCode;
        private System.Windows.Forms.ColumnHeader colSubmissions;
        private System.Windows.Forms.ColumnHeader colCreated;
        private System.Windows.Forms.Label lblSelectedSession;
        private System.Windows.Forms.Button btnCreateSession;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnViewSubmissions;
        private System.Windows.Forms.GroupBox grpUpload;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.TextBox txtStudentName;
        private System.Windows.Forms.Label lblStudentName;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Label lblImagePath;
        private System.Windows.Forms.Button btnUpload;
    }
}
