/*

Remember to run the "antlr_rerun.bat" file at the project root every time you modify this file so that other
source files under the Antlr folder can update and be reflected in other parts of AngouriMath!
It only consists of commands that are consistent across CMD and Bash so you should be able to run that file
regardless of whether you are on Windows, Linux or Mac. You need to have an installed Java Runtime, however.

*/

grammar AngouriMath; // Should be identical to the file name or ANTLR will complain

options
{
    // Equivalent to passing "-Dlanguage=CSharp" into "antlr-4.8-complete.jar" in "antlr_rerun.bat"
    language = CSharp;
}

@modifier{internal}
@ctorModifier{internal}

@parser::header
{
    using System.Linq;
    using AngouriMath;
    using static AngouriMath.Core.Exceptions.FunctionArgumentCountException;
    using static AngouriMath.Entity.Number;
    using AngouriMath.Core.Exceptions;
    using static AngouriMath.Entity.Set;
    using static AngouriMath.Entity;
}

@lexer::members
{
    // As the declaration order of static fields is the initialization order
    // We will get null if we access the private static field _LiteralNames from static fields defined here
    // So these are instance fields
    public readonly CommonToken Multiply = new(Array.IndexOf(_LiteralNames, "'*'"), "*");
    public readonly CommonToken Power = new(Array.IndexOf(_LiteralNames, "'^'"), "^");
}
@parser::members
{
    // Nullable reference type analysis is disabled by default for generated code without '#nullable enable'
    public Entity Result = null;
    
    public void Parse() { this.statement(); }
}

@namespace { Antlr }

// Order of expressions do not affect parsing, but we put dependees first and dependents after to clarify logic 

// But according to https://stackoverflow.com/a/16490720/5429648, 
// You need to place parser rules (which start with a lowercase letter)
// before lexer rules (which start with an uppercase letter) in your grammar
// After encountering a lexer rule, the [ triggers a LEXER_CHAR_SET instead of ARG_ACTION,
// so the token stream seen by the compiler looks like you're passing a set of characters where the
// return value should be.

// So we put UPPER_CASE lexer rules after lower_case parser rules.

// atom is defined later
factorial_expression returns[Entity value]
    : p = atom '!' { $value = MathS.Factorial($p.value); }
    | p = atom { $value = $p.value; }
    ;

power_list returns[List<Entity> value]
    @init { $value = new(); }
    : ('^' factorial_expression { $value.Add($factorial_expression.value); })+
    | ('^' unary_expression { $value.Add($unary_expression.value); })+
    ;
    
power_expression returns[Entity value]
    : factorial_expression { $value = $factorial_expression.value; }
        (power_list {
            $value = $power_list.value
                        .Prepend($factorial_expression.value)
                        .Reverse()
                        .Aggregate((exp, @base) => @base.Pow(exp));
        })?
    ;
    
/*

Numerical nodes

*/

unary_expression returns[Entity value]
    : ('-' p = power_expression { $value = $p.value is Number num ? -num : -$p.value; } | 
       '+' p = power_expression { $value = $p.value; })
    | ('-' u = unary_expression { $value = -$u.value; } | 
       '+' u = unary_expression { $value = $u.value; })
    | p = power_expression { $value = $p.value; }
    ;
    
mult_expression returns[Entity value]
   : u1 = unary_expression { $value = $u1.value; } 
   ('*' u2 = unary_expression { $value = $value * $u2.value; } | 
    '/' u2 = unary_expression { $value = $value / $u2.value; })*
   ;
   
sum_expression returns[Entity value]
   : m1 = mult_expression { $value = $m1.value; }
   ('+' m2 = mult_expression { $value = $value + $m2.value; } | 
    '-' m2 = mult_expression { $value = $value - $m2.value; })*
   ;

/*

Sets

*/

set_operator_intersection returns[Entity value]
    : left = sum_expression { $value = $left.value; }
    (
    'intersect' right = sum_expression { $value = $value.Intersect($right.value); } |
    '/\\' right = sum_expression { $value = $value.Intersect($right.value); }
    )*
    ;

set_operator_union returns[Entity value]
    : left = set_operator_intersection { $value = $left.value; }
    (
    'unite' right = set_operator_intersection { $value = $value.Unite($right.value); } |
    '\\/' right = set_operator_intersection { $value = $value.Unite($right.value); }
    )*
    ;

set_operator_setsubtraction returns[Entity value]
    : left = set_operator_union { $value = $left.value; }
    (
    'setsubtract' right = set_operator_union { $value = $value.SetSubtract($right.value); } |
    '\\' right = set_operator_union { $value = $value.SetSubtract($right.value); }
    )*
    ;

in_operator returns[Entity value]
    : m1 = set_operator_setsubtraction { $value = $m1.value; }
    ('in' m2 = set_operator_setsubtraction { $value = $value.In($m2.value); })*
    ;


/*

Equality/inequality nodes

*/

inequality_expression returns[Entity value]
   : m1 = in_operator { $value = $m1.value; }
   (
    '>=' m2 = in_operator { $value = $value >= $m2.value; } | 
    '<=' m2 = in_operator { $value = $value <= $m2.value; } |
    '>' m2 = in_operator { $value = $value > $m2.value; } |
    '<' m2 = in_operator { $value = $value < $m2.value; } |
    'equalizes' m2 = in_operator { $value = MathS.Equality($value, $m2.value); })*
   ;

terms_list returns[List<Entity> terms]
    @init { $terms = new(); }
    : ('=' term = inequality_expression { $terms.Add($term.value); })+
    ;

equality_expression returns[Entity value]
    : expr = inequality_expression { $value = $expr.value; } (terms_list 
    {
        var list = $terms_list.terms.Prepend($value).ToArray();
        List<Entity> eqTerms = new();
        for (int i = 0; i < list.Length - 1; i++)
            eqTerms.Add(list[i].Equalizes(list[i + 1]));
        $value = eqTerms.Aggregate((eq1, eq2) => eq1 & eq2);
    })?
    ;

/*

Boolean nodes

*/

negate_expression returns[Entity value]
    : 'not' op = equality_expression { $value = !$op.value; }
    | 'not' opn = negate_expression { $value = !$opn.value; }
    | op = equality_expression { $value = $op.value; }
    ;

and_expression returns[Entity value]
    : m1 = negate_expression { $value = $m1.value; }
    ('and' m2 = negate_expression { $value = $value & $m2.value; } |
     '&' m2 = negate_expression { $value = $value & $m2.value; }
    )*
    ;

xor_expression returns[Entity value]
    : m1 = and_expression { $value = $m1.value; }
    ('xor' m2 = and_expression { $value = $value ^ $m2.value; })*
    ;

or_expression returns[Entity value]
    : m1 = xor_expression { $value = $m1.value; }
    ('or' m2 = xor_expression { $value = $value | $m2.value; } |
     '|' m2 = xor_expression { $value = $value | $m2.value; })*
    ;

implies_expression returns[Entity value]
    : m1 = or_expression { $value = $m1.value; }
    ('implies' m2 = or_expression { $value = $value.Implies($m2.value); } |
     '->' m2 = or_expression { $value = $value.Implies($m2.value); })*
    ;

/*

Keyword nodes

*/

provided_expression returns[Entity value]
    : expr = implies_expression { $value = $expr.value; }
    ('provided' pred = implies_expression { $value = $value.Provided($pred.value); })*
    ;

/*

Nodes end

*/

expression returns[Entity value]
    : s = provided_expression { $value = $s.value; }
    ;


function_arguments returns[List<Entity> list]
    @init { $list = new List<Entity>(); }
    : (e = expression { $list.Add($e.value); } (',' e = expression { $list.Add($e.value); })*)?
    ;

interval_arguments returns[(Entity from, Entity to) couple]
    : from = expression { $couple.from = $from.value; } ';' to = expression { $couple.to = $to.value; }
    ;
   
cset_arguments returns[(Entity variable, Entity predicate) couple]
    : variable = expression { $couple.variable = $variable.value; } ':' predicate = expression { $couple.predicate = $predicate.value; }
    ;

atom returns[Entity value]
    : '+oo' { $value = Entity.Number.Real.PositiveInfinity; }
    | '-oo' { $value = Entity.Number.Real.NegativeInfinity; }
    | NUMBER { $value = Entity.Number.Complex.Parse($NUMBER.text); }
    | BOOLEAN { $value = Entity.Boolean.Parse($BOOLEAN.text); }
    | SPECIALSET { $value = Entity.Set.SpecialSet.Create($SPECIALSET.text); }
    | VARIABLE { $value = Entity.Variable.CreateVariableUnchecked($VARIABLE.text); }
    | '(|' expression '|)' { $value = $expression.value.Abs(); }

    | '[' function_arguments ']T' { $value = ParsingHelpers.TryBuildingMatrix($function_arguments.list).T; }
    | '[' function_arguments ']' { $value = ParsingHelpers.TryBuildingMatrix($function_arguments.list); }

    | '(' interval_arguments ')' { $value = new Entity.Set.Interval($interval_arguments.couple.from, false, $interval_arguments.couple.to, false); }
    | '[' interval_arguments ')' { $value = new Entity.Set.Interval($interval_arguments.couple.from, true, $interval_arguments.couple.to, false); }
    | '[' interval_arguments ']' { $value = new Entity.Set.Interval($interval_arguments.couple.from, true, $interval_arguments.couple.to, true); }
    | '(' interval_arguments ']' { $value = new Entity.Set.Interval($interval_arguments.couple.from, false, $interval_arguments.couple.to, true); }
    | '(' expression ')' { $value = $expression.value; }
    | '{' cset_args = cset_arguments '}' { $value = new ConditionalSet($cset_args.couple.variable, $cset_args.couple.predicate); }
    | '{' args = function_arguments '}' { $value = new FiniteSet((IEnumerable<Entity>)$args.list); }
    | 'log(' args = function_arguments ')' { $value = Assert("log", (1, 2), $args.list.Count) ? MathS.Log(10, $args.list[0]) : MathS.Log($args.list[0], $args.list[1]); }
    | 'sqrt(' args = function_arguments ')' { Assert("sqrt", 1, $args.list.Count); $value = MathS.Sqrt($args.list[0]); }
    | 'cbrt(' args = function_arguments ')' { Assert("cbrt", 1, $args.list.Count); $value = MathS.Cbrt($args.list[0]); }
    | 'sqr(' args = function_arguments ')' { Assert("sqr", 1, $args.list.Count); $value = MathS.Sqr($args.list[0]); }
    | 'ln(' args = function_arguments ')' { Assert("ln", 1, $args.list.Count); $value = MathS.Ln($args.list[0]); }

    /* Trigonometric functions */
    | 'sin(' args = function_arguments ')' { Assert("sin", 1, $args.list.Count); $value = MathS.Sin($args.list[0]); }
    | 'cos(' args = function_arguments ')' { Assert("cos", 1, $args.list.Count); $value = MathS.Cos($args.list[0]); }
    | 'tan(' args = function_arguments ')' { Assert("tan", 1, $args.list.Count); $value = MathS.Tan($args.list[0]); }
    | 'cotan(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Cotan($args.list[0]); }
    | 'cot(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Cotan($args.list[0]); }
    | 'sec(' args = function_arguments ')' { Assert("sec", 1, $args.list.Count); $value = MathS.Sec($args.list[0]); }
    | 'cosec(' args = function_arguments ')' { Assert("cosec", 1, $args.list.Count); $value = MathS.Cosec($args.list[0]); }
    | 'csc(' args = function_arguments ')' { Assert("cosec", 1, $args.list.Count); $value = MathS.Cosec($args.list[0]); }
    | 'arcsin(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Arcsin($args.list[0]); }
    | 'arccos(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Arccos($args.list[0]); }
    | 'arctan(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Arctan($args.list[0]); }
    | 'arccotan(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Arccotan($args.list[0]); }
    | 'arcsec(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Arcsec($args.list[0]); }
    | 'arccosec(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Arccosec($args.list[0]); }
    | 'arccsc(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Arccosec($args.list[0]); }
    | 'acsc(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Arccosec($args.list[0]); }
    | 'asin(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Arcsin($args.list[0]); }
    | 'acos(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Arccos($args.list[0]); }
    | 'atan(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Arctan($args.list[0]); }
    | 'acotan(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Arccotan($args.list[0]); }
    | 'asec(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Arcsec($args.list[0]); }
    | 'acosec(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Arccosec($args.list[0]); }
    | 'acot(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Arccotan($args.list[0]); }
    | 'arccot(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Arccotan($args.list[0]); }
    /* End */


    /* Hyperbolic functions */
    | 'sinh(' args = function_arguments ')' { Assert("sin", 1, $args.list.Count); $value = MathS.Hyperbolic.Sinh($args.list[0]); }
    | 'sh(' args = function_arguments ')' { Assert("sin", 1, $args.list.Count); $value = MathS.Hyperbolic.Sinh($args.list[0]); }

    | 'cosh(' args = function_arguments ')' { Assert("cos", 1, $args.list.Count); $value = MathS.Hyperbolic.Cosh($args.list[0]); }
    | 'ch(' args = function_arguments ')' { Assert("cos", 1, $args.list.Count); $value = MathS.Hyperbolic.Cosh($args.list[0]); }

    | 'tanh(' args = function_arguments ')' { Assert("tan", 1, $args.list.Count); $value = MathS.Hyperbolic.Tanh($args.list[0]); }
    | 'th(' args = function_arguments ')' { Assert("tan", 1, $args.list.Count); $value = MathS.Hyperbolic.Tanh($args.list[0]); }

    | 'cotanh(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Cotanh($args.list[0]); }
    | 'coth(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Cotanh($args.list[0]); }
    | 'cth(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Cotanh($args.list[0]); }

    | 'sech(' args = function_arguments ')' { Assert("sec", 1, $args.list.Count); $value = MathS.Hyperbolic.Sech($args.list[0]); }
    | 'sch(' args = function_arguments ')' { Assert("sec", 1, $args.list.Count); $value = MathS.Hyperbolic.Sech($args.list[0]); }

    | 'cosech(' args = function_arguments ')' { Assert("cosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Cosech($args.list[0]); }
    | 'csch(' args = function_arguments ')' { Assert("cosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Cosech($args.list[0]); }

    | 'asinh(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsinh($args.list[0]); }
    | 'arsinh(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsinh($args.list[0]); }
    | 'arsh(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsinh($args.list[0]); }

    | 'acosh(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosh($args.list[0]); }
    | 'arcosh(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosh($args.list[0]); }
    | 'arch(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosh($args.list[0]); }

    | 'atanh(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Hyperbolic.Artanh($args.list[0]); }
    | 'artanh(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Hyperbolic.Artanh($args.list[0]); }
    | 'arth(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Hyperbolic.Artanh($args.list[0]); }

    | 'acoth(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcotanh($args.list[0]); }
    | 'arcoth(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcotanh($args.list[0]); }
    | 'acotanh(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcotanh($args.list[0]); }
    | 'arcotanh(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcotanh($args.list[0]); }
    | 'arcth(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcotanh($args.list[0]); }

    | 'asech(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsech($args.list[0]); }
    | 'arsech(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsech($args.list[0]); }
    | 'arsch(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arsech($args.list[0]); }

    | 'acosech(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosech($args.list[0]); }
    | 'arcosech(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosech($args.list[0]); }
    | 'arcsch(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosech($args.list[0]); }
    | 'acsch(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Hyperbolic.Arcosech($args.list[0]); }
    /* End */


    | 'gamma(' args = function_arguments ')' { Assert("gamma", 1, $args.list.Count); $value = MathS.Gamma($args.list[0]); }
    | 'derivative(' args = function_arguments ')' 
        {
            if (Assert("derivative", (3, 2), $args.list.Count))
            {
                if ($args.list[2] is Integer { EInteger: var asEInt })
                    $value = MathS.Derivative($args.list[0], $args.list[1], asEInt.ToInt32Checked());
                else
                    throw new InvalidArgumentParseException("Expected integer number for the third argument of derivative");
            } 
            else
                $value = MathS.Derivative($args.list[0], $args.list[1]);
        }
    | 'integral(' args = function_arguments ')' 
        { 
            if (Assert("integral", (3, 2), $args.list.Count))
            {
                if ($args.list[2] is Integer { EInteger: var asEInt })
                    $value = MathS.Integral($args.list[0], $args.list[1], asEInt.ToInt32Checked());
                else
                    throw new InvalidArgumentParseException("Expected number for the third argument of integral");
            }
            else
                $value = MathS.Integral($args.list[0], $args.list[1]);
        }
    | 'limit(' args = function_arguments ')' { Assert("limit", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2]); }
    | 'limitleft(' args = function_arguments ')' { Assert("limitleft", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2], AngouriMath.Core.ApproachFrom.Left); }
    | 'limitright(' args = function_arguments ')' { Assert("limitright", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2], AngouriMath.Core.ApproachFrom.Right); }
    | 'signum(' args = function_arguments ')' { Assert("signum", 1, $args.list.Count); $value = MathS.Signum($args.list[0]); }
    | 'sgn(' args = function_arguments ')' { Assert("sgn", 1, $args.list.Count); $value = MathS.Signum($args.list[0]); }
    | 'sign(' args = function_arguments ')' { Assert("sign", 1, $args.list.Count); $value = MathS.Signum($args.list[0]); }
    | 'abs(' args = function_arguments ')' { Assert("abs", 1, $args.list.Count); $value = MathS.Abs($args.list[0]); }
    | 'phi(' args = function_arguments ')' { Assert("phi", 1, $args.list.Count); $value = MathS.NumberTheory.Phi($args.list[0]); }
    | 'domain(' args = function_arguments ')' 
        { 
            Assert("domain", 2, $args.list.Count); 
            if ($args.list[1] is not SpecialSet ss)
                throw new InvalidArgumentParseException($"Unrecognized special set {$args.list[1].Stringize()}");
            $value = $args.list[0].WithCodomain(ss.ToDomain());
        }
    | 'piecewise(' args = function_arguments ')'
        {
            var cases = new List<Providedf>();
            foreach (var arg in $args.list)
                if (arg is Providedf provided)
                    cases.Add(provided);
                else
                    cases.Add(new Providedf(arg, true));
            $value = new Piecewise(cases);
        }
    | 'apply(' args = function_arguments ')'
        {
            if ($args.list.Count < 2)
                throw new FunctionArgumentCountException("Should be at least one argument in apply function");
            $value = $args.list[0].Apply($args.list.Skip(1).ToLList());
        }
    | 'lambda(' args = function_arguments ')'
        {
            if ($args.list.Count < 2)
                throw new FunctionArgumentCountException("Should be at least two arguments in lambda function");
            var body = $args.list.Last();
            foreach (var x in ((IEnumerable<Entity>)$args.list).Reverse().Skip(1))
            {
                if (x is not Variable v) throw new InvalidArgumentParseException($"Lambda is expected to have valid parameters, {x} encountered instead");
                body = body.LambdaOver(v);
            }
            $value = body;
        }
    ;

statement: expression EOF { Result = $expression.value; } ;

NEWLINE: ('\r'?'\n')+ -> skip ;

// A fragment will never be counted as a token, it only serves to simplify a grammar.
fragment EXPONENT: ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

NUMBER: ('0'..'9')+ '.' ('0'..'9')* EXPONENT? 'i'? | '.'? ('0'..'9')+ EXPONENT? 'i'? | 'i' ;

SPECIALSET: ('CC' | 'RR' | 'QQ' | 'ZZ' | 'BB') ;

BOOLEAN: ('true' | 'True' | 'false' | 'False') ;

VARIABLE: ('a'..'z'|'A'..'Z'|'\u0370'..'\u03FF'|'\u1F00'..'\u1FFF'|'\u0400'..'\u04FF')+ ('_' ('a'..'z'|'A'..'Z'|'0'..'9'|'\u0370'..'\u03FF'|'\u1F00'..'\u1FFF'|'\u0400'..'\u04FF')+)? ;
  
COMMENT: ('//' ~[\r\n]* '\r'? '\n' | '/*' .*? '*/') -> skip ;
    
WS : (' ' | '\t')+ -> skip ;