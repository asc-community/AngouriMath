//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// What a <see cref="Transformation"/> claims about its output relative to its input.
    /// Without this, <see cref="Soundness"/> would be meaningless: "sound" is only a
    /// statement about some relation, and the relation is not the same one for every
    /// operation.
    /// </summary>
    public enum TransformationRelation
    {
        /// <summary>
        /// The output denotes the same mathematical value as the input, so
        /// <c>input - output</c> is zero wherever both are defined. Simplification,
        /// expansion, factorisation and rewriting all claim this.
        /// </summary>
        Equivalence,

        /// <summary>
        /// The output is a different mathematical object computed from the input — a
        /// derivative, an antiderivative, a limit, an instance under a substitution.
        /// Subtracting it from the input means nothing, and a test that does so is testing
        /// nothing.
        /// </summary>
        Derivation
    }
}
