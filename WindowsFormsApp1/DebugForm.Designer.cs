namespace WindowsFormsApp1
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Critical Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Min Errors", System.Windows.Forms.HorizontalAlignment.Left);
            this.jsonAnomalyList = new System.Windows.Forms.ListView();
            this.LineCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ErrorCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.debugTextbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // jsonAnomalyList
            // 
            this.jsonAnomalyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LineCol,
            this.ErrorCol});
            listViewGroup1.Header = "Critical Errors";
            listViewGroup1.Name = "CritErrorsGroup";
            listViewGroup2.Header = "Min Errors";
            listViewGroup2.Name = "MinErrorsGroup";
            this.jsonAnomalyList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.jsonAnomalyList.HideSelection = false;
            this.jsonAnomalyList.Location = new System.Drawing.Point(12, 100);
            this.jsonAnomalyList.Name = "jsonAnomalyList";
            this.jsonAnomalyList.Size = new System.Drawing.Size(445, 247);
            this.jsonAnomalyList.TabIndex = 0;
            this.jsonAnomalyList.UseCompatibleStateImageBehavior = false;
            this.jsonAnomalyList.View = System.Windows.Forms.View.Details;
            // 
            // LineCol
            // 
            this.LineCol.Text = "Line";
            // 
            // ErrorCol
            // 
            this.ErrorCol.Text = "Reported Error";
            this.ErrorCol.Width = 382;
            // 
            // debugTextbox
            // 
            this.debugTextbox.Location = new System.Drawing.Point(480, 48);
            this.debugTextbox.Multiline = true;
            this.debugTextbox.Name = "debugTextbox";
            this.debugTextbox.Size = new System.Drawing.Size(289, 299);
            this.debugTextbox.TabIndex = 1;
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.debugTextbox);
            this.Controls.Add(this.jsonAnomalyList);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView jsonAnomalyList;
        private System.Windows.Forms.ColumnHeader LineCol;
        private System.Windows.Forms.ColumnHeader ErrorCol;
        private System.Windows.Forms.TextBox debugTextbox;
    }
}