module AngouriMath.Terminal.Lib.Say

open AngouriMath.Core
open Consts
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Events
open Microsoft.DotNet.Interactive.FSharp
open Microsoft.DotNet.Interactive.Commands
open System

type ExecutionResult =
    | SuccessPackageAdded
    | Error of string
    | VoidSuccess
    | PlainTextSuccess of string
    | LatexSuccess of Latex : string * Source : string
    | EndOfFile


let options = JsonSerializerOptions ()
JsonFSharpConverter () |> options.Converters.Add


let objectEncode (o : obj) =
    match o with
    | :? ILatexiseable as latexiseable -> 
        let toSerialize = LatexSuccess (latexiseable.Latexise (), o.ToString ())
        EncodingLatexPrefix + JsonSerializer.Serialize(toSerialize, options)
    | _ ->
        let toSerialize = PlainTextSuccess (o.ToString ())
        EncodingPlainPrefix + JsonSerializer.Serialize(toSerialize, options)

let objectDecode (s : string) =
    match s with
    | null -> VoidSuccess
    | plain when plain.StartsWith EncodingPlainPrefix ->
        JsonSerializer.Deserialize<ExecutionResult> (plain.[EncodingPlainPrefix.Length..], options)
    | latex when latex.StartsWith EncodingLatexPrefix ->
        JsonSerializer.Deserialize<ExecutionResult> (latex.[EncodingLatexPrefix.Length..], options)
    | _ -> VoidSuccess

let execute (kernel : FSharpKernel) code =
    let submitCode = SubmitCode code
    let computed = (kernel.SendAsync submitCode).Result    // Yes. It's Result.

    let mutable nonVoidResponse : string Option = None
    let mutable res : ExecutionResult Option = None

    use _ = computed.KernelEvents.Subscribe (new Action<KernelEvent>(fun e ->
                match e with
                | :? CommandSucceeded ->
                    res <- objectDecode nonVoidResponse.Value |> Some
                | :? CommandFailed as failed ->
                    res <- Error failed.Message |> Some
                | :? DisplayEvent as display ->
                    nonVoidResponse <- (Seq.head display.FormattedValues).Value |> Some
                | _ -> ()
        ))

    match res with
    | None -> EndOfFile
    | Some res -> res

let loadAssembly