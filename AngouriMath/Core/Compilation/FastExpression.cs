
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Core.FastExpression;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order as you will list them when evaluating.
        /// Constants, i.e. <see cref="MathS.pi"/> and <see cref="MathS.e"/> will be ignored.
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params Variable[] variables) => Compiler.Compile(this, variables);

        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order as you will list them when evaluating.
        /// Constants, i.e. <see cref="MathS.pi"/> and <see cref="MathS.e"/> will be ignored.
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params string[] variables) =>
            Compiler.Compile(this, variables.Select(x => (Variable)x));
    }
}
namespace AngouriMath.Core
{
    public partial class FastExpression
    {
        internal enum InstructionType
        {
            PUSH_VAR,
            PUSH_CONST,
            LOAD_CACHE,
            SAVE_CACHE,

            // 1-arg functions
            CALL_SIN = 50,
            CALL_COS,
            CALL_TAN,
            CALL_COTAN,
            CALL_ARCSIN,
            CALL_ARCCOS,
            CALL_ARCTAN,
            CALL_ARCCOTAN,
            CALL_FACTORIAL,
            CALL_SIGNUM,
            CALL_ABS,

            // 2-arg functions
            CALL_SUM = 100,
            CALL_MINUS,
            CALL_MUL,
            CALL_DIV,
            CALL_POW,
            CALL_LOG,
        }
        internal partial record Instruction(InstructionType Type, int Reference = -1, Complex Value = default)
        {
            public override string ToString() =>
                Type
                + (Reference == -1 ? "" : Reference.ToString())
                + (Type != InstructionType.PUSH_CONST ? "" : Value.ToString());
        }
        private readonly Stack<Complex> stack;
        private readonly Complex[] cache;
        private readonly List<Instruction> instructions;
        private readonly int varCount;

        /// <summary>
        /// You cannot modify this function once it is sealed. The final user will never access to its
        /// direct instructions
        /// </summary>
        internal FastExpression(int varCount, List<Instruction> instructions, int cacheCount)
        {
            this.varCount = varCount;
            this.instructions = instructions;
            stack = new Stack<Complex>(instructions.Count);
            cache = new Complex[cacheCount];
        }

        /// <summary>Calls the compiled function (synonym to <see cref="Substitute(Complex[])"/>)</summary>
        /// <param name="values">List arguments in the same order in which you compiled the function</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex Call(params Complex[] values) => Substitute(values);

        /// <summary>Calls the compiled function (synonym to <see cref="Call(Complex[])"/>)</summary>
        /// <param name="values">List arguments in the same order in which you compiled the function</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the length of <paramref name="values"/> does not match the number of variables compiled.
        /// </exception>
        // TODO: Optimization
        public Complex Substitute(params Complex[] values)
        {
            if (values.Length != varCount)
                throw new System.ArgumentException($"Wrong number of parameters: Expected {varCount} but {values.Length} provided");
            foreach (var instruction in instructions)
                switch (instruction.Type)
                {
                    case InstructionType.PUSH_VAR:
                        stack.Push(values[instruction.Reference]);
                        break;
                    case InstructionType.PUSH_CONST:
                        stack.Push(instruction.Value);
                        break;
                    case InstructionType.LOAD_CACHE:
                        stack.Push(cache[instruction.Reference]);
                        break;
                    case InstructionType.SAVE_CACHE:
                        cache[instruction.Reference] = stack.Peek();
                        break;
                    case InstructionType.CALL_SUM:
                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    case InstructionType.CALL_MINUS:
                        stack.Push(stack.Pop() - stack.Pop());
                        break;
                    case InstructionType.CALL_MUL:
                        stack.Push(stack.Pop() * stack.Pop());
                        break;
                    case InstructionType.CALL_DIV:
                        stack.Push(stack.Pop() / stack.Pop());
                        break;
                    case InstructionType.CALL_POW:
                        stack.Push(Complex.Pow(stack.Pop(), stack.Pop()));
                        break;
                    case InstructionType.CALL_SIN:
                        stack.Push(Complex.Sin(stack.Pop()));
                        break;
                    case InstructionType.CALL_COS:
                        stack.Push(Complex.Cos(stack.Pop()));
                        break;
                    case InstructionType.CALL_TAN:
                        stack.Push(Complex.Tan(stack.Pop()));
                        break;
                    case InstructionType.CALL_COTAN:
                        stack.Push(1 / Complex.Tan(stack.Pop()));
                        break;
                    case InstructionType.CALL_LOG:
                        stack.Push(Complex.Log(stack.Pop(), stack.Pop().Real));
                        break;
                    case InstructionType.CALL_ARCSIN:
                        stack.Push(Complex.Asin(stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCCOS:
                        stack.Push(Complex.Acos(stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCTAN:
                        stack.Push(Complex.Atan(stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCCOTAN:
                        stack.Push(Complex.Atan(1 / stack.Pop()));
                        break;
                    case InstructionType.CALL_FACTORIAL:
                        // https://stackoverflow.com/a/15454784/5429648
                        const int g = 7;
                        static Complex Gamma(Complex z)
                        {
                            if (z.Real < 0.5) return System.Math.PI / (Complex.Sin(System.Math.PI * z) * Gamma(1 - z));
                            else
                            {
                                z -= 1;

                                Complex x = gammaCoeffs[0];
                                for (var i = 1; i < g + 2; i++)
                                    x += gammaCoeffs[i] / (z + i);

                                var t = z + g + 0.5;
                                return System.Math.Sqrt(2 * System.Math.PI) * Complex.Pow(t, z + 0.5) * Complex.Exp(-t) * x;
                            }
                        }
                        stack.Push(Gamma(stack.Pop() + 1));
                        break;
                    case InstructionType.CALL_SIGNUM:
                        stack.Push(stack.Pop().Signum());
                        break;
                    case InstructionType.CALL_ABS:
                        stack.Push(stack.Pop().Abs());
                        break;
                }
            if (stack.Count != 1)
                throw new AngouriBugException("Unused values remain in the stack");
            return stack.Pop();
        }

        static readonly double[] gammaCoeffs = { 
            0.99999999999980993,  676.5203681218851,     -1259.1392167224028, 
            771.32342877765313,   -176.61502916214059,   12.507343278686905, 
            -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 
                };

        /// <summary>Might be useful for debug if a function works too slowly</summary>
        public override string ToString() => string.Join(" \n| ", instructions);
    }
}