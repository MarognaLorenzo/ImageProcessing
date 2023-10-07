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
            this.applyButton = new System.Windows.Forms.Button();
            this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveButton = new System.Windows.Forms.Button();
            this.pictureBoxOut = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.ApplyButton2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.DilateButton = new System.Windows.Forms.Button();
            this.LoadImage2Button = new System.Windows.Forms.Button();
            this.image2FileName = new System.Windows.Forms.TextBox();
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
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(629, -2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(4);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(132, 27);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "PipelineW";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ClickPipilineW);
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
            // ApplyButton2
            // 
            this.ApplyButton2.Location = new System.Drawing.Point(629, 98);
            this.ApplyButton2.Margin = new System.Windows.Forms.Padding(4);
            this.ApplyButton2.Name = "ApplyButton2";
            this.ApplyButton2.Size = new System.Drawing.Size(132, 27);
            this.ApplyButton2.TabIndex = 7;
            this.ApplyButton2.Text = "PipelineZ";
            this.ApplyButton2.UseVisualStyleBackColor = true;
            this.ApplyButton2.Click += new System.EventHandler(this.ClickPipelineZ);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(629, 132);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 27);
            this.button1.TabIndex = 8;
            this.button1.Text = "Detect Areas (Flood Fill)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ClickFloodFillButton);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(629, 166);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(132, 27);
            this.button2.TabIndex = 9;
            this.button2.Text = "Largest object";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ClickLargestButton);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(630, 63);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(132, 27);
            this.button3.TabIndex = 10;
            this.button3.Text = "Erode";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ClickErodeButton);
            // 
            // DilateButton
            // 
            this.DilateButton.Location = new System.Drawing.Point(630, 34);
            this.DilateButton.Margin = new System.Windows.Forms.Padding(4);
            this.DilateButton.Name = "DilateButton";
            this.DilateButton.Size = new System.Drawing.Size(132, 27);
            this.DilateButton.TabIndex = 11;
            this.DilateButton.Text = "Dilate";
            this.DilateButton.UseVisualStyleBackColor = true;
            this.DilateButton.Click += new System.EventHandler(this.ClickDilateButton);
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
            // INFOIBV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 1053);
            this.Controls.Add(this.image2FileName);
            this.Controls.Add(this.LoadImageButton);
            this.Controls.Add(this.LoadImage2Button);
            this.Controls.Add(this.DilateButton);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ApplyButton2);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.pictureBoxOut);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.applyButton);
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
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.SaveFileDialog saveImageDialog;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.PictureBox pictureBoxOut;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button ApplyButton2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button DilateButton;
        private System.Windows.Forms.TextBox image2FileName;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

