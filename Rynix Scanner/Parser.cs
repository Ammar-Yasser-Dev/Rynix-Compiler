using System;
using System.Collections.Generic;
using System.Text;

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

    internal class Parser
    {
        public List<Token> tokens = new List<Token>();
        public List<Error> errors = new List<Error>();
        Token cur_token;
        int cur_token_index = 0;
        bool at_end = false;          // true once every token has been consumed
        int last_error_pos = -1;      // used to suppress cascading errors on the same token

        public Parser(List<Token> _Tokens)
        {
            tokens = RemoveNoise(_Tokens);
            set_current_token_to_next_token();
            Program();
        }

        // =====================================================================
        //  The scanner emits tokens the grammar knows nothing about:
        //     NewLine                         -> dropped
        //     //  Text  NewLine               -> comment, dropped
        //     /*  Text  */                    -> comment, dropped
        //  (string / char literals are kept: they are parsed in Literal())
        // =====================================================================
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

        // =====================================================================
        //  Token helpers
        // =====================================================================
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

        // "init" is not a reserved word in your scanner, so it arrives as an Identifier.
        private bool IsInit()
        {
            return Is(Token_Class.Identifier) && cur_token.lexeme == "init";
        }

        // "!" is not recognised by your scanner (it comes out as an Invalid token with lexeme "!"),
        // so it is detected by lexeme. Works unchanged if you later add a Not token class.
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

        // text of the current token for error messages
        private string CurLexeme()
        {
            if (at_end) return "end of input";
            return cur_token.lexeme;
        }

        // =====================================================================
        //  Core helpers
        // =====================================================================
        private void set_current_token_to_next_token()
        {
            if (cur_token_index < tokens.Count)
                cur_token = tokens[cur_token_index++];
            else
                at_end = true;        // no more tokens
        }

        private void ReportError(string msg)
        {
            // One error per token position, otherwise one mistake produces a flood of errors.
            int pos = at_end ? cur_token_index + 1 : cur_token_index;
            if (pos == last_error_pos) return;
            last_error_pos = pos;
            errors.Add(new Error(msg));
        }

        // On failure: report the error and do NOT consume (acts as if the token had been inserted).
        private void match(Token_Class expected)
        {
            if (Is(expected))
                set_current_token_to_next_token();
            else
                ReportError("unexpected token, Expected: " + expected.ToString() + ", found: " + CurLexeme());
        }

        private void match_identifier()
        {
            match(Token_Class.Identifier);
        }

        // =====================================================================
        //  FIRST-set helpers (decide ε-productions and choices)
        // =====================================================================
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

        // =====================================================================
        //  Program Structure
        // =====================================================================
        public void Program()
        {
            if (IsEOF())
            {
                ReportError("Expected at least one declaration or statement");
                return;
            }

            // Panic-mode recovery at the top level: skip tokens that can't start anything.
            while (!IsEOF())
            {
                if (FirstTopLevelItem())
                    TopLevelList();
                else
                {
                    ReportError("unexpected token ' " + CurLexeme() + " ' at top level");
                    set_current_token_to_next_token();
                }
            }
        }

        public void TopLevelList()
        {
            TopLevelItem();
            TopLevelList_();
        }

        public void TopLevelList_()
        {
            if (FirstTopLevelItem())
            {
                TopLevelItem();
                TopLevelList_();
            }
            // else ε
        }

        public void TopLevelItem()
        {
            if (Is(Token_Class.Fun)) FunctionDecl();
            else if (Is(Token_Class.Class)) ClassDecl();
            else Statement();
        }

        // =====================================================================
        //  Variable and Constant Declarations
        // =====================================================================
        public void VarDecl()
        {
            match(Token_Class.Var);
            match_identifier();
            VarDeclTail();
        }

        public void VarDeclTail()
        {
            if (Is(Token_Class.Colon))
            {
                match(Token_Class.Colon);
                Type();
                VarInit();
            }
            else if (Is(Token_Class.Assign))
            {
                match(Token_Class.Assign);
                Expression();
            }
            else
                ReportError("Expected ':' or '=' after variable name, found: " + CurLexeme());
        }

        public void VarInit()
        {
            if (Is(Token_Class.Assign))
            {
                match(Token_Class.Assign);
                Expression();
            }
            // else ε
        }

        public void ValDecl()
        {
            match(Token_Class.Val);
            match_identifier();
            ValDeclTail();
        }

        public void ValDeclTail()
        {
            if (Is(Token_Class.Colon))
            {
                match(Token_Class.Colon);
                Type();
                match(Token_Class.Assign);
                Expression();
            }
            else if (Is(Token_Class.Assign))
            {
                match(Token_Class.Assign);
                Expression();
            }
            else
                ReportError("Expected ':' or '=' after constant name, found: " + CurLexeme());
        }

        // =====================================================================
        //  Data Types
        // =====================================================================
        public void Type()
        {
            if (IsPrimitiveType())
                set_current_token_to_next_token();
            else if (Is(Token_Class.Array))
                ArrayType();
            else if (IsIdentifier())
                match_identifier();
            else
                ReportError("Expected a type, found: " + CurLexeme());
        }

        public void ArrayType()
        {
            match(Token_Class.Array);
            match(Token_Class.Less);
            Type();
            match(Token_Class.Greater);
        }

        // =====================================================================
        //  Functions and Parameters
        // =====================================================================
        public void FunctionDecl()
        {
            match(Token_Class.Fun);
            match_identifier();
            match(Token_Class.LeftParen);
            ParamsListOptional();
            match(Token_Class.RightParen);
            match(Token_Class.Colon);
            Type();
            Block();
        }

        public void ParamsListOptional()
        {
            if (IsIdentifier())
                ParamList();
            // else ε
        }

        public void ParamList()
        {
            Param();
            ParamList_();
        }

        public void ParamList_()
        {
            if (Is(Token_Class.Comma))
            {
                match(Token_Class.Comma);
                Param();
                ParamList_();
            }
            // else ε
        }

        public void Param()
        {
            match_identifier();
            match(Token_Class.Colon);
            Type();
        }

        // =====================================================================
        //  Classes and Constructors
        // =====================================================================
        public void ClassDecl()
        {
            match(Token_Class.Class);
            match_identifier();
            match(Token_Class.LeftBrace);
            ClassBodyOptional();

            // Panic-mode recovery: skip junk inside the class body until "}" or the next member.
            while (!Is(Token_Class.RightBrace) && !IsEOF())
            {
                ReportError("unexpected token '" + CurLexeme() + "' in class body");
                set_current_token_to_next_token();
                if (FirstClassMember())
                    ClassBodyOptional();
            }

            match(Token_Class.RightBrace);
        }

        public void ClassBodyOptional()
        {
            if (FirstClassMember())
                ClassBody();
            // else ε
        }

        public void ClassBody()
        {
            ClassMember();
            ClassBody_();
        }

        public void ClassBody_()
        {
            if (FirstClassMember())
            {
                ClassMember();
                ClassBody_();
            }
            // else ε
        }

        public void ClassMember()
        {
            if (Is(Token_Class.Var)) VarDecl();
            else if (Is(Token_Class.Val)) ValDecl();
            else if (Is(Token_Class.Fun)) FunctionDecl();
            else if (IsInit()) ConstructorDecl();
            else ReportError("Expected a class member, found: " + CurLexeme());
        }

        public void ConstructorDecl()
        {
            match_identifier();                 // the "init" keyword
            match(Token_Class.LeftParen);
            ParamsListOptional();
            match(Token_Class.RightParen);
            Block();
        }

        // =====================================================================
        //  Blocks and Statements
        // =====================================================================
        public void Block()
        {
            match(Token_Class.LeftBrace);
            StatementListOptional();

            // Panic-mode recovery: skip junk inside the block until "}" or the next statement.
            while (!Is(Token_Class.RightBrace) && !IsEOF())
            {
                ReportError("unexpected token ' " + CurLexeme() + " ' in block");
                set_current_token_to_next_token();
                if (FirstStatement())
                    StatementListOptional();
            }

            match(Token_Class.RightBrace);
        }

        public void StatementListOptional()
        {
            if (FirstStatement())
                StatementList();
            // else ε
        }

        public void StatementList()
        {
            Statement();
            StatementList_();
        }

        public void StatementList_()
        {
            if (FirstStatement())
            {
                Statement();
                StatementList_();
            }
            // else ε
        }

        public void Statement()
        {
            if (Is(Token_Class.Var)) VarDecl();
            else if (Is(Token_Class.Val)) ValDecl();
            else if (IsIdentifier()) AssignmentOrCall();   // Assignment | FunctionCall share the LHS prefix
            else if (Is(Token_Class.If)) IfStatement();
            else if (Is(Token_Class.For)) Forloop();
            else if (Is(Token_Class.While)) Whileloop();
            else if (Is(Token_Class.Return)) ReturnStatement();
            else if (Is(Token_Class.Break) || Is(Token_Class.Continue)) ControlFlowStatement();
            else if (Is(Token_Class.Print)) PrintStatement();
            else ReportError("Expected a statement, found: " + CurLexeme());
        }

        // Assignment   -> LHS "=" Expression      (not defined in your grammar, assumed this form)
        // FunctionCall -> LHS "(" ArgListOpt ")"
        // Both start with LHS, so parse LHS once and decide on the next token.
        private void AssignmentOrCall()
        {
            LHS();
            if (Is(Token_Class.Assign))
            {
                match(Token_Class.Assign);
                Expression();
            }
            else if (Is(Token_Class.LeftParen))
            {
                match(Token_Class.LeftParen);
                ArgListOpt();
                match(Token_Class.RightParen);
            }
            else
                ReportError("Expected '=' or '(' after identifier, found: " + CurLexeme());
        }

        // =====================================================================
        //  Control Flow and Statements
        // =====================================================================
        public void LHS()
        {
            match_identifier();
            LHS_();
        }

        public void LHS_()
        {
            if (Is(Token_Class.LeftBracket))
            {
                match(Token_Class.LeftBracket);
                Expression();
                match(Token_Class.RightBracket);
                LHS_();
            }
            else if (Is(Token_Class.Dot))
            {
                match(Token_Class.Dot);
                match_identifier();
                LHS_();
            }
            // else ε
        }

        public void IfStatement()
        {
            match(Token_Class.If);
            Expression();
            Block();
            IfTail();
        }

        public void IfTail()
        {
            if (Is(Token_Class.Else))
            {
                match(Token_Class.Else);
                Block();
            }
            // else ε
        }

        public void Forloop()
        {
            match(Token_Class.For);
            match_identifier();
            match(Token_Class.In);
            Expression();
            ForTail();
        }

        public void ForTail()
        {
            if (Is(Token_Class.DotDot))
            {
                match(Token_Class.DotDot);
                Expression();
                Block();
            }
            else
                Block();
        }

        public void Whileloop()
        {
            match(Token_Class.While);
            Expression();
            Block();
        }

        public void ReturnStatement()
        {
            match(Token_Class.Return);
            ReturnTail();
        }

        public void ReturnTail()
        {
            if (FirstExpression())
                Expression();
            // else ε
        }

        public void ControlFlowStatement()
        {
            if (Is(Token_Class.Break)) match(Token_Class.Break);
            else if (Is(Token_Class.Continue)) match(Token_Class.Continue);
            else ReportError("Expected 'break' or 'continue', found: " + CurLexeme());
        }

        public void PrintStatement()
        {
            match(Token_Class.Print);
            match(Token_Class.LeftParen);
            ArgListOpt();
            match(Token_Class.RightParen);
        }

        // Not called directly (see AssignmentOrCall), kept to mirror the grammar.
        public void FunctionCall()
        {
            LHS();
            match(Token_Class.LeftParen);
            ArgListOpt();
            match(Token_Class.RightParen);
        }

        // =====================================================================
        //  Expressions and Precedence
        // =====================================================================
        public void Expression()
        {
            AddExpr();
            Expression_();
        }

        public void Expression_()
        {
            if (IsRelOp())
            {
                RelOp();
                AddExpr();
                Expression_();
            }
            // else ε
        }

        public void RelOp()
        {
            if (IsRelOp())
                set_current_token_to_next_token();
            else
                ReportError("Expected a comparison operator, found: " + CurLexeme());
        }

        public void AddExpr()
        {
            MultExpr();
            AddExpr_();
        }

        public void AddExpr_()
        {
            if (Is(Token_Class.Plus) || Is(Token_Class.Minus))
            {
                AddOp();
                MultExpr();
                AddExpr_();
            }
            // else ε
        }

        public void AddOp()
        {
            if (Is(Token_Class.Plus) || Is(Token_Class.Minus))
                set_current_token_to_next_token();
            else
                ReportError("Expected '+' or '-', found: " + CurLexeme());
        }

        public void MultExpr()
        {
            UnaryExpr();
            MultExpr_();
        }

        public void MultExpr_()
        {
            if (Is(Token_Class.Star) || Is(Token_Class.Slash) || Is(Token_Class.Percent))
            {
                MulOp();
                UnaryExpr();
                MultExpr_();
            }
            // else ε
        }

        public void MulOp()
        {
            if (Is(Token_Class.Star) || Is(Token_Class.Slash) || Is(Token_Class.Percent))
                set_current_token_to_next_token();
            else
                ReportError("Expected '*', '/' or '%', found: " + CurLexeme());
        }

        public void UnaryExpr()
        {
            if (Is(Token_Class.Minus) || IsNot())
            {
                set_current_token_to_next_token();
                UnaryExpr();
            }
            else
                PostfixExpr();
        }

        public void PostfixExpr()
        {
            PrimaryExpr();
            PostfixExpr_();
        }

        public void PostfixExpr_()
        {
            if (Is(Token_Class.LeftParen))
            {
                match(Token_Class.LeftParen);
                ArgListOpt();
                match(Token_Class.RightParen);
                PostfixExpr_();
            }
            else if (Is(Token_Class.LeftBracket))
            {
                match(Token_Class.LeftBracket);
                Expression();
                match(Token_Class.RightBracket);
                PostfixExpr_();
            }
            else if (Is(Token_Class.Dot))
            {
                match(Token_Class.Dot);
                match_identifier();
                PostfixExpr_();
            }
            // else ε
        }

        public void PrimaryExpr()
        {
            if (IsIdentifier()) match_identifier();
            else if (IsLiteral()) Literal();
            else if (Is(Token_Class.LeftParen))
            {
                match(Token_Class.LeftParen);
                Expression();
                match(Token_Class.RightParen);
            }
            else if (Is(Token_Class.LeftBracket)) ArrayLiteral();
            else if (Is(Token_Class.New)) ObjectCreation();
            else if (Is(Token_Class.Input)) InputExpr();
            else ReportError("Expected an expression, found: " + CurLexeme());
        }

        // =====================================================================
        //  Literals and Arguments
        // =====================================================================
        // Numbers / true / false / null are one token.
        // Strings and chars are three tokens:  "  Text  "   and   '  Text  '
        public void Literal()
        {
            if (Is(Token_Class.Constant) || Is(Token_Class.True) ||
                Is(Token_Class.False) || Is(Token_Class.Null))
            {
                set_current_token_to_next_token();
            }
            else if (Is(Token_Class.DoubleQuote))
            {
                match(Token_Class.DoubleQuote);
                match(Token_Class.Text);
                match(Token_Class.DoubleQuote);
            }
            else if (Is(Token_Class.SingleQuote))
            {
                match(Token_Class.SingleQuote);
                match(Token_Class.Text);
                match(Token_Class.SingleQuote);
            }
            else
                ReportError("Expected a literal, found: " + CurLexeme());
        }

        public void ArrayLiteral()
        {
            match(Token_Class.LeftBracket);
            ArgListOpt();
            match(Token_Class.RightBracket);
        }

        public void ObjectCreation()
        {
            match(Token_Class.New);
            match_identifier();
            match(Token_Class.LeftParen);
            ArgListOpt();
            match(Token_Class.RightParen);
        }

        public void InputExpr()
        {
            match(Token_Class.Input);
            match(Token_Class.LeftParen);
            match(Token_Class.RightParen);
        }

        public void ArgListOpt()
        {
            if (FirstExpression())
                ArgList();
            // else ε
        }

        public void ArgList()
        {
            Expression();
            ArgList_();
        }

        public void ArgList_()
        {
            if (Is(Token_Class.Comma))
            {
                match(Token_Class.Comma);
                Expression();
                ArgList_();
            }
            // else ε
        }
    }
}