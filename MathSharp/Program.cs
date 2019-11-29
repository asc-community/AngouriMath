using MathSharp.Core;
using MathSharp.Core.FromString;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MathSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var expr = "1 + 0.12 / 8 ^ sin(x + sqrt(3)) - log(3, 4) + 3";
            var lexer = new Lexer(expr);
            while (!lexer.EOF())
            {
                Console.WriteLine(lexer.Current());
                lexer.Next();
            }
        }
    }
}
