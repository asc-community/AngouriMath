

using AngouriMath;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using static AngouriMath.Entity;

namespace Samples
{
    public record A
    {
        public A? Another => another.GetValue(this, e => e);
        private static readonly ConditionalWeakTable<A, A> another = new();

        record Wrapper<T>(T Value) { }
    }

   
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "-1 - i";
            var ex = expr.EvalNumerical();
            Console.WriteLine(ex.Stringize());
        }
    }
}