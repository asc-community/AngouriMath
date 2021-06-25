global using HonkSharp.Fluency;
global using HonkSharp.Functional;

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