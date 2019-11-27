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
    using Table = Dictionary<string, Func<List<Entity>, Entity>>;

    public static partial class Sumf
    {
        static Sumf() => MathFunctions.funcTable["Sumf"] = Eval;
 
        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Sumf");
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Minusf
    {
        static Minusf() => MathFunctions.funcTable["Minusf"] = Eval;

        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Minusf");
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Mulf
    {
        static Mulf() => MathFunctions.funcTable["Mulf"] = Eval;

        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Mulf");
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Divf
    {
        static Divf() => MathFunctions.funcTable["Divf"] = Eval;

        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Divf");
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }
    public static partial class Powf
    {
        static Powf() => MathFunctions.funcTable["Powf"] = Eval;

        public static Entity Hang(Entity a, Entity b)
        {
            var res = new OperatorEntity("Powf");
            res.children.Add(a);
            res.children.Add(b);
            return res;
        }
    }

    public static partial class Sinf
    {
        static Sinf() => MathFunctions.funcTable["Sinf"] = Eval;

        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("Sinf");
            res.children.Add(a);
            return res;
        }
    }

    public static partial class Cosf
    {
        static Cosf() => MathFunctions.funcTable["Cosf"] = Eval;

        public static Entity Hang(Entity a)
        {
            var res = new FunctionEntity("Cosf");
            res.children.Add(a);
            return res;
        }
    }

    public static class MathFunctions
    {
        internal static Table funcTable = new Table();

        public static Entity Invoke(string typeName, List<Entity> args)
        {
            return funcTable[typeName](args);
        }

        public static void Assert(int a, int b)
        {
            if (a != b)
                throw new MathSException("Invalid amount of arguments");
        }
    }
}