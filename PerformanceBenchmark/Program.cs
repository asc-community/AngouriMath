using PerformanceBenchmark.Tests;
using System;

namespace PerformanceBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var simTest = new SimplificationTest();
            Console.WriteLine(simTest.Evaluate());
            var derTest = new SimplificationTest();
            Console.WriteLine(derTest.Evaluate());
        }
    }
}
