//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The answers already found are kept per thread across questions, and a rational function whose
    /// residues want a sum over roots is declined in the first pass and answered in the second. The
    /// first pass's declines were kept as declines, so a later question that asked one of them again
    /// was declined at once: the memo answered for the rule, the rule never said that a second pass
    /// would answer, and none ran. <c>(1 + (1 + sqrt(3)) x^4)/(1 - x^4 + x^8)</c> was declined after
    /// <c>(1 + x^4)/(1 - x^4 + x^8)</c>, and answered alone -- Rubi's 1.2.3.3:53, declined in the
    /// harness's sample and solved when run on its own.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ADeclineASumOverRootsWouldAnswerIsRememberedAsOneTest
    {
        [Fact]
        public void IsAnsweredAfterTheQuestionThatAskedItsPartsFirst()
        {
            const string first = "(1 + x^4)/(1 - x^4 + x^8)";
            const string then = "(1 + (1 + sqrt(3))*x^4)/(1 - x^4 + x^8)";
            Entity? answer = null;
            // A thread of its own, so that what is remembered is what these two leave and nothing a
            // test before them did.
            var task = Task.Factory.StartNew(() =>
            {
                Assert.DoesNotContain("integral(", first.ToEntity().Integrate("x").Stringize());
                answer = then.ToEntity().Integrate("x");
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(TimeSpan.FromMinutes(2)), "the two did not finish within two minutes");
            task.GetAwaiter().GetResult();
            Assert.DoesNotContain("integral(", answer!.Stringize());
            var derivative = answer.Substitute("C", 0).Differentiate("x");
            var original = then.ToEntity();
            foreach (var at in new[] { -1.7, -0.6, 0.3, 0.9, 1.4 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {then} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
