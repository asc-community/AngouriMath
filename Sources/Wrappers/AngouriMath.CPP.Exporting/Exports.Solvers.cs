using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "entity_solve")]
        public static NErrorCode Solve(EntityRef exprPtr, EntityRef varPtr, ref EntityRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.Solve(var));
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_solve_equation")]
        public static NErrorCode SolveEquation(EntityRef exprPtr, EntityRef varPtr, ref EntityRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.SolveEquation(var));
            });
    }
}
