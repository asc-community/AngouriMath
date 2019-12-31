using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    public static class TreeAnalyzer
    {
        public static List<Entity> LinearChildren(Entity tree,
                                             string funcName /*e. g. "sumf" */,
                                             string badFuncName /* e. g. "minusf" */,
                                             string badFuncSuppressorName, /* e. g. "mulf" if we need to convert -3 into -1 * 3*/
                                             int funcSupPrior)
        {
            var res = new List<Entity>();
            if (tree.Name == funcName || tree.Name == badFuncName)
            {
                res.AddRange(LinearChildren(tree.Children[0], funcName, badFuncName, badFuncSuppressorName, funcSupPrior));
                if (tree.Name == funcName)
                    res.AddRange(LinearChildren(tree.Children[1], funcName, badFuncName, badFuncSuppressorName, funcSupPrior));
                else
                {
                    // Here we need to inverse
                    var badRes = LinearChildren(tree.Children[1], funcName, badFuncName, badFuncSuppressorName, funcSupPrior);
                    var goodRes = new List<Entity>();
                    foreach (var child in badRes)
                        goodRes.Add(new OperatorEntity(badFuncSuppressorName, funcSupPrior)
                        {
                            Children = new List<Entity>
                                {
                                    -1,
                                    child,
                                }
                        });
                    res.AddRange(goodRes);
                }
            }
            else
                res.Add(tree);
            return res;
        }
        public static void Sort(List<Entity> children) => children.Sort((a, b) => a.Hash().CompareTo(b.Hash()));
        
    }
}
