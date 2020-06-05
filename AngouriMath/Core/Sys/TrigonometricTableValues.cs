using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Core.Numerix;

namespace AngouriMath
{
    using TrigTable = List<(decimal arg, Entity res)>;
    internal static partial class Const
    {
        internal static class TrigonometryTableValues
        {
            internal static bool PullFromTable(TrigTable table, ComplexNumber arg, out Entity res)
            {
                if (arg.IsImaginary())
                {
                    res = null;
                    return false;
                }
                // arg in [0; 2pi]
                var dArg = arg.Real.Value;
                dArg = (dArg % (2 * MathS.DecimalConst.pi) + 2 * MathS.DecimalConst.pi) % (2 * MathS.DecimalConst.pi);

                int begin = 0;
                int end = table.Count - 1;
                while (end - begin > 1)
                {
                    var mid = (end + begin) / 2;
                    if (table[mid].arg > dArg)
                        begin = mid;
                    else
                        end = mid;
                    if (end >= table.Count)
                    {
                        res = null;
                        return false;
                    }
                }

                for(var j = begin; j <= end; j++)
                {
                    if (Number.Functional.IsZero(table[j].arg - dArg))
                    {
                        res = table[j].res;
                        return true;
                    }
                }
                res = null;
                return false;
            }

            private static Entity Cubt(Entity a)
                => MathS.Pow(a, Number.CreateRational(1, 3));

            private static Entity Sqrt(Entity a)
                => MathS.Pow(a, Number.CreateRational(1, 2));

            private static Entity f1_2 = Number.CreateRational(1, 2);
            private static Entity f1_3 = Number.CreateRational(1, 3);
            private static Entity f1_4 = Number.CreateRational(1, 4);
            private static Entity f1_8 = Number.CreateRational(1, 8);
            private static Entity f1_24 = Number.CreateRational(1, 24);
            private static Entity i = MathS.i;

            private static decimal PiOver(int a)
                => 2 * MathS.DecimalConst.pi / a;

            private static Entity f1_6 = Number.CreateRational(1, 6);

            /// <summary>
            /// Credit: https://en.wikipedia.org/wiki/Trigonometric_constants_expressed_in_real_radicals
            /// Although some formulas have been changed because they are wrong on Wikipedia
            /// </summary>
            internal static TrigTable TableSin = new TrigTable
            {
                (PiOver(1), 0),
                (PiOver(2), 0),
                (PiOver(3), f1_2 * Sqrt(3)),
                (PiOver(4), 1),
                (PiOver(5), f1_4 * Sqrt(10 + 2 * Sqrt(5))),
                (PiOver(6), f1_2 * Sqrt(3)),
                (PiOver(7), Sqrt(1 - MathS.Pow(f1_6 * (-1 + Cubt((7 + 21 * Sqrt(-3)) / 2) + Cubt((7 - 21 * Sqrt(-3)) / 2)), 2))),
                (PiOver(8), f1_2 * Sqrt(2)),
                (PiOver(9), MathS.i / 2 * (Cubt((-1 - Sqrt(-3)) / 2) - Cubt((-1 + Sqrt(-3)) / 2))),
                (PiOver(10), f1_4 * Sqrt(10 - 2 * Sqrt(5))),
                (PiOver(12), f1_2),
                (PiOver(14), f1_24 * Sqrt(3 * 
                                          (112 - Cubt(14336 + Sqrt(-5549064193)) - Cubt(14336 - Sqrt(-5549064193))
                                          ))),
                (PiOver(15), f1_8 * (Sqrt(15) + Sqrt(3) - Sqrt(10 - 2 * Sqrt(5)))),
                (PiOver(16), f1_2 * Sqrt(2 - Sqrt(2))),
                (PiOver(17), f1_4 * Sqrt(8 - Sqrt(2 * (
                                                  15 + Sqrt(17) + Sqrt(34 - 2 * Sqrt(17)) - 2 * Sqrt(
                                                      17 + 3 * Sqrt(17) - Sqrt(170 + 38 * Sqrt(17))
                                                      )
                                                  )))),
                (PiOver(18), i * f1_4 * (Cubt(4 - 4 * Sqrt(-3)) - Cubt(4 + 4 * Sqrt(-3)))),
                (PiOver(20), f1_4 * (Sqrt(5) - 1)),
                (PiOver(24), f1_4 * (Sqrt(6) - Sqrt(2)))
            };

            private static Entity f1_16 = Number.CreateRational(1, 16);

            internal static TrigTable TableCos = new TrigTable
            {
                (PiOver(1), 1),
                (PiOver(2), -1),
                (PiOver(3), -f1_2),
                (PiOver(4), 0),
                (PiOver(5), f1_4 * (Sqrt(5) - 1)),
                (PiOver(6), f1_2),
                (PiOver(7), f1_6 * (-1 + Cubt((7 + 21 * Sqrt(-3)) / 2) + Cubt((7 - 21 * Sqrt(-3)) / 2))),
                (PiOver(8), f1_2 * Sqrt(2)),
                (PiOver(9), f1_2 * (Cubt((-1 + Sqrt(-3)) / 2) + Cubt((-1 - Sqrt(-3)) / 2))),
                (PiOver(10), f1_4 * (Sqrt(5) + 1)),
                (PiOver(12), f1_2 * Sqrt(3)),
                (PiOver(14), f1_24 * Sqrt(3 *
                                          (80 + Cubt(14336 + Sqrt(-5549064193)) + Cubt(14336 - Sqrt(-5549064193))
                                          ))),
                (PiOver(15), f1_8 * (1 + Sqrt(5) + Sqrt(30 - 6 * Sqrt(5)))),
                (PiOver(16), f1_2 * Sqrt(2 + Sqrt(2))),
                (PiOver(17), f1_16 * (-1 + Sqrt(17) + Sqrt(34 - 2 * Sqrt(17)) + 2 * Sqrt(
                    17 + 3 * Sqrt(17) - Sqrt(34 - 2 * Sqrt(17)) - 2 * Sqrt(34 + 2 * Sqrt(17))
                    ))),
                (PiOver(18), f1_4 * (Cubt(4 + 4 * Sqrt(-3)) + Cubt(4 - 4 * Sqrt(-3)))),
                (PiOver(20), f1_4 * Sqrt(10 + 2 * Sqrt(5))),
                (PiOver(24), f1_4 * (Sqrt(6) + Sqrt(2)))
            };

            internal static TrigTable TableTan = new TrigTable
            {
                (PiOver(1), 0),
                (PiOver(2), 0),
                (PiOver(3), -Sqrt(3)),
                (PiOver(4), Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY)),
                (PiOver(5), Sqrt(5 + 2 * Sqrt(5))),
                (PiOver(6), Sqrt(3)),
                (PiOver(8), 1),
                (PiOver(10), Sqrt(5 - 2 * Sqrt(5))),
                (PiOver(12), f1_3 * Sqrt(3)),
                (PiOver(14), Sqrt(
                    (112 - Cubt(14336 + Sqrt(-5549064193)) - Cubt(14336 - Sqrt(-5549064193)))
                    /
                    (80 + Cubt(14336 + Sqrt(-5549064193)) + Cubt(14336 - Sqrt(-5549064193)))
                    )),
                (PiOver(15), f1_2 * (-3 * Sqrt(3) - Sqrt(15) + Sqrt(50 + 22 * Sqrt(5)))),
                (PiOver(16), Sqrt(2) - 1),
                (PiOver(20), Number.CreateRational(1, 5) * Sqrt(25 - 10 * Sqrt(5))),
                (PiOver(24), 2 - Sqrt(3))
            };
        }
    }
}
