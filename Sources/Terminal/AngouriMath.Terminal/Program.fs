open System
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.Consts
open UserInterface

let rec readAndRespond kernel =
    match readLine () |> execute kernel with
    | PlainTextSuccess text ->
        writeLine text
    | LatexSuccess (_, text) ->
        writeLine text
    | Error message ->
        writeLineError message
    | _ -> ()

    readAndRespond kernel

let handleError error =
    printfn $"Error: {error}"
    printfn $"Report about it to the official repo. The terminal will be closed."
    Console.ReadLine() |> ignore

$@"
══════════════════════════════════════════════════════════════════════
                Welcome to AngouriMath.Terminal.

It is an interface to AngouriMath, open source symbolic algebra
library. The terminal uses F# Interactive inside, so that you can
run any command you could in normal F#. AngouriMath.FSharp is
being installed every start, so you are guaranteed to be on the
latest version of it. Type 'preRunCode' to see, what code
was pre-ran before you were able to type.
══════════════════════════════════════════════════════════════════════
".Trim() |> printfn "%s"


printfn "Starting the kernel..."


match createKernel () with
| Result.Error error -> handleError error
| Result.Ok kernel ->
    execute kernel "1 + 1" |> ignore  // warm up
    let innerCode = OpensAndOperators.Replace("\"", "\\\"")
    let preRunCode = OpensAndOperators + $"let preRunCode = \"{innerCode}\""
    match execute kernel preRunCode with
    | Error msg -> handleError msg
    | _ ->
        readAndRespond kernel