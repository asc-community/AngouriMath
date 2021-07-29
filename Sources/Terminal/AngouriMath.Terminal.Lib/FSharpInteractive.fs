module AngouriMath.Terminal.Lib.Say

open AngouriMath.Core
open Consts
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Events
open Microsoft.DotNet.Interactive.FSharp
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Formatting
open System
open AngouriMath
open System.Reflection

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


let loadAssembly kernel (path : string) =
    path.Replace("\\", "\\\\")
    |> (fun loc -> execute kernel $"#r \"{loc}\"")


let aggressiveOperatorsModule =
    match Type.GetType("AngouriMath.Interactive.AggressiveOperators") with
    | null -> raise (Exception("Not found"))
    | existing -> existing
    

let createKernel () =
    let kernel = new FSharpKernel ()
    let load (typeInfo : Type) = loadAssembly kernel typeInfo.Assembly.Location

    

    match load (typeof<MathS>) with
    | Error error -> Result.Error error
    | _ ->
        match load typeof<AngouriMath.FSharp.Core.ParseException> with
        | Error error -> Result.Error error
        | _ -> match load aggressiveOperatorsModule with
               | Error error -> Result.Error error
               | _ ->
                      Formatter.SetPreferredMimeTypeFor(typeof<obj>, "text/plain");
                      Formatter.Register<obj> objectEncode;
                      Result.Ok kernel