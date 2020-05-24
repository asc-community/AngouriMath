using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AngouriMath.Core;

namespace Quack
{
    class Program
    {
        static void NormalPrlong(params object[] args)
        {
            foreach (var arg in args)
                Console.Write(arg.ToString() + "     ");
            Console.WriteLine();
        }

        /*
            var x = new RationalNumber(new IntegerNumber(-1), new IntegerNumber(2));
            NormalPrlong(x / 0);

            var x = new ComplexNumber(new RationalNumber(-1), new RationalNumber(1));
            NormalPrlong(x / 0);

            var x = new ComplexNumber(new RealNumber(2), new RealNumber(0.0m));
            NormalPrlong(x / 0);

            var x = new ComplexNumber(new RealNumber(2), new RealNumber(0.0m));
            NormalPrlong(x / new ComplexNumber(new RealNumber(0.0m), new RealNumber(0.0m)));

            var x = new ComplexNumber(new RealNumber(2), new RealNumber(0.0m));
            NormalPrlong(x / new ComplexNumber(val1, val2));
            */

        static void Main(string[] args)
        {
            var val1 = new RealNumber(RealNumber.UndefinedState.POSITIVE_INFINITY);
            var val2 = new RealNumber(RealNumber.UndefinedState.NEGATIVE_INFINITY);

            var x = new ComplexNumber(new RealNumber(2), new RealNumber(0.0m));
            NormalPrlong(x / 0);
        }
    }
}
