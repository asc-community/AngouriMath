using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Core.FromString
{
    using BraceType = Token.BraceType;
    public partial class Token
    {
        public Entity root;
        public List<Entity> children = new List<Entity>();
    }
    internal class SymbolProcessor
    {
        public static bool IsOperator(char s)
        {
            // TODO
            return SyntaxInfo.operatorNames.ContainsKey(s);
        }
        public static bool IsDelimiter(string s)
        {
            // TODO
            return s == ",";
        }
    }
    internal class BraceProcessor
    {
        private List<BraceType> braces = new List<BraceType>();
        public static readonly List<BraceType> parentheses = new List<BraceType>
        { 
            BraceType.PARENTHESIS_OPEN,
            BraceType.PARENTHESIS_CLOSE
        };
        public static readonly List<BraceType> brackets = new List<BraceType>
        {
            BraceType.BRACKET_OPEN,
            BraceType.BRACKET_CLOSE
        };
        public static bool IsOpen(BraceType s)
        {
            return s == BraceType.BRACKET_OPEN || s == BraceType.PARENTHESIS_OPEN;
        }
        public static bool SameType(BraceType a, BraceType b)
        {
            // TODO: Make more universal
            return parentheses.Contains(a) && parentheses.Contains(b) ||
                   brackets.Contains(a) && brackets.Contains(b);
        }
        public bool IsFinished()
        {
            return braces.Count == 0;
        }
        public void Add(BraceType symbol)
        {
            if (symbol == BraceType.NONE)
            {
                throw new ParseException("Brace error");
            }
            if (!IsOpen(symbol))
            {
                if (IsFinished())
                {
                    throw new ParseException("Brace error");
                }
                else
                {
                    if (SameType(braces[braces.Count - 1], symbol))
                    {
                        if (IsOpen(braces[braces.Count - 1]))
                        {
                            braces.RemoveAt(braces.Count - 1);
                        }
                        // otherwise is unreacheable.
                    }
                    else
                        throw new ParseException("Brace error");
                }
            }
            else
                braces.Add(symbol);
        }
    }
    public static class Parser
    {
        public static List<Entity> Parse(Lexer lexer)
        {
            var res = new List<List<Entity>>();
            res.Add(new List<Entity>());
            var bracer = new BraceProcessor();
            // First, we extract functions
            while (!lexer.EOF())
            {
                if(lexer.Current().type == Token.TokenType.NUMBER)
                {
                    res[res.Count - 1].Add(new NumberEntity(Convert.ToDouble(lexer.Current().Value)));
                }
                else if(lexer.Current().type == Token.TokenType.SYMBOL)
                {
                    if (SymbolProcessor.IsDelimiter(lexer.Current().Value))
                    {
                        if (bracer.IsFinished())
                        {
                            res.Add(new List<Entity>());
                        }
                        else
                        {
                            throw new ParseException("Unexpected delimiter");
                        }
                    }
                    else if (SymbolProcessor.IsOperator(lexer.Current().Value[0]))
                    {
                        res[res.Count - 1].Add(new OperatorEntity(SyntaxInfo.operatorNames[lexer.Current().Value[0]], SyntaxInfo.operatorPriorities[lexer.Current().Value[0]]));
                    }
                    else
                        throw new LexicException("Unresolved token " + lexer.Current().ToString());
                }
                else if(lexer.Current().type == Token.TokenType.FUNCTION)
                {
                    var current = lexer.Current();
                    var interTokenList = new TokenList();
                    var interBracer = new BraceProcessor();
                    lexer.Next();
                    interBracer.Add(BraceType.PARENTHESIS_OPEN);
                    while(!lexer.EOF() && !interBracer.IsFinished())
                    {
                        lexer.Next();
                        interTokenList.Add(lexer.Current());
                        if (lexer.Current().type == Token.TokenType.BRACE)
                            interBracer.Add(Token.GetBracketType(lexer.Current().Value[0]));
                    }
                    if (lexer.EOF() && !interBracer.IsFinished())
                        throw new ParseException("Unexpected EOF");
                    string interSrc = "";
                    foreach (var token in interTokenList)
                        interSrc += token.Value;
                    var interLexer = new Lexer(interSrc);
                    current.children = Parser.Parse(interLexer);
                }
                else if(lexer.Current().type == Token.TokenType.BRACE)
                {
                    bracer.Add(Token.GetBracketType(lexer.Current().Value[0]));
                }

                lexer.Next();
            }
            return null;
        }
    }
}
