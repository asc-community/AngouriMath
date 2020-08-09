
using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Functions.Evaluation.Simplification;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity a = "3 * sin(e^x) + 5 * sin(e^x) + 3 * sin(e^x) ^ 5 + a * sin(e^x) + 3f * sin(e^x) - h * sin(e^x) ^ 5";

            SmartPolynomialCollapser.Collapse(ref a);

            Console.WriteLine(a);
        }
    }
}
