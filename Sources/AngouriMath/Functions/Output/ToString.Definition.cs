namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Converts an expression into a string. Works synonymically into <see cref="Entity.ToString"/>.
        /// </summary>
        public abstract string Stringize();

        /// <summary>
        /// Converts an expression into a string
        /// </summary>
        /// <param name="parenthesesRequired">Whether to wrap with '(' and ')'</param>
        protected internal string Stringize(bool parenthesesRequired) =>
            parenthesesRequired || MathS.Diagnostic.OutputExplicit ? $"({Stringize()})" : Stringize();
    }
}
