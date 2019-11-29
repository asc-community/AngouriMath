using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        public Token Copy()
        {
            var res = new Token();
            res.Value = Value;
            res.Type = Type;
            return res;
        }
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
            PARENTHESIS_OPEN,
            PARENTHESIS_CLOSE,
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
        public TokenType Type;
        public string Value { get; set; }
        public Entity Attribute { get; set; }
        public static BraceType GetBraceType(char s)
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
        public static Regex numberRegexp = new Regex(@"^-?(\d+(\.\d+)?)?i?$");
        public static Regex variableRegexp = new Regex(@"^[a-zA-Z]+$");
        public static bool IsNumber(string s)
        {
            return numberRegexp.IsMatch(s);
        }
        public static bool IsVariable(string s)
        {
            return variableRegexp.IsMatch(s);
        }
        public static bool IsOperator(string s) //TOOD
        {
            return SyntaxInfo.goodCharsForOperators.Contains(s);
        }
        public override string ToString()
        {
            return "{" + Value.ToString() + " | " + this.Type.ToString() + "}";
        }
        public void Seal()
        {
            if (IsNumber(this.Value))
                Type = TokenType.NUMBER;
            else if (IsVariable(this.Value))
            {
                if (SyntaxInfo.goodStringsForFunctions.ContainsKey(this.Value))
                    Type = TokenType.FUNCTION;
                else
                    Type = TokenType.VARIABLE;
            }
            else if (this.Value.Length == 1 && BraceProcessor.parentheses.Contains(Token.GetBraceType(Value[0])))
                Type = Token.GetBraceType(Value[0]) == BraceType.PARENTHESIS_OPEN ? TokenType.PARENTHESIS_OPEN : TokenType.PARENTHESIS_CLOSE;
            else if (GetBraceType(this.Value[0]) != BraceType.NONE)
                Type = TokenType.BRACE;
            else
                Type = TokenType.SYMBOL;
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
        public void ToBegin()
        {
            index = 0;
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
                if(Token.IsOperator(symbol.ToString()))
                {
                    tokens.Add(new Token(symbol));
                    last = new Token();
                    continue;
                }
                if (Token.IsNumber(last.Value + symbol) || Token.IsVariable(last.Value + symbol))
                    last.Value += symbol;
                else
                {
                    if (Token.GetBraceType(symbol) != Token.BraceType.NONE)
                    {
                        tokens.Add(last);
                        tokens.Add(new Token(symbol));
                        last = new Token();
                    }
                }
            }
            tokens.Add(last);
            Seal();
            ToBegin();
        }
    }
}
