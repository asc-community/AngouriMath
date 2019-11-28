using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    public static partial class Sumf
    {
        static Sumf() {
            MathFunctions.evalTable["Sumf"] = Simplify;
            MathFunctions.deriveTable["Sumf"] = Derive;
            MathFunctions.latexTable["Sumf"] = Latex;
            MathFunctions.stringTable["Sumf"] = Stringize;
        }
    }
    public static partial class Minusf
    {
        static Minusf()
        {
            MathFunctions.evalTable["Minusf"] = Simplify;
            MathFunctions.deriveTable["Minusf"] = Derive;
            MathFunctions.latexTable["Minusf"] = Latex;
            MathFunctions.stringTable["Minusf"] = Stringize;
        }
    }
    public static partial class Mulf
    {
        static Mulf()
        {
            MathFunctions.evalTable["Mulf"] = Simplify;
            MathFunctions.deriveTable["Mulf"] = Derive;
            MathFunctions.latexTable["Mulf"] = Latex;
            MathFunctions.stringTable["Mulf"] = Stringize;
        }
    }
    public static partial class Divf
    {
        static Divf()
        {
            MathFunctions.evalTable["Divf"] = Simplify;
            MathFunctions.deriveTable["Divf"] = Derive;
            MathFunctions.latexTable["Divf"] = Latex;
            MathFunctions.stringTable["Divf"] = Stringize;
        }
    }
    public static partial class Powf
    {
        static Powf()
        {
            MathFunctions.evalTable["Powf"] = Simplify;
            MathFunctions.deriveTable["Powf"] = Derive;
            MathFunctions.latexTable["Powf"] = Latex;
            MathFunctions.stringTable["Powf"] = Stringize;
        }
    }
    public static partial class Sinf
    {
        static Sinf()
        {
            MathFunctions.evalTable["Sinf"] = Simplify;
            MathFunctions.deriveTable["Sinf"] = Derive;
            MathFunctions.latexTable["Sinf"] = Latex;
            MathFunctions.stringTable["Sinf"] = Stringize;
        }
    }
    public static partial class Cosf
    {
        static Cosf()
        {
            MathFunctions.evalTable["Cosf"] = Simplify;
            MathFunctions.deriveTable["Cosf"] = Derive;
            MathFunctions.latexTable["Cosf"] = Latex;
            MathFunctions.stringTable["Cosf"] = Stringize;
        }
    }
    public static partial class Logf
    {
        static Logf()
        {
            MathFunctions.evalTable["Logf"] = Eval;
            MathFunctions.deriveTable["Logf"] = Derive;
            MathFunctions.latexTable["Logf"] = Latex;
            MathFunctions.stringTable["Logf"] = Stringize;
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
        public Entity Log(Entity n) => Logf.Hang(this, n);
        public bool IsLowerThan(Entity a)
        {
            return Priority < a.Priority;
        }
    }
}
