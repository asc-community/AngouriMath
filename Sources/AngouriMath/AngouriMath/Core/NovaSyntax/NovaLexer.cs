//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.SynKit.Lexer.Attributes;

namespace AngouriMath.Core.NovaSyntax
{
    [Lexer(typeof(AngouriMathTokenType))]
#pragma warning disable SealedOrAbstract // AMAnalyzer
    public partial class NovaLexer
#pragma warning restore SealedOrAbstract // AMAnalyzer
    {
    }
}
