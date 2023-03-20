namespace MetalManager
{
    partial class StartupScanForm
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
            this.explainLabel = new System.Windows.Forms.Label();
            this.startupMMInfoLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BfGStartupWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // explainLabel
            // 
            this.explainLabel.Location = new System.Drawing.Point(91, 10);
            this.explainLabel.Name = "explainLabel";
            this.explainLabel.Size = new System.Drawing.Size(219, 54);
            this.explainLabel.TabIndex = 0;
            this.explainLabel.Text = "Verifying format integrity for new files...";
            // 
            // startupMMInfoLabel
            // 
            this.startupMMInfoLabel.AutoSize = true;
            this.startupMMInfoLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.startupMMInfoLabel.Location = new System.Drawing.Point(197, 64);
            this.startupMMInfoLabel.Name = "startupMMInfoLabel";
            this.startupMMInfoLabel.Size = new System.Drawing.Size(111, 13);
            this.startupMMInfoLabel.TabIndex = 3;
            this.startupMMInfoLabel.Text = "Metal Manager v1.0.0";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MetalManager.Properties.Resources.mm128;
            this.pictureBox1.Location = new System.Drawing.Point(4, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(77, 77);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // BfGStartupWorker
            // 
            this.BfGStartupWorker.WorkerReportsProgress = true;
            this.BfGStartupWorker.WorkerSupportsCancellation = true;
            this.BfGStartupWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BfGStartupWorker_DoWork);
            this.BfGStartupWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BfGStartupWorker_ProgressChanged);
            this.BfGStartupWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BfGStartupWorker_RunWorkerCompleted);
            // 
            // StartupScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 81);
            this.ControlBox = false;
            this.Controls.Add(this.startupMMInfoLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.explainLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartupScanForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.StartupScanForm_Load);
            this.Shown += new System.EventHandler(this.StartupScanShown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label explainLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label startupMMInfoLabel;
        private System.ComponentModel.BackgroundWorker BfGStartupWorker;
    }
}