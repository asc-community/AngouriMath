
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


using AngouriMath.Core.Exceptions;
using AngouriMath.Functions.Evaluation.Compilation;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order
        /// as you will list them when evaluating
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params VariableEntity[] variables) => Compiler.Compile(this, variables);

        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order
        /// as you will list them when evaluating
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params string[] variables) => Compiler.Compile(this, variables);
    } 
}

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {

        /// <summary>
        /// Obviouosly, returns number of subtrees having exact same stringName
        /// </summary>
        /// <returns></returns>
        internal int CountOccurances(string stringName)
        {
            int res = this.ToString() == stringName ? 1 : 0;
            foreach (var child in Children)
                res += child.CountOccurances(stringName);
            return res;
        }
    }
    public class FastExpression
    {
        private (Stack<Complex> Stack, Complex[] Cache)? @sealed;
        internal readonly InstructionSet instructions;
        private readonly int varCount;

        internal readonly Dictionary<string, int> HashToNum = new Dictionary<string, int>();

        internal Entity RawExpr { get; }

        internal FastExpression(int varCount, Entity rawExpr)
        {
            this.varCount = varCount;
            this.instructions = new InstructionSet();
            RawExpr = rawExpr;
        }

        /// <summary>
        /// You cannot modify this function once it is sealed. The final user will never access to its
        /// direct instructions
        /// </summary>
        internal void Seal()
        {
            @sealed = (new Stack<Complex>(instructions.Count), new Complex[HashToNum.Count]);
        }

        /// <summary>
        /// Calls the compiled function (synonim to Substitute)
        /// </summary>
        /// <param name="values">
        /// List arguments in the same order in which you compiled the function
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex Call(params Complex[] values)
            => Substitute(values);

        /// <summary>
        /// Calls the compiled function (synonim to Call)
        /// </summary>
        /// <param name="values">
        /// List arguments in the same order in which you compiled the function
        /// </param>
        /// <returns></returns>
        // TODO: Optimization
        public Complex Substitute(params Complex[] values)
        {
            if (values.Length != varCount)
                throw new SysException("Wrong amount of parameters");
            if (!(@sealed is var (stack, cache)))
                throw new SysException($"Not sealed. Call {nameof(Seal)} before calling {nameof(Substitute)}.");
            Instruction instruction;
            for (int i = 0; i < instructions.Count; i++)
            {
                instruction = instructions[i];
                switch (instruction.Type)
                {
                    case Instruction.InstructionType.PUSHVAR:
                        stack.Push(values[instruction.VarNumber]);
                        break;
                    case Instruction.InstructionType.PUSHCONST:
                        stack.Push(instruction.Value);
                        break;
                    case Instruction.InstructionType.CALL:
                        switch (instruction.FuncNumber)
                        {
                            case 0:
                                stack.Push(stack.Pop() + stack.Pop());
                                break;
                            case 1:
                                stack.Push(stack.Pop() - stack.Pop());
                                break;
                            case 2:
                                stack.Push(stack.Pop() * stack.Pop());
                                break;
                            case 3:
                                stack.Push(stack.Pop() / stack.Pop());
                                break;
                            case 4:
                                stack.Push(Complex.Pow(stack.Pop(), stack.Pop()));
                                break;
                            case 5:
                                stack.Push(Complex.Sin(stack.Pop()));
                                break;
                            case 6:
                                stack.Push(Complex.Cos(stack.Pop()));
                                break;
                            case 7:
                                stack.Push(Complex.Tan(stack.Pop()));
                                break;
                            case 8:
                                stack.Push(1 / Complex.Tan(stack.Pop()));
                                break;
                            case 9:
                                stack.Push(Complex.Log(stack.Pop(), stack.Pop().Real));
                                break;
                            case 10:
                                stack.Push(Complex.Asin(stack.Pop()));
                                break;
                            case 11:
                                stack.Push(Complex.Acos(stack.Pop()));
                                break;
                            case 12:
                                stack.Push(Complex.Atan(stack.Pop()));
                                break;
                            case 13:
                                stack.Push(Complex.Atan(1 / stack.Pop()));
                                break;
                            case 14:
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
                        }
                        break;
                    case Instruction.InstructionType.PULLCACHE:
                        stack.Push(cache[instruction.CacheNumber]);
                        break;
                    default:
                        cache[instruction.CacheNumber] = stack.Peek();
                        break;
                }
            }

            return stack.Pop();
        }

        static readonly double[] gammaCoeffs = new[] { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313, -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };

        /// <summary>
        /// Might be useful for debug if a function works too slowly
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var instruction in instructions)
                sb.Append(instruction.ToString()).Append("\n");
            return sb.ToString();
        }
    }
}
