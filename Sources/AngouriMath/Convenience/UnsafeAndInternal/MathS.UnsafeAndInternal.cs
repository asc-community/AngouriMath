/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Convenience;
using AngouriMath.Core;
using AngouriMath.Functions;
using System;

namespace AngouriMath
{
    partial class MathS
    {
        /// <summary>
        /// You may need it to manually manage some issues.
        /// Although those functions might be used inside the library
        /// only, the user may want to use them for some reason
        /// </summary>
        public static class UnsafeAndInternal
        {
            /// <summary>
            /// When you implicitly convert string to an Entity,
            /// it caches the result by the string's reference.
            /// If very strict about RAM usage, you can manually
            /// clean it (or use <see cref="MathS.FromString(string, bool)"/>
            /// instead and set the flag useCache to false)
            /// </summary>
            public static void ClearFromStringCache()
                => stringToEntityCache = new();

            /// <summary>
            /// Checks if two expressions are equivalent if 
            /// <see cref="Entity.Simplify"/> does not give the
            /// expected response
            /// </summary>
            public static bool AreEqualNumerically(Entity expr1, Entity expr2, Entity[] checkPoints)
                => ExpressionNumerical.AreEqual(expr1, expr2, checkPoints);

            /// <summary>
            /// Checks if two expressions are equivalent if 
            /// <see cref="Entity.Simplify"/> does not give the
            /// expected response
            /// </summary>
            public static bool AreEqualNumerically(Entity expr1, Entity expr2)
                => ExpressionNumerical.AreEqual(expr1, expr2);

            /// <summary>
            /// Checkpoints for numerical equality check
            /// </summary>
            public static Setting<Entity[]> CheckPoints => checkPoints ??= new Entity[]
            {
                -100, -10, 1, 10, 100, 1.5,
                "-100 + i", "-10 + 2i", "30i"
            };
            [ConstantField] private static Setting<Entity[]>? checkPoints;

            /// <summary>
            /// This attribute is applied to methods which are exported natively.
            /// </summary>
            [AttributeUsage(AttributeTargets.Method)]
            public sealed class NativeExportAttribute : Attribute
            {
                internal NativeExportAttribute() { }
            }
        }
    }
}
