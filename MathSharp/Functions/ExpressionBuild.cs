using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * The main file for building an expression tree of Math#
 * Here we define function Hang for each math function which replaces a node A with a node C and its children A and B
 * However, nothing interesting for users
 */

namespace MathSharp
{
    public static partial class Sumf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Sumf", Const.PRIOR_SUM);
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Minusf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Minusf", Const.PRIOR_MINUS);
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Mulf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Mulf", Const.PRIOR_MUL);
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Divf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Divf", Const.PRIOR_DIV);
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }
    public static partial class Powf
    {
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Powf", Const.PRIOR_POW);
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Sinf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("Sinf");
            res.children.Add(a);
            return res;
        }
    }

    public static partial class Cosf
    {
        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("Cosf");
            res.children.Add(a);
            return res;
        }
    }

    public static partial class Logf
    {
        public static Entity Hang(Entity a, Entity n)
        {
            var res = new FunctionEntity("Logf");
            res.children.Add(a);
            res.children.Add(n);
            return res;
        }
    }

    public static partial class MathFunctions
    {
        public static void AssertArgs(int a, int b)
        {
            if (a != b)
                throw new MathSException("Invalid amount of arguments");
        }
    }
}