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
            LexemeAndTokenDataGridView = new DataGridView();
            Lexeme = new DataGridViewTextBoxColumn();
            Token_Type = new DataGridViewTextBoxColumn();
            button2 = new Button();
            ErrorsDataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)LexemeAndTokenDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ErrorsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(816, 15);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(169, 280);
            button1.TabIndex = 0;
            button1.Text = "Scan";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(44, 15);
            richTextBox1.Margin = new Padding(4);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(376, 448);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // LexemeAndTokenDataGridView
            // 
            LexemeAndTokenDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LexemeAndTokenDataGridView.Columns.AddRange(new DataGridViewColumn[] { Lexeme, Token_Type });
            LexemeAndTokenDataGridView.Location = new Point(429, 15);
            LexemeAndTokenDataGridView.Margin = new Padding(4);
            LexemeAndTokenDataGridView.Name = "LexemeAndTokenDataGridView";
            LexemeAndTokenDataGridView.RowHeadersWidth = 51;
            LexemeAndTokenDataGridView.Size = new Size(380, 449);
            LexemeAndTokenDataGridView.TabIndex = 3;
            LexemeAndTokenDataGridView.CellContentClick += dataGridView2_CellContentClick;
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
            // button2
            // 
            button2.Location = new Point(816, 302);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(169, 161);
            button2.TabIndex = 4;
            button2.Text = "Clear";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // ErrorsDataGridView
            // 
            ErrorsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ErrorsDataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
            ErrorsDataGridView.Location = new Point(44, 476);
            ErrorsDataGridView.Margin = new Padding(4);
            ErrorsDataGridView.Name = "ErrorsDataGridView";
            ErrorsDataGridView.RowHeadersWidth = 51;
            ErrorsDataGridView.Size = new Size(941, 250);
            ErrorsDataGridView.TabIndex = 5;
            ErrorsDataGridView.CellContentClick += dataGridView1_CellContentClick;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Error";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Error Details";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 739);
            Controls.Add(ErrorsDataGridView);
            Controls.Add(button2);
            Controls.Add(LexemeAndTokenDataGridView);
            Controls.Add(richTextBox1);
            Controls.Add(button1);
            Margin = new Padding(4);
            Name = "Form1";
            Text = "Scanner";
            ((System.ComponentModel.ISupportInitialize)LexemeAndTokenDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)ErrorsDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private RichTextBox richTextBox1;
        private DataGridView LexemeAndTokenDataGridView;
        private Button button2;
        private DataGridViewTextBoxColumn Lexeme;
        private DataGridViewTextBoxColumn Token_Type;
        private DataGridView ErrorsDataGridView;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    }
}
