//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;

Console.WriteLine("a / b + b / c".ToEntity().SimplifiedRate);
Console.WriteLine("a / b + b / c".ToEntity().Simplify().SimplifiedRate);


using var _ = Settings.ComplexityCriteria.Set(
    expr => expr.Nodes.Count(node => node is Entity.Divf)
);

Console.WriteLine(FromString("a / b + b / c", useCache: false).SimplifiedRate);
Console.WriteLine(FromString("a / b + b / c", useCache: false).Simplify().SimplifiedRate);