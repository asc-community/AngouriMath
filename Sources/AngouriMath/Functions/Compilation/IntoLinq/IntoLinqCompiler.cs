using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
            Dictionary<Entity, ParameterExpression> args = new();
            foreach (var (type, @var) in typesAndNames)
                args[@var] = Expression.Parameter(type, @var.Name);

            var argParams = args.Values.ToArray(); // copying
            List<ParameterExpression> localVars = new();

            List<Expression> instructionSet = new();
            var tree = BuildTree(expr, (args, instructionSet, localVars, protocol));

            var finalExpr = Expression.Block(localVars, instructionSet.Append(tree));
            Expression finalExprWithCast = returnType is not null ? Expression.Convert(finalExpr, returnType) : finalExpr;
            var finalFunction = Expression.Lambda<TDelegate>(finalExprWithCast, argParams);

            return finalFunction.Compile();
        }

        internal static Expression BuildTree(
            Entity expr, 
            (Dictionary<Entity, ParameterExpression> vars, 
            List<Expression> variableAssignments, 
            List<ParameterExpression> localVars, 
            CompilationProtocol protocol) ot)
        {
            var vars = ot.vars;
            var prot = ot.protocol;
            var instructionSet = ot.variableAssignments;
            var localVars = ot.localVars;

            if (vars.TryGetValue(expr, out var readyVar))
                return readyVar;

            Expression subTree = expr switch
            {
                Variable { IsConstant: true } c => BuildTree(c.Evaled, ot),
                Variable x => vars[x],
                Entity.Boolean or Number => prot.ConstantConverter(expr),
                IUnaryNode oneArg => prot.OneArgumentConverter(BuildTree(oneArg.NodeChild, ot), expr),
                IBinaryNode twoArg => prot.TwoArgumentConverter(BuildTree(twoArg.NodeFirstChild, ot), BuildTree(twoArg.NodeSecondChild, ot), expr),
                var other => prot.AnyArgumentConverter(other.DirectChildren.Select(c => BuildTree(c, ot)), expr)
            };
            var newVar = Expression.Variable(subTree.Type);
            instructionSet.Add(Expression.Assign(newVar, subTree));
            vars[expr] = newVar;
            localVars.Add(newVar);
            return newVar;
        }
    }
}
