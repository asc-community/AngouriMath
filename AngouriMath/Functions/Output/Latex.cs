using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using LatexTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity
    {
        /// <summary>
        /// Returns the expression in format of latex (for example, a / b -> \frac{a}{b})
        /// </summary>
        /// <returns></returns>
        public string Latexise() => Latexise(false);
        internal string Latexise(bool parenthesesRequired)
        {
            if (IsLeaf)
                return this.type == Type.NUMBER ? this.GetValue().ToString() : this.ToString();
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeLatex(Name, Children), parenthesesRequired);
        }
    }

    public static partial class MathFunctions
    {
        internal static readonly LatexTable latexTable = new LatexTable();

        internal static string InvokeLatex(string typeName, List<Entity> args)
        {
            return latexTable[typeName](args);
        }
        internal static string ParenthesesOnNeed(string s, bool need)
        {
            return need ? "(" + s + ")" : s;
        }
    }

    public static partial class Sumf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_SUM) + "+" + args[1].Latexise(args[0].Priority < Const.PRIOR_SUM);
        }
    }
    public static partial class Minusf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_MINUS) + "-" + args[1].Latexise(args[0].Priority < Const.PRIOR_MINUS);
        }
    }
    public static partial class Mulf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_MUL) + "*" + args[1].Latexise(args[1].Priority < Const.PRIOR_MUL);
        }
    }
    public static partial class Divf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return @"\frac{" + args[0].Latexise() + "}{" + args[1].Latexise() + "}";
        }
    }
    public static partial class Sinf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "sin(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Cosf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cos(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Tanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "tan(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Cotanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cotan(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Logf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return "log_{" + args[1].Latexise() + "}(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Powf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[1] == 0.5)
            {
                return @"\sqrt{" + args[0].Latexise() + "}";
            }
            else
            {
                return "{" + args[0].Latexise(args[0].Priority < Const.PRIOR_POW) + "}^{" + args[1].Latexise() + "}";
            }
        }
    }
    public static partial class Arcsinf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arcsin(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Arccosf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccos(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Arctanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arctan(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Arccotanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "arccotan(" + args[0].Latexise() + ")";
        }
    }
}
