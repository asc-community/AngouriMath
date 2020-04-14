
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



﻿using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using StringTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity
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
                return entType switch { 
                    EntType.PATTERN => "{ " + PatternNumber + " : " + PatternType + " }",
                    EntType.TENSOR => (this as Tensor).ToString(),
                    _ => this.Name.Length == 0 || this.Name[0] != '-' ? this.Name : "(" + this.Name + ")"
                };
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeStringize(Name, Children), parenthesesRequired);
        }
    }

    public static partial class MathFunctions
    {
        internal static readonly StringTable stringTable = new StringTable();

        public static string InvokeStringize(string typeName, List<Entity> args)
        {
            return stringTable[typeName](args);
        }
    }
    
    public static partial class Sumf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_SUM) + " + " + args[1].Stringize(args[1].Priority < Const.PRIOR_SUM);
        }
    }
    public static partial class Minusf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_MINUS) + " - " + args[1].Stringize(args[1].Priority <= Const.PRIOR_MINUS);
        }
    }
    public static partial class Mulf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_MUL) + " * " + args[1].Stringize(args[1].Priority < Const.PRIOR_MUL);
        }
    }
    public static partial class Divf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_DIV) + " / " + args[1].Stringize(args[1].entType == Entity.EntType.OPERATOR && args[1].Priority <= Const.PRIOR_DIV);
        }
    }
    public static partial class Sinf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "sin(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Cosf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cos(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Tanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "tan(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Cotanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cotan(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Logf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return "log(" + args[0].Stringize() + ", " + args[1].Stringize() + ")";
        }
    }
    public static partial class Powf
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
    public static partial class Arcsinf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arcsin(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Arccosf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccos(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Arctanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arctan(" + args[0].Stringize() + ")";
        }
    }
    public static partial class Arccotanf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccotan(" + args[0].Stringize() + ")";
        }
    }
}
