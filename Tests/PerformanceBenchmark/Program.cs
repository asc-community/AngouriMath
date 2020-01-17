using PerformanceBenchmark.Tests;
using System;

namespace PerformanceBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var simTest = new SimplificationTest();
            Console.WriteLine("Simplify");
            Console.WriteLine(simTest.Evaluate());
            var derTest = new DerivationTest();
            Console.WriteLine("Derive");
            Console.WriteLine(derTest.Evaluate());
            var utTest = new SubsTest();
            Console.WriteLine("Uncompiled sub");
            Console.WriteLine(utTest.Evaluate());
            var ctTest = new CompiledFunctionTest();
            Console.WriteLine("Compiled sub");
            Console.WriteLine(ctTest.Evaluate());
        }
    }
}
