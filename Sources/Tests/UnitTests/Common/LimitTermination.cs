//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Tests
{
    /// <summary>
    /// How long a test waits before it is willing to call a limit non-terminating.
    /// </summary>
    /// <remarks>
    /// These guards exist because several limits used to run forever, and a test that hangs is
    /// worse than one that fails. What they are <b>not</b> for is measuring how long a limit
    /// takes: a non-terminating limit exceeds any bound, so the bound only has to be past the
    /// slowest honest answer, and every second below that buys nothing while costing a false
    /// failure whenever the machine is busy.
    ///
    /// The bound used to be 30 seconds, which is not past it on a loaded CI runner. The whole
    /// guarded set — 171 cases across nine classes — runs in about 9 seconds on a developer
    /// machine, yet on 2026-09-01 a macOS runner twice failed
    /// <c>x * ln(x) / cos(x)</c> at 30 seconds on library code that master had already run green
    /// on the same platform, in a suite whose own duration moved between 19 and 26 minutes with
    /// load. Two runs of the same binary disagreeing is what makes it the clock and not the code.
    ///
    /// A genuine regression to non-termination still fails, three minutes later than it used to.
    /// That is the trade this constant makes, and it is the right way round: a real regression is
    /// rare and its slower report costs one CI run, while a false failure blocks every merge that
    /// happens to land on a busy runner.
    ///
    /// Two wall clocks in this suite deliberately do <b>not</b> use it, because for them the bound
    /// is not arbitrary:
    /// <list type="bullet">
    /// <item><c>CorpusGateTest</c>'s budget decides <c>Verdict.Timeout</c>, which is a recorded
    /// verdict the gate compares against expectations — widening it would rewrite results rather
    /// than stabilise them.</item>
    /// <item><c>BooleanMinimisationTest.ParityIsNotSearchedAndTerminatesQuickly</c> asserts that
    /// the engine declines instead of searching. There the tightness <i>is</i> the assertion: at
    /// three minutes it would pass while the engine searched for two.</item>
    /// </list>
    /// </remarks>
    internal static class LimitTermination
    {
        /// <summary>The wall clock a limit is given before a test declares it non-terminating.</summary>
        internal static readonly TimeSpan Guard = TimeSpan.FromSeconds(180);
    }
}
