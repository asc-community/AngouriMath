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
        /// Converts an expression into a string. Works synonymically into <see cref="Entity.ToString"/>.
        /// </summary>
        /// <remarks>
        /// A node whose <see cref="Codomain"/> is not the one its type carries by default is
        /// printed inside <c>domain(inner, SET)</c>, which is the syntax the parser already has
        /// for it. Without that the annotation is dropped and the printed form reads back as a
        /// different expression — <c>domain(sqrt(-1), RR)</c> is <see cref="MathS.NaN"/> and
        /// <c>sqrt(-1)</c> is <c>i</c>, and the two used to print the same string.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1022">#1022</a>
        /// </remarks>
        public string Stringize()
            => PrintsItsCodomain
                ? $"domain({StringizeNode()}, {Set.SpecialSet.Create(Codomain).Stringize()})"
                : StringizeNode();

        /// <summary>
        /// Converts this node, and this node only, into a string: the codomain wrapper is
        /// <see cref="Stringize()"/>'s to add, so that it is decided in one place rather than in
        /// each of the sixty-odd nodes.
        /// </summary>
        private protected abstract string StringizeNode();

        /// <summary>
        /// Converts an expression into a string
        /// </summary>
        /// <param name="parenthesesRequired">Whether to wrap with '(' and ')'</param>
        /// <remarks>
        /// A node that prints its codomain needs no brackets whatever its priority, because
        /// <c>domain(...)</c> is a function call and so already binds tighter than any operator
        /// around it.
        /// </remarks>
        protected internal string Stringize(bool parenthesesRequired) =>
            (parenthesesRequired && !PrintsItsCodomain) || MathS.Diagnostic.OutputExplicit
                ? $"({Stringize()})" : Stringize();
    }
}
