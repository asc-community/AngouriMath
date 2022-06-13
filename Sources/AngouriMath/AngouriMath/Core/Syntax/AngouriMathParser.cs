using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Parser;
using Yoakke.Parser.Attributes;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using Token = Yoakke.Lexer.IToken<AngouriMath.Core.Syntax.AngouriMathTokenType>;

namespace AngouriMath.Core.Syntax
{
    /************************************************************
                         REFERENCE GRAMMAR

     call_expression             : call_expression atom+
                                 | call_expression '(' expression_list ')'
                                 | atom

     factorial_expression        : call '!'?
                                 
     power_expression            : factorial_expression '^' unary_expression
                                 | factorial_expression
                                 
     unary_expression            : ('-' | '+') unary_expression
                                 | power_expression

     mult_expression             : mult_expression ('*' | '/') unary_expression
                                 | unary_expression
                                 
     sum_expression              : sum_expression ('+' | '-') mult_expression
                                 | mult_expression
                                 
     set_operator_intersection   : set_operator_intersection ('intersect' | '/\') sum_expression 
                                 | sum_expression
                                 
     set_operator_union          : set_operator_union ('unite' | '\/' ) set_operator_intersection
                                 | set_operator_intersection

     set_operator_setsubtraction : set_operator_setsubtraction ('setsubtract' | '\') set_operator_union
                                 | set_operator_union

     in_operator                 : in_operator 'in' set_operator_setsubtraction
                                 | set_operator_setsubtraction

     inequality_expression       : inequality_expression ('>=' | '<=' | '>' | '<' | 'equalizes') in_operator
                                 | in_operator

     equality_expression         : inequality_expression ('=' inequality_expression)*

     negate_expression           : 'not' negate_expression
                                 | equality_expression

     and_expression              : and_expression ('and' | '&') negate_expression
                                 | negate_expression

     xor_expression              : xor_expression 'xor' and_expression
                                 | and_expression

     or_expression               : or_expression ('or' | '|') xor_expression
                                 | xor_expression

     implies_expression          : implies_expression ('implies' | '->') or_expression
                                 | or_expression

     provided_expression         : provided_expression 'provided' implies_expression
                                 | implies_expression

     expression                  : provided_expression 

     expression_list             : (expression (',' expression)*)?

     atom                        : '+oo'
                                 | '-oo'
                                 | Number
                                 | Boolean
                                 | SpecialSet
                                 | Identifier
                                 | '(|' expression '|)'
                                 | '[' expression_list ']' 'T'?
                                 | ('(' | '[') expression ';' expression  (')' | ']')
                                 | '(' expression ')'
                                 | '{' expression ':' expression '}'
                                 | '{' expression_list '}'
                                 | Identifier '(' expression_list ')'

     statement                   : expression End

     ************************************************************/

    [Parser(typeof(AngouriMathTokenType))]
#pragma warning disable SealedOrAbstract // AMAnalyzer
    internal partial class AngouriMathParser
#pragma warning restore SealedOrAbstract // AMAnalyzer
    {
        [Rule("factorial_expression : call_expression '!'")]
        private static Entity Factorial(Entity call, Token? exclamation) => call.Factorial();

        [Rule("power_expression : factorial_expression '^' unary_expression")]
        private static Entity Power(Entity expr, Token hat, Entity exponent) => expr.Pow(exponent);

        [Rule("unary_expression : '+' unary_expression")]
        private static Entity UnaryPlus(Token plus, Entity unary) => unary;

        [Rule("unary_expression : '-' unary_expression")]
        private static Entity UnaryMinus(Token minus, Entity unary) => unary is Number n ? -n : -unary;

        [Rule("mult_expression : mult_expression ('*' | '/') unary_expression")]
        [Rule("sum_expression : sum_expression ('+' | '-') mult_expression")]
        [Rule(@"set_operator_intersection : set_operator_intersection ('intersect' | '/\\') sum_expression")]
        [Rule(@"set_operator_union : set_operator_union ('unite' | '\\/' ) set_operator_intersection")]
        [Rule(@"set_operator_setsubtraction : set_operator_setsubtraction ('setsubtract' | '\\') set_operator_union")]
        [Rule("in_operator : in_operator 'in' set_operator_setsubtraction")]
        [Rule("inequality_expression : inequality_expression ('>=' | '<=' | '>' | '<' | 'equalizes') in_operator")]
        [Rule("and_expression : and_expression ('and' | '&') negate_expression")]
        [Rule("xor_expression : xor_expression 'xor' and_expression")]
        [Rule("or_expression : or_expression ('or' | '|') xor_expression")]
        [Rule("implies_expression : implies_expression ('implies' | '->') or_expression")]
        [Rule("provided_expression : provided_expression 'provided' implies_expression")]
        private static Entity Binary(Entity left, Token op, Entity right) => op.Text switch
        {
            "*" => left * right,
            "/" => left / right,
            "+" => left + right,
            "-" => left - right,
            "intersect" or @"/\" => left.Intersect(right),
            "unite" or @"\/" => left.Unite(right),
            "setsubtract" or @"\" => left.SetSubtract(right),
            "in" => left.In(right),
            ">" => left > right,
            "<" => left < right,
            ">=" => left >= right,
            "<=" => left <= right,
            "equalizes" => left.Equalizes(right),
            "and" or "&" => left & right,
            "xor" => left ^ right,
            "or" or "|" => left | right,
            "implies" or "->" => left.Implies(right),
            "provided" => left.Provided(right),
            _ => throw new InvalidOperationException(),
        };

        [Rule("equality_expression : inequality_expression ('=' inequality_expression)*")]
        private static Entity Equality(Entity first, IReadOnlyList<(Token Eq, Entity Right)> second)
        {
            if (second.Count == 0) return first;
            var result = first.Equalizes(second[0].Right);
            for (var i = 1; i < second.Count; ++i)
            {
                var eq = second[i - 1].Right.Equalizes(second[i].Right);
                result = result & eq;
            }
            return result;
        }

        [Rule("negate_expression : 'not' negate_expression")]
        private static Entity Negate(Token not, Entity expr) => !expr;

        [Rule("call_expression : atom")]
        [Rule("factorial_expression : call_expression")]
        [Rule("expression : provided_expression")]
        [Rule("power_expression : factorial_expression")]
        [Rule("unary_expression : power_expression")]
        [Rule("mult_expression : unary_expression")]
        [Rule("sum_expression : mult_expression")]
        [Rule("set_operator_intersection : sum_expression")]
        [Rule("set_operator_union : set_operator_intersection")]
        [Rule("set_operator_setsubtraction : set_operator_union")]
        [Rule("in_operator : set_operator_setsubtraction")]
        [Rule("inequality_expression : in_operator")]
        [Rule("negate_expression : equality_expression")]
        [Rule("and_expression : negate_expression")]
        [Rule("xor_expression : and_expression")]
        [Rule("or_expression : xor_expression")]
        [Rule("implies_expression : or_expression")]
        [Rule("provided_expression : implies_expression")]
        private static Entity Identity(Entity e) => e;

        [Rule("expression_list : (expression (',' expression)*)?")]
        private static IReadOnlyList<Entity> ExpressionList(Punctuated<Entity, Token> exprs) => exprs.Values.ToList();

        [Rule("atom : '+oo' | '-oo'")]
        private static Entity Infinity(Token infinity) => infinity.Text[0] == '+' ? Real.PositiveInfinity : Real.NegativeInfinity;

        [Rule("atom : Number")]
        private static Entity Number(Token num) => Complex.Parse(num.Text);

        [Rule("atom : Boolean")]
        private static Entity Boole(Token boolean) => Entity.Boolean.Parse(boolean.Text);

        [Rule("atom : SpecialSet")]
        private static Entity SpecialSet(Token set) => Entity.Set.SpecialSet.Create(set.Text);

        [Rule("atom : Identifier")]
        private static Entity Variable(Token name) => Entity.Variable.CreateVariableUnchecked(name.Text);

        [Rule("atom : '(|' expression '|)'")]
        private static Entity Abs(Token open, Entity expr, Token close) => expr.Abs();

        [Rule("atom : '[' expression_list ']' 'T'?")]
        private static Entity Matrix(Token open, IReadOnlyList<Entity> exprs, Token close, Token? transpose) => transpose is null
            ? TryBuildingMatrix(exprs)
            : TryBuildingMatrix(exprs).T;

        [Rule("atom : ('(' | '[') expression ';' expression (')' | ']')")]
        private static Entity Interval(Token open, Entity from, Token semicol, Entity to, Token close) =>
            new Interval(from, open.Text == "[", to, close.Text == "]");

        [Rule("atom : '(' expression ')'")]
        private static Entity Grouping(Token open, Entity expr, Token close) => expr;

        [Rule("atom : '{' expression ':' expression '}'")]
        private static Entity SetComprehension(Token open, Entity variable, Token col, Entity predicate, Token close) =>
            new ConditionalSet(variable, predicate);

        [Rule("atom : '{' expression_list '}'")]
        private static Entity Set(Token open, IReadOnlyList<Entity> exprs, Token close) => new FiniteSet(exprs);

        [Rule("call_expression : call_expression atom+")]
        private static Entity Call(Entity f, IReadOnlyList<Entity> args)
        {
            if (f is Number && args.Count == 1) return f * args[0];
            return f.Apply(args.ToArray());
        }

        [Rule("call_expression : 'log' '(' expression_list ')'")]
        private static Entity Log(Token log, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count == 1) return MathS.Log(10, args[0]);
            if (args.Count == 2) return MathS.Log(args[0], args[1]);
            throw new FunctionArgumentCountException("log requires 1 or 2 args");
        }

        [Rule("call_expression : 'sqrt' '(' expression_list ')'")]
        private static Entity Sqrt(Token sqrt, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Sqrt(args[0]));

        [Rule("call_expression : 'cbrt' '(' expression_list ')'")]
        private static Entity Cbrt(Token sqrt, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Cbrt(args[0]));

        [Rule("call_expression : 'sqr' '(' expression_list ')'")]
        private static Entity Sqr(Token sqrt, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Sqr(args[0]));

        [Rule("call_expression : 'ln' '(' expression_list ')'")]
        private static Entity Ln(Token sqrt, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Ln(args[0]));

        [Rule("call_expression : 'sin' '(' expression_list ')'")]
        private static Entity Sin(Token sin, Token open, IReadOnlyList<Entity> args, Token close) =>
    AssertArgc(1, args, () => MathS.Sin(args[0]));

        [Rule("call_expression : 'cos' '(' expression_list ')'")]
        private static Entity Cos(Token cos, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Cos(args[0]));

        [Rule("call_expression : 'tan' '(' expression_list ')'")]
        private static Entity Tan(Token tan, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Tan(args[0]));

        [Rule("call_expression : 'cotan' '(' expression_list ')'")]
        [Rule("call_expression : 'cot' '(' expression_list ')'")]
        private static Entity Cotan(Token cotan, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Cotan(args[0]));

        [Rule("call_expression : 'sec' '(' expression_list ')'")]
        private static Entity Sec(Token sec, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Sec(args[0]));

        [Rule("call_expression : 'cosec' '(' expression_list ')'")]
        [Rule("call_expression : 'csc' '(' expression_list ')'")]
        private static Entity Cosec(Token cosec, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Cosec(args[0]));

        [Rule("call_expression : 'arcsin' '(' expression_list ')'")]
        [Rule("call_expression : 'asin' '(' expression_list ')'")]
        private static Entity Arcsin(Token arcsin, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arcsin(args[0]));

        [Rule("call_expression : 'arccos' '(' expression_list ')'")]
        [Rule("call_expression : 'acos' '(' expression_list ')'")]
        private static Entity Arccos(Token arccos, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arccos(args[0]));

        [Rule("call_expression : 'arctan' '(' expression_list ')'")]
        [Rule("call_expression : 'atan' '(' expression_list ')'")]
        private static Entity Arctan(Token arctan, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arctan(args[0]));

        [Rule("call_expression : 'arccotan' '(' expression_list ')'")]
        [Rule("call_expression : 'acotan' '(' expression_list ')'")]
        [Rule("call_expression : 'acot' '(' expression_list ')'")]
        [Rule("call_expression : 'arccot' '(' expression_list ')'")]
        private static Entity Arccotan(Token arccotan, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arccotan(args[0]));

        [Rule("call_expression : 'arcsec' '(' expression_list ')'")]
        [Rule("call_expression : 'asec' '(' expression_list ')'")]
        private static Entity Arcsec(Token arcsec, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arcsec(args[0]));

        [Rule("call_expression : 'arccosec' '(' expression_list ')'")]
        [Rule("call_expression : 'arccsc' '(' expression_list ')'")]
        [Rule("call_expression : 'acsc' '(' expression_list ')'")]
        [Rule("call_expression : 'acosec' '(' expression_list ')'")]
        private static Entity Arccosec(Token arccosec, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Arccosec(args[0]));

        [Rule("call_expression : 'sinh' '(' expression_list ')'")]
        [Rule("call_expression : 'sh' '(' expression_list ')'")]
        private static Entity Sinh(Token sinh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Sinh(args[0]));

        [Rule("call_expression : 'cosh' '(' expression_list ')'")]
        [Rule("call_expression : 'ch' '(' expression_list ')'")]
        private static Entity Cosh(Token cosh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Cosh(args[0]));

        [Rule("call_expression : 'tanh' '(' expression_list ')'")]
        [Rule("call_expression : 'th' '(' expression_list ')'")]
        private static Entity Tanh(Token tanh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Tanh(args[0]));

        [Rule("call_expression : 'cotanh' '(' expression_list ')'")]
        [Rule("call_expression : 'coth' '(' expression_list ')'")]
        [Rule("call_expression : 'cth' '(' expression_list ')'")]
        private static Entity Cotanh(Token cotanh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Cotanh(args[0]));

        [Rule("call_expression : 'sech' '(' expression_list ')'")]
        [Rule("call_expression : 'sch' '(' expression_list ')'")]
        private static Entity Sech(Token sech, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Sech(args[0]));

        [Rule("call_expression : 'cosech' '(' expression_list ')'")]
        [Rule("call_expression : 'csch' '(' expression_list ')'")]
        private static Entity Cosech(Token cosech, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Cosech(args[0]));

        [Rule("call_expression : 'arsinh' '(' expression_list ')'")]
        [Rule("call_expression : 'asinh' '(' expression_list ')'")]
        [Rule("call_expression : 'arsh' '(' expression_list ')'")]
        private static Entity Asinh(Token asinh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Arsinh(args[0]));

        [Rule("call_expression : 'arcosh' '(' expression_list ')'")]
        [Rule("call_expression : 'acosh' '(' expression_list ')'")]
        [Rule("call_expression : 'arch' '(' expression_list ')'")]
        private static Entity Arcosh(Token arcosh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Arcosh(args[0]));

        [Rule("call_expression : 'artanh' '(' expression_list ')'")]
        [Rule("call_expression : 'atanh' '(' expression_list ')'")]
        [Rule("call_expression : 'arth' '(' expression_list ')'")]
        private static Entity Artanh(Token artanh, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Artanh(args[0]));

        [Rule("call_expression : 'arcotanh' '(' expression_list ')'")]
        [Rule("call_expression : 'acotanh' '(' expression_list ')'")]
        [Rule("call_expression : 'arcoth' '(' expression_list ')'")]
        [Rule("call_expression : 'acoth' '(' expression_list ')'")]
        [Rule("call_expression : 'arcth' '(' expression_list ')'")]
        private static Entity Acoth(Token acoth, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Arcotanh(args[0]));

        [Rule("call_expression : 'arsech' '(' expression_list ')'")]
        [Rule("call_expression : 'asech' '(' expression_list ')'")]
        [Rule("call_expression : 'arsch' '(' expression_list ')'")]
        private static Entity Arsech(Token arsech, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Arsech(args[0]));

        [Rule("call_expression : 'arcosech' '(' expression_list ')'")]
        [Rule("call_expression : 'acosech' '(' expression_list ')'")]
        [Rule("call_expression : 'arcsch' '(' expression_list ')'")]
        [Rule("call_expression : 'acsch' '(' expression_list ')'")]
        private static Entity Arcosech(Token arcosech, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Hyperbolic.Arcosech(args[0]));

        [Rule("call_expression : 'gamma' '(' expression_list ')'")]
        private static Entity Gamma(Token gamma, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Gamma(args[0]));

        [Rule("call_expression : 'apply' '(' expression_list ')'")]
        private static Entity Apply(Token apply, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count < 2) throw new FunctionArgumentCountException("apply needs at least 2 arguments");
            return args[0].Apply(args.Skip(1).ToArray());
        }

        [Rule("call_expression : 'lambda' '(' expression_list ')'")]
        private static Entity Lambda(Token lambda, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count < 2) throw new FunctionArgumentCountException("lambda needs at least 2 arguments");
            var result = args[args.Count - 1];
            for (var i = args.Count - 2; i >= 0; --i)
            {
                if (args[i] is not Variable arg) throw new InvalidArgumentParseException("lambda argument must be a variable");
                result = result.LambdaOver(arg);
            }
            return result;
        }

        [Rule("call_expression : 'integral' '(' expression_list ')'")]
        private static Entity Integral(Token integral, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count == 2) return MathS.Integral(args[0], args[1]);
            if (args.Count == 3) return MathS.Integral(args[0], args[1], (int)(Number)args[2]);
            throw new FunctionArgumentCountException("integral requires 2 or 3 args");
        }

        [Rule("call_expression : 'derivative' '(' expression_list ')'")]
        private static Entity Derivative(Token derivative, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count == 2) return MathS.Derivative(args[0], args[1]);
            if (args.Count == 3) return MathS.Derivative(args[0], args[1], (int)(Number)args[2]);
            throw new FunctionArgumentCountException("derivative requires 2 or 3 args");
        }

        [Rule("call_expression : 'limit' '(' expression_list ')'")]
        private static Entity Limit(Token limit, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(3, args, () => MathS.Limit(args[0], args[1], args[2]));

        [Rule("call_expression : 'limitleft' '(' expression_list ')'")]
        private static Entity LimitLeft(Token limit, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(3, args, () => MathS.Limit(args[0], args[1], args[2], ApproachFrom.Left));

        [Rule("call_expression : 'limitright' '(' expression_list ')'")]
        private static Entity LimitRight(Token limit, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(3, args, () => MathS.Limit(args[0], args[1], args[2], ApproachFrom.Right));

        [Rule("call_expression : 'signum' '(' expression_list ')'")]
        [Rule("call_expression : 'sign' '(' expression_list ')'")]
        [Rule("call_expression : 'sgn' '(' expression_list ')'")]
        private static Entity Sign(Token sign, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Signum(args[0]));

        [Rule("call_expression : 'abs' '(' expression_list ')'")]
        private static Entity Abs(Token abs, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.Abs(args[0]));

        [Rule("call_expression : 'phi' '(' expression_list ')'")]
        private static Entity Phi(Token abs, Token open, IReadOnlyList<Entity> args, Token close) =>
            AssertArgc(1, args, () => MathS.NumberTheory.Phi(args[0]));

        [Rule("call_expression : 'domain' '(' expression_list ')'")]
        private static Entity Domain(Token domain, Token open, IReadOnlyList<Entity> args, Token close)
        {
            if (args.Count != 2) throw new FunctionArgumentCountException("domain requires 2 args");
            if (args[1] is not SpecialSet ss) throw new InvalidArgumentParseException($"Unrecognized special set {args[1].Stringize()}");
            return args[0].WithCodomain(ss.ToDomain());
        }

        [Rule("call_expression : 'piecewise' '(' expression_list ')'")]
        private static Entity Piecewise(Token piecewise, Token open, IReadOnlyList<Entity> args, Token close)
        {
            var cases = new List<Providedf>();
            foreach (var arg in args)
            {
                if (arg is Providedf provided) cases.Add(provided);
                else cases.Add(new Providedf(arg, true));
            }
            return new Piecewise(cases);
        }

        [Rule("call_expression : call_expression '(' expression_list ')'")]
        private static Entity Call(Entity f, Token open, IReadOnlyList<Entity> args, Token close) => f.Apply(args.ToArray());

        [Rule("statement : expression End")]
        private static Entity Statement(Entity expr, Token end) => expr;

        private static Entity AssertArgc(int argc, IReadOnlyList<Entity> args, Func<Entity> makeFunc)
        {
            if (args.Count != argc) throw new FunctionArgumentCountException("todo");
            return makeFunc();
        }

        private static Matrix TryBuildingMatrix(IReadOnlyList<Entity> elements)
        {
            if (!elements.Any())
                return MathS.Vector(elements.ToArray());
            var first = elements.First();
            if (first is not Matrix { IsVector: true } firstVec)
                return MathS.Vector(elements.ToArray());
            var tb = new MatrixBuilder(firstVec.RowCount);
            foreach (var row in elements)
            {
                if (row is not Matrix { IsVector: true } rowVec)
                    return MathS.Vector(elements.ToArray());
                if (rowVec.RowCount != firstVec.RowCount)
                    return MathS.Vector(elements.ToArray());
                tb.Add(rowVec);
            }
            return tb.ToMatrix() ?? throw new AngouriBugException("Should've been checked already");
        }
    }
}
