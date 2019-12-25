using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AngouriMath.Core.FromString
{
    internal class TokenList : List<Token>
    {
        public new void Add(Token a)
        {
            if (!string.IsNullOrEmpty(a.Value))
            {
                a.Seal();
                base.Add(a);
            }
        }
    }
    internal partial class Token
    {
        internal Token Copy()
        {
            var res = new Token();
            res.Value = Value;
            res.Type = Type;
            return res;
        }
        internal Token(char s)
        {
            Value = s.ToString();
            Type = TokenType.NONE;
        }
        internal Token()
        {
            Value = "";
            Type = TokenType.NONE;
        }
        internal enum TokenType
        {
            VARIABLE,
            NUMBER,
            FUNCTION,
            SYMBOL,
            PARENTHESIS_OPEN,
            PARENTHESIS_CLOSE,
            BRACE,
            NONE
        }
        internal enum BraceType
        {
            PARENTHESIS_OPEN, 
            PARENTHESIS_CLOSE,
            BRACKET_OPEN,
            BRACKET_CLOSE,
            NONE
        }
        internal TokenType Type { get; private set; }
        internal string Value { get; set; }
        internal Entity Attribute { get; private set; }
        internal static BraceType GetBraceType(char s)
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
        internal static readonly Regex numberRegexp = new Regex(@"^-?(\d+(\.(\d+)?)?)?i?$");
        internal static readonly Regex variableRegexp = new Regex(@"^[a-zA-Z]+$");
        internal static bool IsNumber(string s)
        {
            return numberRegexp.IsMatch(s);
        }
        internal static bool IsVariable(string s)
        {
            return variableRegexp.IsMatch(s);
        }
        internal static bool IsOperator(string s) //TOOD
        {
            return SyntaxInfo.goodCharsForOperators.Contains(s);
        }

        /// <summary>
        /// View a token with this meethod
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{" + Value + " | " + this.Type.ToString() + "}";
        }
        internal TokenType GetCurrentType()
        {
            TokenType type;
            if (this.Value == "")
                return TokenType.NONE;
            if (IsNumber(this.Value))
                type = TokenType.NUMBER;
            else if (IsVariable(this.Value))
            {
                if (SyntaxInfo.goodStringsForFunctions.ContainsKey(this.Value))
                {
                    type = TokenType.FUNCTION;
                }
                else
                {
                    type = TokenType.VARIABLE;
                }
            }
            else if (this.Value.Length == 1 && BraceProcessor.parentheses.Contains(Token.GetBraceType(Value[0])))
            {
                type = Token.GetBraceType(Value[0]) == BraceType.PARENTHESIS_OPEN ? TokenType.PARENTHESIS_OPEN : TokenType.PARENTHESIS_CLOSE;
            }
            else if (GetBraceType(this.Value[0]) != BraceType.NONE)
            {
                type = TokenType.BRACE;
            }
            else
            {
                type = TokenType.SYMBOL;
            }
            return type;
        }
        internal void Seal()
        {
            Type = GetCurrentType();
        }
        internal static bool IsFinishedType(TokenType type)
        {
            return (type == Token.TokenType.PARENTHESIS_CLOSE || type == Token.TokenType.NUMBER || type == Token.TokenType.VARIABLE);
        }
    }
    internal class Lexer
    {
        private readonly TokenList tokens;
        private int index;
        internal void Seal()
        {
            foreach (var token in tokens)
                token.Seal();
        }
        internal bool EOF()
        {
            return index >= tokens.Count;
        }
        internal void Next()
        {
            index++;
        }
        internal Token GlanceNext()
        {
            if (index >= tokens.Count)
            {
                return new Token('?');
            }
            else
            {
                return tokens[index + 1];
            }
        }
        internal void Prev()
        {
            index = Math.Max(index - 1, 0);
        }
        internal Token Current
        {
            get => tokens[index];
        }
        internal void ToBegin()
        {
            index = 0;
        }
        internal Lexer(string src)
        {
            tokens = new TokenList();
            var last = new Token();
            Func<Token.TokenType> GetLastType = () => 
            (last.GetCurrentType() != Token.TokenType.NONE) ? last.GetCurrentType() : (tokens.Count > 0 ? tokens[tokens.Count - 1].GetCurrentType() : Token.TokenType.NONE);
            for (int i = 0; i < src.Length; i++)
            {
                var symbol = src[i];
                if (symbol == ' ')
                {
                    tokens.Add(last);
                    last = new Token();
                    continue;
                }
                if (Token.IsOperator(symbol.ToString()) && (Token.IsFinishedType(GetLastType()) || symbol != '-'))
                {
                    tokens.Add(last);
                    tokens.Add(new Token(symbol));
                    last = new Token();
                    continue;
                }
                if (Token.IsNumber(last.Value + symbol) || Token.IsVariable(last.Value + symbol) || symbol == '-')
                {
                    last.Value += symbol;
                }
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
