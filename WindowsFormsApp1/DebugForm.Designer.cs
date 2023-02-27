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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Critical Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Min Errors", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.jsonAnomalyList = new System.Windows.Forms.ListView();
            this.LineCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ErrorCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.debugTextbox = new System.Windows.Forms.TextBox();
            this.dgJsonEditor = new System.Windows.Forms.DataGridView();
            this.stringValueBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.AddLineButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgJsonEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stringValueBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // jsonAnomalyList
            // 
            this.jsonAnomalyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LineCol,
            this.ErrorCol});
            this.jsonAnomalyList.FullRowSelect = true;
            listViewGroup3.Header = "Critical Errors";
            listViewGroup3.Name = "CritErrorsGroup";
            listViewGroup4.Header = "Min Errors";
            listViewGroup4.Name = "MinErrorsGroup";
            this.jsonAnomalyList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3,
            listViewGroup4});
            this.jsonAnomalyList.HideSelection = false;
            this.jsonAnomalyList.Location = new System.Drawing.Point(12, 100);
            this.jsonAnomalyList.MultiSelect = false;
            this.jsonAnomalyList.Name = "jsonAnomalyList";
            this.jsonAnomalyList.Size = new System.Drawing.Size(445, 247);
            this.jsonAnomalyList.TabIndex = 0;
            this.jsonAnomalyList.UseCompatibleStateImageBehavior = false;
            this.jsonAnomalyList.View = System.Windows.Forms.View.Details;
            this.jsonAnomalyList.Click += new System.EventHandler(this.ErrorListClick);
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
            this.debugTextbox.Location = new System.Drawing.Point(168, 53);
            this.debugTextbox.Multiline = true;
            this.debugTextbox.Name = "debugTextbox";
            this.debugTextbox.Size = new System.Drawing.Size(289, 22);
            this.debugTextbox.TabIndex = 1;
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
            this.dgJsonEditor.Location = new System.Drawing.Point(524, 12);
            this.dgJsonEditor.MultiSelect = false;
            this.dgJsonEditor.Name = "dgJsonEditor";
            this.dgJsonEditor.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgJsonEditor.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgJsonEditor.RowHeadersWidth = 30;
            this.dgJsonEditor.RowTemplate.Height = 17;
            this.dgJsonEditor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgJsonEditor.Size = new System.Drawing.Size(539, 335);
            this.dgJsonEditor.TabIndex = 3;
            this.dgJsonEditor.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.JsonCellUpdate);
            this.dgJsonEditor.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgJsonEditor_RowPostPaint);
            // 
            // stringValueBindingSource
            // 
            this.stringValueBindingSource.DataSource = typeof(WindowsFormsApp1.StringValue);
            // 
            // AddLineButton
            // 
            this.AddLineButton.Image = global::WindowsFormsApp1.Properties.Resources.add;
            this.AddLineButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddLineButton.Location = new System.Drawing.Point(936, 353);
            this.AddLineButton.Margin = new System.Windows.Forms.Padding(0);
            this.AddLineButton.Name = "AddLineButton";
            this.AddLineButton.Size = new System.Drawing.Size(127, 22);
            this.AddLineButton.TabIndex = 4;
            this.AddLineButton.Text = "Add Line at Selected";
            this.AddLineButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AddLineButton.UseVisualStyleBackColor = true;
            this.AddLineButton.Click += new System.EventHandler(this.AddLineClick);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 450);
            this.Controls.Add(this.AddLineButton);
            this.Controls.Add(this.dgJsonEditor);
            this.Controls.Add(this.debugTextbox);
            this.Controls.Add(this.jsonAnomalyList);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.Load += new System.EventHandler(this.DebugFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dgJsonEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stringValueBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView jsonAnomalyList;
        private System.Windows.Forms.ColumnHeader LineCol;
        private System.Windows.Forms.ColumnHeader ErrorCol;
        private System.Windows.Forms.TextBox debugTextbox;
        private System.Windows.Forms.DataGridView dgJsonEditor;
        private System.Windows.Forms.BindingSource stringValueBindingSource;
        private System.Windows.Forms.Button AddLineButton;
    }
}