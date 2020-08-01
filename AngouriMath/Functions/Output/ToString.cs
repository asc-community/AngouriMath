
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



 using AngouriMath.Core;
using System;
using System.Collections.Generic;
 using System.Linq;
 using AngouriMath.Core.Exceptions;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    using StringTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// An expression into a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Stringize();
        }
        internal string Stringize()
        {
            return Stringize(false);
        }
        internal string Stringize(bool parenthesesRequired)
        {
            if (IsLeaf)
                return this switch {
                    Pattern p => "{ " + PatternNumber + " : " + p.patType + " }",
                    Tensor t => t.ToString(),
                    VariableEntity _ => this.Name,
                    // If parentheses are required, they might be only required when complicated numbers are wrapped,
                    // such as fractions and complex but not a single i
                    NumberEntity n => n.Value.ToString(Priority != Const.PRIOR_NUM && parenthesesRequired),
                    _ => throw new UnknownEntityException()
                };
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeStringize(Name, Children), parenthesesRequired, latex: false);
        }
    }

    internal static partial class MathFunctions
    {
        internal static readonly StringTable stringTable = new StringTable();

        public static string InvokeStringize(string typeName, List<Entity> args)
        {
            return stringTable[typeName](args);
        }
    }
    
    internal static partial class Sumf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_SUM) + " + " + args[1].Stringize(args[1].Priority < Const.PRIOR_SUM);
        }
    }
    internal static partial class Minusf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_MINUS) + " - " + args[1].Stringize(args[1].Priority <= Const.PRIOR_MINUS);
        }
    }
    internal static partial class Mulf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_MUL) + " * " + args[1].Stringize(args[1].Priority < Const.PRIOR_MUL);
        }
    }
    internal static partial class Divf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_DIV) + " / " + args[1].Stringize(args[1] is OperatorEntity && args[1].Priority <= Const.PRIOR_DIV);
        }
    }
    internal static partial class Sinf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "sin(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Cosf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cos(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Tanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "tan(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Cotanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cotan(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Logf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return "log(" + args[0].Stringize() + ", " + args[1].Stringize() + ")";
        }
    }
    internal static partial class Powf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[1] == 0.5)
            {
                return "sqrt(" + args[0].Stringize() + ")";
            }
            else
            {
                return args[0].Stringize(args[0].Priority < Const.PRIOR_POW) + " ^ " + args[1].Stringize(args[1].Priority < Const.PRIOR_POW);
            }
        }
    }
    internal static partial class Arcsinf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arcsin(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Arccosf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccos(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Arctanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arctan(" + args[0].Stringize() + ")";
        }
    }
    internal static partial class Arccotanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccotan(" + args[0].Stringize() + ")";
        }
    }

    internal static partial class Factorialf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_NUM) + "!";
        }
    }

    internal static partial class Derivativef
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            return $"derive({args[0].Stringize(false)}, {args[1].Stringize(false)}, {args[2].Stringize(false)})";
        }
    }

    internal static partial class Integralf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            return $"integrate({args[0].Stringize(false)}, {args[1].Stringize(false)}, {args[2].Stringize(false)})";
        }
    }

    internal static class SetToString
    {
        static readonly List<string> Operators = new List<string>
        {
            "|",
            "&",
            @"\"
        };
        internal static string OperatorToString(OperatorSet set)
        {
            // TODO: add check whether set is correct (add TreeAnalyzer.CheckSet)
            if (set.ConnectionType == OperatorSet.OperatorType.INVERSION)
                return "!" + set.Children[0].ToString();
            return "(" + set.Children[0].ToString() + ")" + Operators[(int)set.ConnectionType] + "(" + set.Children[1].ToString() + ")";
        }

        internal static string LinearSetToString(Set set)
        => set.IsEmpty() ? "{}" :
            string.Join("|", set.Pieces.Select(c => c.ToString()));
    }
}
