using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;

namespace DotnetBenchmark
{
    // DO NOT FORGET TO SWITCH TO RELEASE!
    [ArtifactsPath(@"./benchmark_results.csv")]
    [CsvExporter(CsvSeparator.Semicolon)]
    public class CommonFunctionsInterVersionTest
    {
        /*
         *
         * Testing parsing
         *
         */
        [Benchmark]
        public void ParseEasy()
            => "1 + 2 / x + 2 / (y + x)".ToEntity();

        [Benchmark]
        public void ParseHard()
            => "x ^ (x + y ^ 2 ^ sin(3 / z + i)) - log(-i, 3) ^ (x + x * y) - sqrt(y) / (i + sqrt(-1))".ToEntity();



        /*
         *
         * Testing simplification
         *
         */


        private static Entity simplifyEasy = "x + 3 / 3 + x ^ 0 - log(e, e2)";

        [Benchmark]
        public void SimplifyEasy()
            => simplifyEasy.Simplify(4);



        internal static  Entity simplifyHardPart = "y / u + 4 * a * 8";
        internal static Entity simplifyHardPart2 = MathS.Sin(simplifyHardPart) + MathS.Pow(simplifyHardPart, "y + u");
        internal static Entity simplifyHardPart3 = MathS.Log(3, simplifyHardPart2) + MathS.Arccos(simplifyHardPart2);
        internal static Entity simplifyHard = ("(x + 1) + 2 + y + z + 3 + log(3, e + 3 / 6) - " +
                                               "(x2 - 1) / (x - 1) + sin(4 * x) / cos(4 * x) + " + 
                                               "1 / x / a + (x + x * y) + log(3, 9) + a ^ 1 + (cos(a)2 + sin(a))").ToEntity().Substitute("x", simplifyHardPart3);

        [Benchmark]
        public void SimplifyHard()
            => simplifyHard.Simplify(4);



        /*
         *
         * Testing derivation
         *
         */

        private static Entity toDerive = "x + 3 + arccos(x + 2) / sqrt(x2 + 1)";

        [Benchmark]
        public void Derivate()
            => toDerive.Derive("x");


        /*
         *
         * Testing solver
         *
         */

        private static Entity toSolveEasy = "x2 + x + 1";

        [Benchmark]
        public void SolveEasy()
            => toSolveEasy.SolveEquation("x");

        private static Entity toSolveEasyMedium = "x2 + x + a";

        [Benchmark]
        public void SolveEasyMedium()
            => toSolveEasyMedium.SolveEquation("x");

        private static Entity toSolveMedium = "(x + sqr(x) + a)2 + (x + sqr(x) + a) + b";

        [Benchmark]
        public void SolveMedium()
            => toSolveMedium.SolveEquation("x");

        private static Entity toSolveMediumHard = "(sin(x) + sqr(sin(x)) + a)4 + (sin(x) + sqr(sin(x)) + a) + b";

        [Benchmark]
        public void SolveMediumHard()
            => toSolveMediumHard.SolveEquation("x");

        private static Entity toSolveHard = "(sin(cos(x) + sin(x) + c) + sqr(sin(cos(x) + sin(x) + c)) + a)4 + (sin(cos(x) + sin(x) + c) + sqr(sin(cos(x) + sin(x) + c)) + a) + b";

        [Benchmark]
        public void SolveHard()
            => toSolveHard.SolveEquation("x");


        /*
         *
         * Testing evaluation
         *
         */

        private static Entity evalEasy = "1 + 2 + log(2, 3) + sqrt(4) - 4 ^ 7 + e * pi";

        [Benchmark]
        public void EvalEasy()
            => evalEasy.Eval();

        private static Entity eval___ = evalEasy + MathS.Pow(evalEasy, 4) / MathS.Sqrt(evalEasy);
        private static Entity evalHard = eval___ + MathS.Pow(eval___, 4) / MathS.Sqrt(eval___);

        [Benchmark]
        public void EvalHard() 
            => evalHard.Eval();


        /*
         *
         * Testing compilation
         *
         */

        private static Entity toCompileEasy = "x + x / 2 + sin(x)";

        [Benchmark]
        public void CompileEasy()
            => toCompileEasy.Compile("x");

        private static Entity toCompileHard = "(x + log(2, x) + sin(x + cos(x))3) / sqrt(x + tan(x))";

        [Benchmark]
        public void CompileHard()
            => toCompileHard.Compile("x");


        /*
         *
         * Testing compiled functions
         *
         */

        private readonly Complex toSub = new Complex(3, -1);

        private readonly FastExpression toRunEasy = "x + sin(x) + 3"
            .ToEntity().Compile("x");
        [Benchmark]
        public void RunEasy()
            => toRunEasy.Call(toSub);

        private readonly FastExpression toRunMedium = "x + sin(x) + 3 / sin(x + cos(x)) + sqrt(x3) / e ^ x + x * log(e, 4)"
            .ToEntity().Compile("x");
        [Benchmark]
        public void RunMedium()
            => toRunMedium.Call(toSub);

        private static readonly Entity toRunHardEntity = "x + sin(x) + 3 / sin(x + cos(x)) + sqrt(x3) / e ^ x + x * log(e, 4)";
        private static readonly FastExpression toRunHard = (MathS.Sqr(toRunHardEntity) + MathS.Sqrt(toRunHardEntity) + MathS.Sin(toRunHardEntity) / (MathS.Sqrt(toRunHardEntity) + MathS.Sin(toRunHardEntity) + MathS.Tan(toRunHardEntity)))
            .Compile("x");
        [Benchmark]
        public void RunHard()
            => toRunHard.Call(toSub);
    }
}
