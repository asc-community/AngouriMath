using AngouriMath.Core;
using Microsoft.DotNet.Interactive.Formatting;

namespace AngouriMath.Interactive
{
    public sealed class SessionRunner
    {
        private readonly AngouriMathSession session;

        public SessionRunner(AngouriMathSession session)
            => this.session = session;

        public SessionRunner()
            => session = AngouriMathSession.Default;

        public void Init()
        {
            Formatter.Register<ILatexiseable>((value, writer) => writer.Write(
                session.MathJaxBackend.HtmlCode
                +
                @$"\[{value.Latexise()}\]"  
                ), "text/html");
        }
    }
}
