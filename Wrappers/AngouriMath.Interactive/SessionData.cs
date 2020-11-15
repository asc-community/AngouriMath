using Microsoft.DotNet.Interactive.Formatting;

namespace AngouriMath.Interactive
{
    public sealed record MathJaxBackend(string HtmlCode)
    {
        
        public static MathJaxBackend Default = new MathJaxBackend($@"
<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
");

    }

    public sealed record AngouriMathSession(
        MathJaxBackend MathJaxBackend)
    {
        public static AngouriMathSession Default = new AngouriMathSession(MathJaxBackend.Default);
    }
}
