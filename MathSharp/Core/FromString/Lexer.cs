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
    public partial class Token
    {
        public Token(char s)
        {
            Value = s.ToString();
        }
        public Token()
        {
            Value = "";
        }
        public enum TokenType
        {
            VARIABLE,
            NUMBER,
            FUNCTION,
            SYMBOL,
            BRACE
        }
        public enum BraceType
        {
            PARENTHESIS_OPEN, 
            PARENTHESIS_CLOSE,
            BRACKET_OPEN,
            BRACKET_CLOSE,
            NONE
        }
        public TokenType type;
        public string Value { get; set; }
        public Entity Attribute { get; set; }
        public static BraceType GetBracketType(char s)
        {
            switch(s)
            {
                case '(':
                    return BraceType.PARENTHESIS_OPEN;
                case ')':
                    return BraceType.PARENTHESIS_CLOSE;
                case '[':
                    return BraceType.BRACKET_OPEN;
                case ']':
                    return BraceType.PARENTHESIS_CLOSE;
                default:
                    return BraceType.NONE;
            }
        }
        public static bool IsNumber(string s) //TODO WITH REGEXP
        {
            foreach (var c in s)
                if (!SyntaxInfo.goodCharsForNumbers.Contains(c))
                    return false;
            return true;
        }
        public static bool IsVariable(string s) //TODO WITH REGEXP
        {
            foreach (var c in s)
                if (!SyntaxInfo.goodCharsForVars.Contains(c))
                    return false;
            return true;
        }
        public static bool IsOperator(char s) //TODO WITH REGEXP
        {
            return SyntaxInfo.goodCharsForOperators.Contains(s);
        }
        public override string ToString()
        {
            return "{" + Value.ToString() + " | " + this.type.ToString() + "}";
        }
        public void Seal()
        {
            if (IsNumber(this.Value))
                type = TokenType.NUMBER;
            else if (IsVariable(this.Value))
            {
                if (SyntaxInfo.goodStringsForFunctions.ContainsKey(this.Value))
                    type = TokenType.FUNCTION;
                else
                    type = TokenType.VARIABLE;
            }
            else if (GetBracketType(this.Value[0]) != BraceType.NONE)
                type = TokenType.BRACE;
            else
                type = TokenType.SYMBOL;
        }
    }
    public class Lexer
    {
        private TokenList tokens;
        private int index = 0;
        public void Seal()
        {
            foreach (var token in tokens)
                token.Seal();
        }
        public bool EOF()
        {
            return index >= tokens.Count;
        }
        public void Next()
        {
            index++;
        }
        public void Prev()
        {
            index = Math.Max(index - 1, 0);
        }
        public Token Current()
        {
            return tokens[index];
        }
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
                    if (Token.GetBracketType(symbol) != Token.BraceType.NONE)
                    {
                        tokens.Add(last);
                        tokens.Add(new Token(symbol));
                        last = new Token();
                    }
                }
            }
            tokens.Add(last);
            Seal();
        }
    }
}
