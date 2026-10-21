using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public enum Token_Class
{
    Var, Val, Fun, Class, If, Else, While, For, In, Return, True, False, Null, New, Print, Input, Break, Continue,
    Plus, Minus, Star, Slash, Percent, Assign, EqualEqual, Greater, GreaterEqual, Less, LessEqual, LeftParen, RightParen,
    LeftBrace, RightBrace, LeftBracket, RightBracket, LeftAngle,
    RightAngle, Colon, Comma, Dot, DoubleQuote, SingleQuote, DoubleSlash, SlashStar, StarSlash, Identifier, Constant,Invalid, newLine,
    Int, Float, String, Bool, Char, Array , Void , DotDot,NotEqual
}


namespace Rynix_Scanner
{

    public class Token
    {
        public string lexeme;
        public Token_Class token_type;
    }

    internal class Scanner
    {

        public List<Token> Tokens = new List<Token>(); // for all tokens it is used through the entire run

        //for reserved words and operators used only once in the constructor do not modify during the run
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>(); 
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        //this constructor handles all reserved words and operators 
        public Scanner()
        {
            // 1. Reserved Words
            ReservedWords.Add("var", Token_Class.Var);
            ReservedWords.Add("val", Token_Class.Val);
            ReservedWords.Add("fun", Token_Class.Fun);
            ReservedWords.Add("class", Token_Class.Class);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("while", Token_Class.While);
            ReservedWords.Add("for", Token_Class.For);
            ReservedWords.Add("in", Token_Class.In);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("true", Token_Class.True);
            ReservedWords.Add("false", Token_Class.False);
            ReservedWords.Add("null", Token_Class.Null);
            ReservedWords.Add("new", Token_Class.New);
            ReservedWords.Add("print", Token_Class.Print);
            ReservedWords.Add("input", Token_Class.Input);
            ReservedWords.Add("break", Token_Class.Break);
            ReservedWords.Add("continue", Token_Class.Continue);

            ReservedWords.Add("Int", Token_Class.Int);
            ReservedWords.Add("Float", Token_Class.Float);
            ReservedWords.Add("String", Token_Class.String);
            ReservedWords.Add("Bool", Token_Class.Bool);
            ReservedWords.Add("Char", Token_Class.Char);
            ReservedWords.Add("Array", Token_Class.Array);
            ReservedWords.Add("Void", Token_Class.Void);
  


            // 2. Operators & Symbols 
            Operators.Add("+", Token_Class.Plus);
            Operators.Add("-", Token_Class.Minus);
            Operators.Add("*", Token_Class.Star);
            Operators.Add("/", Token_Class.Slash);
            Operators.Add("%", Token_Class.Percent);
            Operators.Add("=", Token_Class.Assign);
            Operators.Add("==", Token_Class.EqualEqual);
            Operators.Add(">", Token_Class.Greater);
            Operators.Add(">=", Token_Class.GreaterEqual);
            Operators.Add("<", Token_Class.Less);
            Operators.Add("<=", Token_Class.LessEqual);
            Operators.Add("(", Token_Class.LeftParen);
            Operators.Add(")", Token_Class.RightParen);
            Operators.Add("{", Token_Class.LeftBrace);
            Operators.Add("}", Token_Class.RightBrace);
            Operators.Add("[", Token_Class.LeftBracket);
            Operators.Add("]", Token_Class.RightBracket);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add("\"", Token_Class.DoubleQuote);
            Operators.Add("'", Token_Class.SingleQuote);
            Operators.Add("//", Token_Class.DoubleSlash);
            Operators.Add("/*", Token_Class.SlashStar);
            Operators.Add("*/", Token_Class.StarSlash);
            Operators.Add("\n", Token_Class.newLine);
            Operators.Add("..", Token_Class.DotDot);
            Operators.Add("!=", Token_Class.NotEqual);


        }

        public void Scan(string code)
        {
            string reserved_words =
            @"\b(var|val|fun|class|if|else|while|for|in|return|true|false|null|new|print|input|break|continue)\b";

            string operators = @"==|>=|<=|//|/\*|\*/|\+|\-|\*|/|%|=|>|<|\(|\)|\{|\}|\[|\]|:|,|\.|""|'|\n|\.\.|!=";

            string identifiers = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b";

            string constants = @"\b[0-9]+(\.[0-9]+)?\b";

            string valid_tokens = reserved_words+ "|" + operators + "|" + identifiers + "|" + constants;

            string invalid_identefier = @$"\b[0-9]+[a-zA-Z_][a-zA-Z0-9_]*\b";
            string invalid_character = @"[^a-zA-Z0-9_=/\*\+\-\%=><\(\)\{\}\[\]:,\.""'\n\s!]";
            string invalid_tokens =  invalid_identefier + "|" + invalid_character;//not valid
     
            MatchCollection Code_matches = Regex.Matches(code, valid_tokens);
            MatchCollection invalid_Code_matches = Regex.Matches(code, invalid_tokens);

            MatchCollection all_matches = Regex.Matches(code, valid_tokens+"|"+invalid_tokens);




            foreach (Match match in all_matches) 
            {
                Token t = new Token();
                t.lexeme = match.Value;

                if (ReservedWords.ContainsKey(match.Value))
                {
                    t.token_type = ReservedWords[match.Value];
                    Tokens.Add(t);
                }
                else if (Operators.ContainsKey(match.Value))
                {
                    t.token_type = Operators[match.Value];
                    Tokens.Add(t);
                }
                else if (Regex.IsMatch(match.ToString(), identifiers))
                {
                    t.token_type = Token_Class.Identifier;
                    Tokens.Add(t);
                }
                else if (Regex.IsMatch(match.ToString(), constants))
                {
                    t.token_type = Token_Class.Constant;
                    Tokens.Add(t);
                }
                else
                {
                    t.token_type = Token_Class.Invalid;
                    Tokens.Add(t);
                }

            }



        }
    }
}