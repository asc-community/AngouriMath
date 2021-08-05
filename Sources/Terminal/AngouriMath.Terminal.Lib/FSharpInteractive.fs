module AngouriMath.Terminal.Lib.FSharpInteractive

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
open Plotly.NET

type ExecutionResult =
    | SuccessPackageAdded
    | Error of string
    | VoidSuccess
    | PlainTextSuccess of string
    | LatexSuccess of Latex : string * Source : string
    | EndOfFile


let options = JsonSerializerOptions ()
JsonFSharpConverter () |> options.Converters.Add


let private objectEncode (o : obj) =
    match o with
    | :? ILatexiseable as latexiseable -> 
        let toSerialize = LatexSuccess (latexiseable.Latexise (), o.ToString ())
        EncodingLatexPrefix + JsonSerializer.Serialize(toSerialize, options)
    | _ ->
        let toSerialize = PlainTextSuccess (o.ToString ())
        EncodingPlainPrefix + JsonSerializer.Serialize(toSerialize, options)


let private objectDecode (s : string Option) =
    match s with
    | None -> VoidSuccess
    | Some plain when plain.StartsWith EncodingPlainPrefix ->
        JsonSerializer.Deserialize<ExecutionResult> (plain.[EncodingPlainPrefix.Length..], options)
    | Some latex when latex.StartsWith EncodingLatexPrefix ->
        JsonSerializer.Deserialize<ExecutionResult> (latex.[EncodingLatexPrefix.Length..], options)
    | _ -> VoidSuccess


let execute (kernel : FSharpKernel) code =
    let submitCode = SubmitCode code
    let computed = (kernel.SendAsync submitCode).Result    // Yes. It's Result.

    let mutable nonVoidResponse : string Option = None
    let mutable res : ExecutionResult Option = None

    computed.KernelEvents.Subscribe (new Action<KernelEvent>(fun e ->
                match e with
                | :? CommandSucceeded ->
                    res <- objectDecode nonVoidResponse |> Some
                | :? CommandFailed as failed ->
                    res <- Error failed.Message |> Some
                | :? DisplayEvent as display ->
                    nonVoidResponse <- (Seq.head display.FormattedValues).Value |> Some
                | _ -> ()
        ))
    |> (fun observer -> observer.Dispose())

    match res with
    | None -> EndOfFile
    | Some res -> res


let private loadAssembly kernel (path : string) =
    path.Replace("\\", "\\\\")
    |> (fun loc -> execute kernel $"#r \"{loc}\"")


let createKernel () =
    let kernel = new FSharpKernel ()

    let load (typeInfo : Type) =
        let assemblyLocation = typeInfo.Assembly.Location
        if System.IO.File.Exists assemblyLocation then
            match loadAssembly kernel assemblyLocation with
            | Error error -> Result.Error error
            | _ -> Result.Ok ()
        else
            Result.Error $"Assembly {assemblyLocation} does not exist"


    load typeof<MathS>
    |> Result.bind (fun _ -> load typeof<AngouriMath.FSharp.Core.ParseException>)
    |> Result.bind (fun _ -> load typeof<AngouriMath.InteractiveExtension.KernelExtension>)
    |> Result.bind (fun _ -> load typeof<Plotly.NET.Chart>)
    |> Result.bind (fun _ ->
        Formatter.SetPreferredMimeTypeFor(typeof<obj>, "text/plain")
        Formatter.Register<obj> objectEncode
        Formatter.Register<Entity.Matrix> (Func<Entity.Matrix, string> (fun m -> m.ToString(true) |> objectEncode), "text/plain")

        (fun c ->
            c 
            |> Chart.withSize (1200., 900.)
            |> Chart.Show
            "Showing in the browser")
        |> (fun f -> Func<GenericChart.GenericChart, string> f)
        |> (fun f -> 
            Formatter.Register<GenericChart.GenericChart> (f, "text/plain"))
        Result.Ok kernel)
