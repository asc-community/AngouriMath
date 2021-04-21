module Tests.HtmlOutput

open Xunit
open Microsoft.DotNet.Interactive.Formatting
open AngouriMath.FSharp.Core
open Microsoft.DotNet.Interactive.FSharp
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Events
open Microsoft.DotNet.Interactive
open System.Linq
open System.IO

[<Fact>]
let ``No latex without magic`` () =
    let entity = parsed "x / 2"
    let html = entity.ToDisplayString("text/html")
    Assert.Contains("<table>", html)

[<Fact>]
let ``Latex with magic`` () =
    AngouriMath.InteractiveExtension.KernelExtension.applyMagic()
    let entity = parsed "x / 2"
    let html = entity.ToDisplayString("text/html")
    Assert.Equal(@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
\[\frac{x}{2}\]", html)


[<Fact(Skip = "Not working in the embedded kernel")>]
let ``Latex formatter automatically applied in Interactive``  () =

    // Set up the right path
    let path = Path.GetFullPath(Path.Join(Directory.GetCurrentDirectory(), "../../../../../Wrappers/AngouriMath.Interactive/bin/Release/netstandard2.1"))
    Assert.True(Directory.Exists(path), $"Directory not found: {path}. We are at {Directory.GetCurrentDirectory()}")

    // The code we will send
    let i = $"#i \"nuget:{path}\""
    let r = "#r \"nuget:AngouriMath.Interactive, *-*\""
    let code = "open Core\nparsed \"x / 2\""
    
    using ((new FSharpKernel())
                       .UseNugetDirective()
                       .UseKernelHelpers().UseDefaultNamespaces()) (fun kernel ->
        async {
            // Execute #i magic command and see for no errors
            let! after_i = kernel.SendAsync(SubmitCode(i), System.Threading.CancellationToken.None) |> Async.AwaitTask
            after_i.KernelEvents.Subscribe(fun ev -> Assert.False(ev :? CommandFailed, "After i: " + ev.ToDisplayString())) |> ignore

            // Execute #r magic command and see for no errors
            let! after_r = kernel.SendAsync(SubmitCode(r), System.Threading.CancellationToken.None) |> Async.AwaitTask
            after_r.KernelEvents.Subscribe(fun ev -> Assert.False(ev :? CommandFailed, "After r: " + ev.ToDisplayString())) |> ignore

            // Execute our code and find the right LaTeX code
            let! res = kernel.SendAsync(SubmitCode(code), System.Threading.CancellationToken.None) |> Async.AwaitTask
            let mutable displayValueReceived = false
            res.KernelEvents.Subscribe(fun ev ->
                match ev with
                | :? DisplayEvent as dp -> 
                    Assert.Contains("<script id='MathJax-script'", dp.FormattedValues.First().ToDisplayString())
                    displayValueReceived <- true
                | _ -> ()
            ) |> ignore
            Assert.True(displayValueReceived, "No display value was received")
        }
    )
