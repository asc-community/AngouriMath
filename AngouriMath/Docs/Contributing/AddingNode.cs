using AngouriMath; using AngouriMath.Core; using AngouriMath.Functions; using AngouriMath.Functions.Algebra;using AngouriMath.Functions.Algebra.NumericalSolving; using AngouriMath.Functions.Algebra.AnalyticalSolving;using static AngouriMath.Entity;using static AngouriMath.Entity.Number;
// TODO: the directives above shouldn't get removed when removing unnecessary directives
// TODO: this guide requires a lot of work

/// To add a new node (function, operator) follow this guide. It covers all the methods you can
/// implement for your nodes.
/// 
/// 1. Numerical evaluation (if appropriate)
///     a. Implement real number evaluation (<see cref="PeterONumbersExtensions.Sin"/>)
///     b. then complex number evaluation (<see cref="Sin"/>)
///
/// 2. Next, Add a new node representing the function as a nested type in <see cref="Entity"/>
///     a. Copy record class (Press F12 -> Longest result of <see cref="Sinf"/>)
///     b. Add instance method to Entity (Press F12 -> <see cref="Entity.Sin"/>).
/// 
/// 3. A few essential methods
///     a. InnerEval (Press F12 -> <see cref="Sinf.InnerEval"/>)
///     b. Stringize (Press F12 -> <see cref="Sinf.Stringize"/>)
///     c. Latexise (Press F12 -> <see cref="Sinf.Latexise"/>)
///     d. InnerSimplify (Press F12 -> <see cref="Sinf.InnerSimplify"/>)
/// 
/// 4. Pattern replacer (Press F12 -> <see cref="Patterns.TrigonometricRules"/> and <see cref="Simplificator.Alternate"/>)
/// 5. Expose to the user (add it to MathS, like <see cref="MathS.Sin">this</see>)
/// 
/// Now, you might be required to complete the following steps as well:
/// 
/// 6. Derivation (if appropriate) (Press F12 -> <see cref="Sinf.Derive"/>)
/// 7. Compilation (if appropriate) (Press F12 -> <see cref="Sinf.InnerCompile_"/> and <see cref="FastExpression.Substitute"/>)
/// 8. Parser (if appropriate) (See ImproveParser.md in the same folder as this file)
/// 9. Analytical Solver (if appropriate) (Press F12 -> <see cref="TrigonometricSolver"/> and <see cref="AnalyticalSolver.Solve"/>)
/// 10. ToSympy (if appropriate) (Press F12 -> <see cref="Sinf.ToSymPy"/>) (Tip: Enter 'import sympy' into https://live.sympy.org/ then test)
/// 
/// And finally, remember to add tests for all the new functionality!