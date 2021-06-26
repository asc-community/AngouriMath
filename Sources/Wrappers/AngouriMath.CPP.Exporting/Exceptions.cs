using System;

namespace AngouriMath.CPP.Exporting
{
    public abstract class ObjectStorageException : Exception { }

    public sealed class DeallocationException : ObjectStorageException
    {

    }
    public sealed class AllocationException : ObjectStorageException
    {
    }

    public sealed class NonExistentObjectAddressingException : ObjectStorageException
    {
    }
}
