
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



﻿using System;
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
