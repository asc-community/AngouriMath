//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
