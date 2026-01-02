//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

#pragma warning disable CS1587 // XML comment is not placed on a valid language element
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
///     a. Implement real number evaluation (<see cref="AngouriMath.InternalAMExtensions.Sin"/>)
///     b. then complex number evaluation (<see cref="AngouriMath.Entity.Number.Sin"/>)
///
/// 2. Next, Add a new node representing the function as a nested type in <see cref="AngouriMath.Entity"/>
///     a. Copy record class (Press F12 -> Longest result of <see cref="AngouriMath.Entity.Sinf"/> <see cref="AngouriMath.Entity.Impliesf"/> <see cref="AngouriMath.Entity.Set.Unionf"/>)
///     b. Add instance method to Entity (Press F12 -> <see cref="AngouriMath.Entity.Sin"/>).
///     c. Add <see cref="AngouriMath.Entity.Boolean.Replace"/> to your node
///     d. Add <see cref="AngouriMath.Entity.Boolean.Priority"/> to your node
///     e. Add <see cref="AngouriMath.Entity.Boolean.InitDirectChildren"/> to your node
///     
/// 
/// 3. A few essential methods
///     a. InnerEval and InnerSimplify (<see cref="AngouriMath.Entity.Sinf.InnerEval"/> for numerical and <see cref="AngouriMath.Entity.Andf.InnerEval"/> for boolean)
///     b. Stringize (<see cref="AngouriMath.Entity.Sinf.Stringize"/>) (and tests to CircleTest.cs)
///     c. Latexise (<see cref="AngouriMath.Entity.Sinf.Latexise"/>) (and tests to LatexTest.cs)
///     d. Limit computation (<see cref="AngouriMath.Entity.Sinf.ComputeLimitDivideEtImpera"/>) (and tests to LimitTest.cs)
///     e. Hash for sorting (<see cref="AngouriMath.Entity.Sinf.SortHashName"/>)
///     f. Default domain <see cref="AngouriMath.Entity.Sinf.Codomain"/>
///     g. Substitute <see cref="AngouriMath.Entity.Sinf.Substitute"/> (and tests to SubstituteTest.cs)
/// 
/// 4. Pattern replacer (<see cref="AngouriMath.Functions.Patterns.CommonRules"/> and <see cref="AngouriMath.Functions.Simplificator.Alternate"/>)
/// (and tests to SimplifyTest.cs or PatternTest.cs)
/// 5. Expose to the user (add it to MathS, like <see cref="AngouriMath.MathS.Sin">this</see>)
///     
/// Now, you might be required to complete the following steps as well:
/// 
/// 6. Derivation (if applicable) (<see cref="AngouriMath.Entity.Sinf.InnerDifferentiate"/>)
/// (and tests to DerivativeTest.cs)
/// 7. Compilation (if applicable) (<see cref="AngouriMath.Entity.Sinf.CompileNode"/> and <see cref="AngouriMath.Core.FastExpression.Substitute"/>)
/// (and tests to CompilationTest.cs)
/// 8. Parser (if applicable) (See ImproveParser.md in the same folder as this file)
/// (and tests to FromStringTest.cs)
/// 9. Analytical Solver (if applicable) (<see cref="AngouriMath.Entity.Sinf.InvertNode"/>)
/// (and tests to SolveOneEquation.cs)
/// 10. ToSympy (if applicable) (<see cref="AngouriMath.Entity.Sinf.ToSymPy"/>) (Tip: Enter 'import sympy' into https://live.sympy.org/ then test)
/// (and tests to ToSymPyTest.cs)

#pragma warning restore CS1587 // XML comment is not placed on a valid language element
