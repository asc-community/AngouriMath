//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Multithreading;
using PeterO.Numbers;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static partial class TreeAnalyzer
    {
        internal interface IPrimitive<T>
        {
            void Add(Real a);
            void AddMp(T a, Real b);
            void Clear();
            T Value { get; }
            bool AllowFloat { get; }
        }
        internal sealed class PrimitiveDecimal : IPrimitive<EDecimal>
        {
            public void Add(Real a) => Value += a.EDecimal;
            public void AddMp(EDecimal a, Real b) => Value += a * b.EDecimal;
            public void Clear() => Value = 0;
            public EDecimal Value { get; private set; } = 0;
            public bool AllowFloat => true;
        }
        internal sealed class PrimitiveInteger : IPrimitive<EInteger>
        {
            public void Add(Real a) => Value += a.EDecimal.ToEInteger();
            public void AddMp(EInteger a, Real b) => Value += (a * b.EDecimal).ToEInteger();
            public void Clear() => Value = 0;
            public EInteger Value { get; private set; } = 0;
            public bool AllowFloat => false;
        }
        /// <summary>
        /// If an evaluable expression is equal to zero, <see langword="true"/>, otherwise, <see langword="false"/>
        /// For example, 1 - 1 is zero, but 1 + a is not
        /// </summary>
        internal static bool IsZero(Entity e) => e.Evaled is Complex c && c == 0;
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    /// <summary>Solves all forms of Polynomials that are trivially solved</summary>
    internal static class PolynomialSolver
    {
        /// <summary>Solves ax + b</summary>
        /// <param name="a">Coefficient of x</param>
        /// <param name="b">Free coefficient</param>
        /// <returns>Set of roots</returns>
        // ax + b = 0
        // ax = -b
        // x = -b / a
        internal static IEnumerable<Entity> SolveLinear(Entity a, Entity b) => new[] { (-b / a).InnerSimplified };

        /// <summary>Solves ax^2 + bx + c</summary>
        /// <param name="a">Coefficient of x^2</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Free coefficient</param>
        /// <returns>Set of roots</returns>
        internal static IEnumerable<Entity> SolveQuadratic(Entity a, Entity b, Entity c)
        {
            if (TreeAnalyzer.IsZero(c)) return SolveLinear(a, b).Append(0);
            if (TreeAnalyzer.IsZero(a)) return SolveLinear(b, c);
            var D = MathS.Sqr(b) - 4 * a * c;
            return new[] { ((-b - MathS.Sqrt(D)) / (2 * a)).InnerSimplified,
                           ((-b + MathS.Sqrt(D)) / (2 * a)).InnerSimplified };
        }

        /// <summary>Solves ax^3 + bx^2 + cx + d</summary>
        /// <param name="a">Coefficient of x^3</param>
        /// <param name="b">Coefficient of x^2</param>
        /// <param name="c">Coefficient of x</param>
        /// <param name="d">Free coefficient</param>
        /// <returns>Set of roots</returns>
        internal static IEnumerable<Entity> SolveCubic(Entity a, Entity b, Entity c, Entity d)
        {
            // en: https://en.wikipedia.org/wiki/Cubic_equation
            // ru: https://ru.wikipedia.org/wiki/%D0%A4%D0%BE%D1%80%D0%BC%D1%83%D0%BB%D0%B0_%D0%9A%D0%B0%D1%80%D0%B4%D0%B0%D0%BD%D0%BE

            if (TreeAnalyzer.IsZero(d)) return SolveQuadratic(a, b, c).Append(0);
            if (TreeAnalyzer.IsZero(a)) return SolveQuadratic(b, c, d);

            var coeff = MathS.i * MathS.Sqrt(3) / 2;
            var u1 = Integer.Create(1);
            var u2 = Rational.Create(-1, 2) + coeff;
            var u3 = Rational.Create(-1, 2) - coeff;
            var D0 = MathS.Sqr(b) - 3 * a * c;
            var D1 = (2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d).InnerSimplified;
            var C = MathS.Pow((D1 + MathS.Sqrt(MathS.Sqr(D1) - 4 * MathS.Pow(D0, 3))) / 2, Rational.Create(1, 3));

            return new[] { u1, u2, u3 }.Select(uk =>
                C.Evaled == 0 && D0.Evaled == 0 ? -(b + uk * C) / 3 / a : -(b + uk * C + D0 / C / uk) / 3 / a);
        }

        [ConstantField] private static readonly int[] sqrtsOf1 = { -1, 1 };
        /// <summary>Solves ax^4 + bx^3 + cx^2 + dx + e</summary>
        /// <param name="a">Coefficient of x^4</param>
        /// <param name="b">Coefficient of x^3</param>
        /// <param name="c">Coefficient of x^2</param>
        /// <param name="d">Coefficient of x</param>
        /// <param name="e">Free coefficient</param>
        /// <returns>Set of roots</returns>
        internal static IEnumerable<Entity> SolveQuartic(Entity a, Entity b, Entity c, Entity d, Entity e)
        {
            // en: https://en.wikipedia.org/wiki/Quartic_function
            // ru: https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%A4%D0%B5%D1%80%D1%80%D0%B0%D1%80%D0%B8

            if (TreeAnalyzer.IsZero(e)) return SolveCubic(a, b, c, d).Append(0);
            if (TreeAnalyzer.IsZero(a)) return SolveCubic(b, c, d, e);

            var alpha = (-3 * MathS.Sqr(b) / (8 * MathS.Sqr(a)) + c / a)
                .InnerSimplified;
            var beta = (MathS.Pow(b, 3) / (8 * MathS.Pow(a, 3)) - (b * c) / (2 * MathS.Sqr(a)) + d / a)
                .InnerSimplified;
            var gamma = (-3 * MathS.Pow(b, 4) / (256 * MathS.Pow(a, 4)) + MathS.Sqr(b) * c / (16 * MathS.Pow(a, 3)) - (b * d) / (4 * MathS.Sqr(a)) + e / a)
                .InnerSimplified;

            if (beta.Evaled == 0)
                return sqrtsOf1.SelectMany(_ => sqrtsOf1,
                    (s, t) => -b / 4 * a + s * MathS.Sqrt((-alpha + t * MathS.Sqrt(MathS.Sqr(alpha) - 4 * gamma)) / 2));

            var oneThird = Rational.Create(1, 3);
            var P = (-MathS.Sqr(alpha) / 12 - gamma)
                .InnerSimplified;
            var Q = (-MathS.Pow(alpha, 3) / 108 + alpha * gamma / 3 - MathS.Sqr(beta) / 8)
                .InnerSimplified;
            var R = -Q / 2 + MathS.Sqrt(MathS.Sqr(Q) / 4 + MathS.Pow(P, 3) / 27);
            var U = MathS.Pow(R, oneThird)
                .InnerSimplified;
            var y = (Rational.Create(-5, 6) * alpha + U + (U.Evaled == 0 ? -MathS.Pow(Q, oneThird) : -P / (3 * U)))
                .InnerSimplified;
            var W = MathS.Sqrt(alpha + 2 * y)
                .InnerSimplified;

            // Now we need to permutate all four combinations
            return sqrtsOf1.SelectMany(_ => sqrtsOf1,
                (s, t) => -b / (4 * a) + (s * W + t * MathS.Sqrt(-(3 * alpha + 2 * y + s * 2 * beta / W))) / 2);
        }

        /// <summary>
        /// So that the final list of powers contains power = 0 and all powers >= 0
        /// (e. g. if the dictionaty's keys are 3, 4, 6, the final answer will contain keys
        /// 0, 1, 3, if the dictionary's keys are -2, 0, 3, the final answer will contain keys
        /// 0, 2, 5)
        /// </summary>
        /// <param name="monomials">
        /// Dictionary to process. Key - power, value - coefficient of the corresponding term
        /// </param>
        /// <returns>
        /// Whether all initial powers where > 0 (if so, x = 0 is a root)
        /// </returns>
        internal static bool ReduceCommonPower(ref Dictionary<EInteger, Entity> monomials)
        {
            var commonPower = monomials.Keys.Min() ?? throw new AngouriBugException("No null expected");
            if (commonPower.IsZero)
                return false;
            var newDict = new Dictionary<EInteger, Entity>();
            foreach (var pair in monomials)
                newDict[pair.Key - commonPower] = pair.Value;
            monomials = newDict;
            return commonPower > 0;
        }

        /// <summary>Tries to solve as polynomial</summary>
        /// <param name="expr">Polynomial of an expression</param>
        /// <param name="subtree">
        /// The expression the polynomial of (e. g. cos(x)^2 + cos(x) + 1 is a polynomial of cos(x))
        /// </param>
        /// <returns>A finite <see cref="Set"/> if successful, <see langword="null"/> otherwise</returns>
        internal static IEnumerable<Entity>? SolveAsPolynomial(Entity expr, Variable subtree)
        {
            // Safely expand the expression
            // Here we find all terms
            var children = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, entity => entity.ContainsNode(subtree));
            if (children is null)
                return null;
            // // //

            // Check if all are like {1} * x^n & gather information about them
            var monomialsByPower = GatherMonomialInformation
                <EInteger, TreeAnalyzer.PrimitiveInteger>(children, subtree);
            if (monomialsByPower == null)
                return null; // meaning that the given equation is not polynomial
            
            var res = ReduceCommonPower(ref monomialsByPower)
                ? new Entity[] { 0 } // x5 + x3 + x2 - common power is 2, one root is 0, then x3 + x + 1
                : Enumerable.Empty<Entity>();
            var powers = monomialsByPower.Keys.ToList();
            var gcdPower = powers.Aggregate((accumulate, current) => accumulate.Gcd(current));
            // // //

            if (gcdPower.IsZero)
                gcdPower = EInteger.One;
            // Change all replacements, x6 + x3 + 1 => x2 + x + 1
            if (!gcdPower.Equals(EInteger.One))
            {
                for (int i = 0; i < powers.Count; i++)
                    powers[i] /= gcdPower;
                monomialsByPower = monomialsByPower.ToDictionary(pair => pair.Key / gcdPower, pair => pair.Value);
            }
            // // //

            MultithreadingFunctional.ExitIfCancelled();

            var gcdPowerRoots = GetAllRootsOf1(gcdPower);
            Entity GetMonomialByPower(EInteger power) =>
                monomialsByPower.TryGetValue(power, out var monomial) ? monomial.InnerSimplified : 0;
            powers.Sort();
            if (!powers.Last().CanFitInInt32())
                return null;
            return (powers.Count, powers.Last().ToInt32Unchecked()) switch
            {
                (0, _) => null,
                (_, 0) => Enumerable.Empty<Entity>(),
                (> 2, > 4) => null, // So far, we can't solve equations of powers more than 4
                // By this moment we know for sure that expr's power is <= 4, that expr is not a monomial,
                // and that it consists of more than 2 monomials
                (1, _) => new Entity[] { 0 },
                (2, _) =>
                    // Solve a x ^ n + b = 0 for x ^ n -> x ^ n = -b / a
                    MathS.Pow(subtree, Integer.Create(powers[1]))
                    .Invert((-1 * monomialsByPower[powers[0]] / monomialsByPower[powers[1]]).InnerSimplified, subtree),
                (_, 2) => SolveQuadratic(GetMonomialByPower(2), GetMonomialByPower(1), GetMonomialByPower(0)),
                (_, 3) => SolveCubic(GetMonomialByPower(3), GetMonomialByPower(2), GetMonomialByPower(1), GetMonomialByPower(0)),
                (_, 4) => SolveQuartic(GetMonomialByPower(4), GetMonomialByPower(3),
                    GetMonomialByPower(2), GetMonomialByPower(1), GetMonomialByPower(0)),
                _ => null
            } is { } resConcat
            ? !gcdPower.Equals(EInteger.One)
              // if we had x^6 + x^3 + 1, we replace it with x^2 + x + 1 and find all cubic roots of the final answer
              ? res.Concat(resConcat).SelectMany(_ => gcdPowerRoots,
                  (root, coef) => coef * MathS.Pow(root, Rational.Create(1, gcdPower)))
              : res.Concat(resConcat)
            : null;
        }

        /// <summary>Finds all terms of a polynomial</summary>
        /// <returns><see langword="null"/> if polynomial is bad</returns>
        internal static Dictionary<T, Entity>? GatherMonomialInformation<T, TPrimitive>(IEnumerable<Entity> terms, Variable subtree)
            where TPrimitive : TreeAnalyzer.IPrimitive<T>, new()
            where T : notnull
        {
            var monomialsByPower = new Dictionary<T, Entity>();
            // here we fill the dictionary with information about monomials' coefficiants
            foreach (var child in terms)
                if (ParseMonomial<T, TPrimitive>(subtree, child) is var (free, q))
                    monomialsByPower[q.Value] =
                        monomialsByPower.TryGetValue(q.Value, out var power) ? power + free : free;
                else return null;
            return monomialsByPower;
        }
        /// <summary>Finds all terms of a polynomial</summary>
        internal static (Dictionary<T, Entity> poly, Entity rem) GatherMonomialInformationAllowingBad<T, TPrimitive>(IEnumerable<Entity> terms, Variable subtree)
            where TPrimitive : TreeAnalyzer.IPrimitive<T>, new()
            where T : notnull
        {
            var monomialsByPower = new Dictionary<T, Entity>();
            Entity rem = 0;
            // here we fill the dictionary with information about monomials' coefficiants
            foreach (var child in terms)
                if (ParseMonomial<T, TPrimitive>(subtree, child) is var (free, q))
                    monomialsByPower[q.Value] =
                        monomialsByPower.TryGetValue(q.Value, out var power) ? power + free : free;
                else
                    rem += child;
            return (monomialsByPower, rem);
        }


        internal static (Entity FreeMono, TPrimitive Power)? ParseMonomial<T, TPrimitive>(Variable aVar, Entity expr)
            where TPrimitive : TreeAnalyzer.IPrimitive<T>, new()
        {
            var power = new TPrimitive();
            if (!expr.ContainsNode(aVar))
                return (expr, power);

            Entity freeMono = 1; // a * b
            foreach (var mp in Mulf.LinearChildren(expr))
                if (!mp.ContainsNode(aVar))
                    freeMono *= mp;
                else if (mp is Powf(var @base, var pow_num))
                {
                    // x ^ a or x ^ i is bad
                    if (pow_num.Evaled is not Real value)
                        return null;
                    // x ^ 0.3 is bad
                    if (!power.AllowFloat && value is not Integer)
                        return null;
                    if (mp == aVar)
                        power.Add(value);
                    else if (ParseMonomial<T, TPrimitive>(aVar, @base) is var (tmpFree, q))
                    {
                        freeMono *= MathS.Pow(tmpFree, value);
                        power.AddMp(q.Value, value);
                    }
                    else return null;
                }
                else if (mp == aVar)
                    power.Add(1);
                // a ^ x, (a + x) etc. are bad
                else if (mp.ContainsNode(aVar))
                    return null;
                else freeMono *= mp;
            // TODO: do we need to simplify freeMono?
            return (freeMono, power);
        }
    }
}