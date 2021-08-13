open System
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.PreRunCode
open AngouriMath.Terminal.Lib.Consts
open UserInterface
open Spectre.Console
open AngouriMath.Terminal.Lib.AssemblyLoadBuilder

Console.WindowHeight <- 50
Console.WindowWidth <- 150

let lineEditor = getLineEditor AnsiConsole.Console

let rec readAndRespond kernel =
    printf "\n"
    match readLine lineEditor |> execute kernel with
    | PlainTextSuccess text ->
        writeLine AnsiConsole.Console text
    | LatexSuccess (_, text) ->
        writeLine AnsiConsole.Console text
    | Error message ->
        writeLineError AnsiConsole.Console message
    | _ -> ()

    readAndRespond kernel

let handleErrors errors =
    let concat = String.concat "\n"
    printfn $"Errors: {concat errors}"
    printfn $"Report about it to the official repo. The terminal will be closed."
    Console.ReadLine() |> ignore

"\n\n" |> Console.Write

FigletText "AngouriMath"
|> AlignableExtensions.Centered
|> (fun p -> FigletTextExtensions.Color(p, Color.Pink1))
|> AnsiConsole.Console.Write

$@"
Hi! Type `help ()` to get more info.
" |> Markup
  |> AlignableExtensions.Centered
  |> AnsiConsole.Console.Write




printf "Starting the kernel..."


match createKernel () with
| Failure reasons -> handleErrors reasons
| Success kernel ->
    execute kernel "1 + 1" |> ignore  // warm up
    match enableAngouriMath kernel with
    | Error msg -> handleErrors [ msg ]
    | _ -> 
        printfn " loaded."
        readAndRespond kernel