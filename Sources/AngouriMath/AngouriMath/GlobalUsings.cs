//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

global using System.Collections.Generic;
global using System.Linq;
global using HonkSharp.Fluency;
global using HonkSharp.Functional;
global using AngouriMath.Core;
global using AngouriMath.Functions;


global using ReasonWhyParsingFailed =
    HonkSharp.Functional.Either<
        AngouriMath.Core.ReasonOfFailureWhileParsing.Unknown,
        AngouriMath.Core.ReasonOfFailureWhileParsing.MissingOperator,
        AngouriMath.Core.ReasonOfFailureWhileParsing.InternalError
      >;

global using ParsingResult =
    HonkSharp.Functional.Either<
        AngouriMath.Entity,
        HonkSharp.Functional.Failure<
            HonkSharp.Functional.Either<
                AngouriMath.Core.ReasonOfFailureWhileParsing.Unknown,
                AngouriMath.Core.ReasonOfFailureWhileParsing.MissingOperator,
                AngouriMath.Core.ReasonOfFailureWhileParsing.InternalError
            >
        >
    >;

global using static AngouriMath.Entity.Number;
