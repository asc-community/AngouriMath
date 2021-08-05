namespace AngouriMath.InteractiveExtension

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Formatting
open AngouriMath.Core
open System.Threading.Tasks
open PeterO.Numbers
open System
open Plotly.NET
open Plotly.NET.GenericChart
open AngouriMath.FSharp.Functions

type KernelExtension() = 
    static member public applyMagic () =
        (fun l -> $"$${latex l}$$")
        |> (fun f -> new Func<ILatexiseable, string>(f))
        |> (fun f -> Formatter.Register<ILatexiseable>(f, "text/latex"))
        Formatter.SetPreferredMimeTypeFor(typeof<ILatexiseable>, "text/latex")

        Formatter.SetPreferredMimeTypeFor(typeof<EDecimal>, "text/plain")
        Formatter.Register<EDecimal>(new Func<EDecimal, string>(fun o -> o.ToString()), "text/plain")

        Formatter.SetPreferredMimeTypeFor(typeof<EInteger>, "text/plain")
        Formatter.Register<EInteger>(new Func<EInteger, string>(fun o -> o.ToString()), "text/plain")

        Formatter.SetPreferredMimeTypeFor(typeof<ERational>, "text/html")
        Formatter.Register<ERational>(
            new Func<ERational, string>(fun o -> $@"$$\frac{{{o.Numerator}}}{{{o.Denominator}}}$$"), "text/latex")

        Formatter.SetPreferredMimeTypeFor(typeof<GenericChart>, "text/html")
        Formatter.Register<GenericChart> (toChartHTML, "text/html")



    interface IKernelExtension with
        member _.OnLoadAsync _ =
            KernelExtension.applyMagic()
            let message = "LaTeX renderer binded. Enjoy!"
            KernelInvocationContext.Current.Display(message, "text/markdown") |> ignore
            Task.CompletedTask