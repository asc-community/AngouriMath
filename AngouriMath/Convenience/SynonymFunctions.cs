using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using FuncTable = Dictionary<string, Func<List<Entity>, Entity>>;
    using SynTable = Dictionary<string, string>;
    internal static class SynonymFunctions
    {
        internal static readonly FuncTable SynFunctions = new FuncTable
        {
            { "sqrtf", args => MathS.Pow(args[0], 0.5) },
            { "sqrf", args => MathS.Pow(args[0], 2) },
            { "lnf", args => MathS.Log(args[0], MathS.e) },
            { "secf", args => MathS.Sec(args[0]) },
            { "cosecf", args => MathS.Cosec(args[0]) },
        };
        internal static Entity Synonymize(Entity tree)
        {
            for (int i = 0; i < tree.Children.Count; i++)
            {
                tree.Children[i] = Synonymize(tree.Children[i]);
            }
            if (SynFunctions.ContainsKey(tree.Name))
                return SynFunctions[tree.Name](tree.Children);
            else
                return tree;
        }
    }
}
