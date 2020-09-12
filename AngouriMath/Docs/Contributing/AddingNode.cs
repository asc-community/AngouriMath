using AngouriMath; using AngouriMath.Core; using AngouriMath.Functions; using AngouriMath.Functions.Algebra;using AngouriMath.Functions.Algebra.NumericalSolving; using AngouriMath.Functions.Algebra.AnalyticalSolving;using static AngouriMath.Entity;using static AngouriMath.Entity.Number;
// TODO: the directives above shouldn't get removed when removing unnecessary directives. If they did, their copy is at the bottom of the document
// TODO: this guide requires a lot of work

/// To add a new node (function, operator) follow this guide. It covers all the methods you can
/// implement for your nodes.
/// 
/// 0. When choosing a file to add to between *.Continuous.Classes.cs and *.Discrete.Classes.cs, choose in the following
///    way: if your node is of the boolean type (so that can be simplified to one), it should go to
///    *.Discrete.Classes.cs, while if it's a numerical node (i. e. can be simplified to a number), it should be
///    added to *.Continuous.Classes.cs. If there's no *.*.Classes.cs, then add it to *.Classes.cs or to the
///    file you need to add.
/// 
/// 1. Numerical evaluation (if appropriate)
///     a. Implement real number evaluation (<see cref="NumbersExtensions.Sin"/>)
///     b. then complex number evaluation (<see cref="Sin"/>)
///
/// 2. Next, Add a new node representing the function as a nested type in <see cref="Entity"/>
///     a. Copy record class (Press F12 -> Longest result of <see cref="Sinf"/>)
///     b. Add instance method to Entity (Press F12 -> <see cref="Entity.Sin"/>).
/// 
/// 3. A few essential methods
///     a. InnerEval (<see cref="Sinf.InnerEval"/> for numerical and <see cref="Andf.InnerEval"/> for boolean)
///     b. Stringize (<see cref="Sinf.Stringize"/>)
///     c. Latexise (<see cref="Sinf.Latexise"/>)
///     d. InnerSimplify (<see cref="Sinf.InnerSimplify"/>)
///     e. Limit computation (<see cref="Sinf.ComputeLimitDivideEtImpera"/>)
///     f. Hash for sorting (<see cref="Sinf.SortHashName"/>)
/// 
/// 4. Pattern replacer (<see cref="Patterns.CommonRules"/> and <see cref="Simplificator.Alternate"/>)
/// 5. Expose to the user (add it to MathS, like <see cref="MathS.Sin">this</see>)
/// 
/// Now, you might be required to complete the following steps as well:
/// 
/// 6. Derivation (if appropriate) (<see cref="Sinf.Derive"/>)
/// 7. Compilation (if appropriate) (<see cref="Sinf.CompileNode"/> and <see cref="FastExpression.Substitute"/>)
/// 8. Parser (if appropriate) (See ImproveParser.md in the same folder as this file)
/// 9. Analytical Solver (if appropriate) (<see cref="Sinf.InvertNode"/>)
/// 10. ToSympy (if appropriate) (<see cref="Sinf.ToSymPy"/>) (Tip: Enter 'import sympy' into https://live.sympy.org/ then test)
/// 



// using AngouriMath; using AngouriMath.Core; using AngouriMath.Functions; using AngouriMath.Functions.Algebra;using AngouriMath.Functions.Algebra.NumericalSolving; using AngouriMath.Functions.Algebra.AnalyticalSolving;using static AngouriMath.Entity;using static AngouriMath.Entity.Number;