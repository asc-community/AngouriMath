using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    public sealed class OutOfQuotaInterruption : Exception
    {
        [ConstantField] internal static OutOfQuotaInterruption Instance = new();
    }
}
