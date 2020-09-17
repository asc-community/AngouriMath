using System;
using System.Collections.Immutable;
using AngouriMath;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;

namespace Samples
{
    class Program
    {
        class A { }
        class B : A { }

        static void Main(string[] _)
        {
            var imm = ImmutableArray.Create(2, 3, 4, 5, 6);
            var values = new [] { False, True };
            Entity expr = "(A -> (B -> C)) -> ((A -> B) -> (A -> C))";
            
            Console.WriteLine(expr.Simplify());

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