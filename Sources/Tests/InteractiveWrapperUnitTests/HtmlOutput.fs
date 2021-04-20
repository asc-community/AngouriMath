module Tests.HtmlOutput

open Xunit
open Microsoft.DotNet.Interactive.Formatting
open Core

[<Fact>]
let ``No latex without magic`` () =
    let entity = parsed "x / 2"
    let html = entity.ToDisplayString("text/html")
    Assert.Contains("<table>", html)

[<Fact>]
let ``Latex with magic`` () =
    AngouriMath.Interactive.Jupyter.magic()
    let entity = parsed "x / 2"
    let html = entity.ToDisplayString("text/html")
    Assert.Equal(@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[\frac{x}{2}\]", html)