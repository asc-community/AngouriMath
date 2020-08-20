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

@parser::header
{
    using System.Linq;
    using AngouriMath;
    using static AngouriMath.Core.Exceptions.FunctionArgumentCountException;
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
    @init { $value = new List<Entity>(); }
    : ('^' factorial_expression { $value.Add($factorial_expression.value); })+ ;

power_expression returns[Entity value]
    : factorial_expression { $value = $factorial_expression.value; } (power_list {
        var list = $power_list.value;
        $value = list.Last();
        list.RemoveAt(list.Count - 1);
        list.Reverse(); 
        list.Add($factorial_expression.value);
        foreach(var p in list) { $value = MathS.Pow(p, $value); }
    })?
    ;
    
unary_expression returns[Entity value]
    : ('-' p = power_expression { $value = -$p.value; } | 
       '+' p = power_expression { $value = $p.value; })
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

expression returns[Entity value]
    : s = sum_expression { $value = $s.value; }
    ;

function_arguments returns[List<Entity> list]
    @init { $list = new List<Entity>(); }
    : (e = expression { $list.Add($e.value); } (',' e = expression { $list.Add($e.value); })*)?
    ;
   
atom returns[Entity value]
    : NUMBER { $value = Entity.Number.Complex.Parse($NUMBER.text); }
    | VARIABLE { $value = MathS.Var($VARIABLE.text); }
    | '(' expression ')' { $value = $expression.value; }
    | 'sin(' args = function_arguments ')' { Assert("sin", 1, $args.list.Count); $value = MathS.Sin($args.list[0]); }
    | 'cos(' args = function_arguments ')' { Assert("cos", 1, $args.list.Count); $value = MathS.Cos($args.list[0]); }
    | 'log(' args = function_arguments ')' { $value = Assert("log", (1, 2), $args.list.Count) ? MathS.Log(10, $args.list[0]) : MathS.Log($args.list[0], $args.list[1]); }
    | 'sqrt(' args = function_arguments ')' { Assert("sqrt", 1, $args.list.Count); $value = MathS.Sqrt($args.list[0]); }
    | 'cbrt(' args = function_arguments ')' { Assert("cbrt", 1, $args.list.Count); $value = MathS.Cbrt($args.list[0]); }
    | 'sqr(' args = function_arguments ')' { Assert("sqr", 1, $args.list.Count); $value = MathS.Sqr($args.list[0]); }
    | 'ln(' args = function_arguments ')' { Assert("ln", 1, $args.list.Count); $value = MathS.Ln($args.list[0]); }
    | 'tan(' args = function_arguments ')' { Assert("tan", 1, $args.list.Count); $value = MathS.Tan($args.list[0]); }
    | 'cotan(' args = function_arguments ')' { Assert("cotan", 1, $args.list.Count); $value = MathS.Cotan($args.list[0]); }
    | 'sec(' args = function_arguments ')' { Assert("sec", 1, $args.list.Count); $value = MathS.Sec($args.list[0]); }
    | 'cosec(' args = function_arguments ')' { Assert("cosec", 1, $args.list.Count); $value = MathS.Cosec($args.list[0]); }
    | 'arcsin(' args = function_arguments ')' { Assert("arcsin", 1, $args.list.Count); $value = MathS.Arcsin($args.list[0]); }
    | 'arccos(' args = function_arguments ')' { Assert("arccos", 1, $args.list.Count); $value = MathS.Arccos($args.list[0]); }
    | 'arctan(' args = function_arguments ')' { Assert("arctan", 1, $args.list.Count); $value = MathS.Arctan($args.list[0]); }
    | 'arccotan(' args = function_arguments ')' { Assert("arccotan", 1, $args.list.Count); $value = MathS.Arccotan($args.list[0]); }
    | 'arcsec(' args = function_arguments ')' { Assert("arcsec", 1, $args.list.Count); $value = MathS.Arcsec($args.list[0]); }
    | 'arccosec(' args = function_arguments ')' { Assert("arccosec", 1, $args.list.Count); $value = MathS.Arccosec($args.list[0]); }
    | 'gamma(' args = function_arguments ')' { Assert("gamma", 1, $args.list.Count); $value = MathS.Gamma($args.list[0]); }
    | 'derivative(' args = function_arguments ')' { Assert("derivative", 3, $args.list.Count); $value = MathS.Derivative($args.list[0], $args.list[1], $args.list[2]); }
    | 'integral(' args = function_arguments ')' { Assert("integral", 3, $args.list.Count); $value = MathS.Integral($args.list[0], $args.list[1], $args.list[2]); }
    | 'limit(' args = function_arguments ')' { Assert("limit", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2]); }
    | 'limitleft(' args = function_arguments ')' { Assert("limitleft", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2], AngouriMath.Limits.ApproachFrom.Left); }
    | 'limitright(' args = function_arguments ')' { Assert("limitright", 3, $args.list.Count); $value = MathS.Limit($args.list[0], $args.list[1], $args.list[2], AngouriMath.Limits.ApproachFrom.Right); }
    ;

statement: expression EOF { Result = $expression.value; } ;

NEWLINE: '\r'?'\n' ;

// A fragment will never be counted as a token, it only serves to simplify a grammar.
fragment EXPONENT: ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

NUMBER: ('0'..'9')+ '.' ('0'..'9')* EXPONENT? 'i'? | '.'? ('0'..'9')+ EXPONENT? 'i'? | 'i' ;

VARIABLE: ('a'..'z'|'A'..'Z')+ ('_' ('a'..'z'|'A'..'Z'|'0'..'9')+)? ;
  
COMMENT: ('//' ~[\r\n]* '\r'? '\n' | '/*' .*? '*/') -> skip ;
    
WS : (' ' | '\t')+ -> skip ;