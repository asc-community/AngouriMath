using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lexer;
using Yoakke.Lexer.Attributes;

namespace AngouriMath.Core.Syntax
{
    internal enum AngouriMathTokenType
    {
        [Error] Error,
        [End] End,

        [Ignore]
        [Regex(Regexes.Whitespace)]
        [Regex(Regexes.LineComment)]
        [Regex(Regexes.MultilineComment)] Ignore,

        [Regex(@"[0-9]+.[0-9]*([eE](\+|-)?[0-9+])?i?")]
        [Regex(@".?[0-9]+([eE](\+|-)?[0-9+])?i?")]
        [Token("i")] Number,

        [Token("+oo"), Token("-oo")] Infinity,

        [Regex(@"[Tt]rue")]
        [Regex(@"[Ff]alse")] Boolean,

        [Token("!")]
        [Token("^")]
        [Token("+"), Token("-")]
        [Token("*"), Token("/")]
        [Token("intersect"), Token(@"/\\")]
        [Token("unite"), Token(@"\\/")]
        [Token("setsubtract"), Token(@"\\")]
        [Token("in")]
        [Token(">"), Token("<"), Token(">="), Token("<="), Token("equalizes")]
        [Token("=")]
        [Token("not")]
        [Token("and"), Token("&")]
        [Token("xor")]
        [Token("or"), Token("|")]
        [Token("implies"), Token("->")]
        [Token("provided")] Operator,

        [Token(","), Token(":"), Token(";")]
        [Token("("), Token(")")]
        [Token("["), Token("]")]
        [Token("{"), Token("}")]
        [Token("(|"), Token("|)")] Punctuation,

        [Token("CC"), Token("RR"), Token("QQ"), Token("ZZ"), Token("BB")] SpecialSet,

        [Token("apply"), Token("lambda"), Token("integral"), Token("derivative"), Token("gamma"),
         Token("limit"), Token("limitleft"), Token("limitright"), 
         Token("signum"), Token("sgn"), Token("sign"), Token("abs"), Token("phi"),
         Token("domain"), Token("piecewise"),
         Token("log"), Token("sqrt"), Token("cbrt"), Token("sqr"), Token("ln"),
         Token("sin"), Token("cos"), Token("tan"), Token("cot"), Token("cotan"),
         Token("sec"), Token("cosec"), Token("csc"), Token("arcsin"), Token("arccos"),
         Token("arctan"), Token("arccotan"), Token("arcsec"), Token("arccosec"), Token("arccsc"),
         Token("acsc"), Token("asin"), Token("acos"), Token("atan"), Token("acotan"),
         Token("asec"), Token("acosec"), Token("acot"), Token("arccot"),
         Token("sinh"), Token("sh"), Token("cosh"), Token("ch"), Token("tanh"), Token("th"),
         Token("cotanh"), Token("coth"), Token("cth"), Token("sech"), Token("sch"),
         Token("cosech"), Token("csch"), Token("asinh"), Token("arsinh"), Token("arsh"),
         Token("acosh"), Token("arcosh"), Token("arch"), Token("atanh"), Token("artanh"), Token("arth"),
         Token("acoth"), Token("arcoth"), Token("acotanh"), Token("arcotanh"), Token("arcth"),
         Token("asech"), Token("arsech"), Token("arsch"), Token("acosech"), Token("arcosech"),
         Token("arcsch"), Token("acsch")] Keyword,

        [Regex(Regexes.Identifier)] Identifier,
    }
}
