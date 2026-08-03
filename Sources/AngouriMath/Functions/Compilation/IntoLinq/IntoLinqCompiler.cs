//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq.Expressions;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    internal static class IntoLinqCompiler
    {
        internal static TDelegate Compile<TDelegate>(
            Entity expr,
            Type? returnType,
            CompilationProtocol protocol,
            IEnumerable<(Type type, Variable variable)> typesAndNames
            ) where TDelegate : Delegate
        {
            // A matrix has no compiled form, but an expression built out of matrices often
            // has a value that is an ordinary number -- [0, 1]T * [[a, b], [c, d]] * [1, 0]
            // is c -- and there is nothing to stop that being compiled. Nothing simplified
            // before compiling, so those failed along with the ones that genuinely cannot
            // be compiled (#425). Only expressions that mention a matrix pay for this.
            if (expr.Nodes.Any(node => node is Entity.Matrix))
                expr = expr.InnerSimplified;

            var subexpressionsCache = typesAndNames.ToDictionary(c => (Entity)c.variable, c => Expression.Parameter(c.type));
            var functionArguments = subexpressionsCache.Select(c => c.Value).ToArray(); // copying
            var localVars = new List<ParameterExpression>();
            var variableAssignments = new List<Expression>();

            var tree = BuildTree(expr, subexpressionsCache, variableAssignments, localVars, protocol);
            var treeWithLocals = Expression.Block(localVars, variableAssignments.Append(tree));
            Expression entireExpression = returnType is not null ? protocol.ConvertType(treeWithLocals, returnType) : treeWithLocals;
            var finalLambda = Expression.Lambda<TDelegate>(entireExpression, functionArguments);

            return finalLambda.Compile();
        }

        internal static Expression BuildTree(
            Entity expr,
            Dictionary<Entity, ParameterExpression> cachedSubexpressions,
            List<Expression> variableAssignments,
            List<ParameterExpression> newLocalVars,
            CompilationProtocol protocol)
        {
            if (cachedSubexpressions.TryGetValue(expr, out var readyVar))
                return readyVar;

            Expression subTree = expr switch
            {
                Variable { IsConstant: true } c
                    => BuildTree(c.Evaled, cachedSubexpressions, variableAssignments, newLocalVars, protocol),

                Variable x => cachedSubexpressions[x],

                Entity.Boolean or Number => protocol.ConvertConstant(expr),

                IUnaryNode oneArg
                    => protocol.ConvertUnaryNode(
                        BuildTree(oneArg.NodeChild, cachedSubexpressions, variableAssignments, newLocalVars, protocol),
                        expr),

                IBinaryNode twoArg
                    => protocol.ConvertBinaryNode(
                        BuildTree(twoArg.NodeFirstChild, cachedSubexpressions, variableAssignments, newLocalVars, protocol), 
                        BuildTree(twoArg.NodeSecondChild, cachedSubexpressions, variableAssignments, newLocalVars, protocol), 
                        expr),

                var other => protocol.ConvertOtherNode(other.DirectChildren.Select(c => BuildTree(c, cachedSubexpressions, variableAssignments, newLocalVars, protocol)), expr)
            };

            var newVar = Expression.Variable(subTree.Type);
            variableAssignments.Add(Expression.Assign(newVar, subTree));
            cachedSubexpressions[expr] = newVar;
            newLocalVars.Add(newVar);
            return newVar;
        }
    }
}
