
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AngouriMath.Core.FromString
{
    using TokenType = Token.TokenType;
    internal class TokenList : List<Token>
    {
        /// <summary>
        /// Safely appends a token to a list
        /// </summary>
        /// <param name="a"></param>
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
        /// <summary>
        /// Safe copy of a token
        /// </summary>
        /// <returns></returns>
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
        internal static readonly Regex variableRegexp = new Regex(@"^[a-zA-ZΑ-Ωα-ω_]+$");
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

        /// <summary>
        /// Normal way to get the type of a token is to seal it and resolve the type
        /// But sometimes we need to get it without sealing the token
        /// </summary>
        /// <returns></returns>
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
            if (Value == "-")
                Type = TokenType.SYMBOL;
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

        /// <summary>
        /// Seals all the tokens
        /// </summary>
        internal void Seal()
        {
            foreach (var token in tokens)
                token.Seal();
        }

        /// <summary>
        /// If EOF, there's no tokens remaining
        /// </summary>
        /// <returns></returns>
        internal bool EOF()
        {
            return index >= tokens.Count;
        }

        /// <summary>
        /// Goes to the next
        /// Current changes
        /// </summary>
        internal void Next()
        {
            index++;
        }

        /// <summary>
        /// Goes to the next without changing Current
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// If needed, we have a way back
        /// </summary>
        internal void Prev()
        {
            index = Math.Max(index - 1, 0);
        }

        /// <summary>
        /// Property. Returns the current token
        /// </summary>
        internal Token Current
        {
            get => tokens[index];
        }

        /// <summary>
        /// Resets the lexer
        /// </summary>
        internal void ToBegin()
        {
            index = 0;
        }


        /// <summary>
        /// DOCTODO
        /// </summary>
        /// <param name="src"></param>
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
                    // If we encounter a brace, we add the brace
                    if (Token.GetBraceType(symbol) != Token.BraceType.NONE)
                    {
                        tokens.Add(last);
                        tokens.Add(new Token(symbol));
                        last = new Token();
                    }
                    // Otherwise, we should deal with it as with normal token
                    else
                    {
                        tokens.Add(last);
                        last = new Token(symbol);
                    }
                }
            }
            tokens.Add(last);
            Seal();
            ToBegin();
        }

        /// <summary>
        /// Inserts operators in between operands where they are omitted,
        /// for example, 2x -> 2 * x
        /// </summary>
        internal void AddOmittedOperators()
        {
            /// <summary>
            /// Provided two types of tokens, returns position of first token if
            /// the pair if found, -1 otherwisely.
            /// </summary>
            int FindSubPair(TokenType t1, TokenType t2)
            {
                for (int i = 0; i < tokens.Count - 1; i++)
                    if (tokens[i].Type == t1 && tokens[i + 1].Type == t2)
                        return i;
                return -1;
            }

            /// <summary>
            /// Finds all occurances of [t1, t2] and inserts token in between each of them
            /// </summary>
            void InsertIntoPair(TokenType t1, TokenType t2, Token token)
            {
                int pos;
                while ((pos = FindSubPair(t1, t2)) != -1)
                {
                    tokens.Insert(pos + 1 /* we need to keep the first one behind*/, token);
                }
            }

            var multiplyer = new Token('*'); multiplyer.Seal();
            var power = new Token('^'); power.Seal();

            // 2x -> 2 * x
            InsertIntoPair(TokenType.NUMBER, TokenType.VARIABLE, multiplyer);

            // x y -> x * y
            InsertIntoPair(TokenType.VARIABLE, TokenType.VARIABLE, multiplyer);

            // 2( -> 2 * (
            InsertIntoPair(TokenType.NUMBER, TokenType.PARENTHESIS_OPEN, multiplyer);

            // )2 -> ) ^ 2
            InsertIntoPair(TokenType.PARENTHESIS_CLOSE, TokenType.NUMBER, power);

            // x( -> x * (
            InsertIntoPair(TokenType.VARIABLE, TokenType.PARENTHESIS_OPEN, multiplyer);

            // )x -> ) * x
            InsertIntoPair(TokenType.PARENTHESIS_CLOSE, TokenType.VARIABLE, multiplyer);

            // x2 -> x ^ 2
            InsertIntoPair(TokenType.VARIABLE, TokenType.NUMBER, power);

            // 3 2 -> 3 ^ 2
            InsertIntoPair(TokenType.NUMBER, TokenType.NUMBER, power);

            // 2sqrt -> 2 * sqrt
            InsertIntoPair(TokenType.NUMBER, TokenType.FUNCTION, multiplyer);

            // x sqrt -> x * sqrt
            InsertIntoPair(TokenType.VARIABLE, TokenType.FUNCTION, multiplyer);

            // )sqrt -> ) * sqrt
            InsertIntoPair(TokenType.PARENTHESIS_CLOSE, TokenType.FUNCTION, multiplyer);

            // )( -> ) * (
            // )sqrt -> ) * sqrt
            InsertIntoPair(TokenType.PARENTHESIS_CLOSE, TokenType.PARENTHESIS_OPEN, multiplyer);
        }
    }
}
