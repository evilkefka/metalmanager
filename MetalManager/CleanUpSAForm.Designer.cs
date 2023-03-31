namespace MetalManager
{
    partial class CleanUpSAForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Critical Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Minor Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Potenial Issues", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("No Errors Remaining", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CleanUpSAForm));
            this.SAAnomalyList = new System.Windows.Forms.ListView();
            this.bankName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.explainCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CleanupSAExplLabel = new System.Windows.Forms.Label();
            this.saClnUpDeleteBtn = new System.Windows.Forms.Button();
            this.checkUncheckAllBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SAAnomalyList
            // 
            this.SAAnomalyList.CheckBoxes = true;
            this.SAAnomalyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.bankName,
            this.explainCol});
            this.SAAnomalyList.Enabled = false;
            this.SAAnomalyList.FullRowSelect = true;
            listViewGroup1.Header = "Critical Errors";
            listViewGroup1.Name = "CritErrorsGroup";
            listViewGroup2.Header = "Minor Errors";
            listViewGroup2.Name = "MinErrorsGroup";
            listViewGroup3.Header = "Potenial Issues";
            listViewGroup3.Name = "potIssuesGroup";
            listViewGroup4.Header = "No Errors Remaining";
            listViewGroup4.Name = "noErrorsGrp";
            this.SAAnomalyList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4});
            this.SAAnomalyList.HideSelection = false;
            this.SAAnomalyList.Location = new System.Drawing.Point(13, 105);
            this.SAAnomalyList.Name = "SAAnomalyList";
            this.SAAnomalyList.ShowGroups = false;
            this.SAAnomalyList.Size = new System.Drawing.Size(454, 247);
            this.SAAnomalyList.TabIndex = 1;
            this.SAAnomalyList.UseCompatibleStateImageBehavior = false;
            this.SAAnomalyList.View = System.Windows.Forms.View.Details;
            this.SAAnomalyList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.SAAnomList_ColClick);
            // 
            // bankName
            // 
            this.bankName.Text = "Bank File";
            this.bankName.Width = 250;
            // 
            // explainCol
            // 
            this.explainCol.Text = "Analysis";
            this.explainCol.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.explainCol.Width = 200;
            // 
            // CleanupSAExplLabel
            // 
            this.CleanupSAExplLabel.Location = new System.Drawing.Point(12, 9);
            this.CleanupSAExplLabel.Name = "CleanupSAExplLabel";
            this.CleanupSAExplLabel.Size = new System.Drawing.Size(454, 92);
            this.CleanupSAExplLabel.TabIndex = 54;
            this.CleanupSAExplLabel.Text = resources.GetString("CleanupSAExplLabel.Text");
            // 
            // saClnUpDeleteBtn
            // 
            this.saClnUpDeleteBtn.Image = global::MetalManager.Properties.Resources.minus;
            this.saClnUpDeleteBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saClnUpDeleteBtn.Location = new System.Drawing.Point(343, 355);
            this.saClnUpDeleteBtn.Margin = new System.Windows.Forms.Padding(0);
            this.saClnUpDeleteBtn.Name = "saClnUpDeleteBtn";
            this.saClnUpDeleteBtn.Size = new System.Drawing.Size(123, 26);
            this.saClnUpDeleteBtn.TabIndex = 55;
            this.saClnUpDeleteBtn.Text = "DELETE CHECKED";
            this.saClnUpDeleteBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.saClnUpDeleteBtn.UseVisualStyleBackColor = true;
            this.saClnUpDeleteBtn.Click += new System.EventHandler(this.SACleanupDeleteClick);
            // 
            // checkUncheckAllBtn
            // 
            this.checkUncheckAllBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkUncheckAllBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkUncheckAllBtn.Location = new System.Drawing.Point(13, 355);
            this.checkUncheckAllBtn.Margin = new System.Windows.Forms.Padding(0);
            this.checkUncheckAllBtn.Name = "checkUncheckAllBtn";
            this.checkUncheckAllBtn.Size = new System.Drawing.Size(101, 26);
            this.checkUncheckAllBtn.TabIndex = 56;
            this.checkUncheckAllBtn.Text = "Check/Uncheck All";
            this.checkUncheckAllBtn.UseVisualStyleBackColor = true;
            this.checkUncheckAllBtn.Click += new System.EventHandler(this.chkUnchkAllClick);
            // 
            // CleanUpSAForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 386);
            this.Controls.Add(this.checkUncheckAllBtn);
            this.Controls.Add(this.saClnUpDeleteBtn);
            this.Controls.Add(this.CleanupSAExplLabel);
            this.Controls.Add(this.SAAnomalyList);
            this.Name = "CleanUpSAForm";
            this.Text = "Clean up StreamingAssets";
            this.Load += new System.EventHandler(this.CleanUpSAForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView SAAnomalyList;
        private System.Windows.Forms.ColumnHeader bankName;
        private System.Windows.Forms.ColumnHeader explainCol;
        private System.Windows.Forms.Label CleanupSAExplLabel;
        private System.Windows.Forms.Button saClnUpDeleteBtn;
        private System.Windows.Forms.Button checkUncheckAllBtn;
    }
}