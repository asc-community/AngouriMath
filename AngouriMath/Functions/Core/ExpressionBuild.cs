using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * The main file for building an expression tree of AngouriMath
 * Here we define function Hang for each math function which replaces a node A with a node C and its children A and B
 * However, nothing interesting for users
 */

namespace AngouriMath
{
    public static partial class Sumf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("sumf", Const.PRIOR_SUM);
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }

    public static partial class Minusf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("minusf", Const.PRIOR_MINUS);
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }

    public static partial class Mulf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("mulf", Const.PRIOR_MUL);
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Divf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("divf", Const.PRIOR_DIV);
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }
    public static partial class Powf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("powf", Const.PRIOR_POW);
            res.Children.Add(a);
            res.Children.Add(b);
            return res;
        }
    }

    public static partial class Sinf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("sinf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Cosf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("cosf");
            res.Children.Add(a);
            return res;
        }
    }

    public static partial class Logf
    {
        public static Entity Hang(Entity a, Entity n)
        {
            var res = new FunctionEntity("logf");
            res.Children.Add(a);
            res.Children.Add(n);
            return res;
        }
    }

    public static partial class MathFunctions
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