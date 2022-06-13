//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    unsafe partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "maths_from_string")]
        public static NErrorCode Parse(IntPtr strPtr, ObjRef* res)
            => ExceptionEncode(res, strPtr, static strPtr =>
            {
                var str = Marshal.PtrToStringAnsi(strPtr);
                return ObjStorage<Entity>.Alloc(str);
            });

        [UnmanagedCallersOnly(EntryPoint = "matrix_from_vector_of_vectors")]
        public static NErrorCode MatrixFromVectorOfVectors(NativeArray arr, ObjRef* res)
            => ExceptionEncode(res, arr, static arr =>
            {
                // TODO:
                throw new InvalidOperationException("This method should be implemented after native-aot-attempt-2 is merged into master");
            });

        [UnmanagedCallersOnly(EntryPoint = "matrix_transpose")]
        public static NErrorCode MatrixTranspose(ObjRef m, ObjRef* res)
            => ExceptionEncode(res, m, static m =>
            {
                // TODO:
                throw new InvalidOperationException("This method should be implemented after native-aot-attempt-2 is merged into master");
            });

        [UnmanagedCallersOnly(EntryPoint = "finite_set_to_vector")]
        public static NErrorCode FiniteSetToVector(ObjRef m, NativeArray* res)
            => ExceptionEncode(res, m, static m 
                => NativeArray.Alloc((Entity.Set.FiniteSet)m.AsEntity)
            );
    }
}
