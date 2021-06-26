using System;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "maths_from_string")]
        public static NErrorCode Parse(IntPtr strPtr, ref ObjRef res)
            => ExceptionEncode(ref res, strPtr, static strPtr =>
            {
                var str = Marshal.PtrToStringAnsi(strPtr);
                return ObjStorage<Entity>.Alloc(str);
            });

        [UnmanagedCallersOnly(EntryPoint = "matrix_from_vector_of_vectors")]
        public static NErrorCode MatrixFromVectorOfVectors(NativeArray arr, ref ObjRef res)
            => ExceptionEncode(ref res, arr, static arr =>
            {
                // TODO:
                throw new InvalidOperationException("This method should be implemented after native-aot-attempt-2 is merged into master");
            });

        [UnmanagedCallersOnly(EntryPoint = "matrix_transpose")]
        public static NErrorCode MatrixTranspose(ObjRef m, ref ObjRef res)
            => ExceptionEncode(ref res, m, static m =>
            {
                // TODO:
                throw new InvalidOperationException("This method should be implemented after native-aot-attempt-2 is merged into master");
            });

        [UnmanagedCallersOnly(EntryPoint = "finite_set_to_vector")]
        public static NErrorCode FiniteSetToVector(ObjRef m, ref NativeArray res)
            => ExceptionEncode(ref res, m, static m 
                => NativeArray.Alloc((Entity.Set.FiniteSet)m.AsEntity)
            );
    }
}
