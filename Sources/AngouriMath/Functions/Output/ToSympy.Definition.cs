namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>Generates Python code that you can use with sympy</summary>
        internal abstract string ToSymPy();
        
        /// <summary>
        /// Generates python code without any additional symbols that can be run in SymPy
        /// </summary>
        /// <param name="parenthesesRequired">
        /// Whether to wrap it with parentheses
        /// Usually depends on its parental nodes
        /// </param>
        protected string ToSymPy(bool parenthesesRequired) =>
            parenthesesRequired ? @$"({ToSymPy()})" : ToSymPy();
    }
}
