using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    internal static class CompiledMathFunctions
    {
        internal static Dictionary<string, int> func2Num = new Dictionary<string, int>
        {
            { "sumf", 0 },
            { "minusf", 1 },
            { "mulf", 2 },
            { "divf", 3 },
            { "powf", 4 },
            { "sinf", 5 },
            { "cosf", 6 },
            { "logf", 7 },
        };

        internal delegate void CompiledFunction(Stack stack);
        internal static CompiledFunction[] functions =
            new CompiledFunction[]
            {
                Sumf,
                Minusf,
                Mulf,
                Divf,
                Powf,
                Sinf,
                Cosf,
                Logf
            };

        internal static Number[] buffer = new Number[10];

        internal static void Sumf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(buffer[0] + buffer[1]);
        }
        internal static void Minusf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(buffer[0] - buffer[1]);
        }
        internal static void Mulf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(buffer[0] * buffer[1]);
        }
        internal static void Divf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(buffer[0] / buffer[1]);
        }
        internal static void Powf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(Number.Pow(buffer[0], buffer[1]));
        }
        internal static void Sinf(Stack stack)
        {
            stack.Pop(1, buffer);
            stack.Push(Number.Sin(buffer[0]));
        }
        internal static void Cosf(Stack stack)
        {
            stack.Pop(1, buffer);
            stack.Push(Number.Cos(buffer[0]));
        }
        internal static void Logf(Stack stack)
        {
            stack.Pop(2, buffer);
            stack.Push(Number.Log(buffer[0], buffer[1]));
        }
    }
}
