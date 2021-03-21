using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "parse")]
        public static NErrorCode Parse(IntPtr strPtr, ref EntityRef res)
        {
            try
            {
                var str = Marshal.PtrToStringAnsi(strPtr);
                res = ExposedObjects<Entity>.Alloc(str);
                return NErrorCode.Ok;
            }
            catch (Exception e)
            {
                return NErrorCode.Thrown(e);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "entity_to_string")]
        public static NErrorCode EntityToString(EntityRef exprPtr, ref IntPtr res)
        {
            try
            {
                var expr = ExposedObjects<Entity>.Get(exprPtr);
                res = Marshal.StringToHGlobalAnsi(expr.ToString());
                return NErrorCode.Ok;
            }
            catch (Exception e)
            {
                return NErrorCode.Thrown(e);
            }
        }
    }
    }
