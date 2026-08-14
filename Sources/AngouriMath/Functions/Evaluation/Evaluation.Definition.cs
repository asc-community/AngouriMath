//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Transformations;
using HonkSharp.Laziness;

namespace AngouriMath
{
    internal static class EntityEvaluationExtension {
        /// <summary>Returns either <see cref="Entity.Evaled"/> or <see cref="Entity.InnerSimplified"/> based on <paramref name="isExact"/></summary>
        internal static Entity InnerSimplified(this Entity @this, bool isExact) => isExact ? @this.InnerSimplified : @this.Evaled;
    }

    partial record Entity
    {
        /// <summary>
        /// Returns the complete condition under which this expression is defined (has a valid value).
        /// This represents the mathematical "domain of definition" as a logical predicate.
        /// 
        /// For example:
        /// - For x/y: returns "y ≠ 0"
        /// - For sqrt(x) (over reals): returns "x ≥ 0"
        /// - For tan(x): returns "cos(x) ≠ 0" (or equivalently "x ≠ π/2 + πn")
        /// - For x + y: returns "true" (always defined)
        /// - For x/y + log(z): returns "y ≠ 0 and z > 0"
        /// 
        /// This combines the node's own definition condition (<see cref="IntrinsicCondition"/>) with all 
        /// its children's conditions using logical AND, propagating domain restrictions throughout the expression tree.
        /// 
        /// This is used by simplification patterns to preserve mathematical correctness by adding
        /// "provided" clauses when simplifications might hide singularities or undefined regions.
        /// For instance: (x-1)/(x-1) simplifies to "1 provided x ≠ 1", not just "1".
        /// </summary>
        /// <remarks>
        /// Mathematical concepts:
        /// - Domain of definition (the set where a function is defined)
        /// - Singularities and poles (points where a function is undefined)
        /// - Piecewise continuity (tracking where discontinuities occur)
        /// </remarks>
        public Entity DomainCondition => domainCondition.GetValue(static @this => @this.DirectChildren.Aggregate(@this.IntrinsicCondition, (accum, curr) =>
            (accum, curr.DomainCondition) switch {
                (Boolean(true), Boolean(true)) => Boolean.True,
                (var l, Boolean(true)) => l,
                (Boolean(true), var r) => r,
                (var l, var r) => l & r,
            }), this).InnerSimplified;
        private LazyPropertyA<Entity> domainCondition;
        
        /// <summary>
        /// Returns the intrinsic condition under which this specific operation is defined, 
        /// not including conditions from child expressions.
        /// 
        /// This represents the inherent domain restrictions of the operation itself.
        /// For example:
        /// - For division (x/y): returns "y ≠ 0" (the divisor must be non-zero)
        /// - For power (x^y): returns conditions for 0^0, 0^negative, etc.
        /// - For logarithm log(b, x): returns "b > 0 and b ≠ 1 and x > 0"
        /// - For addition (x + y): returns Boolean.True (no restrictions)
        /// - For tan(x): returns "cos(x) ≠ 0"
        /// 
        /// Child expression conditions are handled separately by the <see cref="DomainCondition"/> property.
        /// </summary>
        /// <remarks>
        /// This corresponds to the mathematical concept of a function's "natural domain" -
        /// the largest set of inputs for which the function's formula makes sense,
        /// independent of any restrictions on the input variables themselves.
        /// </remarks>
        private protected abstract Entity IntrinsicCondition { get; }

        /// <summary>
        /// This should NOT be called inside itself
        /// </summary>
        protected abstract Entity InnerSimplify(bool isExact);

        private Entity InnerSimplifyWithCheck(bool isExact)
        {
            var innerSimplified = InnerSimplify(isExact);
            if (innerSimplified.DirectChildren.Any(c => c == MathS.NaN))
                return MathS.NaN;
            if (DomainsFunctional.FitsDomainOrNonNumeric(innerSimplified, Codomain))
                return innerSimplified;
            else
                return MathS.NaN;
        }

        /// <summary>
        /// Represents the evaluated value of the given expression, allowing imprecise <see cref="Real"/> values unlike <see cref="InnerSimplified"/>.
        /// Unlike the result of <see cref="EvalNumerical"/> and
        /// <see cref="EvalBoolean"/>
        /// this is not constrained by any type.
        /// 
        /// It only performs an active operation in the first call,
        /// next time it is free to call it in terms of CPU usage. For
        /// consistency's sake, consider the call of this property
        /// as free as the addressing of a field.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var expr1 = x + y;
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Evaled);
        /// Console.WriteLine(expr1.Evaled.GetType());
        /// Console.WriteLine("-----------------------------");
        /// var expr2 = 5 + x * i;
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Evaled);
        /// Console.WriteLine(expr2.Substitute(x, 3).Evaled);
        /// Console.WriteLine(expr2.Substitute(x, 3).Evaled.GetType());
        /// Console.WriteLine("-----------------------------");
        /// var expr3 = GreaterThan(5, 3);
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(expr3.Evaled);
        /// Console.WriteLine(expr3.Evaled.GetType());
        /// </code>
        /// Prints
        /// <code>
        /// x + y
        /// x + y
        /// AngouriMath.Entity+Sumf
        /// -----------------------------
        /// 5 + x * i
        /// 5 + x * i
        /// 5 + 3i
        /// AngouriMath.Entity+Number+Complex
        /// -----------------------------
        /// 5 > 3
        /// True
        /// AngouriMath.Entity+Boolean
        /// </code>
        /// </example>
        public Entity Evaled => evaled.GetValue(static @this => @this.InnerSimplifyWithCheck(false), this);
        private LazyPropertyA<Entity> evaled;

        /// <summary>
        /// This is the result of naive simplifications, but not creating imprecise <see cref="Real"/> values unlike <see cref="Evaled"/>. In other 
        /// symbolic algebra systems it is called "Automatic simplification".
        /// It only performs an active operation in the first call,
        /// next time it is free to call it in terms of CPU usage. For
        /// consistency's sake, consider the call of this property
        /// as free as the addressing of a field.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var x = Var("x");
        /// var expr = Sqr(Sin(x + 0)) + Sqr(Cos(x / 1));
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.InnerSimplified);
        /// Console.WriteLine(expr.Simplify());
        /// </code>
        /// Prints
        /// <code>
        /// sin(x + 0) ^ 2 + cos(x / 1) ^ 2
        /// sin(x) ^ 2 + cos(x) ^ 2
        /// 1
        /// </code>
        /// </example>
        public Entity InnerSimplified => innerSimplified.GetValue(static @this => @this.InnerSimplifyWithCheck(true), this);
        private LazyPropertyA<Entity> innerSimplified;

        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument in case if some parentheses remain)
        /// </param>
        /// <returns>
        /// An expanded Entity if it wasn't too complicated,
        /// current entity otherwise
        /// To change the limit use <see cref="MathS.Settings.MaxExpansionTermCount"/>
        /// </returns>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// 
        /// var expr = (x + 3) * (Sin(y) + 5);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Expand());
        /// Console.WriteLine("-----------------------------------");
        /// var expr2 = Pow(x + y, 8);
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Expand());
        /// </code>
        /// Prints
        /// <code>
        /// (x + 3) * (sin(y) + 5)
        /// x * sin(y) + x * 5 + 3 * sin(y) + 15
        /// -----------------------------------
        /// (x + y) ^ 8
        /// y ^ 8 + 8 * x * y ^ 7 + 28 * x ^ 2 * y ^ 6 + 56 * x ^ 3 * y ^ 5 + 70 * x ^ 4 * y ^ 4 + 56 * x ^ 5 * y ^ 3 + 28 * x ^ 6 * y ^ 2 + 8 * x ^ 7 * y + x ^ 8
        /// </code>
        /// </example> 

        public Entity Expand(int level = 2)
            => Transformation.ExpansionAtLevel(level).ApplyOrKeep(this);

        /// <summary>
        /// What <see cref="Expand(int)"/> does, reachable by
        /// <see cref="Transformation.ExpansionAtLevel(int)"/> without going back through
        /// the public method and round again.
        /// </summary>
        internal Entity ExpandOverSum(int level)
        {
            // A matrix is expanded entry by entry. What follows reads the expression as a sum, and
            // a matrix is not one, so it left through the escape at the bottom and came back as it
            // arrived: [[(x+1)^2, 1]] was not expanded while (x+1)^2 was. Factorize and
            // Differentiate both descend into a matrix, being built out of rewrite rules, and a
            // rule walks the tree -- so this was Expand being the odd one out rather than matrices
            // being held back on purpose.
            // https://github.com/asc-community/AngouriMath/issues/882
            if (this is Matrix matrix)
                return matrix.With((_, _, entry) => entry.Expand(level));

            static Entity Expand_(Entity e, int level) =>
                level <= 1
                ? e.Rewrite(RewriteRules.Expansion)
                : Expand_(e.Rewrite(RewriteRules.Expansion), level - 1);
            var expChildren = new List<Entity>();
            foreach (var linChild in Sumf.LinearChildren(this))
                if (TreeAnalyzer.SmartExpandOver(linChild, entity => true) is { } exp)
                    expChildren.AddRange(exp);
                else
                    return this; // if one is too complicated, return the current one
            return CollectLikeTerms(
                Expand_(TreeAnalyzer.MultiHangBinary(expChildren, (a, b) => new Sumf(a, b)), level).InnerSimplified);
        }

        /// <summary>
        /// Adds up the terms of an expanded sum that differ only by a numeric factor, so
        /// that expanding actually finishes: <c>(x+1)^2 * (x+1)^2</c> multiplied out gives
        /// sixteen terms, of which only five are distinct.
        /// </summary>
        /// <remarks>
        /// Deliberately not <see cref="Simplify(int)"/>, which is far too expensive to run
        /// inside <see cref="Expand(int)"/>. Each term is reduced to a coefficient and a
        /// product of powers, and terms whose products agree are added together --
        /// enough to collect like terms and nothing more.
        /// </remarks>
        /// <summary>
        /// Whether every node of the expression is one that monomial collection describes:
        /// numbers, variables, and the operations that build a polynomial out of them.
        /// </summary>
        /// <remarks>
        /// Opening a product to reach its numeric factor is what lets like terms meet, and
        /// it is also what stops a product cancelling as a whole -- lifting the 1/2 out of
        /// <c>(1/2 * sin(2t))^2 * csc(t)^2</c> costs the cancellation to <c>cos(t)^2</c>.
        /// Collection is a statement about polynomials, so it opens polynomials and leaves
        /// anything carrying a function intact for the rules that do know it.
        /// https://github.com/asc-community/AngouriMath/issues/855
        /// </remarks>
        private static bool IsPolynomialShaped(Entity expression)
            => expression.Nodes.All(node =>
                node is Number or Variable or Mulf or Powf or Sumf or Minusf or Divf);

        private static Entity CollectLikeTerms(Entity expanded)
        {
            if (expanded is not Sumf and not Minusf)
                return expanded;

            // A term carrying a domain condition must not be folded into another: the
            // conditions are what say where the answer holds, and adding coefficients
            // together loses them. (4a - 2)/(2x) + (1 - 2a)/x is 0 only where x is not 0,
            // and collecting it to a plain 0 would drop exactly that.
            if (expanded.Nodes.Any(node => node is Providedf))
                return expanded;

            var coefficients = new Dictionary<string, Entity>();
            var monomials = new Dictionary<string, Entity>();
            var order = new List<string>();

            foreach (var term in Sumf.LinearChildren(expanded))
            {
                Entity coefficient = 1;
                var exponents = new Dictionary<string, Entity>();
                var bases = new Dictionary<string, Entity>();

                var pending = new Stack<Entity>(Mulf.LinearChildren(term));
                while (pending.Count > 0)
                {
                    // PowerRules folds (x^2)^2 into x^4, without which the two would be
                    // counted as different monomials.
                    var reduced = pending.Pop().Rewrite(RewriteRules.Power).InnerSimplified;
                    if (reduced is Number)
                    {
                        coefficient = (coefficient * reduced).InnerSimplified;
                        continue;
                    }
                    // Reducing a factor can turn it into a product -- (2 * x)^2 becomes
                    // 4 * x^2 -- and taken whole that is a monomial of its own, keyed on
                    // "4 * x ^ 2" and unable to meet the plain x^2 terms it belongs with.
                    // Split it again so the 4 reaches the coefficient. Each child is
                    // smaller than the product it came from, so this terminates.
                    // https://github.com/asc-community/AngouriMath/issues/855
                    if (reduced is Mulf && IsPolynomialShaped(reduced))
                    {
                        foreach (var inner in Mulf.LinearChildren(reduced))
                            pending.Push(inner);
                        continue;
                    }
                    // The same one shape out: (2 * x * y)^2 is a power of a product that
                    // the rules above do not distribute. Only for an integer exponent,
                    // where (a * b)^n = a^n * b^n holds for every a and b; for any other
                    // exponent it is a statement about branches.
                    if (reduced is Powf(Mulf product, Integer wholePower) && IsPolynomialShaped(product))
                    {
                        foreach (var inner in Mulf.LinearChildren(product))
                            pending.Push(inner.Pow(wholePower));
                        continue;
                    }
                    var (@base, exponent) = reduced is Powf(var b, var e) ? (b, e) : (reduced, (Entity)1);
                    var key = @base.Stringize();
                    bases[key] = @base;
                    exponents[key] = exponents.TryGetValue(key, out var already)
                        ? (already + exponent).InnerSimplified
                        : exponent;
                }

                var monomialKey = string.Join(" ", (IEnumerable<string>)exponents.Keys.OrderBy(k => k, System.StringComparer.Ordinal)
                    .Select(k => k + "^" + exponents[k].Stringize()));

                if (coefficients.TryGetValue(monomialKey, out var running))
                    coefficients[monomialKey] = (running + coefficient).InnerSimplified;
                else
                {
                    order.Add(monomialKey);
                    coefficients[monomialKey] = coefficient;
                    Entity monomial = 1;
                    foreach (var key in exponents.Keys.OrderBy(k => k, System.StringComparer.Ordinal))
                    {
                        // A factor that appeared once goes back as itself, not as base^1.
                        // Raising to the first power is an identity only where the power is
                        // defined at all: RR^1, ZZ^1 and true^1 are all NaN, so writing the
                        // factor that way turns an expression that had a value into one that
                        // does not. https://github.com/asc-community/AngouriMath/issues/851
                        var factor = exponents[key] == Integer.Create(1)
                            ? bases[key]
                            : bases[key].Pow(exponents[key]);
                        monomial = (monomial * factor).InnerSimplified;
                    }
                    monomials[monomialKey] = monomial;
                }
            }

            Entity result = 0;
            foreach (var key in order)
            {
                // A term whose coefficients cancelled is still written out rather than
                // dropped. Where the monomial is defined everywhere it simplifies to 0 and
                // costs nothing; where it is not -- x^-1, say -- InnerSimplified turns
                // 0 * x^-1 into `0 provided not x = 0`, and dropping the term would have
                // thrown that condition away. (4a - 2)/(2x) + (1 - 2a)/x is the case.
                var term = (coefficients[key] * monomials[key]).InnerSimplified;
                result = result == Integer.Create(0) ? term : result + term;
            }
            var collected = result.InnerSimplified;

            // Collecting is an improvement, never a requirement, so a collection that
            // introduced NaN is discarded rather than returned. The decomposition into
            // coefficient and monomial only describes a node that multiplies and takes
            // powers; a factor of any other kind -- a set raised above the first power,
            // say -- reassembles into something undefined. Keeping the expanded form is
            // always sound. https://github.com/asc-community/AngouriMath/issues/851
            if (collected.Nodes.Any(node => node == MathS.NaN)
                && !expanded.Nodes.Any(node => node == MathS.NaN))
                return expanded;

            return collected;
        }

        /// <summary>
        /// Factorizes an equation trying to eliminate as many power-uses as possible ( e.g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument if some factor operations are still available)
        /// </param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// 
        /// var expr1 = x * y + y + x + 1;
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Factorize());
        /// Console.WriteLine("-----------------------------------");
        /// var expr2 = x * y + y + (1 + x);
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Factorize());
        /// </code>
        /// Prints
        /// <code>
        /// x * y + y + x + 1
        /// y * (1 + x) + x + 1
        /// -----------------------------------
        /// x * y + y + 1 + x
        /// (1 + x) * (1 + y)
        /// </code>
        /// </example>
        /// <remarks>
        /// One pass is the perfect-square rules, then the factorisation rules, then
        /// <see cref="InnerSimplified"/> -- so that the factors come back finished, since
        /// the rules leave <c>x ^ 1</c> where they mean <c>x</c> and <c>sqrt(4)</c> where
        /// they mean <c>2</c> -- and <paramref name="level"/> is how many times that runs.
        /// Built out of <see cref="RewriteRules"/> by
        /// <see cref="Transformation.FactorizationAtLevel(int)"/>.
        /// </remarks>
        public Entity Factorize(int level = 2)
            => Transformation.FactorizationAtLevel(level).ApplyOrKeep(this);

        /// <summary>
        /// Simplifies an equation ( e.g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x )
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, a) = Var("x", "y", "a");
        /// var expr = Sin(x) + y + a;
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr1 = Sin(x - 3) / Tan(x - 3) + Sec(Sqrt(y)) * Cosec(Sqrt(y));
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr2 = Sin(pi / 3) * 2;
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr3 = (Pow(x, 3) + 3 * Sqr(x) * y + 3 * x * Sqr(y) + Pow(y, 3)) / (x + y);
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(expr3.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr4 = Derivative(Sin(Sqr(x * y) + y * x), x);
        /// Console.WriteLine(expr4);
        /// Console.WriteLine(expr4.Simplify());
        /// </code>
        /// Prints
        /// <code>
        /// sin(x) + y + a
        /// sin(x) + a + y
        /// ---------------------
        /// sin(x - 3) / tan(x - 3) + sec(sqrt(y)) * csc(sqrt(y))
        /// 2 * csc(2 * sqrt(y)) + cos(x - 3)
        /// ---------------------
        /// sin(pi / 3) * 2
        /// sqrt(3)
        /// ---------------------
        /// (x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)
        /// x ^ 2 + 2 * x * y + y ^ 2
        /// ---------------------
        /// derivative(sin((x * y) ^ 2 + y * x), x)
        /// cos((x * y) ^ 2 + x * y) * (2 * x * y ^ 2 + y)
        /// </code>
        /// </example>
        public Entity Simplify(int level = 2)
            => Transformation.SimplificationAtLevel(level).ApplyOrKeep(this);

        /// <summary>
        /// A canonical form for the commutative structure: two expressions differing only in
        /// how their sums, products, conjunctions, disjunctions and set operations are
        /// arranged or nested come out as the <b>identical tree</b>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is not <see cref="Simplify(int)"/> and is not trying to be. It makes an
        /// expression <i>comparable</i>, not shorter, and it may well make it longer. What it
        /// buys is that <c>a.Canonicalise() == b.Canonicalise()</c> is a real test of whether
        /// the two are the same expression, where comparing simplified forms is not.
        /// </para>
        /// <para>
        /// <b>Equal trees mean the expressions are equal; different trees mean nothing at
        /// all.</b> There is no canonical form for the whole language — deciding whether an
        /// expression is zero is undecidable once <c>pi</c>, the exponential, the trigonometric
        /// functions and <c>abs</c> are in play — so this canonicalises the part that can be:
        /// the arrangement of commutative operators. For rational functions over <c>Q</c>, where
        /// a complete canonical form does exist, use
        /// <see cref="CanonicaliseAsRationalFunction"/>.
        /// <c>Docs/Contributing/CanonicalForm.md</c> states the boundary and how far off the
        /// library is from it.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using AngouriMath;
        /// using static System.Console;
        ///
        /// WriteLine("x + y".ToEntity().Canonicalise() == "y + x".ToEntity().Canonicalise());
        /// WriteLine("(x + y) + a".ToEntity().Canonicalise() == "x + (y + a)".ToEntity().Canonicalise());
        /// </code>
        /// Prints
        /// <code>
        /// True
        /// True
        /// </code>
        /// </example>
        public Entity Canonicalise()
            => Transformation.Canonicalisation.ApplyOrKeep(this);

        /// <summary>
        /// A canonical form for rational functions over <c>Q</c>, or <see langword="null"/>
        /// where this is not one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Two rational functions are equal <b>exactly when</b> this form is identical, so on
        /// that sublanguage equality is decided by comparing nodes rather than by searching —
        /// which is what <c>(a - b).Simplify()</c> against zero can never be.
        /// </para>
        /// <para>
        /// <b>It answers only where it can, and says so by answering nothing.</b> Anything that
        /// is not a rational function over <c>Q</c> in its free variables gets
        /// <see langword="null"/>, because a form whose whole value is that equal trees mean
        /// equal expressions must not hand back a normalisation that merely resembles one.
        /// </para>
        /// <para>
        /// Cancelling a common factor widens the domain, so where one comes out the answer
        /// carries the condition that it is nonzero: <c>x/x</c> is <c>1 provided not x = 0</c>
        /// and not <c>1</c>, and <c>(x^2 - 1)/(x + 1)</c> is not the same function as
        /// <c>x - 1</c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using AngouriMath;
        /// using static System.Console;
        ///
        /// WriteLine("1/x + 1/y".ToEntity().CanonicaliseAsRationalFunction());
        /// WriteLine("(x + y) / (x * y)".ToEntity().CanonicaliseAsRationalFunction());
        /// WriteLine("sin(x) / x".ToEntity().CanonicaliseAsRationalFunction() is null);
        /// </code>
        /// Prints
        /// <code>
        /// (x + y) / (x * y)
        /// (x + y) / (x * y)
        /// True
        /// </code>
        /// </example>
        public Entity? CanonicaliseAsRationalFunction()
            => Transformation.RationalCanonicalisation.Apply(this).Output;

        /// <summary>Finds all alternative forms of an expression sorted by their complexity</summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, a) = Var("x", "y", "a");
        /// var expr = x * y + y + x + x / y + a * (x + y);
        /// foreach (var alt in expr.Alternate(level: 3))
        ///     Console.WriteLine(alt);
        /// </code>
        /// <code>
        /// a * y + (1 + a + 1 / y + y) * x + y
        /// a * (x + y) + x + x * (y + 1 / y) + y
        /// a * (x + y) + x + x * (1 / y + y) + y
        /// x * y + y + x + x / y + a * (x + y)
        /// a * (x + y) + x + x * y + x / y + y
        /// (x + y) * a + x + x * y + x / y + y
        /// a * (x + y) + x + x * 1 / y + x * y + y
        /// </code>
        /// </example>
        public IEnumerable<Entity> Alternate(int level) => Simplificator.Alternate(this, level);

        /// <summary>
        /// Determines whether a given element can be unambiguously used as a number or boolean
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var expr1 = x + y;
        /// Console.WriteLine(expr1.IsConstant);
        /// Console.WriteLine(expr1.Evaled.IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr2 = 5 + x * i;
        /// Console.WriteLine(expr2.IsConstant);
        /// Console.WriteLine(expr2.Substitute(x, 3).IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr3 = GreaterThan(5, 3);
        /// Console.WriteLine(expr3.IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr4 = pi + 0 * e;
        /// Console.WriteLine(expr4.IsConstant);
        /// </code>
        /// Prints
        /// <code>
        /// False
        /// False
        /// -----------------------------
        /// False
        /// True
        /// -----------------------------
        /// True
        /// -----------------------------
        /// True
        /// </code>
        /// </example>
        public bool IsConstant => Evaled is Number.Complex or Boolean || Evaled is Variable v && Variable.ConstantList.ContainsKey(v.Name);
    }
}
