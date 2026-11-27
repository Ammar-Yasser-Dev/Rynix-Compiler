using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Rynix_Scanner
{
    public class Error
    {
        public string message;
        public Error(string msg)
        {
            message = msg;
        }
        public override string ToString()
        {
            return message;
        }
    }
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }

    internal class Parser
    {
        public List<Token> tokens = new List<Token>();
        public List<Error> errors = new List<Error>();
        Token cur_token;
        int cur_token_index = 0;
        bool at_end = false;
        int last_error_pos = -1;

        public Node root;

        public Parser(List<Token> _Tokens)
        {
            tokens = RemoveNoise(_Tokens);
            set_current_token_to_next_token();
            Program();
        }

        private List<Token> RemoveNoise(List<Token> raw)
        {
            List<Token> result = new List<Token>();
            int i = 0;
            while (i < raw.Count)
            {
                Token_Class tc = raw[i].token_type;

                if (tc == Token_Class.NewLine)
                {
                    i++;
                }
                else if (tc == Token_Class.DoubleSlash)
                {
                    i++;
                    if (i < raw.Count && raw[i].token_type == Token_Class.Text) i++;
                }
                else if (tc == Token_Class.SlashStar)
                {
                    i++;
                    if (i < raw.Count && raw[i].token_type == Token_Class.Text) i++;
                    if (i < raw.Count && raw[i].token_type == Token_Class.StarSlash) i++;
                }
                else
                {
                    result.Add(raw[i]);
                    i++;
                }
            }
            return result;
        }

        #region helper functions
        private bool IsEOF()
        {
            return at_end;
        }

        private bool Is(Token_Class t)
        {
            if (at_end) return false;
            return cur_token.token_type == t;
        }

        private bool IsIdentifier()
        {
            return Is(Token_Class.Identifier);
        }

        private bool IsInit()
        {
            return Is(Token_Class.Identifier) && cur_token.lexeme == "init";
        }

        private bool IsNot()
        {
            if (at_end) return false;
            return cur_token.lexeme == "!";
        }

        private bool IsLiteral()
        {
            return Is(Token_Class.Constant) || Is(Token_Class.True) || Is(Token_Class.False) ||
                   Is(Token_Class.Null) || Is(Token_Class.DoubleQuote) || Is(Token_Class.SingleQuote);
        }

        private bool IsPrimitiveType()
        {
            return Is(Token_Class.Int) || Is(Token_Class.Float) || Is(Token_Class.String) ||
                   Is(Token_Class.Bool) || Is(Token_Class.Char) || Is(Token_Class.Void);
        }

        private string CurLexeme()
        {
            if (at_end) return "end of input";
            return cur_token.lexeme;
        }

        private void set_current_token_to_next_token()
        {
            if (cur_token_index < tokens.Count)
                cur_token = tokens[cur_token_index++];
            else
                at_end = true;
        }

        private void ReportError(string msg)
        {

            int pos = at_end ? cur_token_index + 1 : cur_token_index;
            if (pos == last_error_pos) return;
            last_error_pos = pos;
            errors.Add(new Error(msg));
        }

        private Node match(Token_Class expected)
        {
            if (Is(expected))
            {
                set_current_token_to_next_token();
                return new Node(expected.ToString());
            }
            else
            {
                ReportError("unexpected token, Expected: " + expected.ToString() + ", found: " + CurLexeme());
                return null;
            }
        }

        private Node match_identifier()
        {
            return match(Token_Class.Identifier);
        }

        private bool FirstStatement()
        {
            return Is(Token_Class.Var) || Is(Token_Class.Val) || IsIdentifier() || Is(Token_Class.If) ||
                   Is(Token_Class.For) || Is(Token_Class.While) || Is(Token_Class.Return) ||
                   Is(Token_Class.Break) || Is(Token_Class.Continue) || Is(Token_Class.Print);
        }

        private bool FirstTopLevelItem()
        {
            return Is(Token_Class.Fun) || Is(Token_Class.Class) || FirstStatement();
        }

        private bool FirstClassMember()
        {
            return Is(Token_Class.Var) || Is(Token_Class.Val) || Is(Token_Class.Fun) || IsInit();
        }

        private bool FirstExpression()
        {
            return Is(Token_Class.Minus) || IsNot() || Is(Token_Class.LeftParen) ||
                   Is(Token_Class.LeftBracket) || Is(Token_Class.New) || Is(Token_Class.Input) ||
                   IsIdentifier() || IsLiteral();
        }

        private bool IsRelOp()
        {
            return Is(Token_Class.EqualEqual) || Is(Token_Class.NotEqual) || Is(Token_Class.LessEqual) ||
                   Is(Token_Class.GreaterEqual) || Is(Token_Class.Less) || Is(Token_Class.Greater);
        }
        #endregion

        #region recurse functions

        public Node Program()
        {
            if (IsEOF())
            {
                ReportError("Expected at least one declaration or statement");
            }

            while (!IsEOF())
            {
                if (FirstTopLevelItem())
                {
                    root = new Node("Program");
                    root.Children.Add(TopLevelList());

                }
                else
                {
                    ReportError("unexpected token ' " + CurLexeme() + " ' at top level");
                    set_current_token_to_next_token();
                }
            }
            return root;
        }

        public Node TopLevelList()
        {
            Node TLL = new Node("TopLevelList");
            TLL.Children.Add(TopLevelItem());
            TLL.Children.Add(TopLevelList_());
            return TLL;
        }

        public Node TopLevelList_()
        {
            if (FirstTopLevelItem())
            {
                Node node = new Node("TopLevelList_");
                node.Children.Add(TopLevelItem());
                node.Children.Add(TopLevelList_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node TopLevelItem()
        {
            Node node = new Node("TopLevelItem");
            if (Is(Token_Class.Fun)) node.Children.Add(FunctionDecl());
            else if (Is(Token_Class.Class)) node.Children.Add(ClassDecl());
            else node.Children.Add(Statement());
            return node;
        }

        public Node VarDecl()
        {
            Node node = new Node("VarDecl");
            node.Children.Add(match(Token_Class.Var));
            node.Children.Add(match_identifier());
            node.Children.Add(VarDeclTail());
            return node;
        }

        public Node VarDeclTail()
        {
            Node node = new Node("VarDeclTail");
            if (Is(Token_Class.Colon))
            {
                node.Children.Add(match(Token_Class.Colon));
                node.Children.Add(Type());
                node.Children.Add(VarInit());
            }
            else if (Is(Token_Class.Assign))
            {
                node.Children.Add(match(Token_Class.Assign));
                node.Children.Add(Expression());
            }
            else
                ReportError("Expected ':' or '=' after variable name, found: " + CurLexeme());
            return node;
        }

        public Node VarInit()
        {
            if (Is(Token_Class.Assign))
            {
                Node node = new Node("VarInit");
                node.Children.Add(match(Token_Class.Assign));
                node.Children.Add(Expression());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ValDecl()
        {
            Node node = new Node("ValDecl");
            node.Children.Add(match(Token_Class.Val));
            node.Children.Add(match_identifier());
            node.Children.Add(ValDeclTail());
            return node;
        }

        public Node ValDeclTail()
        {
            Node node = new Node("ValDeclTail");
            if (Is(Token_Class.Colon))
            {
                node.Children.Add(match(Token_Class.Colon));
                node.Children.Add(Type());
                node.Children.Add(match(Token_Class.Assign));
                node.Children.Add(Expression());
            }
            else if (Is(Token_Class.Assign))
            {
                node.Children.Add(match(Token_Class.Assign));
                node.Children.Add(Expression());
            }
            else
                ReportError("Expected ':' or '=' after constant name, found: " + CurLexeme());
            return node;
        }

        public Node Type()
        {
            Node node = new Node("Type");
            if (IsPrimitiveType())
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
            }
            else if (Is(Token_Class.Array))
                node.Children.Add(ArrayType());
            else if (IsIdentifier())
                node.Children.Add(match_identifier());
            else
                ReportError("Expected a type, found: " + CurLexeme());
            return node;
        }

        public Node ArrayType()
        {
            Node node = new Node("ArrayType");
            node.Children.Add(match(Token_Class.Array));
            node.Children.Add(match(Token_Class.Less));
            node.Children.Add(Type());
            node.Children.Add(match(Token_Class.Greater));
            return node;
        }

        public Node FunctionDecl()
        {
            Node node = new Node("FunctionDecl");
            node.Children.Add(match(Token_Class.Fun));
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(ParamsListOptional());
            node.Children.Add(match(Token_Class.RightParen));
            node.Children.Add(match(Token_Class.Colon));
            node.Children.Add(Type());
            node.Children.Add(Block());
            return node;
        }

        public Node ParamsListOptional()
        {
            if (IsIdentifier())
            {
                Node node = new Node("ParamsListOptional");
                node.Children.Add(ParamList());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ParamList()
        {
            Node node = new Node("ParamList");
            node.Children.Add(Param());
            node.Children.Add(ParamList_());
            return node;
        }

        public Node ParamList_()
        {
            if (Is(Token_Class.Comma))
            {
                Node node = new Node("ParamList_");
                node.Children.Add(match(Token_Class.Comma));
                node.Children.Add(Param());
                node.Children.Add(ParamList_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node Param()
        {
            Node node = new Node("Param");
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.Colon));
            node.Children.Add(Type());
            return node;
        }

        public Node ClassDecl()
        {
            Node node = new Node("ClassDecl");
            node.Children.Add(match(Token_Class.Class));
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.LeftBrace));
            node.Children.Add(ClassBodyOptional());

            while (!Is(Token_Class.RightBrace) && !IsEOF())
            {
                ReportError("unexpected token '" + CurLexeme() + "' in class body");
                set_current_token_to_next_token();
                if (FirstClassMember())
                    node.Children.Add(ClassBodyOptional());
            }

            node.Children.Add(match(Token_Class.RightBrace));
            return node;
        }

        public Node ClassBodyOptional()
        {
            if (FirstClassMember())
            {
                Node node = new Node("ClassBodyOptional");
                node.Children.Add(ClassBody());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ClassBody()
        {
            Node node = new Node("ClassBody");
            node.Children.Add(ClassMember());
            node.Children.Add(ClassBody_());
            return node;
        }

        public Node ClassBody_()
        {
            if (FirstClassMember())
            {
                Node node = new Node("ClassBody_");
                node.Children.Add(ClassMember());
                node.Children.Add(ClassBody_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ClassMember()
        {
            Node node = new Node("ClassMember");
            if (Is(Token_Class.Var)) node.Children.Add(VarDecl());
            else if (Is(Token_Class.Val)) node.Children.Add(ValDecl());
            else if (Is(Token_Class.Fun)) node.Children.Add(FunctionDecl());
            else if (IsInit()) node.Children.Add(ConstructorDecl());
            else ReportError("Expected a class member, found: " + CurLexeme());
            return node;
        }

        public Node ConstructorDecl()
        {
            Node node = new Node("ConstructorDecl");
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(ParamsListOptional());
            node.Children.Add(match(Token_Class.RightParen));
            node.Children.Add(Block());
            return node;
        }

        public Node Block()
        {
            Node node = new Node("Block");
            node.Children.Add(match(Token_Class.LeftBrace));
            node.Children.Add(StatementListOptional());

            while (!Is(Token_Class.RightBrace) && !IsEOF())
            {
                ReportError("unexpected token ' " + CurLexeme() + " ' in block");
                set_current_token_to_next_token();
                if (FirstStatement())
                    node.Children.Add(StatementListOptional());
            }

            node.Children.Add(match(Token_Class.RightBrace));
            return node;
        }

        public Node StatementListOptional()
        {
            if (FirstStatement())
            {
                Node node = new Node("StatementListOptional");
                node.Children.Add(StatementList());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node StatementList()
        {
            Node node = new Node("StatementList");
            node.Children.Add(Statement());
            node.Children.Add(StatementList_());
            return node;
        }

        public Node StatementList_()
        {
            if (FirstStatement())
            {
                Node node = new Node("StatementList_");
                node.Children.Add(Statement());
                node.Children.Add(StatementList_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node Statement()
        {
            Node node = new Node("Statement");
            if (Is(Token_Class.Var)) node.Children.Add(VarDecl());
            else if (Is(Token_Class.Val)) node.Children.Add(ValDecl());
            else if (IsIdentifier()) node.Children.Add(AssignmentOrCall());
            else if (Is(Token_Class.If)) node.Children.Add(IfStatement());
            else if (Is(Token_Class.For)) node.Children.Add(Forloop());
            else if (Is(Token_Class.While)) node.Children.Add(Whileloop());
            else if (Is(Token_Class.Return)) node.Children.Add(ReturnStatement());
            else if (Is(Token_Class.Break) || Is(Token_Class.Continue)) node.Children.Add(ControlFlowStatement());
            else if (Is(Token_Class.Print)) node.Children.Add(PrintStatement());
            else ReportError("Expected a statement, found: " + CurLexeme());
            return node;
        }

        private Node AssignmentOrCall()
        {
            Node node = new Node("AssignmentOrCall");
            node.Children.Add(LHS());
            if (Is(Token_Class.Assign))
            {
                node.Children.Add(match(Token_Class.Assign));
                node.Children.Add(Expression());
            }
            else if (Is(Token_Class.LeftParen))
            {
                node.Children.Add(match(Token_Class.LeftParen));
                node.Children.Add(ArgListOpt());
                node.Children.Add(match(Token_Class.RightParen));
            }
            else
                ReportError("Expected '=' or '(' after identifier, found: " + CurLexeme());
            return node;
        }

        public Node LHS()
        {
            Node node = new Node("LHS");
            node.Children.Add(match_identifier());
            node.Children.Add(LHS_());
            return node;
        }

        public Node LHS_()
        {
            if (Is(Token_Class.LeftBracket))
            {
                Node node = new Node("LHS_");
                node.Children.Add(match(Token_Class.LeftBracket));
                node.Children.Add(Expression());
                node.Children.Add(match(Token_Class.RightBracket));
                node.Children.Add(LHS_());
                return node;
            }
            else if (Is(Token_Class.Dot))
            {
                Node node = new Node("LHS_");
                node.Children.Add(match(Token_Class.Dot));
                node.Children.Add(match_identifier());
                node.Children.Add(LHS_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node IfStatement()
        {
            Node node = new Node("IfStatement");
            node.Children.Add(match(Token_Class.If));
            node.Children.Add(Expression());
            node.Children.Add(Block());
            node.Children.Add(IfTail());
            return node;
        }

        public Node IfTail()
        {
            if (Is(Token_Class.Else))
            {
                Node node = new Node("IfTail");
                node.Children.Add(match(Token_Class.Else));
                node.Children.Add(Block());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node Forloop()
        {
            Node node = new Node("Forloop");
            node.Children.Add(match(Token_Class.For));
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.In));
            node.Children.Add(Expression());
            node.Children.Add(ForTail());
            return node;
        }

        public Node ForTail()
        {
            Node node = new Node("ForTail");
            if (Is(Token_Class.DotDot))
            {
                node.Children.Add(match(Token_Class.DotDot));
                node.Children.Add(Expression());
                node.Children.Add(Block());
            }
            else
                node.Children.Add(Block());
            return node;
        }

        public Node Whileloop()
        {
            Node node = new Node("Whileloop");
            node.Children.Add(match(Token_Class.While));
            node.Children.Add(Expression());
            node.Children.Add(Block());
            return node;
        }

        public Node ReturnStatement()
        {
            Node node = new Node("ReturnStatement");
            node.Children.Add(match(Token_Class.Return));
            node.Children.Add(ReturnTail());
            return node;
        }

        public Node ReturnTail()
        {
            if (FirstExpression())
            {
                Node node = new Node("ReturnTail");
                node.Children.Add(Expression());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ControlFlowStatement()
        {
            Node node = new Node("ControlFlowStatement");
            if (Is(Token_Class.Break)) node.Children.Add(match(Token_Class.Break));
            else if (Is(Token_Class.Continue)) node.Children.Add(match(Token_Class.Continue));
            else ReportError("Expected 'break' or 'continue', found: " + CurLexeme());
            return node;
        }

        public Node PrintStatement()
        {
            Node node = new Node("PrintStatement");
            node.Children.Add(match(Token_Class.Print));
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(ArgListOpt());
            node.Children.Add(match(Token_Class.RightParen));
            return node;
        }

        public Node FunctionCall()
        {
            Node node = new Node("FunctionCall");
            node.Children.Add(LHS());
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(ArgListOpt());
            node.Children.Add(match(Token_Class.RightParen));
            return node;
        }

        public Node Expression()
        {
            Node node = new Node("Expression");
            node.Children.Add(AddExpr());
            node.Children.Add(Expression_());
            return node;
        }

        public Node Expression_()
        {
            if (IsRelOp())
            {
                Node node = new Node("Expression_");
                node.Children.Add(RelOp());
                node.Children.Add(AddExpr());
                node.Children.Add(Expression_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node RelOp()
        {
            Node node = new Node("RelOp");
            if (IsRelOp())
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
            }
            else
                ReportError("Expected a comparison operator, found: " + CurLexeme());
            return node;
        }

        public Node AddExpr()
        {
            Node node = new Node("AddExpr");
            node.Children.Add(MultExpr());
            node.Children.Add(AddExpr_());
            return node;
        }

        public Node AddExpr_()
        {
            if (Is(Token_Class.Plus) || Is(Token_Class.Minus))
            {
                Node node = new Node("AddExpr_");
                node.Children.Add(AddOp());
                node.Children.Add(MultExpr());
                node.Children.Add(AddExpr_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node AddOp()
        {
            Node node = new Node("AddOp");
            if (Is(Token_Class.Plus) || Is(Token_Class.Minus))
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
            }
            else
                ReportError("Expected '+' or '-', found: " + CurLexeme());
            return node;
        }

        public Node MultExpr()
        {
            Node node = new Node("MultExpr");
            node.Children.Add(UnaryExpr());
            node.Children.Add(MultExpr_());
            return node;
        }

        public Node MultExpr_()
        {
            if (Is(Token_Class.Star) || Is(Token_Class.Slash) || Is(Token_Class.Percent))
            {
                Node node = new Node("MultExpr_");
                node.Children.Add(MulOp());
                node.Children.Add(UnaryExpr());
                node.Children.Add(MultExpr_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node MulOp()
        {
            Node node = new Node("MulOp");
            if (Is(Token_Class.Star) || Is(Token_Class.Slash) || Is(Token_Class.Percent))
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
            }
            else
                ReportError("Expected '*', '/' or '%', found: " + CurLexeme());
            return node;
        }

        public Node UnaryExpr()
        {
            Node node = new Node("UnaryExpr");
            if (Is(Token_Class.Minus) || IsNot())
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
                node.Children.Add(UnaryExpr());
            }
            else
                node.Children.Add(PostfixExpr());
            return node;
        }

        public Node PostfixExpr()
        {
            Node node = new Node("PostfixExpr");
            node.Children.Add(PrimaryExpr());
            node.Children.Add(PostfixExpr_());
            return node;
        }

        public Node PostfixExpr_()
        {
            if (Is(Token_Class.LeftParen))
            {
                Node node = new Node("PostfixExpr_");
                node.Children.Add(match(Token_Class.LeftParen));
                node.Children.Add(ArgListOpt());
                node.Children.Add(match(Token_Class.RightParen));
                node.Children.Add(PostfixExpr_());
                return node;
            }
            else if (Is(Token_Class.LeftBracket))
            {
                Node node = new Node("PostfixExpr_");
                node.Children.Add(match(Token_Class.LeftBracket));
                node.Children.Add(Expression());
                node.Children.Add(match(Token_Class.RightBracket));
                node.Children.Add(PostfixExpr_());
                return node;
            }
            else if (Is(Token_Class.Dot))
            {
                Node node = new Node("PostfixExpr_");
                node.Children.Add(match(Token_Class.Dot));
                node.Children.Add(match_identifier());
                node.Children.Add(PostfixExpr_());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node PrimaryExpr()
        {
            Node node = new Node("PrimaryExpr");
            if (IsIdentifier()) node.Children.Add(match_identifier());
            else if (IsLiteral()) node.Children.Add(Literal());
            else if (Is(Token_Class.LeftParen))
            {
                node.Children.Add(match(Token_Class.LeftParen));
                node.Children.Add(Expression());
                node.Children.Add(match(Token_Class.RightParen));
            }
            else if (Is(Token_Class.LeftBracket)) node.Children.Add(ArrayLiteral());
            else if (Is(Token_Class.New)) node.Children.Add(ObjectCreation());
            else if (Is(Token_Class.Input)) node.Children.Add(InputExpr());
            else ReportError("Expected an expression, found: " + CurLexeme());
            return node;
        }

        public Node Literal()
        {
            Node node = new Node("Literal");
            if (Is(Token_Class.Constant) || Is(Token_Class.True) ||
                Is(Token_Class.False) || Is(Token_Class.Null))
            {
                node.Children.Add(new Node(cur_token.token_type.ToString()));
                set_current_token_to_next_token();
            }
            else if (Is(Token_Class.DoubleQuote))
            {
                node.Children.Add(match(Token_Class.DoubleQuote));
                if (!Is(Token_Class.Text))
                {
                    ReportError("Expected \" ");
                    set_current_token_to_next_token();
                }
                else
                {

                Node ret = match(Token_Class.Text);
                node.Children.Add(ret);
                node.Children.Add(match(Token_Class.DoubleQuote));
                }
            }
            else if (Is(Token_Class.SingleQuote))
            {
                node.Children.Add(match(Token_Class.SingleQuote));
                if (Is(Token_Class.Text) && cur_token.lexeme.Length > 1 && !(cur_token.lexeme.Length == 2 && cur_token.lexeme[0] == '\\'))
                {
                    ReportError("Expected a single character between single quotes ");
                }
                if (!Is(Token_Class.Text))
                {
                    ReportError("Expected \' ");
                    set_current_token_to_next_token();
                }
                else
                {

                Node ret = match(Token_Class.Text);
           
                node.Children.Add(ret);
                node.Children.Add(match(Token_Class.SingleQuote));
                }
            }
            else
                ReportError("Expected a literal, found: " + CurLexeme());
            return node;
        }

        public Node ArrayLiteral()
        {
            Node node = new Node("ArrayLiteral");
            node.Children.Add(match(Token_Class.LeftBracket));
            node.Children.Add(ArgListOpt());
            node.Children.Add(match(Token_Class.RightBracket));
            return node;
        }

        public Node ObjectCreation()
        {
            Node node = new Node("ObjectCreation");
            node.Children.Add(match(Token_Class.New));
            node.Children.Add(match_identifier());
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(ArgListOpt());
            node.Children.Add(match(Token_Class.RightParen));
            return node;
        }

        public Node InputExpr()
        {
            Node node = new Node("InputExpr");
            node.Children.Add(match(Token_Class.Input));
            node.Children.Add(match(Token_Class.LeftParen));
            node.Children.Add(match(Token_Class.RightParen));
            return node;
        }

        public Node ArgListOpt()
        {
            if (FirstExpression())
            {
                Node node = new Node("ArgListOpt");
                node.Children.Add(ArgList());
                return node;
            }
            // else epsilon
            return null;
        }

        public Node ArgList()
        {
            Node node = new Node("ArgList");
            node.Children.Add(Expression());
            node.Children.Add(ArgList_());
            return node;
        }

        public Node ArgList_()
        {
            if (Is(Token_Class.Comma))
            {
                Node node = new Node("ArgList_");
                node.Children.Add(match(Token_Class.Comma));
                node.Children.Add(Expression());
                node.Children.Add(ArgList_());
                return node;
            }
            // else epsilon
            return null;
        }
        #endregion

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}