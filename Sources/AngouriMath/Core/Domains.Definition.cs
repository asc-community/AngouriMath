//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Coomain of an expression
        /// If its node value is outside of the domain when evaluated,
        /// it turns into a <see cref="MathS.NaN"/>
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath.Core;
        /// using static AngouriMath.MathS;
        /// 
        /// var expr = Sqrt(-1);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Codomain);
        /// Console.WriteLine(expr.Evaled);
        /// Console.WriteLine("------------------------------------");
        /// var newExpr = expr.WithCodomain(Domain.Real);
        /// Console.WriteLine(newExpr);
        /// Console.WriteLine(newExpr.Codomain);
        /// Console.WriteLine(newExpr.Evaled);
        /// </code>
        /// Prints
        /// <code>
        /// sqrt(-1)
        /// Complex
        /// i
        /// ------------------------------------
        /// sqrt(-1)
        /// Real
        /// NaN
        /// </code>
        /// </example>
        public abstract Domain Codomain { get; protected init; }

        /// <summary>
        /// Returns this node with the specified codomain, 
        /// keeping all the subnodes in the same domain they were in
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath.Core;
        /// using static AngouriMath.MathS;
        /// 
        /// var expr = Sqrt(-1);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Codomain);
        /// Console.WriteLine(expr.Evaled);
        /// Console.WriteLine("------------------------------------");
        /// var newExpr = expr.WithCodomain(Domain.Real);
        /// Console.WriteLine(newExpr);
        /// Console.WriteLine(newExpr.Codomain);
        /// Console.WriteLine(newExpr.Evaled);
        /// </code>
        /// Prints
        /// <code>
        /// sqrt(-1)
        /// Complex
        /// i
        /// ------------------------------------
        /// sqrt(-1)
        /// Real
        /// NaN
        /// </code>
        /// </example>
        public Entity WithCodomain(Domain newDomain)
            => Codomain == newDomain ? this : this with { Codomain = newDomain };
    }
}
