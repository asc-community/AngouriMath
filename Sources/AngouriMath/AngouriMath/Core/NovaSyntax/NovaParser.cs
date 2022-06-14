//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.SynKit.Parser;
using Yoakke.SynKit.Parser.Attributes;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using Token = Yoakke.SynKit.Lexer.IToken<AngouriMath.Core.NovaSyntax.AngouriMathTokenType>;

namespace AngouriMath.Core.NovaSyntax
{
    /*
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

     */

    [Yoakke.SynKit.Parser.Attributes.Parser(typeof(AngouriMathTokenType))]
#pragma warning disable SealedOrAbstract // AMAnalyzer
    public partial class NovaParser
#pragma warning restore SealedOrAbstract // AMAnalyzer
    {
        [Rule("factorial_expression : call_expression '!'")]
        private static Entity Factorial(Entity call, Token? exclamation) => call.Factorial();

        [Rule("power_expression : factorial_expression '^' unary_expression")]
        private static Entity Power(Entity expr, Token hat, Entity exponent) => expr.Pow(exponent);

        [Rule("unary_expression : '+' unary_expression")]
        private static Entity UnaryPlus(Token plus, Entity unary) => unary;

        [Rule("unary_expression : '-' unary_expression")]
        private static Entity UnaryMinus(Token minus, Entity unary)
            => unary is Real { IsNegative: false } n ? -n : -unary;

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
        private static Entity Matrix(Token open, IReadOnlyList<Entity> exprs, Token close, Token? transpose)
            => transpose is null
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
            => f is Number ? throw new CannotApplyException($"{f} cannot take arguments") : f.Apply(args.ToArray());

        [Rule("call_expression : Keyword '(' expression_list ')'")]
        private static Entity CallToKeyword(Token kw, Token open, IReadOnlyList<Entity> args, Token close)
        {
            static IEnumerable<Entity> SupplyOptionalArgs(IReadOnlyList<Entity> currArgs, string funcName)
                => (funcName, currArgs.Count) switch
                {
                    ("log", 1) => currArgs.Prepend(10),
                    ("derivative" or "integral", 2) => currArgs.Append(1),
                    _ => currArgs
                };

            if (args.Count is 0)
                throw new FunctionArgumentCountException("When using (), one is required to provide at least one argument");
            
            if (kw.Text == "apply")
                return Apply(kw, open, args, close);
            if (kw.Text == "lambda")
                return Lambda(kw, open, args, close);
            if (kw.Text == "piecewise")
                return Piecewise(kw, open, args, close);

            var tryApplied = Application.KeywordToApplied(kw.Text, SupplyOptionalArgs(args, kw.Text).ToLList(), out var badArgument);
            
            if (badArgument)
                throw new InvalidArgumentParseException($"Bad arguments {args} for function {kw.Text}");
            if (tryApplied is not var (applied, otherArgs))
                throw new AngouriBugException($"Unrecognized function {kw.Text}");
            if (otherArgs is not LEmpty<Entity>)
                throw new FunctionArgumentCountException($"Too many ({args.Count}) args for function {kw.Text}");
            if (applied is Application or Entity.Variable)
                throw new FunctionArgumentCountException($"Not enough ({args.Count}) arguments for function {kw.Text}");
            return applied;
        }

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
        private static Entity Call(Entity f, Token open, IReadOnlyList<Entity> args, Token close)
            => f.Apply(args.ToArray());

        [Rule("statement : expression End")]
        private static Entity Statement(Entity expr, Token end) => expr;

        private static Entity AssertArgc(int argc, IReadOnlyList<Entity> args, Func<Entity> makeFunc)
        {
            if (args.Count != argc) throw new FunctionArgumentCountException($"Expected {argc} arguments, but {args.Count} were provided");
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
