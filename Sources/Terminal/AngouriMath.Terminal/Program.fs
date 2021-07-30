open System
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.PreRunCode
open UserInterface
open Spectre.Console

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

let handleError error =
    printfn $"Error: {error}"
    printfn $"Report about it to the official repo. The terminal will be closed."
    Console.ReadLine() |> ignore


FigletText "AngouriMath"
|> AlignableExtensions.Centered
|> (fun p -> FigletTextExtensions.Color(p, Color.Pink1))
|> AnsiConsole.Console.Write

$@"
It is an interface to AngouriMath, open source symbolic algebra
library. The terminal uses F# Interactive inside, so that you can
run any command you could in normal F#. Type 'preRunCode' to see, 
what code was pre-ran before you were able to type.
" |> Markup
  |> AlignableExtensions.Centered
  |> AnsiConsole.Console.Write




printf "Starting the kernel..."


match createKernel () with
| Result.Error error -> handleError error
| Result.Ok kernel ->
    execute kernel "1 + 1" |> ignore  // warm up
    match enableAngouriMath kernel with
    | Error msg -> handleError msg
    | _ -> 
        printfn " loaded."
        readAndRespond kernel