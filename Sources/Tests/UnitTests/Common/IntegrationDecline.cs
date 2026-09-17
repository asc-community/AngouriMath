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
    /// How long a test allows an integral that is declined, or answered after a search that once
    /// ran away, before it calls the decline slow.
    /// </summary>
    /// <remarks>
    /// The four tests that use it each pin a decline that used to take thirty seconds or more
    /// and takes a few now -- <c>atan(x)^2/x^2</c> is 3.4 seconds on a developer machine. Their
    /// bound was 20 seconds, and on 2026-09-17 the same test failed twice on an Ubuntu runner at
    /// 20.7 and 25.7 seconds, in suites that took 13 and 12 minutes where the Windows leg beside
    /// them took 4 and 6: the same binary, seven times slower, which makes it the runner and not
    /// the code. A bound that fails on a busy runner blocks every merge that lands on one.
    ///
    /// Sixty seconds is past the slowest honest decline by a wide margin and still well short of
    /// the runaway each test was written against, so a regression to that still fails; what it
    /// no longer does is fail a loaded runner. As <see cref="LimitTermination"/>: the bound is
    /// there to catch a search that does not stop, not to measure the machine.
    /// </remarks>
    internal static class IntegrationDecline
    {
        /// <summary>The wall clock an integral is given before a test calls its decline slow.</summary>
        internal static readonly TimeSpan Guard = TimeSpan.FromSeconds(60);
    }
}
