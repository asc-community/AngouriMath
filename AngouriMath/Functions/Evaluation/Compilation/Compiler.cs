
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



﻿using AngouriMath.Core.Exceptions;
 using System.Collections.Generic;

namespace AngouriMath.Functions.Evaluation.Compilation
{
    internal static class Compiler
    {
        /// <summary>
        /// Returns a compiled expression
        /// Allows to boost substitution a lot
        /// </summary>
        /// <param name="func"></param>
        /// <param name="variables">
        /// Must be equal to func's variables (ignoring constants)
        /// </param>
        /// <returns></returns>
        internal static FastExpression Compile(Entity func, params VariableEntity[] variables)
        {
            var strings = new List<string>();
            foreach (var varEnt in variables)
                if (!MathS.IsConstant(varEnt))
                    strings.Add(varEnt.Name);
            return Compile(func, strings.ToArray());
        }

        /// <summary>
        /// Compiles from strings (see Compile for more details)
        /// </summary>
        /// <param name="func"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        internal static FastExpression Compile(Entity func, params string[] variables)
        {
            var varNamespace = new Dictionary<string, int>();
            int id = 0;
            foreach (var varName in variables)
            {
                varNamespace[varName] = id;
                id++;
            }
            func = func.SubstituteConstants();
            var res = new FastExpression(variables.Length, func);
            func.UpdateHash(); // Count hash for O(N)
            Entity.HashOccurancesUpdate(func); // Update occurances for each node for O(N) instead of O(N^2)
            InnerCompile(func, res, variables, varNamespace);
            res.Seal(); // Creates stack
            return res;
        }

        /// <summary>
        /// Recursive compilation that pushes intructions to the stack (fe.instructions)
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="fe"></param>
        /// <param name="variables"></param>
        /// <param name="varNamespace"></param>
        private static void InnerCompile(Entity expr, FastExpression fe, string[] variables, Dictionary<string, int> varNamespace)
        {
            // Check whether it's better to pull from cache or not
            string hash = expr.Hash;
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
                fe.instructions.AddPushNumInstruction(expr.GetValue().AsComplex());
            else if (expr.entType == Entity.EntType.VARIABLE)
                fe.instructions.AddPushVarInstruction(varNamespace[expr.Name]);
            else
                throw new SysException("Unknown entity");

            // If the function is used more than once AND complex enough, we put it in cache
            if (expr.HashOccurances > 1 /*expr.HashOccurances is the number of this expression being replicated*/
                && !expr.IsLeaf /* we don't check if it is already in cache as in this case it will pull from cache*/)
            {
                fe.HashToNum[hash] = fe.HashToNum.Count;
                fe.instructions.AddPushCacheInstruction(fe.HashToNum[hash]);
            }
        }
    }
}
