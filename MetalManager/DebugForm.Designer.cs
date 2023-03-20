namespace MetalManager
{
    partial class DebugForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Critical Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Minor Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Potenial Issues", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("No Errors Remaining", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.jsonAnomalyList = new System.Windows.Forms.ListView();
            this.LineCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ErrorCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dgJsonEditor = new System.Windows.Forms.DataGridView();
            this.stringValueBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.songTitleLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.suspendSongButton = new System.Windows.Forms.Button();
            this.susSongExplLabel = new System.Windows.Forms.Label();
            this.SaCBtnLabel = new System.Windows.Forms.Label();
            this.debug_undrJsonAnomLbl = new System.Windows.Forms.Label();
            this.DebugSaveJsonBtn = new System.Windows.Forms.Button();
            this.RescanJsonLinesBtn = new System.Windows.Forms.Button();
            this.RemoveSlctdLineBtn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.AddLineBlwBtn = new System.Windows.Forms.Button();
            this.AddLineButton = new System.Windows.Forms.Button();
            this.debugSJBtnPanel = new System.Windows.Forms.Panel();
            this.debugCopyAllLines = new System.Windows.Forms.Button();
            this.debugVersionLabel = new System.Windows.Forms.Label();
            this.debugPastebox = new System.Windows.Forms.TextBox();
            this.debugSongSelectCombo = new System.Windows.Forms.ComboBox();
            this.debugPasteBoxPanel = new System.Windows.Forms.Panel();
            this.debugGoButton = new System.Windows.Forms.Button();
            this.clearDebugTxtbx = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgJsonEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stringValueBindingSource)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.debugSJBtnPanel.SuspendLayout();
            this.debugPasteBoxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // jsonAnomalyList
            // 
            this.jsonAnomalyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LineCol,
            this.ErrorCol});
            this.jsonAnomalyList.Enabled = false;
            this.jsonAnomalyList.FullRowSelect = true;
            listViewGroup1.Header = "Critical Errors";
            listViewGroup1.Name = "CritErrorsGroup";
            listViewGroup2.Header = "Minor Errors";
            listViewGroup2.Name = "MinErrorsGroup";
            listViewGroup3.Header = "Potenial Issues";
            listViewGroup3.Name = "potIssuesGroup";
            listViewGroup4.Header = "No Errors Remaining";
            listViewGroup4.Name = "noErrorsGrp";
            this.jsonAnomalyList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4});
            this.jsonAnomalyList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.jsonAnomalyList.HideSelection = false;
            this.jsonAnomalyList.Location = new System.Drawing.Point(12, 100);
            this.jsonAnomalyList.MultiSelect = false;
            this.jsonAnomalyList.Name = "jsonAnomalyList";
            this.jsonAnomalyList.Size = new System.Drawing.Size(482, 247);
            this.jsonAnomalyList.TabIndex = 0;
            this.jsonAnomalyList.UseCompatibleStateImageBehavior = false;
            this.jsonAnomalyList.View = System.Windows.Forms.View.Details;
            this.jsonAnomalyList.Click += new System.EventHandler(this.ErrorListClick);
            // 
            // LineCol
            // 
            this.LineCol.Text = "Line";
            this.LineCol.Width = 50;
            // 
            // ErrorCol
            // 
            this.ErrorCol.Text = "Reported Error";
            this.ErrorCol.Width = 428;
            // 
            // dgJsonEditor
            // 
            this.dgJsonEditor.AllowUserToResizeRows = false;
            this.dgJsonEditor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgJsonEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgJsonEditor.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgJsonEditor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgJsonEditor.ColumnHeadersVisible = false;
            this.dgJsonEditor.DataBindings.Add(new System.Windows.Forms.Binding("Tag", this.stringValueBindingSource, "Value", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dgJsonEditor.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgJsonEditor.EnableHeadersVisualStyles = false;
            this.dgJsonEditor.Location = new System.Drawing.Point(3, 3);
            this.dgJsonEditor.MultiSelect = false;
            this.dgJsonEditor.Name = "dgJsonEditor";
            this.dgJsonEditor.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgJsonEditor.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgJsonEditor.RowHeadersWidth = 30;
            this.dgJsonEditor.RowTemplate.Height = 17;
            this.dgJsonEditor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgJsonEditor.ShowCellToolTips = false;
            this.dgJsonEditor.Size = new System.Drawing.Size(539, 335);
            this.dgJsonEditor.TabIndex = 3;
            this.dgJsonEditor.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.JsonCellUpdate);
            this.dgJsonEditor.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgJsonEditor_RowPostPaint);
            // 
            // stringValueBindingSource
            // 
            this.stringValueBindingSource.DataSource = typeof(MetalManager.StringValue);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Controls.Add(this.dgJsonEditor);
            this.panel1.Location = new System.Drawing.Point(515, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(545, 341);
            this.panel1.TabIndex = 7;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.titleLabel.Location = new System.Drawing.Point(92, 16);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(69, 13);
            this.titleLabel.TabIndex = 8;
            this.titleLabel.Text = "Debug Panel";
            // 
            // songTitleLabel
            // 
            this.songTitleLabel.AutoSize = true;
            this.songTitleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.songTitleLabel.Location = new System.Drawing.Point(103, 34);
            this.songTitleLabel.Name = "songTitleLabel";
            this.songTitleLabel.Size = new System.Drawing.Size(77, 13);
            this.songTitleLabel.TabIndex = 12;
            this.songTitleLabel.Text = "Select a Song:";
            // 
            // pathLabel
            // 
            this.pathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pathLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pathLabel.Location = new System.Drawing.Point(105, 53);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(405, 22);
            this.pathLabel.TabIndex = 14;
            this.pathLabel.Text = "M:/";
            // 
            // suspendSongButton
            // 
            this.suspendSongButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.suspendSongButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.suspendSongButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.suspendSongButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Maroon;
            this.suspendSongButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.suspendSongButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.suspendSongButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.suspendSongButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.suspendSongButton.Location = new System.Drawing.Point(518, 390);
            this.suspendSongButton.Margin = new System.Windows.Forms.Padding(0);
            this.suspendSongButton.Name = "suspendSongButton";
            this.suspendSongButton.Size = new System.Drawing.Size(191, 32);
            this.suspendSongButton.TabIndex = 17;
            this.suspendSongButton.Text = "Continue without Fixing Errors";
            this.suspendSongButton.UseVisualStyleBackColor = false;
            this.suspendSongButton.Click += new System.EventHandler(this.closeDebugger);
            this.suspendSongButton.MouseLeave += new System.EventHandler(this.suspendBtn_mouseOut);
            this.suspendSongButton.MouseMove += new System.Windows.Forms.MouseEventHandler(this.suspendBtn_mouseOver);
            // 
            // susSongExplLabel
            // 
            this.susSongExplLabel.Location = new System.Drawing.Point(711, 386);
            this.susSongExplLabel.Name = "susSongExplLabel";
            this.susSongExplLabel.Size = new System.Drawing.Size(201, 51);
            this.susSongExplLabel.TabIndex = 18;
            this.susSongExplLabel.Text = "Warning: Any changes to the .json will      be discarded, and the song will remai" +
    "n unselectable until errors have been fixed.";
            this.susSongExplLabel.Visible = false;
            // 
            // SaCBtnLabel
            // 
            this.SaCBtnLabel.Location = new System.Drawing.Point(742, 392);
            this.SaCBtnLabel.Name = "SaCBtnLabel";
            this.SaCBtnLabel.Size = new System.Drawing.Size(171, 32);
            this.SaCBtnLabel.TabIndex = 19;
            this.SaCBtnLabel.Text = "All Critical Errors must be fixed before .json can be saved.";
            this.SaCBtnLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.SaCBtnLabel.Visible = false;
            // 
            // debug_undrJsonAnomLbl
            // 
            this.debug_undrJsonAnomLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debug_undrJsonAnomLbl.Location = new System.Drawing.Point(13, 349);
            this.debug_undrJsonAnomLbl.Name = "debug_undrJsonAnomLbl";
            this.debug_undrJsonAnomLbl.Size = new System.Drawing.Size(482, 29);
            this.debug_undrJsonAnomLbl.TabIndex = 20;
            this.debug_undrJsonAnomLbl.Text = "This customsongs.json contains errors that must be addressed before Metal Manager" +
    " can select it.";
            // 
            // DebugSaveJsonBtn
            // 
            this.DebugSaveJsonBtn.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.DebugSaveJsonBtn.Enabled = false;
            this.DebugSaveJsonBtn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.DebugSaveJsonBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DebugSaveJsonBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DebugSaveJsonBtn.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DebugSaveJsonBtn.Image = global::MetalManager.Properties.Resources.check_stillframe;
            this.DebugSaveJsonBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DebugSaveJsonBtn.Location = new System.Drawing.Point(3, 3);
            this.DebugSaveJsonBtn.Margin = new System.Windows.Forms.Padding(0);
            this.DebugSaveJsonBtn.Name = "DebugSaveJsonBtn";
            this.DebugSaveJsonBtn.Size = new System.Drawing.Size(141, 32);
            this.DebugSaveJsonBtn.TabIndex = 15;
            this.DebugSaveJsonBtn.Text = "Save and Continue";
            this.DebugSaveJsonBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.DebugSaveJsonBtn.UseVisualStyleBackColor = false;
            this.DebugSaveJsonBtn.Click += new System.EventHandler(this.SaveAndCloseDebugger);
            this.DebugSaveJsonBtn.MouseLeave += new System.EventHandler(this.SaC_MouseOut);
            this.DebugSaveJsonBtn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SaC_MouseOver);
            // 
            // RescanJsonLinesBtn
            // 
            this.RescanJsonLinesBtn.Image = global::MetalManager.Properties.Resources.recheck;
            this.RescanJsonLinesBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RescanJsonLinesBtn.Location = new System.Drawing.Point(516, 355);
            this.RescanJsonLinesBtn.Margin = new System.Windows.Forms.Padding(0);
            this.RescanJsonLinesBtn.Name = "RescanJsonLinesBtn";
            this.RescanJsonLinesBtn.Size = new System.Drawing.Size(111, 22);
            this.RescanJsonLinesBtn.TabIndex = 11;
            this.RescanJsonLinesBtn.Text = "Rescan for Errors";
            this.RescanJsonLinesBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RescanJsonLinesBtn.UseVisualStyleBackColor = true;
            this.RescanJsonLinesBtn.Click += new System.EventHandler(this.RescanJsonBtn_Click);
            // 
            // RemoveSlctdLineBtn
            // 
            this.RemoveSlctdLineBtn.Image = global::MetalManager.Properties.Resources.minus;
            this.RemoveSlctdLineBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RemoveSlctdLineBtn.Location = new System.Drawing.Point(627, 355);
            this.RemoveSlctdLineBtn.Margin = new System.Windows.Forms.Padding(0);
            this.RemoveSlctdLineBtn.Name = "RemoveSlctdLineBtn";
            this.RemoveSlctdLineBtn.Size = new System.Drawing.Size(137, 22);
            this.RemoveSlctdLineBtn.TabIndex = 10;
            this.RemoveSlctdLineBtn.Text = "Remove Selected Line";
            this.RemoveSlctdLineBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RemoveSlctdLineBtn.UseVisualStyleBackColor = true;
            this.RemoveSlctdLineBtn.Click += new System.EventHandler(this.RmvSlctdLineBtn_click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MetalManager.Properties.Resources.pazCrossbones;
            this.pictureBox1.Location = new System.Drawing.Point(14, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(88, 88);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // AddLineBlwBtn
            // 
            this.AddLineBlwBtn.Image = global::MetalManager.Properties.Resources.addBelow;
            this.AddLineBlwBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddLineBlwBtn.Location = new System.Drawing.Point(764, 355);
            this.AddLineBlwBtn.Margin = new System.Windows.Forms.Padding(0);
            this.AddLineBlwBtn.Name = "AddLineBlwBtn";
            this.AddLineBlwBtn.Size = new System.Drawing.Size(146, 22);
            this.AddLineBlwBtn.TabIndex = 5;
            this.AddLineBlwBtn.Text = "Add Line Under Selected";
            this.AddLineBlwBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AddLineBlwBtn.UseVisualStyleBackColor = true;
            this.AddLineBlwBtn.Click += new System.EventHandler(this.AddLineBelowClick);
            // 
            // AddLineButton
            // 
            this.AddLineButton.Image = global::MetalManager.Properties.Resources.add;
            this.AddLineButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddLineButton.Location = new System.Drawing.Point(910, 355);
            this.AddLineButton.Margin = new System.Windows.Forms.Padding(0);
            this.AddLineButton.Name = "AddLineButton";
            this.AddLineButton.Size = new System.Drawing.Size(148, 22);
            this.AddLineButton.TabIndex = 4;
            this.AddLineButton.Text = "Add Line Above Selected";
            this.AddLineButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AddLineButton.UseVisualStyleBackColor = true;
            this.AddLineButton.Click += new System.EventHandler(this.AddLineClick);
            // 
            // debugSJBtnPanel
            // 
            this.debugSJBtnPanel.Controls.Add(this.DebugSaveJsonBtn);
            this.debugSJBtnPanel.Location = new System.Drawing.Point(913, 387);
            this.debugSJBtnPanel.Name = "debugSJBtnPanel";
            this.debugSJBtnPanel.Size = new System.Drawing.Size(147, 38);
            this.debugSJBtnPanel.TabIndex = 21;
            this.debugSJBtnPanel.MouseLeave += new System.EventHandler(this.SaC_MouseOut);
            this.debugSJBtnPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SaC_MouseOver);
            // 
            // debugCopyAllLines
            // 
            this.debugCopyAllLines.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.debugCopyAllLines.Location = new System.Drawing.Point(911, 392);
            this.debugCopyAllLines.Margin = new System.Windows.Forms.Padding(0);
            this.debugCopyAllLines.Name = "debugCopyAllLines";
            this.debugCopyAllLines.Size = new System.Drawing.Size(135, 26);
            this.debugCopyAllLines.TabIndex = 16;
            this.debugCopyAllLines.Text = "Copy all lines to Clipboard";
            this.debugCopyAllLines.UseVisualStyleBackColor = true;
            this.debugCopyAllLines.Click += new System.EventHandler(this.CopyAllLinesBtn_Click);
            // 
            // debugVersionLabel
            // 
            this.debugVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugVersionLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.debugVersionLabel.Location = new System.Drawing.Point(604, 431);
            this.debugVersionLabel.Name = "debugVersionLabel";
            this.debugVersionLabel.Size = new System.Drawing.Size(482, 16);
            this.debugVersionLabel.TabIndex = 22;
            this.debugVersionLabel.Text = "Metal Manager Debug Panel v0.1.0  ";
            this.debugVersionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // debugPastebox
            // 
            this.debugPastebox.AcceptsReturn = true;
            this.debugPastebox.Location = new System.Drawing.Point(514, 11);
            this.debugPastebox.Multiline = true;
            this.debugPastebox.Name = "debugPastebox";
            this.debugPastebox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugPastebox.Size = new System.Drawing.Size(546, 365);
            this.debugPastebox.TabIndex = 23;
            this.debugPastebox.Text = "Copy your customsongs.json and Paste its contents here, then hit GO in the bottom" +
    "-right corner,\r\nor select a song in drop-down box on the left to get started. Cl" +
    "ose this window to cancel.";
            this.debugPastebox.WordWrap = false;
            this.debugPastebox.Enter += new System.EventHandler(this.debugPasteBox_enter);
            this.debugPastebox.LostFocus += new System.EventHandler(this.debugPasteBox_unfocus);
            // 
            // debugSongSelectCombo
            // 
            this.debugSongSelectCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.debugSongSelectCombo.FormattingEnabled = true;
            this.debugSongSelectCombo.Location = new System.Drawing.Point(106, 51);
            this.debugSongSelectCombo.Name = "debugSongSelectCombo";
            this.debugSongSelectCombo.Size = new System.Drawing.Size(298, 21);
            this.debugSongSelectCombo.TabIndex = 24;
            this.debugSongSelectCombo.SelectedIndexChanged += new System.EventHandler(this.debugChoseDDSlct);
            // 
            // debugPasteBoxPanel
            // 
            this.debugPasteBoxPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.debugPasteBoxPanel.Controls.Add(this.debugGoButton);
            this.debugPasteBoxPanel.Controls.Add(this.clearDebugTxtbx);
            this.debugPasteBoxPanel.Location = new System.Drawing.Point(514, 375);
            this.debugPasteBoxPanel.Name = "debugPasteBoxPanel";
            this.debugPasteBoxPanel.Size = new System.Drawing.Size(546, 33);
            this.debugPasteBoxPanel.TabIndex = 25;
            // 
            // debugGoButton
            // 
            this.debugGoButton.Location = new System.Drawing.Point(466, 4);
            this.debugGoButton.Name = "debugGoButton";
            this.debugGoButton.Size = new System.Drawing.Size(75, 23);
            this.debugGoButton.TabIndex = 2;
            this.debugGoButton.Text = "GO";
            this.debugGoButton.UseVisualStyleBackColor = true;
            this.debugGoButton.Click += new System.EventHandler(this.debuggerPasteBox);
            // 
            // clearDebugTxtbx
            // 
            this.clearDebugTxtbx.Location = new System.Drawing.Point(4, 4);
            this.clearDebugTxtbx.Name = "clearDebugTxtbx";
            this.clearDebugTxtbx.Size = new System.Drawing.Size(75, 23);
            this.clearDebugTxtbx.TabIndex = 0;
            this.clearDebugTxtbx.Text = "Clear";
            this.clearDebugTxtbx.UseVisualStyleBackColor = true;
            this.clearDebugTxtbx.Click += new System.EventHandler(this.clearDebugPastebox);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 450);
            this.Controls.Add(this.debugCopyAllLines);
            this.Controls.Add(this.debugSongSelectCombo);
            this.Controls.Add(this.debugPastebox);
            this.Controls.Add(this.debugVersionLabel);
            this.Controls.Add(this.susSongExplLabel);
            this.Controls.Add(this.debugSJBtnPanel);
            this.Controls.Add(this.debug_undrJsonAnomLbl);
            this.Controls.Add(this.SaCBtnLabel);
            this.Controls.Add(this.suspendSongButton);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.songTitleLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.RescanJsonLinesBtn);
            this.Controls.Add(this.RemoveSlctdLineBtn);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.AddLineBlwBtn);
            this.Controls.Add(this.AddLineButton);
            this.Controls.Add(this.jsonAnomalyList);
            this.Controls.Add(this.debugPasteBoxPanel);
            this.Name = "DebugForm";
            this.Text = "Debug";
            this.Load += new System.EventHandler(this.DebugFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dgJsonEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stringValueBindingSource)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.debugSJBtnPanel.ResumeLayout(false);
            this.debugPasteBoxPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView jsonAnomalyList;
        private System.Windows.Forms.ColumnHeader LineCol;
        private System.Windows.Forms.ColumnHeader ErrorCol;
        private System.Windows.Forms.DataGridView dgJsonEditor;
        private System.Windows.Forms.BindingSource stringValueBindingSource;
        private System.Windows.Forms.Button AddLineButton;
        private System.Windows.Forms.Button AddLineBlwBtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button RemoveSlctdLineBtn;
        private System.Windows.Forms.Button RescanJsonLinesBtn;
        private System.Windows.Forms.Label songTitleLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button DebugSaveJsonBtn;
        private System.Windows.Forms.Button suspendSongButton;
        private System.Windows.Forms.Label susSongExplLabel;
        private System.Windows.Forms.Label SaCBtnLabel;
        private System.Windows.Forms.Label debug_undrJsonAnomLbl;
        private System.Windows.Forms.Panel debugSJBtnPanel;
        private System.Windows.Forms.Label debugVersionLabel;
        private System.Windows.Forms.TextBox debugPastebox;
        private System.Windows.Forms.ComboBox debugSongSelectCombo;
        private System.Windows.Forms.Panel debugPasteBoxPanel;
        private System.Windows.Forms.Button debugGoButton;
        private System.Windows.Forms.Button clearDebugTxtbx;
        private System.Windows.Forms.Button debugCopyAllLines;
    }
}