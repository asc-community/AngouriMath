using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    public sealed class OutOfQuotaException : Exception
    {
        [ConstantField] internal static OutOfQuotaException Instance = new();
    }
}
