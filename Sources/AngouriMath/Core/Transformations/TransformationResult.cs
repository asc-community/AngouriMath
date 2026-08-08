//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// What one application of a <see cref="Transformation"/> produced: the input, the
    /// output if there was one, and which transformation was asked.
    /// </summary>
    /// <remarks>
    /// A struct, so that routing an ordinary <c>Simplify</c> or <c>Differentiate</c> call
    /// through this layer costs no allocation.
    /// </remarks>
    public readonly struct TransformationResult
    {
        internal TransformationResult(Transformation transformation, Entity input, Entity? output)
            => (Transformation, Input, Output) = (transformation, input, output);

        /// <summary>The transformation that was applied.</summary>
        public Transformation Transformation { get; }

        /// <summary>The expression it was applied to.</summary>
        public Entity Input { get; }

        /// <summary>
        /// The result, or <see langword="null"/> where the transformation could not settle
        /// the question. Null means "no answer" and nothing more — in particular it does not
        /// mean the answer does not exist.
        /// </summary>
        public Entity? Output { get; }

        /// <summary>Whether there is an answer at all.</summary>
        public bool Succeeded => Output is not null;

        /// <summary>
        /// Whether there is an answer and it differs from the input. A transformation that
        /// succeeded without changing anything has reached a fixed point, which is what
        /// <see cref="Transformation.UntilStable(int)"/> looks for.
        /// </summary>
        public bool Changed => Output is not null && Output != Input;

        /// <summary>
        /// The answer where there is one, the untouched input otherwise. This is what the
        /// 1.x API surface wants: those methods have always returned the original
        /// expression rather than nothing when they could not improve on it.
        /// </summary>
        public Entity OutputOrInput => Output ?? Input;

        /// <summary>What the transformation claims relates <see cref="Input"/> to <see cref="Output"/>.</summary>
        public TransformationRelation Relation => Transformation.Relation;

        /// <summary>How well justified that claim is.</summary>
        public Soundness Soundness => Transformation.Soundness;

        /// <inheritdoc/>
        public override string ToString()
            => Output is null
                ? $"{Transformation.Name}: no answer for {Input.Stringize()}"
                : $"{Transformation.Name}: {Input.Stringize()} -> {Output.Stringize()}";
    }
}
