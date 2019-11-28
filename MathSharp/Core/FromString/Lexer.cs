using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Core.FromString
{
    public class TokenList : List<Token>
    {
        public void Add(Token a)
        {
            if (!string.IsNullOrEmpty(a.Value))
                base.Add(a);
        }
    }
    public class Token
    {
        public Token(char s)
        {
            Value = s.ToString();
        }
        public Token()
        {
            Value = "";
        }
        public static readonly string goodCharsForNumbers = "1234567890i.";
        public static readonly string goodCharsForVars = "qwertyuiopasdfghjklzxcvbnm";
        public static readonly string goodCharsForOperators = "+-*/^,";
        public enum TokenType
        {
            VARIABLE,
            NUMBER,
            FUNCTION,
            SYMBOL
        }
        public enum BracketType
        {
            PARENTHESIS_OPEN, 
            PARENTHESIS_CLOSE,
            BRACKET_OPEN,
            BRACKET_CLOSE,
            NONE
        }
        public string Value { get; set; }
        public Entity Attribute { get; set; }
        public static BracketType GetBracketType(char s)
        {
            switch(s)
            {
                case '(':
                    return BracketType.PARENTHESIS_OPEN;
                case ')':
                    return BracketType.PARENTHESIS_CLOSE;
                case '[':
                    return BracketType.BRACKET_OPEN;
                case ']':
                    return BracketType.PARENTHESIS_CLOSE;
                default:
                    return BracketType.NONE;
            }
        }
        public static bool IsNumber(string s) //TODO WITH REGEXP
        {
            foreach (var c in s)
                if (!goodCharsForNumbers.Contains(c))
                    return false;
            return true;
        }
        public static bool IsVariable(string s) //TODO WITH REGEXP
        {
            foreach (var c in s)
                if (!goodCharsForVars.Contains(c))
                    return false;
            return true;
        }
        public static bool IsOperator(char s) //TODO WITH REGEXP
        {
            return goodCharsForOperators.Contains(s);
        }
        public override string ToString()
        {
            return "{" + Value.ToString() + "}";
        }
    }
    public class Lexer
    {
        public TokenList tokens;
        public Lexer(string src)
        {
            tokens = new TokenList();
            var last = new Token();
            foreach(var symbol in src)
            {
                if (symbol == ' ')
                {
                    tokens.Add(last);
                    last = new Token();
                    continue;
                }
                if(Token.IsOperator(symbol))
                {
                    tokens.Add(new Token(symbol));
                    last = new Token();
                    continue;
                }
                if (Token.IsNumber(last.Value + symbol) || Token.IsVariable(last.Value + symbol))
                    last.Value += symbol;
                else
                {
                    if (Token.GetBracketType(symbol) != Token.BracketType.NONE)
                    {
                        tokens.Add(last);
                        tokens.Add(new Token(symbol));
                        last = new Token();
                    }
                }
            }
            tokens.Add(last);
        }
    }
}
