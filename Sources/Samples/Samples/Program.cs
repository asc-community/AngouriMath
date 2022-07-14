//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using static AngouriMath.MathS.Sets;

var set1 = Finite(1, 2, 3);
var set2 = Finite(2, 3, 4);
var set3 = MathS.Interval(-6, 2);
var set4 = new ConditionalSet("x", "100 > x2 > 81");
Console.WriteLine(Union(set1, set2));
Console.WriteLine(Union(set1, set2).Simplify());
Console.WriteLine("----------------------");
Console.WriteLine(Union(set1, set3));
Console.WriteLine(Union(set1, set3).Simplify());
Console.WriteLine("----------------------");
Console.WriteLine(Union(set1, set4));
Console.WriteLine(ElementInSet(3, Union(set1, set4)));
Console.WriteLine(ElementInSet(3, Union(set1, set4)).Simplify());
Console.WriteLine(ElementInSet(4, Union(set1, set4)));
Console.WriteLine(ElementInSet(4, Union(set1, set4)).Simplify());
Console.WriteLine(ElementInSet(9.5, Union(set1, set4)));
Console.WriteLine(ElementInSet(9.5, Union(set1, set4)).Simplify());
Console.WriteLine("----------------------");
Console.WriteLine(Intersection(set1, set2));
Console.WriteLine(Intersection(set1, set2).Simplify());
Console.WriteLine("----------------------");
Console.WriteLine(Intersection(set2, set3));
Console.WriteLine(Intersection(set2, set3).Simplify());
Console.WriteLine("----------------------");
var set5 = MathS.Interval(-3, 11);
Console.WriteLine(Intersection(set3, set5));
Console.WriteLine(Intersection(set3, set5).Simplify());
Console.WriteLine(Union(set3, set5));
Console.WriteLine(Union(set3, set5).Simplify());
Console.WriteLine(SetSubtraction(set3, set5));
Console.WriteLine(SetSubtraction(set3, set5).Simplify());
Console.WriteLine("----------------------");
Entity syntax1 = @"{ 1, 2, 3 } /\ { 2, 3, 4 }";
Console.WriteLine(syntax1);
Console.WriteLine(syntax1.Simplify());
Console.WriteLine("----------------------");
Entity syntax2 = @"5 in ([1; +oo) \/ { x : x < -4 })";
Console.WriteLine(syntax2);
Console.WriteLine(syntax2.Simplify());
Console.WriteLine("----------------------");
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), Q));
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), Q).Simplify());
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), R));
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), R).Simplify());
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), C));
Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), C).Simplify());