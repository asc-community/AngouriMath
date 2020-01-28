using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions.Evaluation.Compilation
{
    internal static class Compiler
    {
        internal static FastExpression Compile(Entity func, params VariableEntity[] variables)
        {
            var strings = new List<string>();
            foreach (var varEnt in variables)
                if (!MathS.IsConstant(varEnt))
                    strings.Add(varEnt.Name);
            return Compile(func, strings.ToArray());
        }
        internal static FastExpression Compile(Entity func, params string[] variables)
        {
            var varNamespace = new Dictionary<string, int>();
            int id = 0;
            foreach (var varName in variables)
            {
                varNamespace[varName] = id;
                id++;
            }
            var res = new FastExpression(variables.Length, func.SubstituteConstants());
            InnerCompile(func, res, variables, varNamespace);
            res.Seal(); // Creates stack
            return res;
        }

        private static void InnerCompile(Entity expr, FastExpression fe, string[] variables, Dictionary<string, int> varNamespace)
        {
            // Check whether it's better to pull from cache or not
            string hash = expr.ToString();
            if (fe.HashToNum.ContainsKey(hash))
            {
                fe.instructions.AddPullCacheInstruction(fe.HashToNum[hash]);
                return;
            }

            for (int i = expr.Children.Count - 1; i >= 0; i--)
                InnerCompile(expr.Children[i], fe, variables, varNamespace);
            if (expr.entType == Entity.EntType.OPERATOR || expr.entType == Entity.EntType.FUNCTION)
                fe.instructions.AddCallInstruction(expr.Name, expr.Children.Count);
            else if (expr.entType == Entity.EntType.NUMBER)
                fe.instructions.AddPushNumInstruction(expr.GetValue().value);
            else if (expr.entType == Entity.EntType.VARIABLE)
                fe.instructions.AddPushVarInstruction(varNamespace[expr.Name]);
            else
                throw new SysException("Unknown entity");

            // If the function is used more than once AND complex enough, we put it in cache
            if (fe.RawExpr.CountOccurances(hash) > 1 /*Raw expr is basically the root entity that we're compiling*/
                && expr.Complexity() > 1 /* we don't check if it is already in cache as in this case it will pull from cache*/)
            {
                fe.HashToNum[hash] = fe.HashToNum.Count;
                fe.instructions.AddPushCacheInstruction(fe.HashToNum[hash]);
            }
        }
    }
}
