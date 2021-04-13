using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Convenience;
using AngouriMath.Core.Exceptions;

namespace AngouriMath.Core.QuotaGC
{
    internal sealed class QuotaLeft
    {
        private int left;
        private bool infinite;

        internal static QuotaLeft CreateFinite(int quotaInitial)
            => new() { left = quotaInitial, infinite = false };

        internal static QuotaLeft CreateInfinite()
            => new() { left = 0, infinite = true };

        internal void DecreaseAndCheck()
        {
            if (infinite)
                return;
            left--;
            if (left is 0)
                throw new OutOfQuotaException();
        }
    }

    internal static class QuotaCounter
    {
        internal static Setting<QuotaLeft> QuotaLeft => quotaLeft ??= new(QuotaLeft.CreateInfinite());
        [ThreadStatic] internal static Setting<QuotaLeft>? quotaLeft;
    }
}
