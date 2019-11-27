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
        }
    }
    public static partial class Minusf
    {
        static Minusf()
        {
            MathFunctions.evalTable["Minusf"] = Simplify;
            MathFunctions.deriveTable["Minusf"] = Derive;
        }
    }
    public static partial class Mulf
    {
        static Mulf()
        {
            MathFunctions.evalTable["Mulf"] = Simplify;
            MathFunctions.deriveTable["Mulf"] = Derive;
        }
    }
    public static partial class Divf
    {
        static Divf()
        {
            MathFunctions.evalTable["Divf"] = Simplify;
            MathFunctions.deriveTable["Divf"] = Derive;
        }
    }
    public static partial class Powf
    {
        static Powf()
        {
            MathFunctions.evalTable["Powf"] = Simplify;
            MathFunctions.deriveTable["Powf"] = Derive;
        }
    }
    public static partial class Sinf
    {
        static Sinf()
        {
            MathFunctions.evalTable["Sinf"] = Simplify;
            MathFunctions.deriveTable["Sinf"] = Derive;
        }
    }
    public static partial class Cosf
    {
        static Cosf()
        {
            MathFunctions.evalTable["Cosf"] = Simplify;
            MathFunctions.deriveTable["Cosf"] = Derive;
        }
    }
    public static partial class Logf
    {
        static Logf()
        {
            MathFunctions.evalTable["Logf"] = Eval;
            MathFunctions.deriveTable["Logf"] = Derive;
        }
    }

    public abstract partial class Entity
    {
        public static Entity operator +(Entity a, Entity b) => Sumf.Hang(a, b);
        public static Entity operator -(Entity a, Entity b) => Minusf.Hang(a, b);
        public static Entity operator *(Entity a, Entity b) => Mulf.Hang(a, b);
        public static Entity operator /(Entity a, Entity b) => Divf.Hang(a, b);
        public static Entity operator ^(Entity a, Entity b) => Powf.Hang(a, b);
        public Entity Sin() => Sinf.Hang(this);
        public Entity Cos() => Cosf.Hang(this);
        public Entity Log(Entity n) => Logf.Hang(this, n);
    }
}
