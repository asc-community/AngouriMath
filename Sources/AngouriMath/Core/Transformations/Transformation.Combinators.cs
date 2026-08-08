//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Transformations
{
    partial class Transformation
    {
        /// <summary>
        /// Runs this transformation and then <paramref name="next"/> on what it produced.
        /// Where either step has no answer, the composition has none: a step that could not
        /// be settled is not silently skipped.
        /// </summary>
        /// <param name="next">The transformation to apply to this one's output.</param>
        public Transformation Then(Transformation next)
            => new SequentialTransformation(this, next ?? throw new ArgumentNullException(nameof(next)));

        /// <summary>
        /// Runs this transformation <paramref name="times"/> times in a row. The bound is
        /// the caller's, which is what keeps repetition terminating by construction rather
        /// than by the good behaviour of whatever is being repeated.
        /// </summary>
        /// <param name="times">How many applications; must not be negative.</param>
        public Transformation Repeat(int times)
            => times < 0
                ? throw new ArgumentOutOfRangeException(nameof(times))
                : new RepeatedTransformation(this, times);

        /// <summary>
        /// Runs this transformation until its output stops changing, giving up after
        /// <paramref name="maxIterations"/> applications.
        /// </summary>
        /// <param name="maxIterations">The bound on applications; must be positive.</param>
        /// <remarks>
        /// Hitting the bound is reported as <b>no answer</b> rather than as the last value
        /// reached. An unbounded rewrite loop is the failure mode this layer is supposed to
        /// make visible, and handing back a value from the middle of one would hide exactly
        /// the case worth seeing. It is also what lets a test assert that a rule set has a
        /// fixed point on a given expression instead of assuming it.
        /// </remarks>
        public Transformation UntilStable(int maxIterations)
            => maxIterations < 1
                ? throw new ArgumentOutOfRangeException(nameof(maxIterations))
                : new StableTransformation(this, maxIterations);

        /// <summary>
        /// The weaker of two justifications. Composing anything with a heuristic gives a
        /// heuristic; nothing composes upwards into a stronger claim.
        /// </summary>
        private static Soundness Weaker(Soundness one, Soundness another)
            => one > another ? one : another;

        /// <summary>
        /// A chain is an equivalence only if every link is. One derivation anywhere in it
        /// means the end no longer denotes the same value as the start.
        /// </summary>
        private static TransformationRelation Combine(TransformationRelation one, TransformationRelation another)
            => one is TransformationRelation.Equivalence && another is TransformationRelation.Equivalence
                ? TransformationRelation.Equivalence
                : TransformationRelation.Derivation;

        private sealed class SequentialTransformation : Transformation
        {
            private readonly Transformation first, second;

            internal SequentialTransformation(Transformation first, Transformation second)
                => (this.first, this.second) = (first, second);

            public override string Name => $"{first.Name} then {second.Name}";

            public override TransformationRelation Relation => Combine(first.Relation, second.Relation);

            public override Soundness Soundness => Weaker(first.Soundness, second.Soundness);

            protected override Entity? ApplyCore(Entity input)
                => first.Apply(input).Output is { } intermediate
                    ? second.Apply(intermediate).Output
                    : null;
        }

        private sealed class RepeatedTransformation : Transformation
        {
            private readonly Transformation inner;
            private readonly int times;

            internal RepeatedTransformation(Transformation inner, int times)
                => (this.inner, this.times) = (inner, times);

            public override string Name => $"{inner.Name} x{times}";

            public override TransformationRelation Relation => inner.Relation;

            public override Soundness Soundness => inner.Soundness;

            protected override Entity? ApplyCore(Entity input)
            {
                var current = input;
                for (var i = 0; i < times; i++)
                    if (inner.Apply(current).Output is { } next)
                        current = next;
                    else
                        return null;
                return current;
            }
        }

        private sealed class StableTransformation : Transformation
        {
            private readonly Transformation inner;
            private readonly int maxIterations;

            internal StableTransformation(Transformation inner, int maxIterations)
                => (this.inner, this.maxIterations) = (inner, maxIterations);

            public override string Name => $"{inner.Name} until stable (<={maxIterations})";

            public override TransformationRelation Relation => inner.Relation;

            public override Soundness Soundness => inner.Soundness;

            protected override Entity? ApplyCore(Entity input)
            {
                var current = input;
                for (var i = 0; i < maxIterations; i++)
                {
                    if (inner.Apply(current).Output is not { } next)
                        return null;
                    if (next == current)
                        return current;
                    current = next;
                }
                return null;
            }
        }
    }
}
