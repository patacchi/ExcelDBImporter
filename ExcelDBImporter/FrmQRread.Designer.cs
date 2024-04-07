namespace ExcelDBImporter
{
    partial class FrmQRread
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
            TextBoxQRread = new TextBox();
            SuspendLayout();
            // 
            // TextBoxQRread
            // 
            TextBoxQRread.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxQRread.Location = new Point(33, 28);
            TextBoxQRread.Multiline = true;
            TextBoxQRread.Name = "TextBoxQRread";
            TextBoxQRread.Size = new Size(315, 352);
            TextBoxQRread.TabIndex = 0;
            TextBoxQRread.TextChanged += TextBoxQRread_TextChanged;
            // 
            // FrmQRread
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(389, 421);
            Controls.Add(TextBoxQRread);
            Name = "FrmQRread";
            Text = "QRコード入力";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TextBoxQRread;
    }
}