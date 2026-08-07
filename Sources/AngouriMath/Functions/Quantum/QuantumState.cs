//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions.Algebra.MonoidAlgebra;

namespace AngouriMath.Functions.Quantum
{
    using static Entity;

    /// <summary>
    /// Reading a quantum state out of an ordinary expression, and writing one back.
    /// </summary>
    /// <remarks>
    /// **A state is not a new kind of node.** A ket is written <c>apply(ket, 0, 1)</c> -- an
    /// application of an undeclared name, which the library already understands -- so a state
    /// is an ordinary sum of products and everything that reads expressions reads it. That is
    /// not only tidy: it is why <c>a|x&gt; + b|x&gt; = (a+b)|x&gt;</c> and <c>0|x&gt; = 0</c>
    /// need no code here at all. <c>Simplify</c> collects like terms over an opaque
    /// application already, and it factors the amplitude out of a Bell state without being
    /// told what one is.
    /// <para/>
    /// The cost of the choice is that <c>ket</c> is a name rather than a type: nothing stops
    /// <c>apply(ket, 5, 7)</c>, and a caller with their own variable called <c>ket</c> will
    /// collide with it. A dedicated node would close both and would forfeit everything in the
    /// paragraph above, so the loose reading is taken deliberately and is reversible.
    /// </remarks>
    internal static class QuantumState
    {
        /// <summary>The name an application must have to be read as a ket.</summary>
        internal const string KetHead = "ket";

        /// <summary>
        /// The state as a map from basis ket to amplitude, or <see langword="null"/> where the
        /// expression is not one -- which includes an expression with no ket in it at all, and
        /// one whose kets are of different widths.
        /// </summary>
        internal static SparseTerms<Ket>? TryRead(Entity expr)
        {
            var terms = new List<KeyValuePair<Ket, Entity>>();
            if (!TryReadSum(expr, terms) || terms.Count == 0)
                return null;
            var width = terms[0].Key.Width;
            if (terms.Any(term => term.Key.Width != width))
                return null;
            return SparseTerms<Ket>.From(terms, Semiring.Field);
        }

        private static bool TryReadSum(Entity expr, List<KeyValuePair<Ket, Entity>> into)
            => TryReadTerm(expr, 1, into);

        /// <summary>
        /// One term: exactly one ket among its multiplicative factors, and everything else is
        /// the amplitude. Two kets in a product would be a tensor product of states rather
        /// than a term of one, and is not read here.
        /// </summary>
        /// <remarks>
        /// Sums are handled here rather than above it because an amplitude may be carried in
        /// from outside: <c>(|00&gt; + |01&gt;) / sqrt(2)</c> reaches this as a quotient whose
        /// dividend is a sum, and reading the sum only at the top would refuse it -- which is
        /// exactly how every normalised state in the tests failed to parse at first.
        /// </remarks>
        private static bool TryReadTerm(Entity expr, Entity amplitude, List<KeyValuePair<Ket, Entity>> into)
        {
            switch (expr)
            {
                case Sumf(var augend, var addend):
                    return TryReadTerm(augend, amplitude, into)
                        && TryReadTerm(addend, amplitude, into);
                case Minusf(var minuend, var subtrahend):
                    return TryReadTerm(minuend, amplitude, into)
                        && TryReadTerm(subtrahend, -amplitude, into);
                case Mulf(var multiplier, var multiplicand):
                    // Whichever side is free of kets is amplitude, and the other side is the
                    // state -- chosen by where the ket is rather than by which side it is on,
                    // so that a|x> and (a)(|x>) read alike. Kets on both sides means a tensor
                    // product of two states, which is not a term of one and is not read here.
                    if (NoKetIn(multiplicand))
                        return TryReadTerm(multiplier, amplitude * multiplicand, into);
                    if (NoKetIn(multiplier))
                        return TryReadTerm(multiplicand, amplitude * multiplier, into);
                    return false;
                case Divf(var dividend, var divisor):
                    return NoKetIn(divisor) && TryReadTerm(dividend, amplitude / divisor, into);
                default:
                    return TryReadKet(expr) is { } alone && Record(alone, amplitude, into);
            }
        }

        private static bool Record(Ket ket, Entity amplitude, List<KeyValuePair<Ket, Entity>> into)
        {
            into.Add(new KeyValuePair<Ket, Entity>(ket, amplitude));
            return true;
        }

        private static bool NoKetIn(Entity expr) => !expr.Nodes.Any(IsKet);

        private static bool IsKet(Entity node)
            => node is Application(Variable { Name: KetHead }, _);

        /// <summary>
        /// The ket an expression denotes, or <see langword="null"/>. Every argument has to be
        /// 0 or 1: <c>apply(ket, 5)</c> is an application of something called <c>ket</c> and
        /// is not a basis state.
        /// </summary>
        internal static Ket? TryReadKet(Entity expr)
        {
            if (expr is not Application(Variable { Name: KetHead }, var arguments))
                return null;
            var cells = new List<int>();
            foreach (var argument in arguments)
            {
                if (argument.Evaled is not Number.Integer value || (value != 0 && value != 1))
                    return null;
                cells.Add(value == 1 ? 1 : 0);
            }
            return cells.Count == 0 ? null : new Ket(cells.ToArray());
        }

        /// <summary>A ket written back as the expression it came from.</summary>
        internal static Entity ToEntity(Ket ket)
            => Variable.CreateVariableUnchecked(KetHead)
                .Apply(ket.Cells.Where(cell => cell != Ket.Free).Select(cell => (Entity)cell).ToArray());

        /// <summary>The state written back as an ordinary sum of products.</summary>
        internal static Entity ToEntity(SparseTerms<Ket> state)
            => state.IsEmpty
                ? 0
                : state.Terms
                    .Select(term => term.Value == 1 ? ToEntity(term.Key) : term.Value * ToEntity(term.Key))
                    .Aggregate((left, right) => left + right);
    }
}
