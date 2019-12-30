using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using AngouriMath.Core;

namespace AngouriMath
{
    /// <summary>
    /// If I need to add a function or operator, I first do it here.
    /// </summary>
    public static partial class MathS
    {
        public delegate Entity OneArg(Entity a);
        public delegate Entity TwoArg(Entity a, Entity n);
        public delegate VariableEntity VarFunc(string v);
        public delegate Number NumFunc(double a, double b = 0);
        static MathS()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            Sumf.Wakeup();
            Minusf.Wakeup();
            Mulf.Wakeup();
            Divf.Wakeup();
            Powf.Wakeup();
            Sinf.Wakeup();
            Cosf.Wakeup();
            Logf.Wakeup();
        }
    }
    public static partial class Sumf
    {
        public static void Wakeup() { }
        static Sumf() {
            MathFunctions.evalTable["sumf"] = Simplify;
            MathFunctions.deriveTable["sumf"] = Derive;
            MathFunctions.latexTable["sumf"] = Latex;
            MathFunctions.stringTable["sumf"] = Stringize;
        }
    }
    public static partial class Minusf
    {
        public static void Wakeup() { }
        static Minusf()
        {
            MathFunctions.evalTable["minusf"] = Simplify;
            MathFunctions.deriveTable["minusf"] = Derive;
            MathFunctions.latexTable["minusf"] = Latex;
            MathFunctions.stringTable["minusf"] = Stringize;
        }
    }
    public static partial class Mulf
    {
        public static void Wakeup() { }
        static Mulf()
        {
            MathFunctions.evalTable["mulf"] = Simplify;
            MathFunctions.deriveTable["mulf"] = Derive;
            MathFunctions.latexTable["mulf"] = Latex;
            MathFunctions.stringTable["mulf"] = Stringize;
        }
    }
    public static partial class Divf
    {
        public static void Wakeup() { }
        static Divf()
        {
            MathFunctions.evalTable["divf"] = Simplify;
            MathFunctions.deriveTable["divf"] = Derive;
            MathFunctions.latexTable["divf"] = Latex;
            MathFunctions.stringTable["divf"] = Stringize;
        }
    }
    public static partial class Powf
    {
        public static void Wakeup() { }
        static Powf()
        {
            MathFunctions.evalTable["powf"] = Simplify;
            MathFunctions.deriveTable["powf"] = Derive;
            MathFunctions.latexTable["powf"] = Latex;
            MathFunctions.stringTable["powf"] = Stringize;
        }
    }
    public static partial class Sinf
    {
        public static void Wakeup() { }
        static Sinf()
        {
            MathFunctions.evalTable["sinf"] = Simplify;
            MathFunctions.deriveTable["sinf"] = Derive;
            MathFunctions.latexTable["sinf"] = Latex;
            MathFunctions.stringTable["sinf"] = Stringize;
        }
    }
    public static partial class Cosf
    {
        public static void Wakeup() { }
        static Cosf()
        {
            MathFunctions.evalTable["cosf"] = Simplify;
            MathFunctions.deriveTable["cosf"] = Derive;
            MathFunctions.latexTable["cosf"] = Latex;
            MathFunctions.stringTable["cosf"] = Stringize;
        }
    }
    public static partial class Tanf
    {
        public static void Wakeup() { }
        static Tanf()
        {
            MathFunctions.evalTable["tanf"] = Simplify;
            MathFunctions.deriveTable["tanf"] = Derive;
            MathFunctions.latexTable["tanf"] = Latex;
            MathFunctions.stringTable["tanf"] = Stringize;
        }
    }
    public static partial class Cotanf
    {
        public static void Wakeup() { }
        static Cotanf()
        {
            MathFunctions.evalTable["cotanf"] = Simplify;
            MathFunctions.deriveTable["cotanf"] = Derive;
            MathFunctions.latexTable["cotanf"] = Latex;
            MathFunctions.stringTable["cotanf"] = Stringize;
        }
    }
    public static partial class Logf
    {
        public static void Wakeup() { }
        static Logf()
        {
            MathFunctions.evalTable["logf"] = Eval;
            MathFunctions.deriveTable["logf"] = Derive;
            MathFunctions.latexTable["logf"] = Latex;
            MathFunctions.stringTable["logf"] = Stringize;
        }
    }
    public static partial class Arcsinf
    {
        public static void Wakeup() { }
        static Arcsinf()
        {
            MathFunctions.evalTable["arcsinf"] = Eval;
            MathFunctions.deriveTable["arcsinf"] = Derive;
            MathFunctions.latexTable["arcsinf"] = Latex;
            MathFunctions.stringTable["arcsinf"] = Stringize;
        }
    }
    public static partial class Arccosf
    {
        public static void Wakeup() { }
        static Arccosf()
        {
            MathFunctions.evalTable["arccosf"] = Eval;
            MathFunctions.deriveTable["arccosf"] = Derive;
            MathFunctions.latexTable["arccosf"] = Latex;
            MathFunctions.stringTable["arccosf"] = Stringize;
        }
    }
    public static partial class Arctanf
    {
        public static void Wakeup() { }
        static Arctanf()
        {
            MathFunctions.evalTable["arctanf"] = Eval;
            MathFunctions.deriveTable["arctanf"] = Derive;
            MathFunctions.latexTable["arctanf"] = Latex;
            MathFunctions.stringTable["arctanf"] = Stringize;
        }
    }
    public static partial class Arccotanf
    {
        public static void Wakeup() { }
        static Arccotanf()
        {
            MathFunctions.evalTable["arccotanf"] = Eval;
            MathFunctions.deriveTable["arccotanf"] = Derive;
            MathFunctions.latexTable["arccotanf"] = Latex;
            MathFunctions.stringTable["arccotanf"] = Stringize;
        }
    }

    public abstract partial class Entity
    {
        public int Priority { get; internal set; }
        public static Entity operator +(Entity a, Entity b) => Sumf.Hang(a, b);
        public static Entity operator +(Entity a) => a;
        public static Entity operator -(Entity a, Entity b) => Minusf.Hang(a, b);
        public static Entity operator -(Entity a) => Mulf.Hang(-1, a);
        public static Entity operator *(Entity a, Entity b) => Mulf.Hang(a, b);
        public static Entity operator /(Entity a, Entity b) => Divf.Hang(a, b);
        public Entity Pow(Entity n) => Powf.Hang(this, n);
        public Entity Sin() => Sinf.Hang(this);
        public Entity Cos() => Cosf.Hang(this);
        public Entity Tan() => Tanf.Hang(this);
        public Entity Cotan() => Cotanf.Hang(this);
        public Entity Arcsin() => Arcsinf.Hang(this);
        public Entity Arccos() => Arccosf.Hang(this);
        public Entity Arctan() => Arctanf.Hang(this);
        public Entity Arccotan() => Arccotanf.Hang(this);
        public Entity Log(Entity n) => Logf.Hang(this, n);
        public bool IsLowerThan(Entity a)
        {
            return Priority < a.Priority;
        }
    }
}
