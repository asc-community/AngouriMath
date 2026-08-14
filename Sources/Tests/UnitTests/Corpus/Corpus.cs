//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;

namespace AngouriMath.Tests.Corpus
{
    /// <summary>What the library did with a problem.</summary>
    public enum Verdict
    {
        /// <summary>Answered, and the answer was checked and holds.</summary>
        Solved,

        /// <summary>Declined to answer. Not a failure: "I could not settle this" is legitimate.</summary>
        Unsolved,

        /// <summary><b>Answered, and the answer is not right.</b> Always a failure.</summary>
        Wrong,

        /// <summary>Threw something that is not the library declining.</summary>
        Error,

        /// <summary>Did not finish inside the budget.</summary>
        Timeout,
    }

    /// <summary>Which call to make.</summary>
    public enum Op
    {
        /// <summary>Simplify, checked against the input at sampled points.</summary>
        Simplify,

        /// <summary>Solve for x, checked by substituting each root back in.</summary>
        Solve,

        /// <summary>Integrate over x, checked by differentiating the answer back.</summary>
        Integrate,

        /// <summary>Take a limit, checked against a stated value.</summary>
        Limit,
    }

    /// <summary>One problem, and what the library is currently expected to do with it.</summary>
    /// <param name="Name">What to call it in the report.</param>
    /// <param name="Op">Which call to make.</param>
    /// <param name="Input">The expression, as written.</param>
    /// <param name="Expect">
    /// The verdict recorded when this entry was last measured. <see cref="Verdict.Wrong"/> is
    /// never an acceptable expectation and the gate rejects it outright.
    /// </param>
    /// <param name="Approach">Where the variable goes, for a limit.</param>
    /// <param name="Expected">The answer, for a limit.</param>
    public sealed record Problem(
        string Name,
        Op Op,
        string Input,
        Verdict Expect = Verdict.Solved,
        string? Approach = null,
        string? Expected = null);

    /// <summary>
    /// A fixed set of problems with checkable answers, run on every commit, reporting
    /// <b>solved / unsolved / wrong / error / timeout</b> rather than pass or fail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> v1.0 asks for
    /// exactly this and names it as required infrastructure, together with
    /// <a href="https://github.com/asc-community/AngouriMath/issues/529">#529</a> and
    /// <a href="https://github.com/asc-community/AngouriMath/issues/500">#500</a>. The point is
    /// the four-way split: a suite that only passes or fails cannot tell <i>answered wrongly</i>
    /// from <i>declined to answer</i>, and AGENTS.md is explicit that a change which solves one
    /// more problem and introduces one wrong answer is a regression rather than progress.
    /// </para>
    /// <para>
    /// This is a <b>gate</b>, and it is not the exploratory harness. The harnesses live outside
    /// the repository, in the analysis workspace, where they can generate inputs, take minutes
    /// and be read by a person. This one runs in CI on every commit, so it is small, fast, and
    /// answers one question: did anything get worse.
    /// </para>
    /// <para>
    /// The problems are the library's own — drawn from its issue tracker, where each was reported
    /// as something that ought to work — plus a few standard limits. Nothing here is vendored
    /// from another project's test suite.
    /// </para>
    /// </remarks>
    public static class Corpus
    {
        /// <summary>The problems, with the verdict each currently earns.</summary>
        public static readonly IReadOnlyList<Problem> All = new List<Problem>
        {
            // --- Simplification: the answer must equal the input wherever both are defined ---
            new("simp:polynomial", Op.Simplify, "(x + 1) ^ 2 - x ^ 2 - 2 * x - 1"),
            new("simp:rational", Op.Simplify, "(x ^ 2 - 1) / (x - 1)"),
            new("simp:rational-two-vars", Op.Simplify, "(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)"),
            new("simp:factoring", Op.Simplify, "a * c + a * d + b * c + b * d"),
            new("simp:surds", Op.Simplify, "sqrt(12) + sqrt(27)"),
            new("simp:surd-product", Op.Simplify, "sqrt(2) * sqrt(3)"),
            new("simp:trig-pythagoras", Op.Simplify, "sin(x) ^ 2 + cos(x) ^ 2"),
            new("simp:trig-double", Op.Simplify, "sin(2 * x) - 2 * sin(x) * cos(x)"),
            new("simp:trig-cyclic", Op.Simplify, "(sin(2 * x) * csc(x)) ^ 2 / 4 - cos(2 * x) - sin(x) ^ 2"),
            new("simp:inverse-trig", Op.Simplify, "arcsin(x) + arccos(x)"),
            new("simp:log-ratio", Op.Simplify, "ln(2 ^ 1000) / ln(2 ^ (-1000))"),
            new("simp:collapse", Op.Simplify, "x ^ 2 + 2 * x + 1"),
            new("simp:parity", Op.Simplify, "sin(-x) + sin(x)"),
            new("simp:abs", Op.Simplify, "abs(-x) - abs(x)"),
            new("simp:power-tower", Op.Simplify, "(x ^ 2) ^ 3 - x ^ 6"),

            // --- Solving: every root is substituted back and must vanish ---
            new("solve:linear", Op.Solve, "3 * x + 5"),
            new("solve:quadratic", Op.Solve, "x ^ 2 - 5 * x + 6"),
            new("solve:quadratic-complex", Op.Solve, "x ^ 2 + 1"),
            new("solve:biquadratic", Op.Solve, "x ^ 4 + 3 * x ^ 2 + 2"),
            new("solve:quartic-nested", Op.Solve, "x ^ 4 + x ^ 2 + 1"),
            new("solve:quintic-factorable", Op.Solve, "x ^ 5 + 2 * x ^ 3 - 2 * x ^ 2 - 4"),
            new("solve:cubic", Op.Solve, "x ^ 3 - 6 * x ^ 2 + 11 * x - 6"),
            new("solve:exponential", Op.Solve, "e ^ x - 1"),
            new("solve:trig", Op.Solve, "sin(x)"),
            new("solve:product", Op.Solve, "(x - 1) * (x - 2) * (x - 3)"),

            // --- Integration: the answer is differentiated back and compared ---
            new("int:power", Op.Integrate, "x ^ 3"),
            new("int:reciprocal", Op.Integrate, "1 / x"),
            new("int:exponential", Op.Integrate, "e ^ x"),
            new("int:trig", Op.Integrate, "sin(x)"),
            new("int:u-sub", Op.Integrate, "cos(x ^ 2) * x"),
            new("int:by-parts-cyclic", Op.Integrate, "sin(x) * e ^ x"),
            new("int:arctan-form", Op.Integrate, "1 / (1 + x ^ 2)"),
            new("int:partial-fractions", Op.Integrate, "1 / (x ^ 4 + 3 * x ^ 2 + 2)"),
            new("int:hard", Op.Integrate, "sqrt(tan(x))", Verdict.Unsolved),

            // --- Limits: compared against the stated value ---
            new("lim:0/0", Op.Limit, "sin(x) / x", Approach: "0", Expected: "1"),
            new("lim:0/0-arcsin", Op.Limit, "arcsin(x) / x", Approach: "0", Expected: "1"),
            new("lim:1^oo", Op.Limit, "(1 + 1/x) ^ x", Approach: "+oo", Expected: "e"),
            new("lim:oo-oo", Op.Limit, "e ^ x - x", Approach: "+oo", Expected: "+oo"),
            new("lim:log", Op.Limit, "1/x + ln(x)", Approach: "+oo", Expected: "+oo"),
            new("lim:ratio", Op.Limit, "(x ^ 2 + 1) / (x ^ 2 - 1)", Approach: "+oo", Expected: "1"),
        };
    }
}
