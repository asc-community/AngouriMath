/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

/*
 *
 * Great thanks to Andrey Kurdyumov for help! 
 *
 */

using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using GenericTensor.Core;
using GenericTensor.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AngouriMath.Entity;

namespace AngouriMath
{
    public static class Exports
    {
        private static class ExposedObjects<T>
        {
            private static ulong lastId = 0;
            private readonly static Dictionary<ulong, T> allocations = new();
            internal static ulong Alloc(T obj)
            {
                lastId++;
                allocations[lastId] = obj;
                return lastId;
            }
            internal static void Dealloc(ulong ptr)
                => allocations.Remove(ptr);
            internal static T Get(ulong ptr)
                => allocations[ptr];
        }

        [UnmanagedCallersOnly(EntryPoint = "parse")]
        public static ulong Parse(IntPtr strPtr)
        {
            return 0;
            var str = Marshal.PtrToStringAnsi(strPtr);
            return ExposedObjects<Entity>.Alloc(str);
        }

        [UnmanagedCallersOnly(EntryPoint = "free")]
        public static void Free(ulong handle)
        {
            ExposedObjects<Entity>.Dealloc(handle);
        }

        [UnmanagedCallersOnly(EntryPoint = "add")]
        public static int Add(int a, int b)
        {
            return a + b;
        }

        [UnmanagedCallersOnly(EntryPoint = "diff")]
        public static IntPtr Differentiate(IntPtr exprPtr, IntPtr varPtr)
        {
            // Parse strings from the passed pointers 
            var exprRaw = Marshal.PtrToStringAnsi(exprPtr);
            var varRaw = Marshal.PtrToStringAnsi(varPtr);

            Entity expr = exprRaw;
            Entity.Variable var = varRaw;

            var diffed = expr.Differentiate(var);

            var resRaw = diffed.ToString();

            var resPtr = Marshal.StringToHGlobalAnsi(resRaw);

            return resPtr;
        }
        /*
        [UnmanagedCallersOnly(EntryPoint = "scalar")]
        public static (int, int) ScalarProduct(int a1, int a2, int b1, int b2)
        {
            var v1 = GenTensor<int, IntWrapper>.CreateVector(a1, a2);
            var v2 = GenTensor<int, IntWrapper>.CreateVector(b1, b2);
            var v3 = GenTensor<int, IntWrapper>.PiecewiseMultiply(v1, v2);

            var c1 = v3[0];
            var c2 = v3[1];

            return (c1, c2);
        }*/
    }
}