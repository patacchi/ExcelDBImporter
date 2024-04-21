namespace ExcelDBImporter
{
    partial class FrmDataViewer
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            dgvUpdater = new DataGridView();
            BtnSaveChanges = new Button();
            CmbBoxSelectClass = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgvUpdater).BeginInit();
            SuspendLayout();
            // 
            // dgvUpdater
            // 
            dgvUpdater.AllowUserToDeleteRows = false;
            dgvUpdater.AllowUserToOrderColumns = true;
            dgvUpdater.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Yu Gothic UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.NullValue = "--null--";
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvUpdater.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvUpdater.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Yu Gothic UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.NullValue = "--null--";
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvUpdater.DefaultCellStyle = dataGridViewCellStyle2;
            dgvUpdater.Location = new Point(88, 68);
            dgvUpdater.Name = "dgvUpdater";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Yu Gothic UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.NullValue = "--null--";
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvUpdater.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.NullValue = "--null--";
            dgvUpdater.RowsDefaultCellStyle = dataGridViewCellStyle4;
            dgvUpdater.RowTemplate.DefaultCellStyle.NullValue = "--null--";
            dgvUpdater.RowTemplate.Resizable = DataGridViewTriState.True;
            dgvUpdater.Size = new Size(709, 317);
            dgvUpdater.TabIndex = 1;
            // 
            // BtnSaveChanges
            // 
            BtnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            BtnSaveChanges.Location = new Point(111, 450);
            BtnSaveChanges.Name = "BtnSaveChanges";
            BtnSaveChanges.Size = new Size(174, 49);
            BtnSaveChanges.TabIndex = 2;
            BtnSaveChanges.Text = "変更をDBに適用する";
            BtnSaveChanges.UseVisualStyleBackColor = true;
            BtnSaveChanges.Click += BtnSaveChanges_Click;
            // 
            // CmbBoxSelectClass
            // 
            CmbBoxSelectClass.FormattingEnabled = true;
            CmbBoxSelectClass.Location = new Point(20, 20);
            CmbBoxSelectClass.Name = "CmbBoxSelectClass";
            CmbBoxSelectClass.Size = new Size(239, 23);
            CmbBoxSelectClass.TabIndex = 3;
            CmbBoxSelectClass.SelectionChangeCommitted += this.CmbBoxSelectClass_SelectionChangeCommitted;
            // 
            // FrmDataViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 512);
            Controls.Add(CmbBoxSelectClass);
            Controls.Add(BtnSaveChanges);
            Controls.Add(dgvUpdater);
            Name = "FrmDataViewer";
            Text = "Form2";
            Load += FrmDataViewer_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUpdater).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvUpdater;
        private Button BtnSaveChanges;
        private ComboBox CmbBoxSelectClass;
    }
}