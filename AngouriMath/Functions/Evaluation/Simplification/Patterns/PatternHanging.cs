
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


namespace AngouriMath
{
    internal static partial class Sumf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, Entity.PatType.OPERATOR, Const.Patterns.AlwaysTrue, "sumf");
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Minusf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, Entity.PatType.OPERATOR, Const.Patterns.AlwaysTrue, "minusf");
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Mulf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, Entity.PatType.OPERATOR, Const.Patterns.AlwaysTrue, "mulf");
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Divf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, Entity.PatType.OPERATOR, Const.Patterns.AlwaysTrue, "divf");
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Powf
    {
        internal static Pattern PHang(Entity a, Entity b)
        {
            var res = new Pattern(-1, Entity.PatType.OPERATOR, Const.Patterns.AlwaysTrue, "powf");
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }

    internal static partial class Sinf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "sinf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Cosf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "cosf");
            res.AddChild(a);
            return res;
        }
    }
    internal static partial class Tanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "tanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Cotanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "cotanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Arcsinf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "arcsinf");
            res.AddChild(a);
            return res;
        }
    }
    internal static partial class Arccosf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "arccosf");
            res.AddChild(a);
            return res;
        }
    }
    internal static partial class Arctanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "arctanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Arccotanf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "arccotanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Logf
    {
        internal static Pattern PHang(Entity a, Entity n)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "logf");
            res.AddChild(n);
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Factorialf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "factorialf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Factorialf
    {
        internal static Pattern PHang(Entity a)
        {
            var res = new Pattern(-1, Entity.PatType.FUNCTION, Const.Patterns.AlwaysTrue, "factorialf");
            res.AddChild(a);
            return res;
        }
    }
}
