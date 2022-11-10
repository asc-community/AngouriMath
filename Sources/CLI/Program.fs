open System.CommandLine
open System.Collections.Generic
open System.IO
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.Matrices
open AngouriMath.FSharp.Shortcuts
open AngouriMath.FSharp.Constants
open AngouriMath

[<Verb("eval", HelpText = "Evaluate an expression and print a single number or boolean")>]
type EvalOptions = {
    [<Option('e', "expression", Required = true, HelpText = "Expression")>]
    expr : string;
}

let args = System.Environment.GetCommandLineArgs()[1..]

/// let expr = Option<string>(
///     name: "--expr",
///     description: "Supplied expression"
/// )

let eval (x : obj) : string = 
    match (parsed x).InnerSimplified with
    | :? Entity.Number.Integer as i -> i.ToString()
    | :? Entity.Number.Rational as i -> i.RealPart.EDecimal.ToString()
    | :? Entity.Number.Real as re -> re.RealPart.EDecimal.ToString()
    | :? Entity.Number.Complex as cx -> cx.RealPart.EDecimal.ToString() + " + " + cx.ImaginaryPart.EDecimal.ToString() + "i"
    | other -> (evaled other).ToString()

match CommandLine.Parser.Default.ParseArguments<EvalOptions> args with
| :? CommandLine.Parsed<EvalOptions> as evalOpt -> 
    evalOpt.Value.expr
    |> eval
    |> printf "%s\n"
    0
| _ -> 1
|> exit
