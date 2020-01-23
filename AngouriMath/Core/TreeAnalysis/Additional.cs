using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        // TODO: duplication
        internal static Entity R() => new VariableEntity("r");

        /*
        /// <summary>
        /// Counts all combinations of roots, for example
        /// 3 ^ 0.5 + 4 ^ 0.25 will return a set of 8 different numbers
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// TODO: realize all methods
        internal static EntitySet EvalAll(Entity expr)
        {
            var res = new EntitySet();
            EvalCombs(expr, res);
            return res;
        }
        
        internal static void EvalCombs(Entity expr, EntitySet set)
        {
            if (expr.Name == "Powf" && 
                MathS.CanBeEvaluated(expr.Children[1]) && 
                MathS.CanBeEvaluated(expr.Children[0]) &&
                ())
            {

            }
            else
                set.Add(expr.InnerSimplify());
        }*/
    }
}
