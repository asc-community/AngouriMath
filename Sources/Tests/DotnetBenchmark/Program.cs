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
        public static void Main(string[] args)
        {
            var benchmarkName = args.Length > 0 ? args[0] : "";

            var reports =
                args
                .Select(arg =>
                    arg switch
                    {
                        "RAMUsageTest" => GetReportByBenchmark(typeof(RAMUsageTest), "Gen 0", "Gen 1", "Gen 2", "Allocated"),
                        "CommonFunctionsInterVersion" => GetReportByBenchmark(typeof(CommonFunctionsInterVersion), "Mean", "Error", "StdDev"),
                        "CompiledFuncTest" => GetReportByBenchmark(typeof(CompiledFuncTest), "Mean", "Error", "StdDev"),
                        "NumbersBenchmark" => GetReportByBenchmark(typeof(NumbersBenchmark), "Mean", "Error", "StdDev"),
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
            // BenchmarkRunner.Run<BenchLinqCompilation>();
            Console.ReadLine(); Console.ReadLine(); Console.ReadLine();
        }

        public static string GetReportByBenchmark(Type report, params string[] columns)
            => TableToString(BenchmarkRunner.Run(report).Table, columns);

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