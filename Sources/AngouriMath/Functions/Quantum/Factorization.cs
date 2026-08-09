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
    /// Writing a state as a tensor product where its leading or trailing qubits are in a
    /// definite basis state.
    /// </summary>
    /// <remarks>
    /// <c>|001&gt; + |011&gt;</c> is <c>|0&gt; (x) (|0&gt; + |1&gt;) (x) |1&gt;</c>: the first
    /// and last qubits agree across the whole superposition, so they factor out and only the
    /// middle carries it.
    /// <para/>
    /// **Which positions agree is not computed here.** It is
    /// <see cref="SparseTerms{TBasis}.FactorOutCommon"/>, the same call that takes the common
    /// monomial out of a polynomial -- the meet of the support. What is specific to states is
    /// only how the answer is written down, and that division is the whole point of the spine.
    /// <para/>
    /// **What this does not do.** Only a common *prefix or suffix* is factored, because a ket
    /// says which qubit it describes by its position in the product and there is no notation
    /// here for "the state of qubits 1 and 3". Nor does it find general separability:
    /// <c>(|0&gt; + |1&gt;) (x) (|0&gt; + |1&gt;)</c> has no qubit in a definite state at all,
    /// so the meet is empty and nothing is factored, yet it is a product state. Detecting that
    /// is a rank-one test on the amplitudes across a bipartition -- a different algorithm,
    /// belonging to this file rather than to the spine, and not written yet.
    /// </remarks>
    internal static class Factorization
    {
        /// <summary>
        /// The state rewritten as a product, or <see langword="null"/> where it is not a state
        /// or has no qubit in a definite basis state at either end.
        /// </summary>
        internal static Entity? TensorFactorize(Entity expr)
        {
            if (QuantumState.TryRead(expr) is not { } state || state.IsEmpty)
                return null;
            var width = state.Terms.Keys.First().Width;
            var (common, remainder) = state.FactorOutCommon(new KetOps(width));
            if (common.IsAllFree)
                return null;

            var prefix = common.Cells.TakeWhile(cell => cell != Ket.Free).ToArray();
            var suffix = common.Cells.Skip(prefix.Length)
                .Reverse().TakeWhile(cell => cell != Ket.Free).Reverse().ToArray();
            if (prefix.Length + suffix.Length == 0)
                return null;
            if (prefix.Length + suffix.Length == width)
                // Every position is fixed, so the state is a single basis ket and the whole of
                // it is the "common" part. Writing that as a product of two kets would be a
                // longer way of saying the same thing.
                return null;

            var middle = Slice(remainder, prefix.Length, width - suffix.Length);
            var factors = new List<Entity>();
            if (prefix.Length > 0)
                factors.Add(QuantumState.ToEntity(new Ket(prefix)));
            factors.Add(QuantumState.ToEntity(middle));
            if (suffix.Length > 0)
                factors.Add(QuantumState.ToEntity(new Ket(suffix)));
            return factors.Aggregate((left, right) => left * right);
        }

        /// <summary>
        /// A product of states over consecutive qubits multiplied back out into a single
        /// superposition, or <see langword="null"/> where the expression is not such a product.
        /// </summary>
        /// <remarks>
        /// The inverse of <see cref="TensorFactorize"/>, and the reason it is here rather than on the
        /// spine: within one state the qubits are a fixed width and combining two kets overlays
        /// them, while a *product* of states concatenates their widths. Those are two different
        /// monoids on the same type, and only the first is what
        /// <see cref="SparseTerms{TBasis}"/> multiplies with.
        /// </remarks>
        internal static Entity? TensorExpand(Entity expr)
        {
            var factors = new List<Entity>();
            Flatten(expr, factors);
            var states = new List<SparseTerms<Ket>>();
            Entity scalar = 1;
            foreach (var factor in factors)
                if (QuantumState.TryRead(factor) is { } state)
                    states.Add(state);
                else if (!factor.Nodes.Any(node => QuantumState.TryReadKet(node) is not null))
                    scalar *= factor;
                else
                    return null;
            if (states.Count == 0)
                return null;

            var combined = states.Aggregate(Concatenate);
            return QuantumState.ToEntity(combined.Scale(scalar));
        }

        private static void Flatten(Entity expr, List<Entity> into)
        {
            if (expr is Mulf(var multiplier, var multiplicand))
            {
                Flatten(multiplier, into);
                Flatten(multiplicand, into);
            }
            else into.Add(expr);
        }

        /// <summary>
        /// Every ket of the left state joined to every ket of the right, widths added and
        /// amplitudes multiplied -- the tensor product proper.
        /// </summary>
        private static SparseTerms<Ket> Concatenate(SparseTerms<Ket> left, SparseTerms<Ket> right)
            => SparseTerms<Ket>.From(
                left.Terms.SelectMany(l => right.Terms.Select(r =>
                    new KeyValuePair<Ket, Entity>(
                        new Ket(l.Key.Cells.Concat(r.Key.Cells).ToArray()),
                        l.Value * r.Value))),
                left.Coefficients);

        /// <summary>
        /// The state restricted to the positions in <c>[from, to)</c>, which is what is left
        /// once the definite qubits at the ends have been taken away.
        /// </summary>
        private static SparseTerms<Ket> Slice(SparseTerms<Ket> state, int from, int to)
            => SparseTerms<Ket>.From(
                state.Terms.Select(term => new KeyValuePair<Ket, Entity>(
                    new Ket(term.Key.Cells.Skip(from).Take(to - from).ToArray()), term.Value)),
                state.Coefficients);

        /// <summary>
        /// Whether the amplitudes square-sum to one, or <see langword="null"/> where that
        /// cannot be settled -- a symbolic amplitude need not have a decidable modulus.
        /// </summary>
        /// <remarks>
        /// The sum is over |c|^2, written as <c>abs(c)^2</c> since there is no conjugate in
        /// the library. Left to <c>Simplify</c> to settle rather than evaluated numerically,
        /// so <c>1/sqrt(2)</c> is recognised exactly.
        /// </remarks>
        internal static bool? IsNormalized(Entity expr)
        {
            if (QuantumState.TryRead(expr) is not { } state || state.IsEmpty)
                return null;
            var total = state.Terms
                .Select(term => MathS.Sqr(MathS.Abs(term.Value)))
                .Aggregate((left, right) => left + right)
                .Simplify();
            if (total == 1)
                return true;
            return total.Evaled is Number.Complex { IsFinite: true } value
                ? value == 1
                : null;
        }

        /// <summary>
        /// Whether two states are the same up to a global phase -- that is, whether one is a
        /// scalar multiple of the other, that scalar having modulus one.
        /// </summary>
        /// <remarks>
        /// Two states with the same physical content may be written differently, and a global
        /// factor is exactly the difference that no measurement can see. Decided by taking the
        /// ratio on one shared basis ket and checking every other amplitude against it, which
        /// is symbolic throughout -- <c>|00&gt; + |11&gt;</c> and
        /// <c>-|00&gt; - |11&gt;</c> differ by -1 and are the same state.
        /// </remarks>
        internal static bool? EqualUpToGlobalPhase(Entity left, Entity right)
        {
            if (QuantumState.TryRead(left) is not { } a || QuantumState.TryRead(right) is not { } b)
                return null;
            if (a.Count != b.Count || a.Count == 0)
                return a.IsEmpty && b.IsEmpty;
            var shared = a.Terms.Keys.First();
            if (!b.Terms.TryGetValue(shared, out var scale) || scale == 0)
                return false;
            var phase = (a.Terms[shared] / scale).Simplify();
            foreach (var term in a.Terms)
            {
                if (!b.Terms.TryGetValue(term.Key, out var other))
                    return false;
                if ((term.Value - phase * other).Simplify() != 0)
                    return false;
            }
            // A phase is a unit: rescaling by anything else is a different state, not the same
            // one seen differently.
            var modulus = MathS.Sqr(MathS.Abs(phase)).Simplify();
            return modulus == 1 ? true : modulus.Evaled is Number.Complex value ? value == 1 : null;
        }
    }
}
