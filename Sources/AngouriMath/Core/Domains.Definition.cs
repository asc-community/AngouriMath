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
        /// domain(sqrt(-1), RR)
        /// Real
        /// NaN
        /// </code>
        /// </example>
        public abstract Domain Codomain { get; protected init; }

        /// <summary>
        /// The codomain a node of this type carries when nothing has narrowed it: the one that
        /// parsing the node's own printed form, without a <c>domain(...)</c> around it, gives back.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is not <see cref="Domain.Complex"/> everywhere, which is why the printer cannot
        /// assume a single default: a <see cref="Variable"/> and a <see cref="Matrix"/> are
        /// <see cref="Domain.Any"/>, an <see cref="Absf"/> and an <see cref="Set.Interval"/> are
        /// <see cref="Domain.Real"/>, every boolean node is <see cref="Domain.Boolean"/>, and each
        /// numeric literal takes the domain of its own type.
        /// </para>
        /// <para>
        /// <see cref="Stringize()"/> compares against this to decide whether to print the
        /// annotation at all. A codomain equal to it is already what the bare text means, so
        /// wrapping it would put <c>domain(...)</c> around every expression in the library; one
        /// that differs is lost unless it is printed.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1022">#1022</a>
        /// </para>
        /// <para>
        /// Abstract rather than defaulted, so that a node declaring a <see cref="Codomain"/> and
        /// forgetting this one does not compile. The two are declared side by side in
        /// <c>Domains.Classes.cs</c> and must name the same domain;
        /// <c>CodomainSurvivesPrintingTest.AFreshNodeCarriesItsDefaultCodomainAndPrintsNoAnnotation</c>
        /// is the check that they do.
        /// </para>
        /// </remarks>
        internal abstract Domain DefaultCodomain { get; }

        /// <summary>
        /// Whether the printers have to spell this node's codomain out, because parsing what they
        /// would otherwise print gives a node with a different one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Domain.Any"/> is excluded because the grammar has no way to write it: the
        /// second argument of <c>domain(...)</c> must be a <see cref="Set.SpecialSet"/>, and there
        /// is no node for "no restriction" — see <see cref="Set.SpecialSet.Create(Domain)"/>. So a
        /// node widened to <see cref="Domain.Any"/> from a narrower default still prints as though
        /// it had not been. Printing <c>CC</c> there would be a different lie, and throwing from a
        /// printer would take out every diagnostic message that quotes an expression.
        /// </para>
        /// </remarks>
        internal bool PrintsItsCodomain => Codomain != DefaultCodomain && Codomain != Domain.Any;

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
        /// domain(sqrt(-1), RR)
        /// Real
        /// NaN
        /// </code>
        /// </example>
        public Entity WithCodomain(Domain newDomain)
            => Codomain == newDomain ? this : this with { Codomain = newDomain };
    }
}
