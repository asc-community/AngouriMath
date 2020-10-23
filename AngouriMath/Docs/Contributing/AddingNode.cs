/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath; using AngouriMath.Core; using AngouriMath.Functions; using AngouriMath.Functions.Algebra;using AngouriMath.Functions.Algebra.NumericalSolving; using AngouriMath.Functions.Algebra.AnalyticalSolving;using static AngouriMath.Entity;using static AngouriMath.Entity.Number;
// TODO: the directives above shouldn't get removed when removing unnecessary directives. If they did, their copy is at the bottom of the document
// TODO: this guide requires a lot of work

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
///     a. Implement real number evaluation (<see cref="NumbersExtensions.Sin"/>)
///     b. then complex number evaluation (<see cref="Sin"/>)
///
/// 2. Next, Add a new node representing the function as a nested type in <see cref="Entity"/>
///     a. Copy record class (Press F12 -> Longest result of <see cref="Sinf"/> <see cref="Impliesf"/> <see cref="Set.Unionf"/>)
///     b. Add instance method to Entity (Press F12 -> <see cref="Entity.Sin"/>).
///     c. Add <see cref="Boolean.Replace"/> to your node
///     d. Add <see cref="Boolean.Priority"/> to your node
///     e. Add <see cref="Boolean.InitDirectChildren"/> to your node
///     
/// 
/// 3. A few essential methods
///     a. InnerEval and InnerSimplify (<see cref="Sinf.InnerEval"/> for numerical and <see cref="Andf.InnerEval"/> for boolean)
///     b. Stringize (<see cref="Sinf.Stringize"/>)
///     c. Latexise (<see cref="Sinf.Latexise"/>)
///     d. Limit computation (<see cref="Sinf.ComputeLimitDivideEtImpera"/>)
///     e. Hash for sorting (<see cref="Sinf.SortHashName"/>)
///     f. Default domain <see cref="Sinf.Codomain"/>
///     g. Substitute <see cref="Sinf.Substitute"/>
/// 
/// 4. Pattern replacer (<see cref="Patterns.CommonRules"/> and <see cref="Simplificator.Alternate"/>)
/// 5. Expose to the user (add it to MathS, like <see cref="MathS.Sin">this</see>)
/// 
/// Now, you might be required to complete the following steps as well:
/// 
/// 6. Derivation (if applicable) (<see cref="Sinf.InnerDifferentiate"/>)
/// 7. Compilation (if applicable) (<see cref="Sinf.CompileNode"/> and <see cref="FastExpression.Substitute"/>)
/// 8. Parser (if applicable) (See ImproveParser.md in the same folder as this file)
/// 9. Analytical Solver (if applicable) (<see cref="Sinf.InvertNode"/>)
/// 10. ToSympy (if applicable) (<see cref="Sinf.ToSymPy"/>) (Tip: Enter 'import sympy' into https://live.sympy.org/ then test)

#pragma warning restore CS1587 // XML comment is not placed on a valid language element


// using AngouriMath; using AngouriMath.Core; using AngouriMath.Functions; using AngouriMath.Functions.Algebra;using AngouriMath.Functions.Algebra.NumericalSolving; using AngouriMath.Functions.Algebra.AnalyticalSolving;using static AngouriMath.Entity;using static AngouriMath.Entity.Number;