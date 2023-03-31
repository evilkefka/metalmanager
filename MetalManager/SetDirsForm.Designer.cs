namespace MetalManager
{
    partial class SetDirsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetDirsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pazBuddyIcon = new System.Windows.Forms.PictureBox();
            this.mod_prsEntrLabel = new System.Windows.Forms.Label();
            this.modDirFlagLabel = new System.Windows.Forms.Label();
            this.ModDirLabel = new System.Windows.Forms.Label();
            this.browseModDirBtn = new System.Windows.Forms.Button();
            this.modDirTextbox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.game_prsEntrLabel = new System.Windows.Forms.Label();
            this.gameDirFlagLabel = new System.Windows.Forms.Label();
            this.dontLinkGame = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.browseGameDirBtn = new System.Windows.Forms.Button();
            this.gameDirTextbox = new System.Windows.Forms.TextBox();
            this.confirmDirsBtn = new System.Windows.Forms.Button();
            this.confirmLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.startupMMInfoLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pazBuddyIcon)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pazBuddyIcon);
            this.groupBox1.Controls.Add(this.mod_prsEntrLabel);
            this.groupBox1.Controls.Add(this.modDirFlagLabel);
            this.groupBox1.Controls.Add(this.ModDirLabel);
            this.groupBox1.Controls.Add(this.browseModDirBtn);
            this.groupBox1.Controls.Add(this.modDirTextbox);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(446, 124);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mod Directory";
            // 
            // pazBuddyIcon
            // 
            this.pazBuddyIcon.BackgroundImage = global::MetalManager.Properties.Resources.Paz;
            this.pazBuddyIcon.Location = new System.Drawing.Point(345, 7);
            this.pazBuddyIcon.Margin = new System.Windows.Forms.Padding(0);
            this.pazBuddyIcon.Name = "pazBuddyIcon";
            this.pazBuddyIcon.Size = new System.Drawing.Size(58, 60);
            this.pazBuddyIcon.TabIndex = 5;
            this.pazBuddyIcon.TabStop = false;
            // 
            // mod_prsEntrLabel
            // 
            this.mod_prsEntrLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mod_prsEntrLabel.Location = new System.Drawing.Point(179, 96);
            this.mod_prsEntrLabel.Name = "mod_prsEntrLabel";
            this.mod_prsEntrLabel.Size = new System.Drawing.Size(181, 15);
            this.mod_prsEntrLabel.TabIndex = 4;
            this.mod_prsEntrLabel.Text = "Press Enter/Return when complete";
            this.mod_prsEntrLabel.Visible = false;
            // 
            // modDirFlagLabel
            // 
            this.modDirFlagLabel.Location = new System.Drawing.Point(42, 56);
            this.modDirFlagLabel.Name = "modDirFlagLabel";
            this.modDirFlagLabel.Size = new System.Drawing.Size(314, 16);
            this.modDirFlagLabel.TabIndex = 3;
            this.modDirFlagLabel.Text = "No directory set";
            // 
            // ModDirLabel
            // 
            this.ModDirLabel.Location = new System.Drawing.Point(18, 20);
            this.ModDirLabel.Name = "ModDirLabel";
            this.ModDirLabel.Size = new System.Drawing.Size(411, 17);
            this.ModDirLabel.TabIndex = 2;
            this.ModDirLabel.Text = "Please select or create a folder to store your custom songs";
            // 
            // browseModDirBtn
            // 
            this.browseModDirBtn.Location = new System.Drawing.Point(362, 72);
            this.browseModDirBtn.Name = "browseModDirBtn";
            this.browseModDirBtn.Size = new System.Drawing.Size(67, 23);
            this.browseModDirBtn.TabIndex = 1;
            this.browseModDirBtn.Text = "Browse...";
            this.browseModDirBtn.UseVisualStyleBackColor = true;
            this.browseModDirBtn.Click += new System.EventHandler(this.SetModFolderDialogue);
            // 
            // modDirTextbox
            // 
            this.modDirTextbox.Location = new System.Drawing.Point(45, 74);
            this.modDirTextbox.Name = "modDirTextbox";
            this.modDirTextbox.Size = new System.Drawing.Size(311, 20);
            this.modDirTextbox.TabIndex = 2;
            this.modDirTextbox.Text = "Copy directory and Paste it here, or click Browse...";
            this.modDirTextbox.Enter += new System.EventHandler(this.DirTboxEnter);
            this.modDirTextbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DirTboxKeyDown);
            this.modDirTextbox.LostFocus += new System.EventHandler(this.dirTboxLostFocus);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.game_prsEntrLabel);
            this.groupBox2.Controls.Add(this.gameDirFlagLabel);
            this.groupBox2.Controls.Add(this.dontLinkGame);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.browseGameDirBtn);
            this.groupBox2.Controls.Add(this.gameDirTextbox);
            this.groupBox2.Location = new System.Drawing.Point(13, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(446, 124);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Directory";
            // 
            // game_prsEntrLabel
            // 
            this.game_prsEntrLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.game_prsEntrLabel.Location = new System.Drawing.Point(179, 102);
            this.game_prsEntrLabel.Name = "game_prsEntrLabel";
            this.game_prsEntrLabel.Size = new System.Drawing.Size(181, 15);
            this.game_prsEntrLabel.TabIndex = 5;
            this.game_prsEntrLabel.Text = "Press Enter/Return when complete";
            this.game_prsEntrLabel.Visible = false;
            // 
            // gameDirFlagLabel
            // 
            this.gameDirFlagLabel.Location = new System.Drawing.Point(42, 62);
            this.gameDirFlagLabel.Name = "gameDirFlagLabel";
            this.gameDirFlagLabel.Size = new System.Drawing.Size(314, 16);
            this.gameDirFlagLabel.TabIndex = 4;
            this.gameDirFlagLabel.Text = "No directory set";
            // 
            // dontLinkGame
            // 
            this.dontLinkGame.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.dontLinkGame.Location = new System.Drawing.Point(45, 40);
            this.dontLinkGame.Name = "dontLinkGame";
            this.dontLinkGame.Size = new System.Drawing.Size(384, 17);
            this.dontLinkGame.TabIndex = 5;
            this.dontLinkGame.Text = "Continue without setting game directory (not recommended)";
            this.dontLinkGame.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.dontLinkGame.UseVisualStyleBackColor = true;
            this.dontLinkGame.CheckedChanged += new System.EventHandler(this.dontLinkGameChkChng);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(18, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(411, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Please select your game directory or StreamingAssets folder for Metal: Hellsinger" +
    "";
            // 
            // browseGameDirBtn
            // 
            this.browseGameDirBtn.Location = new System.Drawing.Point(362, 78);
            this.browseGameDirBtn.Name = "browseGameDirBtn";
            this.browseGameDirBtn.Size = new System.Drawing.Size(67, 23);
            this.browseGameDirBtn.TabIndex = 3;
            this.browseGameDirBtn.Text = "Browse...";
            this.browseGameDirBtn.UseVisualStyleBackColor = true;
            this.browseGameDirBtn.Click += new System.EventHandler(this.SetGameFolderDialogue);
            // 
            // gameDirTextbox
            // 
            this.gameDirTextbox.Location = new System.Drawing.Point(45, 80);
            this.gameDirTextbox.Name = "gameDirTextbox";
            this.gameDirTextbox.Size = new System.Drawing.Size(311, 20);
            this.gameDirTextbox.TabIndex = 3;
            this.gameDirTextbox.Text = "Copy directory and Paste it here, or click Browse...";
            this.gameDirTextbox.Enter += new System.EventHandler(this.DirTboxEnter);
            this.gameDirTextbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DirTboxKeyDown);
            this.gameDirTextbox.LostFocus += new System.EventHandler(this.dirTboxLostFocus);
            // 
            // confirmDirsBtn
            // 
            this.confirmDirsBtn.Enabled = false;
            this.confirmDirsBtn.Location = new System.Drawing.Point(6, 1);
            this.confirmDirsBtn.Name = "confirmDirsBtn";
            this.confirmDirsBtn.Size = new System.Drawing.Size(75, 23);
            this.confirmDirsBtn.TabIndex = 2;
            this.confirmDirsBtn.Text = "Confirm";
            this.confirmDirsBtn.UseVisualStyleBackColor = true;
            this.confirmDirsBtn.Click += new System.EventHandler(this.ConfirmDirsClick);
            // 
            // confirmLabel
            // 
            this.confirmLabel.Location = new System.Drawing.Point(122, 276);
            this.confirmLabel.Name = "confirmLabel";
            this.confirmLabel.Size = new System.Drawing.Size(249, 18);
            this.confirmLabel.TabIndex = 5;
            this.confirmLabel.Text = " ";
            this.confirmLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.confirmDirsBtn);
            this.panel1.Location = new System.Drawing.Point(373, 272);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(86, 26);
            this.panel1.TabIndex = 6;
            this.panel1.MouseEnter += new System.EventHandler(this.confirmBtnMouseOver);
            this.panel1.MouseLeave += new System.EventHandler(this.confirmBtnMouseLeft);
            // 
            // startupMMInfoLabel
            // 
            this.startupMMInfoLabel.AutoSize = true;
            this.startupMMInfoLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.startupMMInfoLabel.Location = new System.Drawing.Point(5, 285);
            this.startupMMInfoLabel.Name = "startupMMInfoLabel";
            this.startupMMInfoLabel.Size = new System.Drawing.Size(111, 13);
            this.startupMMInfoLabel.TabIndex = 7;
            this.startupMMInfoLabel.Text = "Metal Manager v0.1.2";
            // 
            // SetDirsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 303);
            this.Controls.Add(this.startupMMInfoLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.confirmLabel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetDirsForm";
            this.Text = "Get ready to burn!";
            this.Load += new System.EventHandler(this.SetDirsForm_Load);
            this.Shown += new System.EventHandler(this.SetDirsForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pazBuddyIcon)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label modDirFlagLabel;
        private System.Windows.Forms.Label ModDirLabel;
        private System.Windows.Forms.Button browseModDirBtn;
        private System.Windows.Forms.TextBox modDirTextbox;
        private System.Windows.Forms.Label gameDirFlagLabel;
        private System.Windows.Forms.CheckBox dontLinkGame;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button browseGameDirBtn;
        private System.Windows.Forms.TextBox gameDirTextbox;
        private System.Windows.Forms.Label mod_prsEntrLabel;
        private System.Windows.Forms.Label game_prsEntrLabel;
        private System.Windows.Forms.Button confirmDirsBtn;
        private System.Windows.Forms.Label confirmLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pazBuddyIcon;
        private System.Windows.Forms.Label startupMMInfoLabel;
    }
}