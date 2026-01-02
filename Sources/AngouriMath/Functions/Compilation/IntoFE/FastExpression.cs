//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Runtime.CompilerServices;
using AngouriMath.Core.Exceptions;


namespace AngouriMath.Core
{
    public sealed partial class FastExpression
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
            CALL_SECANT,
            CALL_COSECANT,
            CALL_TAN,
            CALL_COTAN,
            CALL_ARCSIN,
            CALL_ARCCOS,
            CALL_ARCTAN,
            CALL_ARCCOTAN,
            CALL_ARCSECANT,
            CALL_ARCCOSECANT,
            CALL_FACTORIAL,
            CALL_SIGNUM,
            CALL_ABS,
            CALL_PHI,

            // 2-arg functions
            CALL_SUM = 100,
            CALL_MINUS,
            CALL_MUL,
            CALL_DIV,
            CALL_POW,
            CALL_LOG,
        }
        internal sealed partial record Instruction(InstructionType Type, int Reference = -1, System.Numerics.Complex Value = default)
        {
            public override string ToString() =>
                Type
                + (Reference == -1 ? "" : Reference.ToString())
                + (Type != InstructionType.PUSH_CONST ? "" : Value.ToString());
        }

        private readonly Stack<System.Numerics.Complex> stack;
        private readonly System.Numerics.Complex[] cache;
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
            stack = new Stack<System.Numerics.Complex>(instructions.Count);
            cache = new System.Numerics.Complex[cacheCount];
        }

        /// <summary>Calls the compiled function (synonym to <see cref="Substitute(System.Numerics.Complex[])"/>)</summary>
        /// <param name="values">List arguments in the same order in which you compiled the function</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System.Numerics.Complex Call(params System.Numerics.Complex[] values) => Substitute(values);

        /// <summary>Calls the compiled function (synonym to <see cref="Call(System.Numerics.Complex[])"/>)</summary>
        /// <param name="values">List arguments in the same order in which you compiled the function</param>
        /// <exception cref="WrongNumberOfArgumentsException">
        /// Thrown when the length of <paramref name="values"/> does not match the number of variables compiled.
        /// </exception>
        public System.Numerics.Complex Substitute(params System.Numerics.Complex[] values)
        {
            if (values.Length != varCount)
                throw new WrongNumberOfArgumentsException($"Wrong number of parameters: Expected {varCount} but {values.Length} provided");
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
                        stack.Push(System.Numerics.Complex.Pow(stack.Pop(), stack.Pop()));
                        break;
                    case InstructionType.CALL_SIN:
                        stack.Push(System.Numerics.Complex.Sin(stack.Pop()));
                        break;
                    case InstructionType.CALL_COS:
                        stack.Push(System.Numerics.Complex.Cos(stack.Pop()));
                        break;
                    case InstructionType.CALL_SECANT:
                        stack.Push(1 / System.Numerics.Complex.Cos(stack.Pop()));
                        break;
                    case InstructionType.CALL_COSECANT:
                        stack.Push(1 / System.Numerics.Complex.Sin(stack.Pop()));
                        break;
                    case InstructionType.CALL_TAN:
                        stack.Push(System.Numerics.Complex.Tan(stack.Pop()));
                        break;
                    case InstructionType.CALL_COTAN:
                        stack.Push(1 / System.Numerics.Complex.Tan(stack.Pop()));
                        break;
                    case InstructionType.CALL_LOG:
                        stack.Push(System.Numerics.Complex.Log(stack.Pop(), stack.Pop().Real));
                        break;
                    case InstructionType.CALL_ARCSIN:
                        stack.Push(System.Numerics.Complex.Conjugate(System.Numerics.Complex.Asin(stack.Pop())));
                        break;
                    case InstructionType.CALL_ARCCOS:
                        stack.Push(System.Numerics.Complex.Acos(stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCTAN:
                        stack.Push(System.Numerics.Complex.Atan(stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCCOTAN:
                        stack.Push(System.Numerics.Complex.Atan(1 / stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCSECANT:
                        stack.Push(System.Numerics.Complex.Acos(1 / stack.Pop()));
                        break;
                    case InstructionType.CALL_ARCCOSECANT:
                        stack.Push(System.Numerics.Complex.Asin(1 / stack.Pop()));
                        break;
                    case InstructionType.CALL_FACTORIAL:
                        // https://stackoverflow.com/a/15454784/5429648
                        const int g = 7;
                        static System.Numerics.Complex Gamma(System.Numerics.Complex z)
                        {
                            if (z.Real < 0.5) return System.Math.PI / (System.Numerics.Complex.Sin(System.Math.PI * z) * Gamma(1 - z));
                            else
                            {
                                z -= 1;

                                System.Numerics.Complex x = gammaCoeffs[0];
                                for (var i = 1; i < g + 2; i++)
                                    x += gammaCoeffs[i] / (z + i);

                                var t = z + g + 0.5;
                                return System.Math.Sqrt(2 * System.Math.PI) * System.Numerics.Complex.Pow(t, z + 0.5) * System.Numerics.Complex.Exp(-t) * x;
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
                    case InstructionType.CALL_PHI:
                        var n = (long) stack.Pop().Real;
                        stack.Push(n.Phi());
                        break;
                }
            if (stack.Count != 1)
                throw new AngouriBugException("Unused values remain in the stack");
            return stack.Pop();
        }

        [ConstantField] static readonly double[] gammaCoeffs = { 
            0.99999999999980993,  676.5203681218851,     -1259.1392167224028, 
            771.32342877765313,   -176.61502916214059,   12.507343278686905, 
            -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 
                };

        /// <summary>Might be useful for debug if a function works too slowly</summary>
        public override string ToString() => string.Join(" \n| ", instructions);
    }
}