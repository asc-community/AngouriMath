using MathSharp.Core;
using MathSharp.Core.FromString;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;


/*
 * MathS.Sin(x) - sine of an expression
 * MathS.Cos(x) - cosine of an expression
 * MathS.Log(a, n) - logarithm of a and base n
 * MathS.Pow(a, n) - a ^ n
 * MathS.Sqr, MathS.Sqrt
 * 
 * MathS.Var() - creating an instance of variable
 * MathS.Num() - creating an instance of number (but in most cases you can use actual numbers, for example `Var("x") + 4` is ok)
 */

namespace MathSharp
{
    public class MathS
    {
        public delegate Entity OneArg(Entity a);
        public delegate Entity TwoArg(Entity a, Entity n);
        public delegate VariableEntity VarFunc(string v);
        public delegate NumberEntity NumFunc(Number v);

        public static OneArg Sin = Sinf.Hang;
        public static OneArg Cos = Cosf.Hang;
        public static TwoArg Log = Logf.Hang;
        public static TwoArg Pow = Powf.Hang;
        public static OneArg Sqrt = a => Powf.Hang(a, 0.5);
        public static OneArg Sqr = a => Powf.Hang(a, 2);
        public static OneArg Tan = a => MathS.Sin(a) / MathS.Cos(a);
        public static OneArg Cotan = a => 1 / Tan(a);
        public static OneArg Sec = a => 1 / Cos(a);
        public static OneArg Cosec = a => 1 / Sin(a);
        public static OneArg Ln => v => Logf.Hang(v, e);
        public static VarFunc Var = v => new VariableEntity(v);
        public static VarFunc Symbol = v => new VariableEntity(v);
        public static NumFunc Num = v => new NumberEntity(v);
        public static Number e = 2.718281828459045235;
        public static Number i = new Number(0, 1);
        public static Number pi = 3.141592653589793;
        private static void InitOps()
        {
            // TODO
            Sumf.Wakeup();
            Minusf.Wakeup();
            Mulf.Wakeup();
            Divf.Wakeup();
            Sinf.Wakeup();
            Cosf.Wakeup();
            Powf.Wakeup();
            Logf.Wakeup();
        }
        public static Entity FromString(string expr)
        {
            InitOps();
            var lexer = new Lexer(expr);
            return Parser.Parse(lexer);
        }
        static MathS()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
        }
    }
}
