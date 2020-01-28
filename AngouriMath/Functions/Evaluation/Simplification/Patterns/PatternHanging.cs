using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using PatType = Entity.PatType;
    public static partial class Sumf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, PatType.OPERATOR, "sumf");
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Minusf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, PatType.OPERATOR, "minusf");
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Mulf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, PatType.OPERATOR, "mulf");
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Divf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, PatType.OPERATOR, "divf");
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Powf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, PatType.OPERATOR, "powf");
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }

    public static partial class Sinf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "sinf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Cosf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "cosf");
            res.Children.Add(a);
            return res;
        }
    }
    public static partial class Tanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "tanf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Cotanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "cotanf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Arcsinf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "arcsinf");
            res.Children.Add(a);
            return res;
        }
    }
    public static partial class Arccosf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "arccosf");
            res.Children.Add(a);
            return res;
        }
    }
    public static partial class Arctanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "arctanf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Arccotanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "arccotanf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Logf
    {
        internal static Pattern PHang(Entity a, Entity n)
        {
            var res = new Pattern(-1, PatType.FUNCTION, "logf");
            res.Children.Add(a);
            res.Children.Add(n);
            return res;
        }
    }
}
