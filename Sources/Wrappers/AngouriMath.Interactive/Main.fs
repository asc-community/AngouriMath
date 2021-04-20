module Interactive.Jupyter

open AngouriMath.Core
open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Formatting


let magic() =
    let register (value : ILatexiseable) = $@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[{value.Latexise()}\]"

    Formatter.Register<ILatexiseable>(register, "text/html")

    
type KernelExtension = 
    interface IKernelExtension with
        member this.OnLoadAsync (kernel : Kernel) =
            magic()
            let quack = System.Action<System.Threading.Tasks.Task<KernelCommandResult>>(fun _ -> ())
            kernel.SendAsync(DisplayValue(FormattedValue("text/html", "Quack!"))).ContinueWith(quack)