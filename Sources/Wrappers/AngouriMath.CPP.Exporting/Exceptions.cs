using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    public abstract class ObjectStorateException : Exception { }

    public sealed class DeallocationException : ObjectStorateException
    {

    }
    public sealed class AllocationException : ObjectStorateException
    {
    }

    public sealed class NonExistentObjectAddressingException : ObjectStorateException
    {
    }
}
