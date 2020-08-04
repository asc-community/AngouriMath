
using System;
using AngouriMath;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity ent = "3 * sin(2 * x + 1) - sin(x) - a";
            VariableEntity x = "x";
            for (int i = 0; i < 1000; i++)
            {
                ent.SolveEquation(x);
            }
        }
    }
}
