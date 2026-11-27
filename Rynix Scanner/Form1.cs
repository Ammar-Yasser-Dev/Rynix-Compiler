using System.Diagnostics.Eventing.Reader;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Rynix_Scanner
{
    public partial class Form1 : Form
    {
        Scanner rynix_scanner = new Scanner();
        Parser prsr;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Code = richTextBox1.Text;
            rynix_scanner.Scan(Code); // calls the scan function in Scanner.cs
            PrintTokens();
            prsr = new Parser(rynix_scanner.Tokens);
            treeView2.Nodes.Add(Parser.PrintParseTree(prsr.root));
            print_parse_errors();
            rynix_scanner.Tokens.Clear();
            prsr.errors.Clear();


        }
        void print_parse_errors()
        {
            for (int i = 0; i < prsr.errors.Count; i++)
            {
                ErrorsDataGridView.Rows.Add("parse error", prsr.errors.ElementAt(i));

            }
        }
        void PrintTokens()
        {
            // clear the DataGrid before adding to it.
            LexemeAndTokenDataGridView.Rows.Clear();
            ErrorsDataGridView.Rows.Clear();
            bool errors_found = false;
            for (int i = 0; i < rynix_scanner.Tokens.Count; i++)
            {
                if (rynix_scanner.Tokens.ElementAt(i).token_type == Token_Class.Invalid)
                {
                    ErrorsDataGridView.Rows.Add(rynix_scanner.Tokens.ElementAt(i).lexeme, rynix_scanner.Tokens.ElementAt(i).token_type);
                    errors_found = true;
                }
            }
            if (errors_found)
            {
                //if errors found should we not show other tokens?
                // return;
            }
            for (int i = 0; i < rynix_scanner.Tokens.Count; i++)
            {
                if(i+2< rynix_scanner.Tokens.Count && rynix_scanner.Tokens.ElementAt(i).token_type == Token_Class.DoubleQuote && rynix_scanner.Tokens.ElementAt(i+1).token_type == Token_Class.Text && rynix_scanner.Tokens.ElementAt(i+2).token_type == Token_Class.DoubleQuote)
                {
                    LexemeAndTokenDataGridView.Rows.Add("\""+rynix_scanner.Tokens.ElementAt(i+1).lexeme+"\"", "Double quoted content");
                    i+=2;
                }
                else if (i + 2 < rynix_scanner.Tokens.Count && rynix_scanner.Tokens.ElementAt(i).token_type == Token_Class.SingleQuote && rynix_scanner.Tokens.ElementAt(i + 1).token_type == Token_Class.Text && rynix_scanner.Tokens.ElementAt(i + 2).token_type == Token_Class.SingleQuote)
                {
                    LexemeAndTokenDataGridView.Rows.Add("\'" + rynix_scanner.Tokens.ElementAt(i+1).lexeme + "\'", "Single quoted content");
                    i += 2;
                }
                else if (rynix_scanner.Tokens.ElementAt(i).token_type != Token_Class.Invalid)
                    LexemeAndTokenDataGridView.Rows.Add(rynix_scanner.Tokens.ElementAt(i).lexeme, rynix_scanner.Tokens.ElementAt(i).token_type);
            }
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            LexemeAndTokenDataGridView.Rows.Clear();
            ErrorsDataGridView.Rows.Clear();
            treeView2.Nodes.Clear();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
