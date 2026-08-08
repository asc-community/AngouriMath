//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// A named mathematical operation that consumes an <see cref="Entity"/> and produces
    /// one, together with enough about itself — what it claims, how well justified the
    /// claim is — to be inspected and composed rather than only invoked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the layer the 1.x entry points sit on:
    /// <see cref="Entity.Simplify(int)"/>, <see cref="Entity.Expand(int)"/> and
    /// <see cref="Entity.Factorize(int)"/> are adapters over the transformations named
    /// below, and the algorithms underneath are unchanged. Callers who only want an answer
    /// should keep using those methods; this type is for callers who want to know which
    /// operation produced it, or to build an operation out of others.
    /// </para>
    /// <para>
    /// Deterministic: the same transformation applied to the same expression under the same
    /// <see cref="MathS.Settings"/> gives the same result. Composition is by explicit
    /// ordering — there is no registry that decides what to run next, and nothing here
    /// consults reflection, so the layer stays trimmable and AOT-publishable.
    /// </para>
    /// <para>
    /// <b>Experimental.</b> The three concepts — a transformation, its relation, its
    /// soundness tier — are meant to last; the catalogue of factories will grow and the
    /// signatures here may still move. The stable surface is <see cref="MathS"/> and the
    /// methods on <see cref="Entity"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using System;
    /// using AngouriMath;
    /// using AngouriMath.Core.Transformations;
    ///
    /// Entity expr = "(x + 1) ^ 2";
    /// var result = Transformation.Expansion.Apply(expr);
    /// Console.WriteLine(result.Output);
    /// Console.WriteLine(result.Relation);
    /// Console.WriteLine(result.Soundness);
    /// </code>
    /// Prints
    /// <code>
    /// 1 + 2 * x + x ^ 2
    /// Equivalence
    /// SoundUnderAssumptions
    /// </code>
    /// </example>
    public abstract partial class Transformation
    {
        /// <summary>
        /// A stable identity for this operation, used in diagnostics and in the failure a
        /// caller is handed. Composed transformations name their parts.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>What this operation claims about its output relative to its input.</summary>
        public abstract TransformationRelation Relation { get; }

        /// <summary>How well justified that claim is.</summary>
        public abstract Soundness Soundness { get; }

        /// <summary>
        /// Does the work. Returns <see langword="null"/> to mean "I could not settle this" —
        /// never an unevaluated node of the input, and never <see cref="MathS.NaN"/> for a
        /// question that merely went unanswered.
        /// </summary>
        /// <param name="input">The expression to transform.</param>
        protected abstract Entity? ApplyCore(Entity input);

        /// <summary>
        /// Applies the transformation, reporting what happened rather than only the value.
        /// </summary>
        /// <param name="input">The expression to transform.</param>
        /// <returns>
        /// The input, the output where there is one, and this transformation. See
        /// <see cref="TransformationResult.Succeeded"/> for the case where there is none.
        /// </returns>
        public TransformationResult Apply(Entity input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new TransformationResult(this, input, ApplyCore(input));
        }

        /// <summary>
        /// Applies the transformation and hands back the input untouched where it produced
        /// no answer — the convention the 1.x methods have always followed.
        /// </summary>
        /// <param name="input">The expression to transform.</param>
        public Entity ApplyOrKeep(Entity input) => Apply(input).OutputOrInput;

        /// <inheritdoc/>
        public override string ToString() => Name;
    }
}
