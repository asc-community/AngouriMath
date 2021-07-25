namespace AngouriMath.InteractiveExtension

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Formatting
open AngouriMath.Core
open System.Threading.Tasks
open PeterO.Numbers
open System

type KernelExtension() = 
    static member public applyMagic () =
        let latexWrap latex = $@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[{latex}\]"

        let register (value : ILatexiseable) = value.Latexise() |> latexWrap
        
        Formatter.Register<ILatexiseable>(register, "text/html")
        Formatter.SetPreferredMimeTypeFor(typeof<ILatexiseable>, "text/html")

        Formatter.SetPreferredMimeTypeFor(typeof<EDecimal>, "text/plain")
        Formatter.Register<EDecimal>(new Func<EDecimal, string>(fun o -> o.ToString()), "text/plain")

        Formatter.SetPreferredMimeTypeFor(typeof<EInteger>, "text/plain")
        Formatter.Register<EInteger>(new Func<EInteger, string>(fun o -> o.ToString()), "text/plain")

        Formatter.SetPreferredMimeTypeFor(typeof<ERational>, "text/html")
        Formatter.Register<ERational>(
            new Func<ERational, string>(fun o -> latexWrap $@"\frac{{{o.Numerator}}}{{{o.Denominator}}}"), "text/html")




    interface IKernelExtension with
        member _.OnLoadAsync _ =
            KernelExtension.applyMagic()
            let message = "LaTeX renderer binded. Enjoy!"
            KernelInvocationContext.Current.Display(message, "text/markdown") |> ignore
            Task.CompletedTask