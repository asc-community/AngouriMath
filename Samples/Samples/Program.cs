using System;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static string ToWeb(ILatexiseable l)
            => "<img src=\"https://render.githubusercontent.com/render/math?math=" +
               l.Latexise().Replace("+", "%2B")
               + "\">";
        static void Main(string[] _)
        {
            Entity expr = "2x + sin(x) / sin(2 ^ x)";
            var subs = expr.Substitute("x", 0.3m);
            Console.WriteLine(ToWeb(subs));
        }
    }
}
