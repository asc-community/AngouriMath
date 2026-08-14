//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace AngouriMath.Tests.Corpus
{
    /// <summary>
    /// Runs <see cref="Corpus"/> and fails where the library got <b>worse</b> — or where it got
    /// better and the record was not updated to say so.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The answers are checked rather than compared to a stored string. A solution is substituted
    /// back into the equation, an antiderivative is differentiated back, a simplification is
    /// evaluated against the expression it came from. That means the corpus does not have to
    /// record what the library happens to print today, so a change in form is not a failure and
    /// only a change in <i>value</i> is.
    /// </para>
    /// <para>
    /// Checking is numeric at sampled points, and the points are complex and away from the axes
    /// on purpose: a rule that is wrong off the real line is the kind this library keeps finding,
    /// and real sample points would not see it.
    /// </para>
    /// </remarks>
    [Trait("Area", "Corpus")]
    public sealed class CorpusGateTest
    {
        private readonly ITestOutputHelper output;

        public CorpusGateTest(ITestOutputHelper output) => this.output = output;

        /// <summary>Generous: this is a gate against regression, not a performance measurement.</summary>
        private static readonly TimeSpan Budget = TimeSpan.FromSeconds(30);

        /// <summary>Away from the real axis, and away from the small integers.</summary>
        private static readonly System.Numerics.Complex[] Samples =
        {
            new(0.3721, 0.5813), new(-0.7211, 0.2917), new(1.4523, -0.6131), new(-1.1907, -0.8419),
        };

        /// <summary>Positive and away from 1, for the antiderivative check. See CheckIntegrate.</summary>
        private static readonly System.Numerics.Complex[] RealSamples =
        {
            new(0.4137, 0), new(1.7219, 0), new(2.9041, 0), new(0.8353, 0),
        };

        /// <summary>Values given to a parameter that a root is stated in terms of.</summary>
        private static readonly int[] ParameterValues = { 0, 1, -1, 2 };

        /// <summary>
        /// The root itself where it names only <c>x</c>, or one root per value of each parameter
        /// it is stated in terms of.
        /// </summary>
        private static IEnumerable<Entity> Instantiate(Entity root)
        {
            var parameters = root.Vars.Where(v => v.Name != "x").ToList();
            if (parameters.Count == 0)
            {
                yield return root;
                yield break;
            }
            foreach (var value in ParameterValues)
            {
                var instance = root;
                foreach (var parameter in parameters)
                    instance = instance.Substitute(parameter, value);
                yield return instance;
            }
        }

        private const double Tolerance = 1e-6;

        [Fact]
        public void TheCorpusHasNotGotWorse()
        {
            var results = Corpus.All.Select(Run).ToList();
            output.WriteLine(Report(results));

            var wrong = results.Where(r => r.Verdict == Verdict.Wrong).ToList();
            var regressed = results
                .Where(r => r.Verdict != r.Problem.Expect && r.Verdict != Verdict.Wrong)
                .ToList();

            var complaint = new StringBuilder();
            foreach (var r in wrong)
                complaint.AppendLine($"WRONG    {r.Problem.Name}: {r.Problem.Input} -> {r.Answer} ({r.Note})");
            foreach (var r in regressed)
                complaint.AppendLine(
                    $"CHANGED  {r.Problem.Name}: expected {r.Problem.Expect}, got {r.Verdict}"
                    + $" ({r.Note}). If this is an improvement, record it in Corpus.cs.");

            Assert.True(complaint.Length == 0, "\n" + complaint + "\n" + Report(results));
        }

        /// <summary>
        /// A corpus entry may not record a wrong answer as acceptable. Recording one would turn
        /// the gate into a description of the defect rather than a check against it.
        /// </summary>
        [Fact]
        public void NoEntryExpectsAWrongAnswer()
            => Assert.Empty(Corpus.All.Where(p => p.Expect == Verdict.Wrong));

        private sealed record Result(Problem Problem, Verdict Verdict, string Answer, string Note);

        private static Result Run(Problem problem)
        {
            try
            {
                var task = Task.Run(() => Attempt(problem));
                return task.Wait(Budget)
                    ? task.Result
                    : new Result(problem, Verdict.Timeout, "", $"no answer in {Budget.TotalSeconds:0}s");
            }
            catch (AggregateException e) when (e.InnerException is { } inner)
            {
                // The library declining is not an error; anything else is.
                return inner is AngouriMathBaseException and not AngouriBugException
                    ? new Result(problem, Verdict.Unsolved, "", Short(inner))
                    : new Result(problem, Verdict.Error, "", Short(inner));
            }
            catch (Exception e)
            {
                return new Result(problem, Verdict.Error, "", Short(e));
            }
        }

        private static Result Attempt(Problem problem) => problem.Op switch
        {
            Op.Simplify => CheckSimplify(problem),
            Op.Solve => CheckSolve(problem),
            Op.Integrate => CheckIntegrate(problem),
            Op.Limit => CheckLimit(problem),
            _ => new Result(problem, Verdict.Error, "", "unsupported operation"),
        };

        private static Result CheckSimplify(Problem problem)
        {
            var input = problem.Input.ToEntity();
            var simplified = input.Simplify();
            return AgreeAtSamples(input, simplified, out var note)
                ? new Result(problem, Verdict.Solved, simplified.Stringize(), note)
                : new Result(problem, Verdict.Wrong, simplified.Stringize(), note);
        }

        private static Result CheckSolve(Problem problem)
        {
            var input = problem.Input.ToEntity();
            var roots = input.SolveEquation("x");
            if (roots is not Entity.Set.FiniteSet finite)
                return new Result(problem, Verdict.Unsolved, roots.Stringize(), "not a finite root set");
            if (finite.Count == 0)
                return new Result(problem, Verdict.Unsolved, roots.Stringize(), "no roots found");

            var checkedRoots = 0;
            foreach (var root in finite)
            {
                // A root may carry a parameter -- sin(x) = 0 is solved by 2*pi*n, for every
                // integer n -- and such a root is right rather than unverifiable. Each parameter
                // is given a few integer values and the root is checked at each of them, which
                // also checks that the parameterisation itself is right and not only one branch
                // of it.
                foreach (var instance in Instantiate(root))
                {
                    if (!TryEvaluate(input.Substitute("x", instance), out var residual))
                        return new Result(problem, Verdict.Unsolved, roots.Stringize(),
                            $"root {instance.Stringize()} could not be checked");
                    if (residual.Magnitude > Tolerance)
                        return new Result(problem, Verdict.Wrong, roots.Stringize(),
                            $"root {instance.Stringize()} leaves {residual.Magnitude:0.###e+0}");
                    checkedRoots++;
                }
            }
            return new Result(problem, Verdict.Solved, roots.Stringize(), $"{checkedRoots} root(s) verified");
        }

        /// <summary>
        /// An antiderivative, checked by differentiating it back.
        /// </summary>
        /// <remarks>
        /// Sampled on the <b>positive reals</b> rather than off the axis, unlike everything else
        /// here, because the antiderivatives this library returns are the real ones: the integral
        /// of <c>1/x</c> comes back as <c>ln(abs(x)) + C</c>, and <c>abs</c> is not holomorphic,
        /// so differentiating that does not reproduce <c>1/x</c> away from the real line. Whether
        /// <c>ln(x)</c> would be the better answer is a real question and not one a regression
        /// gate should decide by failing every build until someone settles it.
        /// </remarks>
        private static Result CheckIntegrate(Problem problem)
        {
            var input = problem.Input.ToEntity();
            var integral = input.Integrate("x");
            if (integral.Nodes.Any(node => node is Entity.Integralf))
                return new Result(problem, Verdict.Unsolved, integral.Stringize(), "integral remains");
            var back = integral.Differentiate("x");
            return AgreeAtSamples(input, back, RealSamples, out var note)
                ? new Result(problem, Verdict.Solved, integral.Stringize(), "differentiates back")
                : new Result(problem, Verdict.Wrong, integral.Stringize(), note);
        }

        private static Result CheckLimit(Problem problem)
        {
            var input = problem.Input.ToEntity();
            var limit = input.Limit("x", problem.Approach!.ToEntity());
            if (limit.Nodes.Any(node => node is Entity.Limitf))
                return new Result(problem, Verdict.Unsolved, limit.Stringize(), "limit remains");

            var expected = problem.Expected!.ToEntity();
            if (limit.Simplify() == expected.Simplify())
                return new Result(problem, Verdict.Solved, limit.Stringize(), $"reached {problem.Expected}");
            // Infinities do not compare numerically, so a difference of forms is all there is.
            if (!TryEvaluate(limit, out var got) || !TryEvaluate(expected, out var want))
                return new Result(problem, Verdict.Wrong, limit.Stringize(), $"expected {problem.Expected}");
            return (got - want).Magnitude <= Tolerance
                ? new Result(problem, Verdict.Solved, limit.Stringize(), $"reached {problem.Expected}")
                : new Result(problem, Verdict.Wrong, limit.Stringize(), $"expected {problem.Expected}");
        }

        /// <summary>
        /// Whether two expressions take the same value at the sample points, over the variables
        /// they mention.
        /// </summary>
        /// <remarks>
        /// A point where <b>both</b> are undefined proves nothing and is skipped; a point where
        /// one has a value and the other does not is a disagreement, which is how a rewrite that
        /// quietly widens or narrows a domain gets caught.
        /// </remarks>
        private static bool AgreeAtSamples(Entity left, Entity right, out string note)
            => AgreeAtSamples(left, right, Samples, out note);

        private static bool AgreeAtSamples(
            Entity left, Entity right, System.Numerics.Complex[] points, out string note)
        {
            var variables = left.Vars.Concat(right.Vars).Distinct().ToList();
            var compared = 0;
            for (var i = 0; i < points.Length; i++)
            {
                Entity l = left, r = right;
                foreach (var (variable, index) in variables.Select((v, n) => (v, n)))
                {
                    var point = points[(i + index) % points.Length];
                    l = l.Substitute(variable, point);
                    r = r.Substitute(variable, point);
                }
                var okL = TryEvaluate(l, out var valueL);
                var okR = TryEvaluate(r, out var valueR);
                if (!okL && !okR) continue;
                if (okL != okR)
                {
                    note = $"one side is undefined at sample {i}";
                    return false;
                }
                compared++;
                var scale = Math.Max(1, Math.Max(valueL.Magnitude, valueR.Magnitude));
                if ((valueL - valueR).Magnitude / scale > Tolerance)
                {
                    note = $"differ at sample {i} by {(valueL - valueR).Magnitude:0.###e+0}";
                    return false;
                }
            }
            note = compared == 0 ? "undefined at every sample" : $"agree at {compared} point(s)";
            return true;
        }

        private static bool TryEvaluate(Entity expr, out System.Numerics.Complex value)
        {
            value = default;
            try
            {
                if (!expr.EvaluableNumerical) return false;
                var evaluated = expr.EvalNumerical();
                value = (System.Numerics.Complex)evaluated;
                return !double.IsNaN(value.Real) && !double.IsNaN(value.Imaginary);
            }
            catch (AngouriMathBaseException)
            {
                return false;
            }
        }

        private static string Short(Exception e)
        {
            var text = e.Message.Replace('\n', ' ');
            return e.GetType().Name + ": " + (text.Length > 90 ? text[..90] : text);
        }

        private static string Report(IReadOnlyList<Result> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var group in results.GroupBy(r => r.Problem.Op))
            {
                sb.AppendLine($"{group.Key}:");
                foreach (var r in group)
                    sb.AppendLine($"  {r.Verdict,-8} {r.Problem.Name,-26} {r.Note}");
            }
            sb.AppendLine();
            foreach (var verdict in Enum.GetValues(typeof(Verdict)).Cast<Verdict>())
                sb.Append($"{verdict}={results.Count(r => r.Verdict == verdict)}  ");
            sb.AppendLine($"of {results.Count}");
            return sb.ToString();
        }
    }
}
