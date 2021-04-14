using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Convenience;
using AngouriMath.Core.Exceptions;

namespace AngouriMath.Core
{
    public sealed class QuotaLeft
    {
        private int left;
        private int initial;
        private bool infinite;

        public static QuotaLeft CreateFinite(int quotaInitial)
            => new() { left = quotaInitial, initial = quotaInitial, infinite = false };

        public static QuotaLeft CreateInfinite()
            => new() { left = 0, infinite = true };

        public void Reset() => left = initial;

        public void DecreaseAndCheck()
        {
            if (infinite)
                return;
            left--;
            if (left is 0)
                throw OutOfQuotaException.Instance;
        }
    }

    public static class QuotaCounter
    {
        public static Setting<QuotaLeft> QuotaLeft => quotaLeft ??= new(Core.QuotaLeft.CreateInfinite());
        [ThreadStatic] internal static Setting<QuotaLeft>? quotaLeft;
    }
}
