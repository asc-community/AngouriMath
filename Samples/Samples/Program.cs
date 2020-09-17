using System;
using System.Collections.Immutable;
using AngouriMath;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var values = new [] { False, True };
            Entity expr = "(A -> (B -> C)) -> ((A -> B) -> (A -> C))";
            Entity expr1 = "(A -> (B -> C)) -> ((A -> B) -> (A -> C))";

            Console.WriteLine(expr.Simplify());
            Console.WriteLine(expr1 == expr);

            foreach (var A in values)
                foreach (var B in values)
                    foreach (var C in values)
                    {
                        var res = expr.Substitute("A", A);
                        res = res.Substitute("B", B);
                        res = res.Substitute("C", C);
                        Console.WriteLine(res.EvalBoolean());
                    }
        }
    }
}