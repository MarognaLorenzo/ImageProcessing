namespace INFOIBV
{
    partial class INFOIBV
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LoadImageButton = new System.Windows.Forms.Button();
            this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageFileName = new System.Windows.Forms.TextBox();
            this.pictureBoxIn1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxIn2 = new System.Windows.Forms.PictureBox();
            this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveButton = new System.Windows.Forms.Button();
            this.pictureBoxOut = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.HoughLineDetectionButton = new System.Windows.Forms.Button();
            this.ThreshVisualizeButton = new System.Windows.Forms.Button();
            this.CloseVisualizeButton = new System.Windows.Forms.Button();
            this.LineButton = new System.Windows.Forms.Button();
            this.Hough = new System.Windows.Forms.Button();
            this.LoadImage2Button = new System.Windows.Forms.Button();
            this.image2FileName = new System.Windows.Forms.TextBox();
            this.HoughCirclesButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.NumberBox = new System.Windows.Forms.TextBox();
            this.HTAngleLimit = new System.Windows.Forms.Button();
            this.FindSocketButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOut)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadImageButton
            // 
            this.LoadImageButton.Location = new System.Drawing.Point(16, 15);
            this.LoadImageButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoadImageButton.Name = "LoadImageButton";
            this.LoadImageButton.Size = new System.Drawing.Size(131, 28);
            this.LoadImageButton.TabIndex = 0;
            this.LoadImageButton.Text = "Load image...";
            this.LoadImageButton.UseVisualStyleBackColor = true;
            this.LoadImageButton.Click += new System.EventHandler(this.loadImageButton_Click);
            // 
            // openImageDialog
            // 
            this.openImageDialog.Filter = "Bitmap files (*.bmp;*.gif;*.jpg;*.png;*.tiff;*.jpeg)|*.bmp;*.gif;*.jpg;*.png;*.ti" +
    "ff;*.jpeg";
            this.openImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // imageFileName
            // 
            this.imageFileName.Location = new System.Drawing.Point(155, 17);
            this.imageFileName.Margin = new System.Windows.Forms.Padding(4);
            this.imageFileName.Name = "imageFileName";
            this.imageFileName.ReadOnly = true;
            this.imageFileName.Size = new System.Drawing.Size(420, 22);
            this.imageFileName.TabIndex = 1;
            // 
            // pictureBoxIn1
            // 
            this.pictureBoxIn1.Location = new System.Drawing.Point(13, 198);
            this.pictureBoxIn1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxIn1.Name = "pictureBoxIn1";
            this.pictureBoxIn1.Size = new System.Drawing.Size(612, 426);
            this.pictureBoxIn1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxIn1.TabIndex = 2;
            this.pictureBoxIn1.TabStop = false;
            // 
            // pictureBoxIn2
            // 
            this.pictureBoxIn2.Location = new System.Drawing.Point(13, 632);
            this.pictureBoxIn2.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxIn2.Name = "pictureBoxIn2";
            this.pictureBoxIn2.Size = new System.Drawing.Size(612, 408);
            this.pictureBoxIn2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxIn2.TabIndex = 14;
            this.pictureBoxIn2.TabStop = false;
            // 
            // saveImageDialog
            // 
            this.saveImageDialog.Filter = "Bitmap file (*.bmp)|*.bmp";
            this.saveImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(1264, 14);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(127, 28);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save as BMP...";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // pictureBoxOut
            // 
            this.pictureBoxOut.Location = new System.Drawing.Point(705, 198);
            this.pictureBoxOut.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxOut.Name = "pictureBoxOut";
            this.pictureBoxOut.Size = new System.Drawing.Size(628, 426);
            this.pictureBoxOut.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxOut.TabIndex = 5;
            this.pictureBoxOut.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(865, 18);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(368, 25);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // HoughLineDetectionButton
            // 
            this.HoughLineDetectionButton.Location = new System.Drawing.Point(629, 98);
            this.HoughLineDetectionButton.Margin = new System.Windows.Forms.Padding(4);
            this.HoughLineDetectionButton.Name = "HoughLineDetectionButton";
            this.HoughLineDetectionButton.Size = new System.Drawing.Size(132, 27);
            this.HoughLineDetectionButton.TabIndex = 7;
            this.HoughLineDetectionButton.Text = "houge_line_detection";
            this.HoughLineDetectionButton.UseVisualStyleBackColor = true;
            this.HoughLineDetectionButton.Click += new System.EventHandler(this.houghLineDetectionClick);
            // 
            // ThreshVisualizeButton
            // 
            this.ThreshVisualizeButton.Location = new System.Drawing.Point(629, 163);
            this.ThreshVisualizeButton.Margin = new System.Windows.Forms.Padding(4);
            this.ThreshVisualizeButton.Name = "ThreshVisualizeButton";
            this.ThreshVisualizeButton.Size = new System.Drawing.Size(132, 27);
            this.ThreshVisualizeButton.TabIndex = 8;
            this.ThreshVisualizeButton.Text = "ThresholdVisualize";
            this.ThreshVisualizeButton.UseVisualStyleBackColor = true;
            this.ThreshVisualizeButton.Click += new System.EventHandler(this.showStudyThresholdClick);
            // 
            // CloseVisualizeButton
            // 
            this.CloseVisualizeButton.Location = new System.Drawing.Point(769, 163);
            this.CloseVisualizeButton.Margin = new System.Windows.Forms.Padding(4);
            this.CloseVisualizeButton.Name = "CloseVisualizeButton";
            this.CloseVisualizeButton.Size = new System.Drawing.Size(132, 27);
            this.CloseVisualizeButton.TabIndex = 9;
            this.CloseVisualizeButton.Text = "CloseVisualize";
            this.CloseVisualizeButton.UseVisualStyleBackColor = true;
            this.CloseVisualizeButton.Click += new System.EventHandler(this.showStudyCloseImageClick);
            // 
            // LineButton
            // 
            this.LineButton.Location = new System.Drawing.Point(630, 63);
            this.LineButton.Margin = new System.Windows.Forms.Padding(4);
            this.LineButton.Name = "LineButton";
            this.LineButton.Size = new System.Drawing.Size(132, 27);
            this.LineButton.TabIndex = 10;
            this.LineButton.Text = "Line Detection";
            this.LineButton.UseVisualStyleBackColor = true;
            this.LineButton.Click += new System.EventHandler(this.LineDetectionClick);
            // 
            // Hough
            // 
            this.Hough.Location = new System.Drawing.Point(630, 34);
            this.Hough.Margin = new System.Windows.Forms.Padding(4);
            this.Hough.Name = "Hough";
            this.Hough.Size = new System.Drawing.Size(132, 27);
            this.Hough.TabIndex = 11;
            this.Hough.Text = "HoughTransform";
            this.Hough.UseVisualStyleBackColor = true;
            this.Hough.Click += new System.EventHandler(this.houghTransformClick);
            // 
            // LoadImage2Button
            // 
            this.LoadImage2Button.Location = new System.Drawing.Point(16, 63);
            this.LoadImage2Button.Margin = new System.Windows.Forms.Padding(4);
            this.LoadImage2Button.Name = "LoadImage2Button";
            this.LoadImage2Button.Size = new System.Drawing.Size(131, 28);
            this.LoadImage2Button.TabIndex = 12;
            this.LoadImage2Button.Text = "Load image 2...";
            this.LoadImage2Button.UseVisualStyleBackColor = true;
            this.LoadImage2Button.Click += new System.EventHandler(this.loadImageButton2_Click);
            // 
            // image2FileName
            // 
            this.image2FileName.Location = new System.Drawing.Point(155, 69);
            this.image2FileName.Margin = new System.Windows.Forms.Padding(4);
            this.image2FileName.Name = "image2FileName";
            this.image2FileName.ReadOnly = true;
            this.image2FileName.Size = new System.Drawing.Size(420, 22);
            this.image2FileName.TabIndex = 13;
            // 
            // HoughCirclesButton
            // 
            this.HoughCirclesButton.Location = new System.Drawing.Point(769, 98);
            this.HoughCirclesButton.Margin = new System.Windows.Forms.Padding(4);
            this.HoughCirclesButton.Name = "HoughCirclesButton";
            this.HoughCirclesButton.Size = new System.Drawing.Size(132, 27);
            this.HoughCirclesButton.TabIndex = 15;
            this.HoughCirclesButton.Text = "HoughCircles";
            this.HoughCirclesButton.UseVisualStyleBackColor = true;
            this.HoughCirclesButton.Click += new System.EventHandler(this.HoughCirclesClick);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(630, 128);
            this.CloseButton.Margin = new System.Windows.Forms.Padding(4);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(132, 27);
            this.CloseButton.TabIndex = 18;
            this.CloseButton.Text = "EdgeDetection";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.edgedetectionClick);
            // 
            // NumberBox
            // 
            this.NumberBox.Location = new System.Drawing.Point(909, 102);
            this.NumberBox.Name = "NumberBox";
            this.NumberBox.Size = new System.Drawing.Size(100, 22);
            this.NumberBox.TabIndex = 19;
            this.NumberBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumberBox_KeyPress);
            // 
            // HTAngleLimit
            // 
            this.HTAngleLimit.Location = new System.Drawing.Point(770, 128);
            this.HTAngleLimit.Margin = new System.Windows.Forms.Padding(4);
            this.HTAngleLimit.Name = "HTAngleLimit";
            this.HTAngleLimit.Size = new System.Drawing.Size(132, 27);
            this.HTAngleLimit.TabIndex = 20;
            this.HTAngleLimit.Text = "HTAngleLimit";
            this.HTAngleLimit.UseVisualStyleBackColor = true;
            this.HTAngleLimit.Click += new System.EventHandler(this.houghTransformAngleLimitClick);
            // 
            // FindSocketButton
            // 
            this.FindSocketButton.Location = new System.Drawing.Point(490, 128);
            this.FindSocketButton.Margin = new System.Windows.Forms.Padding(4);
            this.FindSocketButton.Name = "FindSocketButton";
            this.FindSocketButton.Size = new System.Drawing.Size(132, 27);
            this.FindSocketButton.TabIndex = 21;
            this.FindSocketButton.Text = "FindSocket";
            this.FindSocketButton.UseVisualStyleBackColor = true;
            this.FindSocketButton.Click += new System.EventHandler(this.FindSocketClick);
            // 
            // INFOIBV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 1053);
            this.Controls.Add(this.FindSocketButton);
            this.Controls.Add(this.HTAngleLimit);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.NumberBox);
            this.Controls.Add(this.HoughCirclesButton);
            this.Controls.Add(this.image2FileName);
            this.Controls.Add(this.LoadImageButton);
            this.Controls.Add(this.LoadImage2Button);
            this.Controls.Add(this.Hough);
            this.Controls.Add(this.LineButton);
            this.Controls.Add(this.CloseVisualizeButton);
            this.Controls.Add(this.ThreshVisualizeButton);
            this.Controls.Add(this.HoughLineDetectionButton);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.pictureBoxOut);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.pictureBoxIn1);
            this.Controls.Add(this.pictureBoxIn2);
            this.Controls.Add(this.imageFileName);
            this.Location = new System.Drawing.Point(10, 10);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "INFOIBV";
            this.ShowIcon = false;
            this.Text = "INFOIBV";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOut)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadImageButton;
        private System.Windows.Forms.Button LoadImage2Button;
        private System.Windows.Forms.OpenFileDialog openImageDialog;
        private System.Windows.Forms.TextBox imageFileName;
        private System.Windows.Forms.PictureBox pictureBoxIn1;
        private System.Windows.Forms.PictureBox pictureBoxIn2;
        private System.Windows.Forms.SaveFileDialog saveImageDialog;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.PictureBox pictureBoxOut;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button HoughLineDetectionButton;
        private System.Windows.Forms.Button ThreshVisualizeButton;
        private System.Windows.Forms.Button CloseVisualizeButton;
        private System.Windows.Forms.Button LineButton;
        private System.Windows.Forms.Button Hough;
        private System.Windows.Forms.TextBox image2FileName;
        private System.Windows.Forms.Button HoughCirclesButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.TextBox NumberBox;
        private System.Windows.Forms.Button HTAngleLimit;
        private System.Windows.Forms.Button FindSocketButton;
    }
}
