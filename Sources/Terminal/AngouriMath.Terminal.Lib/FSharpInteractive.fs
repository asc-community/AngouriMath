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
open AngouriMath.Terminal.Lib.Consts
open AssemblyLoadBuilder
open AngouriMath.FSharp.Core
open AngouriMath.InteractiveExtension


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





let createKernel () =
    let kernel = new FSharpKernel ()

    let assemblyLoad = AssemblyLoadBuilder (execute, kernel)

    assemblyLoad {
        typeof<AngouriMath.MathS>
        typeof<AngouriMath.FSharp.Core.ParseException>
        typeof<AngouriMath.InteractiveExtension.KernelExtension>
        typeof<Plotly.NET.Chart>
        typeof<PeterO.Numbers.EDecimal>
        typeof<GenericTensor.Core.TensorShape>

        Formatter.SetPreferredMimeTypesFor(typeof<obj>, "text/plain")
        Formatter.Register<obj> objectEncode
        Formatter.Register<Entity.Matrix> (Func<Entity.Matrix, string> (fun m -> m.ToString(true) |> objectEncode), "text/plain")

        (fun c ->
            c 
            |> Chart.withSize (1200., 900.)
            |> Chart.show
            "Showing in the browser")
        |> (fun f -> Func<GenericChart.GenericChart, string> f)
        |> (fun f -> 
            Formatter.Register<GenericChart.GenericChart> (f, "text/plain"))
    }
