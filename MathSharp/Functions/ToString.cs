using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    using StringTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity
    {
        public override string ToString()
        {
            return Stringize();
        }
        public string Stringize(bool parenthesesRequired = false)
        {
            if (IsLeaf)
                return this.Name;
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeStringize(Name, children), parenthesesRequired);
        }
    }

    public static partial class MathFunctions
    {
        internal static StringTable stringTable = new StringTable();

        public static string InvokeStringize(string typeName, List<Entity> args)
        {
            return stringTable[typeName](args);
        }
    }
    
    // TODO
    public static partial class Sumf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_SUM) + " + " + args[1].Stringize(args[0].Priority < Const.PRIOR_SUM);
        }
    }
    public static partial class Minusf
    {
        public static string Stringize(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Stringize(args[0].Priority < Const.PRIOR_MINUS) + " - " + args[1].Stringize(args[0].Priority < Const.PRIOR_MINUS);
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
            return args[0].Stringize(args[0].Priority < Const.PRIOR_DIV) + " / " + args[1].Stringize(args[1].Priority < Const.PRIOR_DIV);
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
}
