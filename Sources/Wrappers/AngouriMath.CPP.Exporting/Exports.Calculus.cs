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

namespace AngouriMath.CPP.Exporting
{
    using EntityRef = System.UInt64;

    internal static class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "parse")]
        public static EntityRef Parse(IntPtr strPtr)
        {
            var str = Marshal.PtrToStringAnsi(strPtr);
            return ExposedObjects<Entity>.Alloc(str);
        }

        [UnmanagedCallersOnly(EntryPoint = "entity_to_string")]
        public static IntPtr EntityToString(EntityRef exprPtr)
        {
            var expr = ExposedObjects<Entity>.Get(exprPtr);
            var strPtr = Marshal.StringToHGlobalAnsi(expr.ToString());
            return strPtr;
        }

        [UnmanagedCallersOnly(EntryPoint = "free_entity")]
        public static void Free(EntityRef handle)
        {
            ExposedObjects<Entity>.Dealloc(handle);
        }

        [UnmanagedCallersOnly(EntryPoint = "add")]
        public static int Add(int a, int b)
        {
            return a + b;
        }

        [UnmanagedCallersOnly(EntryPoint = "diff")]
        public static EntityRef Differentiate(EntityRef exprPtr, EntityRef varPtr)
        {
            var expr = ExposedObjects<Entity>.Get(exprPtr);
            var varRaw = (Variable)ExposedObjects<Entity>.Get(varPtr);
            return ExposedObjects<Entity>.Alloc(expr.Differentiate(varRaw));
        }


    }
}