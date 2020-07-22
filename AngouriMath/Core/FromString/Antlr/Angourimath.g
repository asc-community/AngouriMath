/*

Remember to run the "antlr_rerun.bat" file at the project root every time you modify this file so that other
source files under the Antlr folder can update and be reflected in other parts of AngouriMath!
It only consists of commands that are consistent across CMD and Bash so you should be able to run that file
regardless of whether you are on Windows, Linux or Mac. You need to have an installed Java Runtime, however.

*/

grammar Angourimath;

options
{
    // Equivalent to passing "-Dlanguage=CSharp" into "antlr-4.8-complete.jar" in "antlr_rerun.bat"
    language = CSharp;
}

@header // These items are unindented because they appear in the generated source code verbatim
{
using System.Linq;
using System.Collections;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using System.Globalization;
}

@parser::members
{
    // Nullable reference type analysis is disabled by default for generated code without '#nullable enable'
    public Entity Result = null;
    
    public void Parse() { this.statement(); }
}

@namespace { Antlr }

NEWLINE  :  '\r'?'\n'
    ;

ID  :	('a'..'z'|'A'..'Z')+ ('_' ('a'..'z'|'A'..'Z'|'0'..'9')+)?
    ;

NUMBER
    : ('0'..'9')+ '.' ('0'..'9')* EXPONENT? 'i'?
    | '.'? ('0'..'9')+ EXPONENT? 'i'?
    ;

COMMENT
    :   ( '//' ~[\r\n]* '\r'? '\n'
        | '/*' .*? '*/'
        ) -> skip
    ;
    
WS : (' ' | '\t')+ -> skip
   ;

fragment
EXPONENT : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

statement
    : expression EOF { Result = $expression.value; }
    ;

expression returns[Entity value]
   	: s = sum_expression { $value = $s.value; }
   	;
    
power_expression returns[Entity value]
	: atom { $value = $atom.value; } (power_list {
		var list = $power_list.value;
		$value = list.Last();
		list.RemoveAt(list.Count - 1);
		list.Reverse(); 
		list.Add($atom.value);
		foreach(var p in list) { $value = MathS.Pow(p, $value); }
		})?
	;
	
power_list returns[List<Entity> value]
	@init
	{
		$value = new List<Entity>();
	}
    : ('^' atom { $value.Add($atom.value); })+
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
   
atom returns[Entity value]
    : NUMBER { $value = ComplexNumber.Parse($NUMBER.text); }
    | ID { $value = new VariableEntity($ID.text); }
    | '(' expression ')' { $value = $expression.value; }
    | ID '(' args = function_arguments ')' { $value = new FunctionEntity($ID.text + 'f'); foreach(var arg in $args.list) { $value.Children.Add(arg); } }
    | ID '(' ')' { $value = new FunctionEntity($ID.text + 'f'); }
    ;

function_arguments returns[List<Entity> list]
	@init
	{
		$list = new List<Entity>();
	}
    : e = expression { $list.Add($e.value); } (',' e = expression	{ $list.Add($e.value); })*
    ;