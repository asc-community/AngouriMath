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
        let registerLatexRendering (latexiser : 'a -> string) =
            // register text/latex
            (fun o -> $"$${latexiser o}$$")
            |> (fun f -> new Func<'a, string>(f))
            |> (fun f -> Formatter.Register<'a>(f, "text/latex"))

            // register text/html
            (fun o -> $@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[{latexiser o}\]")
            |> (fun f -> new Func<'a, string>(f))
            |> (fun f -> Formatter.Register<'a>(f, "text/html"))

            // if possible, use text/latex
            Formatter.SetPreferredMimeTypesFor(typeof<'a>, "text/latex")


        Formatter.SetPreferredMimeTypesFor(typeof<EDecimal>, "text/plain")
        Formatter.Register<EDecimal>(new Func<EDecimal, string>(fun o -> o.ToString()), "text/plain")

        Formatter.SetPreferredMimeTypesFor(typeof<EInteger>, "text/plain")
        Formatter.Register<EInteger>(new Func<EInteger, string>(fun o -> o.ToString()), "text/plain")

        registerLatexRendering (fun (o : ILatexiseable) -> latex o)

        registerLatexRendering (fun (o : ERational) -> $@"\frac{{{o.Numerator}}}{{{o.Denominator}}}")

        Formatter.SetPreferredMimeTypesFor(typeof<GenericChart>, "text/html")
        Formatter.Register<GenericChart> (toChartHTML, "text/html")



    interface IKernelExtension with
        member _.OnLoadAsync _ =
            KernelExtension.applyMagic()
            // let message = "LaTeX renderer binded. Enjoy!"
            // KernelInvocationContext.Current.Display(message, "text/html") |> ignore
            printfn $"LaTeX renderer binded. Enjoy!"
            Task.CompletedTask