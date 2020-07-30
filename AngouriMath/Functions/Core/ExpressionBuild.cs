
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




/*
 * The main file for building an expression tree of AngouriMath
 * Here we define function Hang for each math function which replaces a node A with a node C and its children A and B
 * However, nothing interesting for users
 */

namespace AngouriMath
{
    internal static partial class Sumf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("sumf", Const.PRIOR_SUM);
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }

    internal static partial class Minusf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("minusf", Const.PRIOR_MINUS);
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }

    internal static partial class Mulf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("mulf", Const.PRIOR_MUL);
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Divf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("divf", Const.PRIOR_DIV);
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }
    internal static partial class Powf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("powf", Const.PRIOR_POW);
            res.AddChild(a);
            res.AddChild(b);
            return res;
        }
    }

    internal static partial class Sinf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("sinf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Cosf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("cosf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Tanf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("tanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Cotanf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("cotanf");
            res.AddChild(a);
            return res;
        }
    }


    internal static partial class Logf
    {
        public static Entity Hang(Entity a, Entity n)
        {
            var res = new FunctionEntity("logf");
            res.AddChild(a);
            res.AddChild(n);
            return res;
        }
    }

    internal static partial class Arcsinf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("arcsinf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Arccosf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("arccosf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Arctanf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("arctanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Arccotanf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("arccotanf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class Factorialf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("factorialf");
            res.AddChild(a);
            return res;
        }
    }

    internal static partial class MathFunctions
    {
        public static void AssertArgs(int a, int b)
        {
            if (a != b)
            {
                throw new MathSException("Invalid amount of arguments");
            }
        }
    }
}