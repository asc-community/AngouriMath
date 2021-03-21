/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

using System;
using System.Runtime.InteropServices;
using static AngouriMath.Entity;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "diff")]
        public static NErrorCode Differentiate(EntityRef exprPtr, EntityRef varPtr, ref EntityRef res)
        {
            try
            {
                var expr = ExposedObjects<Entity>.Get(exprPtr);
                var varRaw = (Variable)ExposedObjects<Entity>.Get(varPtr);
                res = ExposedObjects<Entity>.Alloc(expr.Differentiate(varRaw));
                return NErrorCode.Ok;
            }
            catch (Exception e)
            {
                return NErrorCode.Thrown(e);
            }
        }
    }
}