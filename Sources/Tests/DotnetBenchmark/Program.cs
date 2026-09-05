//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq.Expressions;
using System.Numerics;
using PeterO.Numbers;
using AngouriMath;
using AngouriMath.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HonkSharp.Fluency;
using GenericTensor.Core;
using BenchmarkDotNet.Reports;
using System.Linq;

namespace DotnetBenchmark
{
    public class CompiledFuncTest
    {
        private readonly Entity.Variable x = MathS.Var("x");
        private readonly FastExpression multiFunc;
        private readonly Entity multiFuncNotCompiled;
        private readonly Func<Complex, Complex> linqFunc;
        private readonly Complex CToSub = 3;
        private readonly Entity.Number.Complex CNumToSub = 3;
        public CompiledFuncTest()
        {
            multiFuncNotCompiled = (MathS.Log(3, x) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x));
            multiFunc = multiFuncNotCompiled.Compile(x);
            Expression<Func<Complex, Complex>> expr = x => (Complex.Log(x, 3) + Complex.Pow(x, 2)) * Complex.Sin(x + 1 / Complex.Sin(x));
            linqFunc = expr.Compile();
        }
        [Benchmark] public void MultiFunc() => multiFunc.Call(CToSub);
        [Benchmark] public void LinqSin() => linqFunc(3);
        [Benchmark] public void NotCompiled() => multiFuncNotCompiled.Substitute(x, 3).EvalNumerical();
    }

    public class NumbersBenchmark
    {
        private readonly Entity.Number.Real a = 3.4m;
        private readonly Entity.Number.Real b = 5.4m;
        [Benchmark] public void InitComplexImpl() => Entity.Number.Complex.Create(3.4m, 56m);
        [Benchmark] public void InitComplexImpl2() => Entity.Number.Complex.Create(3.4, 56);
        [Benchmark] public void InitComplexExpl() => Entity.Number.Complex.Create(a, b);
        [Benchmark] public void InitReal() => Entity.Number.Real.Create(3.4m);
        [Benchmark] public void InitRational() => Entity.Number.Rational.Create(6, 7);
        [Benchmark] public void InitInteger() => Entity.Number.Integer.Create(68);
        
        [Benchmark] public void DowncastComplexSuccessfully() => Entity.Number.Complex.Create(3.4m, 6.4m);
        [Benchmark] public void DowncastComplexNotSucc() => Entity.Number.Complex.Create(3.487449272953435m, 6.401380141304m);
        
        [Benchmark] public void DowncastRealSuccessfully() => Entity.Number.Real.Create(3.4m);
        [Benchmark] public void DowncastRealNotSucc() => Entity.Number.Real.Create(3.42748273484m);
        
        [Benchmark] public void FindRationalSuccess() => Entity.Number.Rational.FindRational(3.4m);
        [Benchmark] public void FindRationalNotSuccess() => Entity.Number.Rational.FindRational(3.48426482675284m);

        private EDecimal dec = 3;
        private readonly EDecimal coef = EDecimal.FromDecimal(0.2m);
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            // PerformanceGate is not a benchmark: it compares the file the last run wrote against
            // the committed baseline, so it is cheap and can be its own CI step without paying for
            // the measurement twice. https://github.com/asc-community/AngouriMath/issues/529
            if (args.Contains(PerformanceGate.Command))
                return PerformanceGate.Compare(Console.Out);

            var reports =
                args
                .Select(arg =>
                    arg switch
                    {
// Every case but the inter-version one is compiled out when the project is built for the
// key-commit run, because a case that reaches API an older kernel did not have stops the whole
// project compiling and takes the row with it. Only CommonFunctionsInterVersion is executed by
// that run. https://github.com/asc-community/AngouriMath/issues/529
                        // Allocated as well as Mean: the regressions this one exists to catch
                        // show up in allocation and are invisible in the timings.
                        "CommonFunctionsInterVersion" => GetReportByBenchmark(typeof(CommonFunctionsInterVersion), "Mean", "Error", "StdDev", "Allocated"),
#if !INTERVERSION_ONLY
                        // Allocated: the class has carried [MemoryDiagnoser] all along, so the
                        // figure was being collected on every run and then dropped here.
                        "RAMUsageTest" => GetReportByBenchmark(typeof(RAMUsageTest), "Gen 0", "Gen 1", "Gen 2", "Allocated"),
                        "TransformationLayer" => GetReportByBenchmark(typeof(TransformationLayer), "Mean", "Error", "StdDev", "Allocated"),
                        // Ratio as well: this one exists to compare two forms of the same rule,
                        // and the absolute nanoseconds matter far less than the factor between
                        // them. See MatchingEngine for what the factor decides.
                        "MatchingEngine" => GetReportByBenchmark(typeof(MatchingEngine), "Mean", "Error", "StdDev", "Ratio", "Allocated"),
                        "CompiledFuncTest" => GetReportByBenchmark(typeof(CompiledFuncTest), "Mean", "Error", "StdDev"),
                        // The two compilation benchmarks existed as classes that nothing could
                        // run: one was a commented-out BenchmarkRunner.Run below, the other had
                        // no arm at all. A benchmark nobody can invoke measures nothing.
                        "BenchLinqCompilation" => GetReportByBenchmark(typeof(BenchLinqCompilation), "Mean", "Error", "StdDev", "Allocated"),
                        "CacheCompiledFunc" => GetReportByBenchmark(typeof(CacheCompiledFunc), "Mean", "Error", "StdDev", "Allocated"),
                        "NumbersBenchmark" => GetReportByBenchmark(typeof(NumbersBenchmark), "Mean", "Error", "StdDev"),
#endif
                        _ => throw new($"Unexpected benchmark {arg}")
                    }).ToArray(); // active action
            Console.WriteLine();
            Console.WriteLine();
            foreach (var (id, report) in reports.Enumerate())
            {
                Console.WriteLine($"Report # {id}");
                Console.WriteLine();
                Console.WriteLine(report);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.ReadLine(); Console.ReadLine(); Console.ReadLine();
            return 0;
        }

        public static string GetReportByBenchmark(Type report, params string[] columns)
        {
            var summary = BenchmarkRunner.Run(report);
            // Both numbers, in the baseline's own format, so that updating the baseline is copying
            // a file rather than transcribing a table.
            PerformanceGate.WriteMeasurements(summary);
            return TableToString(summary.Table, columns);
        }

        public static string TableToString(SummaryTable table, params string[] columns)
        {
            var colsToSelectFrom = table.Columns
                .Where(col => columns.Contains(col.Header))
                .Prepend(table.Columns.Single(c => c.Header == "Method"))
                .ToArray();
            var tensor = GenTensor<string, StringWrapper>.CreateMatrix(
                table.Columns.First().Content.Length + 1,
                colsToSelectFrom.Length,
                (x, y) => x is 0 ? colsToSelectFrom[y].Header : colsToSelectFrom[y].Content[x - 1]
                );
            return tensor.ToString();
        }

        private struct StringWrapper : IOperations<string>
        {
            public string Add(string a, string b)
            {
                throw new NotImplementedException();
            }

            public string Subtract(string a, string b)
            {
                throw new NotImplementedException();
            }

            public string Multiply(string a, string b)
            {
                throw new NotImplementedException();
            }

            public string Negate(string a)
            {
                throw new NotImplementedException();
            }

            public string Divide(string a, string b)
            {
                throw new NotImplementedException();
            }

            public string CreateOne()
            {
                throw new NotImplementedException();
            }

            public string CreateZero()
            {
                throw new NotImplementedException();
            }

            public string Copy(string a)
            {
                throw new NotImplementedException();
            }

            public bool AreEqual(string a, string b)
            {
                throw new NotImplementedException();
            }

            public bool IsZero(string a)
            {
                throw new NotImplementedException();
            }

            public string ToString(string a)
            {
                return a;
            }

            public byte[] Serialize(string a)
            {
                throw new NotImplementedException();
            }

            public string Deserialize(byte[] data)
            {
                throw new NotImplementedException();
            }
        }
    }
}