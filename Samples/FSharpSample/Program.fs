open AngouriMath
open AngouriMath.Extensions

[<EntryPoint>]
let main argv =
    let expr = MathS.FromString("x = 2")
    printfn $"""2 + 3 is {"2 + 3".EvalNumerical().ToString()}""";
    printfn $"""{"(x + 1)(x + 3) = 0 and (x - 1)(x + 3) = 0".Solve(MathS.Var("x"))}"""
    0