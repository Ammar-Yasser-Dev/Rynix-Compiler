namespace Rynix_Scanner
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            richTextBox1 = new RichTextBox();
            dataGridView2 = new DataGridView();
            button2 = new Button();
            Lexeme = new DataGridViewTextBoxColumn();
            Token_Type = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(653, 12);
            button1.Name = "button1";
            button1.Size = new Size(135, 224);
            button1.TabIndex = 0;
            button1.Text = "Scan";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(35, 12);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(302, 359);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { Lexeme, Token_Type });
            dataGridView2.Location = new Point(343, 12);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.Size = new Size(304, 359);
            dataGridView2.TabIndex = 3;
            dataGridView2.CellContentClick += dataGridView2_CellContentClick;
            // 
            // button2
            // 
            button2.Location = new Point(653, 242);
            button2.Name = "button2";
            button2.Size = new Size(135, 129);
            button2.TabIndex = 4;
            button2.Text = "Clear";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Lexeme
            // 
            Lexeme.HeaderText = "Lexeme";
            Lexeme.MinimumWidth = 6;
            Lexeme.Name = "Lexeme";
            Lexeme.Width = 125;
            // 
            // Token_Type
            // 
            Token_Type.HeaderText = "Token Type";
            Token_Type.MinimumWidth = 6;
            Token_Type.Name = "Token_Type";
            Token_Type.Width = 125;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button2);
            Controls.Add(dataGridView2);
            Controls.Add(richTextBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Scanner";
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private RichTextBox richTextBox1;
        private DataGridView dataGridView2;
        private Button button2;
        private DataGridViewTextBoxColumn Lexeme;
        private DataGridViewTextBoxColumn Token_Type;
    }
}
