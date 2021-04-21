namespace AngouriMath.InteractiveExtension

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Formatting
open AngouriMath.Core
open System.Threading.Tasks

type KernelExtension() = 
    static member public applyMagic () =
        let register (value : ILatexiseable) = $@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[{value.Latexise()}\]"
        
        Formatter.Register<ILatexiseable>(register, "text/html")
        Formatter.SetPreferredMimeTypeFor(typeof<ILatexiseable>, "text/html")


    interface IKernelExtension with
        member _.OnLoadAsync _ =
            KernelExtension.applyMagic()
            let message = "LaTeX renderer binded. Enjoy!"
            KernelInvocationContext.Current.Display(message, "text/markdown") |> ignore
            Task.CompletedTask