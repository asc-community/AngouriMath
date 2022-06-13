using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lexer.Attributes;

namespace AngouriMath.Core.Syntax
{
    [Lexer(typeof(AngouriMathTokenType))]
#pragma warning disable SealedOrAbstract // AMAnalyzer
    internal partial class AngouriMathLexer
#pragma warning restore SealedOrAbstract // AMAnalyzer
    {
    }
}
