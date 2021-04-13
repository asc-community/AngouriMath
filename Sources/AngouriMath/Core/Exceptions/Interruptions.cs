using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    internal sealed class OutOfQuotaException : Exception
    {
        [ConstantField] internal static OutOfQuotaException Instance = new();
    }
}
