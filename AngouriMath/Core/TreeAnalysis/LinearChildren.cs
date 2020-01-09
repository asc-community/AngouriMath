using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static List<Entity> LinearChildren(Entity tree,
                                             string funcName /*e. g. "sumf" */,
                                             string badFuncName /* e. g. "minusf" */,
                                             Func<Entity, OperatorEntity> supCreator)
        {
            var res = new List<Entity>();
            if (tree.Name == funcName || tree.Name == badFuncName)
            {
                res.AddRange(LinearChildren(tree.Children[0], funcName, badFuncName, supCreator));
                if (tree.Name == funcName)
                    res.AddRange(LinearChildren(tree.Children[1], funcName, badFuncName, supCreator));
                else
                {
                    // Here we need to inverse
                    var badRes = LinearChildren(tree.Children[1], funcName, badFuncName, supCreator);
                    var goodRes = new List<Entity>();
                    foreach (var child in badRes)
                        goodRes.Add(supCreator(child));
                    res.AddRange(goodRes);
                }
            }
            else
                res.Add(tree);
            return res;
        }
        internal static void Sort(List<Entity> children, SortLevel level) => children.Sort((a, b) => a.Hash(level).CompareTo(b.Hash(level)));        
    }
}
