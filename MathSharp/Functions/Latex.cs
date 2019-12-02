using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    using LatexTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity
    {
        public string Latexise() => Latexise(false);
        public string Latexise(bool parenthesesRequired)
        {
            if (IsLeaf)
                return this is NumberEntity ? this.GetValue().ToString() : this.ToString();
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeLatex(Name, children), parenthesesRequired);
        }
    }

    public static partial class MathFunctions
    {
        internal static readonly LatexTable latexTable = new LatexTable();

        public static string InvokeLatex(string typeName, List<Entity> args)
        {
            return latexTable[typeName](args);
        }
        public static string ParenthesesOnNeed(string s, bool need)
        {
            return need ? "(" + s + ")" : s;
        }
    }

    // TODO
    public static partial class Sumf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_SUM) + "+" + args[1].Latexise(args[0].Priority < Const.PRIOR_SUM);
        }
    }
    public static partial class Minusf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_MINUS) + "-" + args[1].Latexise(args[0].Priority < Const.PRIOR_MINUS);
        }
    }
    public static partial class Mulf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_MUL) + "*" + args[1].Latexise(args[1].Priority < Const.PRIOR_MUL);
        }
    }
    public static partial class Divf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return @"\frac{" + args[0].Latexise() + "}{" + args[1].Latexise() + "}";
        }
    }
    public static partial class Sinf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "sin(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Cosf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return "cos(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Logf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return "log_{" + args[1].Latexise() + "}(" + args[0].Latexise() + ")";
        }
    }
    public static partial class Powf
    {
        public static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[1] == 0.5)
            {
                return @"\sqrt{" + args[0].Latexise() + "}";
            }
            else
            {
                return "{" + args[0].Latexise() + "}^{" + args[1].Latexise() + "}";
            }
        }
    }
}
