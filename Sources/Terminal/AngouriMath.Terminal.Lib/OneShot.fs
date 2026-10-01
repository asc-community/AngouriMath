/// <summary>
/// The commands that answer once and exit, so that <c>amcli</c> can sit in a pipe or a script:
/// <c>amcli simp "sin(x)^2 + cos(x)^2"</c> prints <c>1</c>. With no arguments, <c>amcli</c> opens
/// the terminal instead.
/// </summary>
/// <remarks>
/// These are the commands of the command-line tool that lived in asc-community/AngouriMathCLI,
/// with its names, its argument order and its reading of stdin, so that a script written for it
/// keeps working. Each is one call to the library, and none of them starts the F# kernel, which
/// is what makes the terminal take seconds to open.
/// https://github.com/asc-community/AngouriMath/issues/1649
/// </remarks>
module AngouriMath.Terminal.Lib.OneShot

open System
open System.IO
open AngouriMath

/// The usage text, printed by <c>amcli help</c>.
let usage =
    """amcli                        opens the terminal
amcli eval EXPR              evaluates to a number, a boolean, or a + bi
amcli simp EXPR              simplifies
amcli fsimp EXPR             simplifies quickly: the normalisation, without the search
amcli diff VAR EXPR          differentiates over VAR
amcli solve VAR STATEMENT    solves over VAR, one root per line when there are finitely many
amcli sub VAR VALUE EXPR     substitutes VALUE for VAR
amcli latex EXPR             writes EXPR as LaTeX
amcli help                   prints this

An argument written as _ is read from stdin, and so is one that is left out:
    echo "1 + x^2" | amcli diff x
"""

/// The names <c>run</c> answers. Anything else as a first argument is a usage error.
let commands = set [ "eval"; "simp"; "fsimp"; "diff"; "solve"; "sub"; "latex"; "help"; "-h"; "--help" ]

/// The arguments still to read, falling back to a line of stdin where one is missing or is <c>_</c>.
type private Arguments(given: string list, input: TextReader) =
    let mutable rest = given

    /// The next argument, or None where it is missing and stdin has ended.
    member _.Next() : string option =
        match rest with
        | head :: tail ->
            rest <- tail
            if head = "_" then Option.ofObj (input.ReadLine()) else Some head
        | [] -> Option.ofObj (input.ReadLine())

/// What a command printed, one line each.
let private answer (command: string) (next: unit -> string option) : Result<string list, string> =
    let expression () = next () |> Option.map MathS.FromString
    let variable () = next () |> Option.map MathS.Var
    let missing = Error $"amcli {command} needs more arguments. Run amcli help for what each command takes."
    match command with
    | "eval" ->
        match expression () with
        | Some e ->
            match e.Evaled with
            | :? Entity.Number.Rational as r -> Ok [ r.EDecimal.ToString() ]
            | value -> Ok [ value.Stringize() ]
        | None -> missing
    | "simp" ->
        match expression () with
        | Some e -> Ok [ e.Simplify().Stringize() ]
        | None -> missing
    | "fsimp" ->
        match expression () with
        | Some e -> Ok [ e.InnerSimplified.Stringize() ]
        | None -> missing
    | "diff" ->
        match variable () with
        | Some v ->
            match expression () with
            | Some e -> Ok [ e.Differentiate(v).Stringize() ]
            | None -> missing
        | None -> missing
    | "solve" ->
        match variable () with
        | Some v ->
            match expression () with
            | Some e ->
                match e.Solve(v) with
                | :? Entity.Set.FiniteSet as roots -> Ok [ for root in roots -> root.Stringize() ]
                | roots -> Ok [ roots.Stringize() ]
            | None -> missing
        | None -> missing
    | "sub" ->
        match variable () with
        | Some v ->
            match expression () with
            | Some value ->
                match expression () with
                | Some e -> Ok [ e.Substitute(v, value).Stringize() ]
                | None -> missing
            | None -> missing
        | None -> missing
    | "latex" ->
        match expression () with
        | Some e -> Ok [ e.Latexize() ]
        | None -> missing
    | _ -> Ok (usage.TrimEnd().Split('\n') |> List.ofArray)

/// <summary>
/// Runs one command and returns the process's exit code: 0 for an answer, 1 when the input did
/// not parse or the library could not answer it, 2 for a command that does not exist or is
/// missing an argument.
/// </summary>
/// <param name="args">The command line after the program's own name, which must not be empty.</param>
let run (args: string list) (input: TextReader) (output: TextWriter) (error: TextWriter) : int =
    match args with
    | command :: rest when commands.Contains command ->
        let arguments = Arguments(rest, input)
        try
            match answer command arguments.Next with
            | Ok lines ->
                for line in lines do output.WriteLine line
                0
            | Error message ->
                error.WriteLine message
                2
        with ex ->
            error.WriteLine ex.Message
            1
    | command :: _ ->
        error.WriteLine $"amcli has no command {command}. Run amcli help for the list, or amcli with no arguments for the terminal."
        2
    | [] ->
        error.WriteLine usage
        2
