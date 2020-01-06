using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath
{
    internal static class CompiledMathFunctions
    {
        internal static readonly Dictionary<string, int> func2Num = new Dictionary<string, int>
        {
            { "sumf", 0 },
            { "minusf", 1 },
            { "mulf", 2 },
            { "divf", 3 },
            { "powf", 4 },
            { "sinf", 5 },
            { "cosf", 6 },
            { "tanf", 7 },
            { "cotanf", 8 },
            { "logf", 9 },
            { "arcsinf", 10 },
            { "arccosf", 11 },
            { "arctanf", 12 },
            { "arccotanf", 13 },
        };

        internal delegate void CompiledFunction(Stack<Complex> stack);
        internal static readonly CompiledFunction[] functions =
            new CompiledFunction[]
            {
                Sumf,
                Minusf,
                Mulf,
                Divf,
                Powf,
                Sinf,
                Cosf,
                Tanf,
                Cotanf,
                Logf,
                Arcsinf,
                Arccosf,
                Arctanf,
                Arccotanf
            };

        internal static readonly Complex[] buffer = new Complex[10];

        internal static void Sumf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(n1 + n2);
        }
        internal static void Minusf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(n1 - n2);
        }
        internal static void Mulf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(n1 * n2);
        }
        internal static void Divf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(n1 / n2);
        }
        internal static void Powf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(Complex.Pow(n1, n2));
        }
        internal static void Sinf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Sin(n));
        }
        internal static void Cosf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Cos(n));
        }
        internal static void Tanf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Tan(n));
        }
        internal static void Cotanf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(1 / Complex.Tan(n));
        }
        internal static void Logf(Stack<Complex> stack)
        {
            Complex n1 = stack.Pop();
            Complex n2 = stack.Pop();
            stack.Push(Complex.Log(n1, n2.Real));
        }
        internal static void Arcsinf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Asin(n));
        }
        internal static void Arccosf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Acos(n));
        }
        internal static void Arctanf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Atan(n));
        }
        internal static void Arccotanf(Stack<Complex> stack)
        {
            Complex n = stack.Pop();
            stack.Push(Complex.Atan(1 / n));
        }
    }
}
