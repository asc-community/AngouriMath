module Interactive

open AngouriMath.Core
open Microsoft.DotNet.Interactive.Formatting

let magic() =
    let register (value : ILatexiseable) = $@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[{value.Latexise()}\]"

    Formatter.Register<ILatexiseable>(register, "text/html")
