//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// How well justified the relation a <see cref="Transformation"/> claims between its
    /// input and its output is. The three tiers are never blurred: a heuristic labelled as
    /// a proof is a wrong answer with a friendly face.
    /// </summary>
    /// <remarks>
    /// The tier is <b>declared</b> by whoever wrote the transformation, not derived from it.
    /// Nothing in the library checks a declaration today, so a tier is a claim to be argued
    /// with rather than a guarantee to be relied on, and the registry starts conservative
    /// on purpose: tightening a label needs an argument, loosening one does not.
    /// </remarks>
    public enum Soundness
    {
        /// <summary>
        /// The claimed relation holds for every value of the free variables, with no side
        /// conditions and no choice of branch.
        /// </summary>
        Sound,

        /// <summary>
        /// The claimed relation holds only where the stated assumptions do: wherever both
        /// sides are defined, under the conditions the output carries as
        /// <see cref="Entity.Providedf"/>, or under a branch-cut convention. This is the
        /// honest tier for most of the rewrite rules in this library.
        /// </summary>
        SoundUnderAssumptions,

        /// <summary>
        /// Worth trying; proves nothing. A heuristic result has to be checked by something
        /// else before it may be returned as an answer.
        /// </summary>
        Heuristic
    }
}
