//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ./AngouriMath.g by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace AngouriMath.Core.Antlr {
using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
internal partial class AngouriMathLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		T__24=25, T__25=26, T__26=27, T__27=28, T__28=29, T__29=30, T__30=31, 
		T__31=32, T__32=33, T__33=34, T__34=35, T__35=36, T__36=37, T__37=38, 
		T__38=39, T__39=40, T__40=41, T__41=42, T__42=43, T__43=44, T__44=45, 
		T__45=46, T__46=47, T__47=48, T__48=49, T__49=50, T__50=51, T__51=52, 
		T__52=53, T__53=54, T__54=55, T__55=56, T__56=57, T__57=58, T__58=59, 
		T__59=60, T__60=61, T__61=62, T__62=63, T__63=64, T__64=65, T__65=66, 
		T__66=67, T__67=68, T__68=69, T__69=70, T__70=71, T__71=72, T__72=73, 
		T__73=74, T__74=75, T__75=76, T__76=77, T__77=78, T__78=79, T__79=80, 
		T__80=81, T__81=82, T__82=83, T__83=84, T__84=85, T__85=86, T__86=87, 
		T__87=88, T__88=89, T__89=90, T__90=91, T__91=92, T__92=93, T__93=94, 
		T__94=95, T__95=96, T__96=97, T__97=98, T__98=99, T__99=100, T__100=101, 
		T__101=102, T__102=103, T__103=104, T__104=105, T__105=106, T__106=107, 
		T__107=108, T__108=109, T__109=110, T__110=111, T__111=112, T__112=113, 
		T__113=114, T__114=115, T__115=116, T__116=117, T__117=118, T__118=119, 
		T__119=120, NEWLINE=121, NUMBER=122, SPECIALSET=123, BOOLEAN=124, VARIABLE=125, 
		COMMENT=126, WS=127;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "T__23", "T__24", 
		"T__25", "T__26", "T__27", "T__28", "T__29", "T__30", "T__31", "T__32", 
		"T__33", "T__34", "T__35", "T__36", "T__37", "T__38", "T__39", "T__40", 
		"T__41", "T__42", "T__43", "T__44", "T__45", "T__46", "T__47", "T__48", 
		"T__49", "T__50", "T__51", "T__52", "T__53", "T__54", "T__55", "T__56", 
		"T__57", "T__58", "T__59", "T__60", "T__61", "T__62", "T__63", "T__64", 
		"T__65", "T__66", "T__67", "T__68", "T__69", "T__70", "T__71", "T__72", 
		"T__73", "T__74", "T__75", "T__76", "T__77", "T__78", "T__79", "T__80", 
		"T__81", "T__82", "T__83", "T__84", "T__85", "T__86", "T__87", "T__88", 
		"T__89", "T__90", "T__91", "T__92", "T__93", "T__94", "T__95", "T__96", 
		"T__97", "T__98", "T__99", "T__100", "T__101", "T__102", "T__103", "T__104", 
		"T__105", "T__106", "T__107", "T__108", "T__109", "T__110", "T__111", 
		"T__112", "T__113", "T__114", "T__115", "T__116", "T__117", "T__118", 
		"T__119", "NEWLINE", "EXPONENT", "NUMBER", "SPECIALSET", "BOOLEAN", "VARIABLE", 
		"COMMENT", "WS"
	};


	    // As the declaration order of static fields is the initialization order
	    // We will get null if we access the private static field _LiteralNames from static fields defined here
	    // So these are instance fields
	    public readonly CommonToken Multiply = new(Array.IndexOf(_LiteralNames, "'*'"), "*");
	    public readonly CommonToken Power = new(Array.IndexOf(_LiteralNames, "'^'"), "^");


	public AngouriMathLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public AngouriMathLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'!'", "'^'", "'-'", "'+'", "'*'", "'/'", "'intersect'", "'/\\'", 
		"'unite'", "'\\/'", "'setsubtract'", "'\\'", "'in'", "'>='", "'<='", "'>'", 
		"'<'", "'equalizes'", "'='", "'not'", "'and'", "'&'", "'xor'", "'or'", 
		"'|'", "'implies'", "'->'", "'provided'", "','", "';'", "':'", "'+oo'", 
		"'-oo'", "'(|'", "'|)'", "'['", "']T'", "']'", "'('", "')'", "'{'", "'}'", 
		"'log('", "'sqrt('", "'cbrt('", "'sqr('", "'ln('", "'sin('", "'cos('", 
		"'tan('", "'cotan('", "'cot('", "'sec('", "'cosec('", "'csc('", "'arcsin('", 
		"'arccos('", "'arctan('", "'arccotan('", "'arcsec('", "'arccosec('", "'arccsc('", 
		"'acsc('", "'asin('", "'acos('", "'atan('", "'acotan('", "'asec('", "'acosec('", 
		"'acot('", "'arccot('", "'sinh('", "'sh('", "'cosh('", "'ch('", "'tanh('", 
		"'th('", "'cotanh('", "'coth('", "'cth('", "'sech('", "'sch('", "'cosech('", 
		"'csch('", "'asinh('", "'arsinh('", "'arsh('", "'acosh('", "'arcosh('", 
		"'arch('", "'atanh('", "'artanh('", "'arth('", "'acoth('", "'arcoth('", 
		"'acotanh('", "'arcotanh('", "'arcth('", "'asech('", "'arsech('", "'arsch('", 
		"'acosech('", "'arcosech('", "'arcsch('", "'acsch('", "'gamma('", "'derivative('", 
		"'integral('", "'limit('", "'limitleft('", "'limitright('", "'signum('", 
		"'sgn('", "'sign('", "'abs('", "'phi('", "'domain('", "'piecewise('", 
		"'apply('", "'lambda('"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, "NEWLINE", "NUMBER", "SPECIALSET", "BOOLEAN", "VARIABLE", "COMMENT", 
		"WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "AngouriMath.g"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static AngouriMathLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,127,1084,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,
		7,6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,
		14,7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,
		21,7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,
		28,7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,
		35,7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,
		42,7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,7,48,2,
		49,7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,2,55,7,55,2,
		56,7,56,2,57,7,57,2,58,7,58,2,59,7,59,2,60,7,60,2,61,7,61,2,62,7,62,2,
		63,7,63,2,64,7,64,2,65,7,65,2,66,7,66,2,67,7,67,2,68,7,68,2,69,7,69,2,
		70,7,70,2,71,7,71,2,72,7,72,2,73,7,73,2,74,7,74,2,75,7,75,2,76,7,76,2,
		77,7,77,2,78,7,78,2,79,7,79,2,80,7,80,2,81,7,81,2,82,7,82,2,83,7,83,2,
		84,7,84,2,85,7,85,2,86,7,86,2,87,7,87,2,88,7,88,2,89,7,89,2,90,7,90,2,
		91,7,91,2,92,7,92,2,93,7,93,2,94,7,94,2,95,7,95,2,96,7,96,2,97,7,97,2,
		98,7,98,2,99,7,99,2,100,7,100,2,101,7,101,2,102,7,102,2,103,7,103,2,104,
		7,104,2,105,7,105,2,106,7,106,2,107,7,107,2,108,7,108,2,109,7,109,2,110,
		7,110,2,111,7,111,2,112,7,112,2,113,7,113,2,114,7,114,2,115,7,115,2,116,
		7,116,2,117,7,117,2,118,7,118,2,119,7,119,2,120,7,120,2,121,7,121,2,122,
		7,122,2,123,7,123,2,124,7,124,2,125,7,125,2,126,7,126,2,127,7,127,1,0,
		1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,
		6,1,6,1,6,1,6,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,10,1,10,
		1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,1,12,1,12,
		1,12,1,13,1,13,1,13,1,14,1,14,1,14,1,15,1,15,1,16,1,16,1,17,1,17,1,17,
		1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,18,1,18,1,19,1,19,1,19,1,19,1,20,
		1,20,1,20,1,20,1,21,1,21,1,22,1,22,1,22,1,22,1,23,1,23,1,23,1,24,1,24,
		1,25,1,25,1,25,1,25,1,25,1,25,1,25,1,25,1,26,1,26,1,26,1,27,1,27,1,27,
		1,27,1,27,1,27,1,27,1,27,1,27,1,28,1,28,1,29,1,29,1,30,1,30,1,31,1,31,
		1,31,1,31,1,32,1,32,1,32,1,32,1,33,1,33,1,33,1,34,1,34,1,34,1,35,1,35,
		1,36,1,36,1,36,1,37,1,37,1,38,1,38,1,39,1,39,1,40,1,40,1,41,1,41,1,42,
		1,42,1,42,1,42,1,42,1,43,1,43,1,43,1,43,1,43,1,43,1,44,1,44,1,44,1,44,
		1,44,1,44,1,45,1,45,1,45,1,45,1,45,1,46,1,46,1,46,1,46,1,47,1,47,1,47,
		1,47,1,47,1,48,1,48,1,48,1,48,1,48,1,49,1,49,1,49,1,49,1,49,1,50,1,50,
		1,50,1,50,1,50,1,50,1,50,1,51,1,51,1,51,1,51,1,51,1,52,1,52,1,52,1,52,
		1,52,1,53,1,53,1,53,1,53,1,53,1,53,1,53,1,54,1,54,1,54,1,54,1,54,1,55,
		1,55,1,55,1,55,1,55,1,55,1,55,1,55,1,56,1,56,1,56,1,56,1,56,1,56,1,56,
		1,56,1,57,1,57,1,57,1,57,1,57,1,57,1,57,1,57,1,58,1,58,1,58,1,58,1,58,
		1,58,1,58,1,58,1,58,1,58,1,59,1,59,1,59,1,59,1,59,1,59,1,59,1,59,1,60,
		1,60,1,60,1,60,1,60,1,60,1,60,1,60,1,60,1,60,1,61,1,61,1,61,1,61,1,61,
		1,61,1,61,1,61,1,62,1,62,1,62,1,62,1,62,1,62,1,63,1,63,1,63,1,63,1,63,
		1,63,1,64,1,64,1,64,1,64,1,64,1,64,1,65,1,65,1,65,1,65,1,65,1,65,1,66,
		1,66,1,66,1,66,1,66,1,66,1,66,1,66,1,67,1,67,1,67,1,67,1,67,1,67,1,68,
		1,68,1,68,1,68,1,68,1,68,1,68,1,68,1,69,1,69,1,69,1,69,1,69,1,69,1,70,
		1,70,1,70,1,70,1,70,1,70,1,70,1,70,1,71,1,71,1,71,1,71,1,71,1,71,1,72,
		1,72,1,72,1,72,1,73,1,73,1,73,1,73,1,73,1,73,1,74,1,74,1,74,1,74,1,75,
		1,75,1,75,1,75,1,75,1,75,1,76,1,76,1,76,1,76,1,77,1,77,1,77,1,77,1,77,
		1,77,1,77,1,77,1,78,1,78,1,78,1,78,1,78,1,78,1,79,1,79,1,79,1,79,1,79,
		1,80,1,80,1,80,1,80,1,80,1,80,1,81,1,81,1,81,1,81,1,81,1,82,1,82,1,82,
		1,82,1,82,1,82,1,82,1,82,1,83,1,83,1,83,1,83,1,83,1,83,1,84,1,84,1,84,
		1,84,1,84,1,84,1,84,1,85,1,85,1,85,1,85,1,85,1,85,1,85,1,85,1,86,1,86,
		1,86,1,86,1,86,1,86,1,87,1,87,1,87,1,87,1,87,1,87,1,87,1,88,1,88,1,88,
		1,88,1,88,1,88,1,88,1,88,1,89,1,89,1,89,1,89,1,89,1,89,1,90,1,90,1,90,
		1,90,1,90,1,90,1,90,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,92,1,92,
		1,92,1,92,1,92,1,92,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,94,1,94,1,94,
		1,94,1,94,1,94,1,94,1,94,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,
		1,96,1,96,1,96,1,96,1,96,1,96,1,96,1,96,1,96,1,96,1,97,1,97,1,97,1,97,
		1,97,1,97,1,97,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,99,1,99,1,99,1,99,
		1,99,1,99,1,99,1,99,1,100,1,100,1,100,1,100,1,100,1,100,1,100,1,101,1,
		101,1,101,1,101,1,101,1,101,1,101,1,101,1,101,1,102,1,102,1,102,1,102,
		1,102,1,102,1,102,1,102,1,102,1,102,1,103,1,103,1,103,1,103,1,103,1,103,
		1,103,1,103,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,105,1,105,1,105,
		1,105,1,105,1,105,1,105,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,
		1,106,1,106,1,106,1,106,1,107,1,107,1,107,1,107,1,107,1,107,1,107,1,107,
		1,107,1,107,1,108,1,108,1,108,1,108,1,108,1,108,1,108,1,109,1,109,1,109,
		1,109,1,109,1,109,1,109,1,109,1,109,1,109,1,109,1,110,1,110,1,110,1,110,
		1,110,1,110,1,110,1,110,1,110,1,110,1,110,1,110,1,111,1,111,1,111,1,111,
		1,111,1,111,1,111,1,111,1,112,1,112,1,112,1,112,1,112,1,113,1,113,1,113,
		1,113,1,113,1,113,1,114,1,114,1,114,1,114,1,114,1,115,1,115,1,115,1,115,
		1,115,1,116,1,116,1,116,1,116,1,116,1,116,1,116,1,116,1,117,1,117,1,117,
		1,117,1,117,1,117,1,117,1,117,1,117,1,117,1,117,1,118,1,118,1,118,1,118,
		1,118,1,118,1,118,1,119,1,119,1,119,1,119,1,119,1,119,1,119,1,119,1,120,
		3,120,952,8,120,1,120,4,120,955,8,120,11,120,12,120,956,1,120,1,120,1,
		121,1,121,3,121,963,8,121,1,121,4,121,966,8,121,11,121,12,121,967,1,122,
		4,122,971,8,122,11,122,12,122,972,1,122,1,122,5,122,977,8,122,10,122,12,
		122,980,9,122,1,122,3,122,983,8,122,1,122,3,122,986,8,122,1,122,3,122,
		989,8,122,1,122,4,122,992,8,122,11,122,12,122,993,1,122,3,122,997,8,122,
		1,122,3,122,1000,8,122,1,122,3,122,1003,8,122,1,123,1,123,1,123,1,123,
		1,123,1,123,1,123,1,123,1,123,1,123,3,123,1015,8,123,1,124,1,124,1,124,
		1,124,1,124,1,124,1,124,1,124,1,124,1,124,1,124,1,124,1,124,1,124,1,124,
		1,124,1,124,1,124,3,124,1035,8,124,1,125,4,125,1038,8,125,11,125,12,125,
		1039,1,125,1,125,4,125,1044,8,125,11,125,12,125,1045,3,125,1048,8,125,
		1,126,1,126,1,126,1,126,5,126,1054,8,126,10,126,12,126,1057,9,126,1,126,
		3,126,1060,8,126,1,126,1,126,1,126,1,126,1,126,5,126,1067,8,126,10,126,
		12,126,1070,9,126,1,126,1,126,3,126,1074,8,126,1,126,1,126,1,127,4,127,
		1079,8,127,11,127,12,127,1080,1,127,1,127,1,1068,0,128,1,1,3,2,5,3,7,4,
		9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,12,25,13,27,14,29,15,31,16,33,17,
		35,18,37,19,39,20,41,21,43,22,45,23,47,24,49,25,51,26,53,27,55,28,57,29,
		59,30,61,31,63,32,65,33,67,34,69,35,71,36,73,37,75,38,77,39,79,40,81,41,
		83,42,85,43,87,44,89,45,91,46,93,47,95,48,97,49,99,50,101,51,103,52,105,
		53,107,54,109,55,111,56,113,57,115,58,117,59,119,60,121,61,123,62,125,
		63,127,64,129,65,131,66,133,67,135,68,137,69,139,70,141,71,143,72,145,
		73,147,74,149,75,151,76,153,77,155,78,157,79,159,80,161,81,163,82,165,
		83,167,84,169,85,171,86,173,87,175,88,177,89,179,90,181,91,183,92,185,
		93,187,94,189,95,191,96,193,97,195,98,197,99,199,100,201,101,203,102,205,
		103,207,104,209,105,211,106,213,107,215,108,217,109,219,110,221,111,223,
		112,225,113,227,114,229,115,231,116,233,117,235,118,237,119,239,120,241,
		121,243,0,245,122,247,123,249,124,251,125,253,126,255,127,1,0,6,2,0,69,
		69,101,101,2,0,43,43,45,45,4,0,65,90,97,122,880,1279,7936,8191,5,0,48,
		57,65,90,97,122,880,1279,7936,8191,2,0,10,10,13,13,2,0,9,9,32,32,1111,
		0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,
		0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,
		1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,
		0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,
		1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,
		0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,
		1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,
		0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,
		1,0,0,0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,
		0,0,101,1,0,0,0,0,103,1,0,0,0,0,105,1,0,0,0,0,107,1,0,0,0,0,109,1,0,0,
		0,0,111,1,0,0,0,0,113,1,0,0,0,0,115,1,0,0,0,0,117,1,0,0,0,0,119,1,0,0,
		0,0,121,1,0,0,0,0,123,1,0,0,0,0,125,1,0,0,0,0,127,1,0,0,0,0,129,1,0,0,
		0,0,131,1,0,0,0,0,133,1,0,0,0,0,135,1,0,0,0,0,137,1,0,0,0,0,139,1,0,0,
		0,0,141,1,0,0,0,0,143,1,0,0,0,0,145,1,0,0,0,0,147,1,0,0,0,0,149,1,0,0,
		0,0,151,1,0,0,0,0,153,1,0,0,0,0,155,1,0,0,0,0,157,1,0,0,0,0,159,1,0,0,
		0,0,161,1,0,0,0,0,163,1,0,0,0,0,165,1,0,0,0,0,167,1,0,0,0,0,169,1,0,0,
		0,0,171,1,0,0,0,0,173,1,0,0,0,0,175,1,0,0,0,0,177,1,0,0,0,0,179,1,0,0,
		0,0,181,1,0,0,0,0,183,1,0,0,0,0,185,1,0,0,0,0,187,1,0,0,0,0,189,1,0,0,
		0,0,191,1,0,0,0,0,193,1,0,0,0,0,195,1,0,0,0,0,197,1,0,0,0,0,199,1,0,0,
		0,0,201,1,0,0,0,0,203,1,0,0,0,0,205,1,0,0,0,0,207,1,0,0,0,0,209,1,0,0,
		0,0,211,1,0,0,0,0,213,1,0,0,0,0,215,1,0,0,0,0,217,1,0,0,0,0,219,1,0,0,
		0,0,221,1,0,0,0,0,223,1,0,0,0,0,225,1,0,0,0,0,227,1,0,0,0,0,229,1,0,0,
		0,0,231,1,0,0,0,0,233,1,0,0,0,0,235,1,0,0,0,0,237,1,0,0,0,0,239,1,0,0,
		0,0,241,1,0,0,0,0,245,1,0,0,0,0,247,1,0,0,0,0,249,1,0,0,0,0,251,1,0,0,
		0,0,253,1,0,0,0,0,255,1,0,0,0,1,257,1,0,0,0,3,259,1,0,0,0,5,261,1,0,0,
		0,7,263,1,0,0,0,9,265,1,0,0,0,11,267,1,0,0,0,13,269,1,0,0,0,15,279,1,0,
		0,0,17,282,1,0,0,0,19,288,1,0,0,0,21,291,1,0,0,0,23,303,1,0,0,0,25,305,
		1,0,0,0,27,308,1,0,0,0,29,311,1,0,0,0,31,314,1,0,0,0,33,316,1,0,0,0,35,
		318,1,0,0,0,37,328,1,0,0,0,39,330,1,0,0,0,41,334,1,0,0,0,43,338,1,0,0,
		0,45,340,1,0,0,0,47,344,1,0,0,0,49,347,1,0,0,0,51,349,1,0,0,0,53,357,1,
		0,0,0,55,360,1,0,0,0,57,369,1,0,0,0,59,371,1,0,0,0,61,373,1,0,0,0,63,375,
		1,0,0,0,65,379,1,0,0,0,67,383,1,0,0,0,69,386,1,0,0,0,71,389,1,0,0,0,73,
		391,1,0,0,0,75,394,1,0,0,0,77,396,1,0,0,0,79,398,1,0,0,0,81,400,1,0,0,
		0,83,402,1,0,0,0,85,404,1,0,0,0,87,409,1,0,0,0,89,415,1,0,0,0,91,421,1,
		0,0,0,93,426,1,0,0,0,95,430,1,0,0,0,97,435,1,0,0,0,99,440,1,0,0,0,101,
		445,1,0,0,0,103,452,1,0,0,0,105,457,1,0,0,0,107,462,1,0,0,0,109,469,1,
		0,0,0,111,474,1,0,0,0,113,482,1,0,0,0,115,490,1,0,0,0,117,498,1,0,0,0,
		119,508,1,0,0,0,121,516,1,0,0,0,123,526,1,0,0,0,125,534,1,0,0,0,127,540,
		1,0,0,0,129,546,1,0,0,0,131,552,1,0,0,0,133,558,1,0,0,0,135,566,1,0,0,
		0,137,572,1,0,0,0,139,580,1,0,0,0,141,586,1,0,0,0,143,594,1,0,0,0,145,
		600,1,0,0,0,147,604,1,0,0,0,149,610,1,0,0,0,151,614,1,0,0,0,153,620,1,
		0,0,0,155,624,1,0,0,0,157,632,1,0,0,0,159,638,1,0,0,0,161,643,1,0,0,0,
		163,649,1,0,0,0,165,654,1,0,0,0,167,662,1,0,0,0,169,668,1,0,0,0,171,675,
		1,0,0,0,173,683,1,0,0,0,175,689,1,0,0,0,177,696,1,0,0,0,179,704,1,0,0,
		0,181,710,1,0,0,0,183,717,1,0,0,0,185,725,1,0,0,0,187,731,1,0,0,0,189,
		738,1,0,0,0,191,746,1,0,0,0,193,755,1,0,0,0,195,765,1,0,0,0,197,772,1,
		0,0,0,199,779,1,0,0,0,201,787,1,0,0,0,203,794,1,0,0,0,205,803,1,0,0,0,
		207,813,1,0,0,0,209,821,1,0,0,0,211,828,1,0,0,0,213,835,1,0,0,0,215,847,
		1,0,0,0,217,857,1,0,0,0,219,864,1,0,0,0,221,875,1,0,0,0,223,887,1,0,0,
		0,225,895,1,0,0,0,227,900,1,0,0,0,229,906,1,0,0,0,231,911,1,0,0,0,233,
		916,1,0,0,0,235,924,1,0,0,0,237,935,1,0,0,0,239,942,1,0,0,0,241,954,1,
		0,0,0,243,960,1,0,0,0,245,1002,1,0,0,0,247,1014,1,0,0,0,249,1034,1,0,0,
		0,251,1037,1,0,0,0,253,1073,1,0,0,0,255,1078,1,0,0,0,257,258,5,33,0,0,
		258,2,1,0,0,0,259,260,5,94,0,0,260,4,1,0,0,0,261,262,5,45,0,0,262,6,1,
		0,0,0,263,264,5,43,0,0,264,8,1,0,0,0,265,266,5,42,0,0,266,10,1,0,0,0,267,
		268,5,47,0,0,268,12,1,0,0,0,269,270,5,105,0,0,270,271,5,110,0,0,271,272,
		5,116,0,0,272,273,5,101,0,0,273,274,5,114,0,0,274,275,5,115,0,0,275,276,
		5,101,0,0,276,277,5,99,0,0,277,278,5,116,0,0,278,14,1,0,0,0,279,280,5,
		47,0,0,280,281,5,92,0,0,281,16,1,0,0,0,282,283,5,117,0,0,283,284,5,110,
		0,0,284,285,5,105,0,0,285,286,5,116,0,0,286,287,5,101,0,0,287,18,1,0,0,
		0,288,289,5,92,0,0,289,290,5,47,0,0,290,20,1,0,0,0,291,292,5,115,0,0,292,
		293,5,101,0,0,293,294,5,116,0,0,294,295,5,115,0,0,295,296,5,117,0,0,296,
		297,5,98,0,0,297,298,5,116,0,0,298,299,5,114,0,0,299,300,5,97,0,0,300,
		301,5,99,0,0,301,302,5,116,0,0,302,22,1,0,0,0,303,304,5,92,0,0,304,24,
		1,0,0,0,305,306,5,105,0,0,306,307,5,110,0,0,307,26,1,0,0,0,308,309,5,62,
		0,0,309,310,5,61,0,0,310,28,1,0,0,0,311,312,5,60,0,0,312,313,5,61,0,0,
		313,30,1,0,0,0,314,315,5,62,0,0,315,32,1,0,0,0,316,317,5,60,0,0,317,34,
		1,0,0,0,318,319,5,101,0,0,319,320,5,113,0,0,320,321,5,117,0,0,321,322,
		5,97,0,0,322,323,5,108,0,0,323,324,5,105,0,0,324,325,5,122,0,0,325,326,
		5,101,0,0,326,327,5,115,0,0,327,36,1,0,0,0,328,329,5,61,0,0,329,38,1,0,
		0,0,330,331,5,110,0,0,331,332,5,111,0,0,332,333,5,116,0,0,333,40,1,0,0,
		0,334,335,5,97,0,0,335,336,5,110,0,0,336,337,5,100,0,0,337,42,1,0,0,0,
		338,339,5,38,0,0,339,44,1,0,0,0,340,341,5,120,0,0,341,342,5,111,0,0,342,
		343,5,114,0,0,343,46,1,0,0,0,344,345,5,111,0,0,345,346,5,114,0,0,346,48,
		1,0,0,0,347,348,5,124,0,0,348,50,1,0,0,0,349,350,5,105,0,0,350,351,5,109,
		0,0,351,352,5,112,0,0,352,353,5,108,0,0,353,354,5,105,0,0,354,355,5,101,
		0,0,355,356,5,115,0,0,356,52,1,0,0,0,357,358,5,45,0,0,358,359,5,62,0,0,
		359,54,1,0,0,0,360,361,5,112,0,0,361,362,5,114,0,0,362,363,5,111,0,0,363,
		364,5,118,0,0,364,365,5,105,0,0,365,366,5,100,0,0,366,367,5,101,0,0,367,
		368,5,100,0,0,368,56,1,0,0,0,369,370,5,44,0,0,370,58,1,0,0,0,371,372,5,
		59,0,0,372,60,1,0,0,0,373,374,5,58,0,0,374,62,1,0,0,0,375,376,5,43,0,0,
		376,377,5,111,0,0,377,378,5,111,0,0,378,64,1,0,0,0,379,380,5,45,0,0,380,
		381,5,111,0,0,381,382,5,111,0,0,382,66,1,0,0,0,383,384,5,40,0,0,384,385,
		5,124,0,0,385,68,1,0,0,0,386,387,5,124,0,0,387,388,5,41,0,0,388,70,1,0,
		0,0,389,390,5,91,0,0,390,72,1,0,0,0,391,392,5,93,0,0,392,393,5,84,0,0,
		393,74,1,0,0,0,394,395,5,93,0,0,395,76,1,0,0,0,396,397,5,40,0,0,397,78,
		1,0,0,0,398,399,5,41,0,0,399,80,1,0,0,0,400,401,5,123,0,0,401,82,1,0,0,
		0,402,403,5,125,0,0,403,84,1,0,0,0,404,405,5,108,0,0,405,406,5,111,0,0,
		406,407,5,103,0,0,407,408,5,40,0,0,408,86,1,0,0,0,409,410,5,115,0,0,410,
		411,5,113,0,0,411,412,5,114,0,0,412,413,5,116,0,0,413,414,5,40,0,0,414,
		88,1,0,0,0,415,416,5,99,0,0,416,417,5,98,0,0,417,418,5,114,0,0,418,419,
		5,116,0,0,419,420,5,40,0,0,420,90,1,0,0,0,421,422,5,115,0,0,422,423,5,
		113,0,0,423,424,5,114,0,0,424,425,5,40,0,0,425,92,1,0,0,0,426,427,5,108,
		0,0,427,428,5,110,0,0,428,429,5,40,0,0,429,94,1,0,0,0,430,431,5,115,0,
		0,431,432,5,105,0,0,432,433,5,110,0,0,433,434,5,40,0,0,434,96,1,0,0,0,
		435,436,5,99,0,0,436,437,5,111,0,0,437,438,5,115,0,0,438,439,5,40,0,0,
		439,98,1,0,0,0,440,441,5,116,0,0,441,442,5,97,0,0,442,443,5,110,0,0,443,
		444,5,40,0,0,444,100,1,0,0,0,445,446,5,99,0,0,446,447,5,111,0,0,447,448,
		5,116,0,0,448,449,5,97,0,0,449,450,5,110,0,0,450,451,5,40,0,0,451,102,
		1,0,0,0,452,453,5,99,0,0,453,454,5,111,0,0,454,455,5,116,0,0,455,456,5,
		40,0,0,456,104,1,0,0,0,457,458,5,115,0,0,458,459,5,101,0,0,459,460,5,99,
		0,0,460,461,5,40,0,0,461,106,1,0,0,0,462,463,5,99,0,0,463,464,5,111,0,
		0,464,465,5,115,0,0,465,466,5,101,0,0,466,467,5,99,0,0,467,468,5,40,0,
		0,468,108,1,0,0,0,469,470,5,99,0,0,470,471,5,115,0,0,471,472,5,99,0,0,
		472,473,5,40,0,0,473,110,1,0,0,0,474,475,5,97,0,0,475,476,5,114,0,0,476,
		477,5,99,0,0,477,478,5,115,0,0,478,479,5,105,0,0,479,480,5,110,0,0,480,
		481,5,40,0,0,481,112,1,0,0,0,482,483,5,97,0,0,483,484,5,114,0,0,484,485,
		5,99,0,0,485,486,5,99,0,0,486,487,5,111,0,0,487,488,5,115,0,0,488,489,
		5,40,0,0,489,114,1,0,0,0,490,491,5,97,0,0,491,492,5,114,0,0,492,493,5,
		99,0,0,493,494,5,116,0,0,494,495,5,97,0,0,495,496,5,110,0,0,496,497,5,
		40,0,0,497,116,1,0,0,0,498,499,5,97,0,0,499,500,5,114,0,0,500,501,5,99,
		0,0,501,502,5,99,0,0,502,503,5,111,0,0,503,504,5,116,0,0,504,505,5,97,
		0,0,505,506,5,110,0,0,506,507,5,40,0,0,507,118,1,0,0,0,508,509,5,97,0,
		0,509,510,5,114,0,0,510,511,5,99,0,0,511,512,5,115,0,0,512,513,5,101,0,
		0,513,514,5,99,0,0,514,515,5,40,0,0,515,120,1,0,0,0,516,517,5,97,0,0,517,
		518,5,114,0,0,518,519,5,99,0,0,519,520,5,99,0,0,520,521,5,111,0,0,521,
		522,5,115,0,0,522,523,5,101,0,0,523,524,5,99,0,0,524,525,5,40,0,0,525,
		122,1,0,0,0,526,527,5,97,0,0,527,528,5,114,0,0,528,529,5,99,0,0,529,530,
		5,99,0,0,530,531,5,115,0,0,531,532,5,99,0,0,532,533,5,40,0,0,533,124,1,
		0,0,0,534,535,5,97,0,0,535,536,5,99,0,0,536,537,5,115,0,0,537,538,5,99,
		0,0,538,539,5,40,0,0,539,126,1,0,0,0,540,541,5,97,0,0,541,542,5,115,0,
		0,542,543,5,105,0,0,543,544,5,110,0,0,544,545,5,40,0,0,545,128,1,0,0,0,
		546,547,5,97,0,0,547,548,5,99,0,0,548,549,5,111,0,0,549,550,5,115,0,0,
		550,551,5,40,0,0,551,130,1,0,0,0,552,553,5,97,0,0,553,554,5,116,0,0,554,
		555,5,97,0,0,555,556,5,110,0,0,556,557,5,40,0,0,557,132,1,0,0,0,558,559,
		5,97,0,0,559,560,5,99,0,0,560,561,5,111,0,0,561,562,5,116,0,0,562,563,
		5,97,0,0,563,564,5,110,0,0,564,565,5,40,0,0,565,134,1,0,0,0,566,567,5,
		97,0,0,567,568,5,115,0,0,568,569,5,101,0,0,569,570,5,99,0,0,570,571,5,
		40,0,0,571,136,1,0,0,0,572,573,5,97,0,0,573,574,5,99,0,0,574,575,5,111,
		0,0,575,576,5,115,0,0,576,577,5,101,0,0,577,578,5,99,0,0,578,579,5,40,
		0,0,579,138,1,0,0,0,580,581,5,97,0,0,581,582,5,99,0,0,582,583,5,111,0,
		0,583,584,5,116,0,0,584,585,5,40,0,0,585,140,1,0,0,0,586,587,5,97,0,0,
		587,588,5,114,0,0,588,589,5,99,0,0,589,590,5,99,0,0,590,591,5,111,0,0,
		591,592,5,116,0,0,592,593,5,40,0,0,593,142,1,0,0,0,594,595,5,115,0,0,595,
		596,5,105,0,0,596,597,5,110,0,0,597,598,5,104,0,0,598,599,5,40,0,0,599,
		144,1,0,0,0,600,601,5,115,0,0,601,602,5,104,0,0,602,603,5,40,0,0,603,146,
		1,0,0,0,604,605,5,99,0,0,605,606,5,111,0,0,606,607,5,115,0,0,607,608,5,
		104,0,0,608,609,5,40,0,0,609,148,1,0,0,0,610,611,5,99,0,0,611,612,5,104,
		0,0,612,613,5,40,0,0,613,150,1,0,0,0,614,615,5,116,0,0,615,616,5,97,0,
		0,616,617,5,110,0,0,617,618,5,104,0,0,618,619,5,40,0,0,619,152,1,0,0,0,
		620,621,5,116,0,0,621,622,5,104,0,0,622,623,5,40,0,0,623,154,1,0,0,0,624,
		625,5,99,0,0,625,626,5,111,0,0,626,627,5,116,0,0,627,628,5,97,0,0,628,
		629,5,110,0,0,629,630,5,104,0,0,630,631,5,40,0,0,631,156,1,0,0,0,632,633,
		5,99,0,0,633,634,5,111,0,0,634,635,5,116,0,0,635,636,5,104,0,0,636,637,
		5,40,0,0,637,158,1,0,0,0,638,639,5,99,0,0,639,640,5,116,0,0,640,641,5,
		104,0,0,641,642,5,40,0,0,642,160,1,0,0,0,643,644,5,115,0,0,644,645,5,101,
		0,0,645,646,5,99,0,0,646,647,5,104,0,0,647,648,5,40,0,0,648,162,1,0,0,
		0,649,650,5,115,0,0,650,651,5,99,0,0,651,652,5,104,0,0,652,653,5,40,0,
		0,653,164,1,0,0,0,654,655,5,99,0,0,655,656,5,111,0,0,656,657,5,115,0,0,
		657,658,5,101,0,0,658,659,5,99,0,0,659,660,5,104,0,0,660,661,5,40,0,0,
		661,166,1,0,0,0,662,663,5,99,0,0,663,664,5,115,0,0,664,665,5,99,0,0,665,
		666,5,104,0,0,666,667,5,40,0,0,667,168,1,0,0,0,668,669,5,97,0,0,669,670,
		5,115,0,0,670,671,5,105,0,0,671,672,5,110,0,0,672,673,5,104,0,0,673,674,
		5,40,0,0,674,170,1,0,0,0,675,676,5,97,0,0,676,677,5,114,0,0,677,678,5,
		115,0,0,678,679,5,105,0,0,679,680,5,110,0,0,680,681,5,104,0,0,681,682,
		5,40,0,0,682,172,1,0,0,0,683,684,5,97,0,0,684,685,5,114,0,0,685,686,5,
		115,0,0,686,687,5,104,0,0,687,688,5,40,0,0,688,174,1,0,0,0,689,690,5,97,
		0,0,690,691,5,99,0,0,691,692,5,111,0,0,692,693,5,115,0,0,693,694,5,104,
		0,0,694,695,5,40,0,0,695,176,1,0,0,0,696,697,5,97,0,0,697,698,5,114,0,
		0,698,699,5,99,0,0,699,700,5,111,0,0,700,701,5,115,0,0,701,702,5,104,0,
		0,702,703,5,40,0,0,703,178,1,0,0,0,704,705,5,97,0,0,705,706,5,114,0,0,
		706,707,5,99,0,0,707,708,5,104,0,0,708,709,5,40,0,0,709,180,1,0,0,0,710,
		711,5,97,0,0,711,712,5,116,0,0,712,713,5,97,0,0,713,714,5,110,0,0,714,
		715,5,104,0,0,715,716,5,40,0,0,716,182,1,0,0,0,717,718,5,97,0,0,718,719,
		5,114,0,0,719,720,5,116,0,0,720,721,5,97,0,0,721,722,5,110,0,0,722,723,
		5,104,0,0,723,724,5,40,0,0,724,184,1,0,0,0,725,726,5,97,0,0,726,727,5,
		114,0,0,727,728,5,116,0,0,728,729,5,104,0,0,729,730,5,40,0,0,730,186,1,
		0,0,0,731,732,5,97,0,0,732,733,5,99,0,0,733,734,5,111,0,0,734,735,5,116,
		0,0,735,736,5,104,0,0,736,737,5,40,0,0,737,188,1,0,0,0,738,739,5,97,0,
		0,739,740,5,114,0,0,740,741,5,99,0,0,741,742,5,111,0,0,742,743,5,116,0,
		0,743,744,5,104,0,0,744,745,5,40,0,0,745,190,1,0,0,0,746,747,5,97,0,0,
		747,748,5,99,0,0,748,749,5,111,0,0,749,750,5,116,0,0,750,751,5,97,0,0,
		751,752,5,110,0,0,752,753,5,104,0,0,753,754,5,40,0,0,754,192,1,0,0,0,755,
		756,5,97,0,0,756,757,5,114,0,0,757,758,5,99,0,0,758,759,5,111,0,0,759,
		760,5,116,0,0,760,761,5,97,0,0,761,762,5,110,0,0,762,763,5,104,0,0,763,
		764,5,40,0,0,764,194,1,0,0,0,765,766,5,97,0,0,766,767,5,114,0,0,767,768,
		5,99,0,0,768,769,5,116,0,0,769,770,5,104,0,0,770,771,5,40,0,0,771,196,
		1,0,0,0,772,773,5,97,0,0,773,774,5,115,0,0,774,775,5,101,0,0,775,776,5,
		99,0,0,776,777,5,104,0,0,777,778,5,40,0,0,778,198,1,0,0,0,779,780,5,97,
		0,0,780,781,5,114,0,0,781,782,5,115,0,0,782,783,5,101,0,0,783,784,5,99,
		0,0,784,785,5,104,0,0,785,786,5,40,0,0,786,200,1,0,0,0,787,788,5,97,0,
		0,788,789,5,114,0,0,789,790,5,115,0,0,790,791,5,99,0,0,791,792,5,104,0,
		0,792,793,5,40,0,0,793,202,1,0,0,0,794,795,5,97,0,0,795,796,5,99,0,0,796,
		797,5,111,0,0,797,798,5,115,0,0,798,799,5,101,0,0,799,800,5,99,0,0,800,
		801,5,104,0,0,801,802,5,40,0,0,802,204,1,0,0,0,803,804,5,97,0,0,804,805,
		5,114,0,0,805,806,5,99,0,0,806,807,5,111,0,0,807,808,5,115,0,0,808,809,
		5,101,0,0,809,810,5,99,0,0,810,811,5,104,0,0,811,812,5,40,0,0,812,206,
		1,0,0,0,813,814,5,97,0,0,814,815,5,114,0,0,815,816,5,99,0,0,816,817,5,
		115,0,0,817,818,5,99,0,0,818,819,5,104,0,0,819,820,5,40,0,0,820,208,1,
		0,0,0,821,822,5,97,0,0,822,823,5,99,0,0,823,824,5,115,0,0,824,825,5,99,
		0,0,825,826,5,104,0,0,826,827,5,40,0,0,827,210,1,0,0,0,828,829,5,103,0,
		0,829,830,5,97,0,0,830,831,5,109,0,0,831,832,5,109,0,0,832,833,5,97,0,
		0,833,834,5,40,0,0,834,212,1,0,0,0,835,836,5,100,0,0,836,837,5,101,0,0,
		837,838,5,114,0,0,838,839,5,105,0,0,839,840,5,118,0,0,840,841,5,97,0,0,
		841,842,5,116,0,0,842,843,5,105,0,0,843,844,5,118,0,0,844,845,5,101,0,
		0,845,846,5,40,0,0,846,214,1,0,0,0,847,848,5,105,0,0,848,849,5,110,0,0,
		849,850,5,116,0,0,850,851,5,101,0,0,851,852,5,103,0,0,852,853,5,114,0,
		0,853,854,5,97,0,0,854,855,5,108,0,0,855,856,5,40,0,0,856,216,1,0,0,0,
		857,858,5,108,0,0,858,859,5,105,0,0,859,860,5,109,0,0,860,861,5,105,0,
		0,861,862,5,116,0,0,862,863,5,40,0,0,863,218,1,0,0,0,864,865,5,108,0,0,
		865,866,5,105,0,0,866,867,5,109,0,0,867,868,5,105,0,0,868,869,5,116,0,
		0,869,870,5,108,0,0,870,871,5,101,0,0,871,872,5,102,0,0,872,873,5,116,
		0,0,873,874,5,40,0,0,874,220,1,0,0,0,875,876,5,108,0,0,876,877,5,105,0,
		0,877,878,5,109,0,0,878,879,5,105,0,0,879,880,5,116,0,0,880,881,5,114,
		0,0,881,882,5,105,0,0,882,883,5,103,0,0,883,884,5,104,0,0,884,885,5,116,
		0,0,885,886,5,40,0,0,886,222,1,0,0,0,887,888,5,115,0,0,888,889,5,105,0,
		0,889,890,5,103,0,0,890,891,5,110,0,0,891,892,5,117,0,0,892,893,5,109,
		0,0,893,894,5,40,0,0,894,224,1,0,0,0,895,896,5,115,0,0,896,897,5,103,0,
		0,897,898,5,110,0,0,898,899,5,40,0,0,899,226,1,0,0,0,900,901,5,115,0,0,
		901,902,5,105,0,0,902,903,5,103,0,0,903,904,5,110,0,0,904,905,5,40,0,0,
		905,228,1,0,0,0,906,907,5,97,0,0,907,908,5,98,0,0,908,909,5,115,0,0,909,
		910,5,40,0,0,910,230,1,0,0,0,911,912,5,112,0,0,912,913,5,104,0,0,913,914,
		5,105,0,0,914,915,5,40,0,0,915,232,1,0,0,0,916,917,5,100,0,0,917,918,5,
		111,0,0,918,919,5,109,0,0,919,920,5,97,0,0,920,921,5,105,0,0,921,922,5,
		110,0,0,922,923,5,40,0,0,923,234,1,0,0,0,924,925,5,112,0,0,925,926,5,105,
		0,0,926,927,5,101,0,0,927,928,5,99,0,0,928,929,5,101,0,0,929,930,5,119,
		0,0,930,931,5,105,0,0,931,932,5,115,0,0,932,933,5,101,0,0,933,934,5,40,
		0,0,934,236,1,0,0,0,935,936,5,97,0,0,936,937,5,112,0,0,937,938,5,112,0,
		0,938,939,5,108,0,0,939,940,5,121,0,0,940,941,5,40,0,0,941,238,1,0,0,0,
		942,943,5,108,0,0,943,944,5,97,0,0,944,945,5,109,0,0,945,946,5,98,0,0,
		946,947,5,100,0,0,947,948,5,97,0,0,948,949,5,40,0,0,949,240,1,0,0,0,950,
		952,5,13,0,0,951,950,1,0,0,0,951,952,1,0,0,0,952,953,1,0,0,0,953,955,5,
		10,0,0,954,951,1,0,0,0,955,956,1,0,0,0,956,954,1,0,0,0,956,957,1,0,0,0,
		957,958,1,0,0,0,958,959,6,120,0,0,959,242,1,0,0,0,960,962,7,0,0,0,961,
		963,7,1,0,0,962,961,1,0,0,0,962,963,1,0,0,0,963,965,1,0,0,0,964,966,2,
		48,57,0,965,964,1,0,0,0,966,967,1,0,0,0,967,965,1,0,0,0,967,968,1,0,0,
		0,968,244,1,0,0,0,969,971,2,48,57,0,970,969,1,0,0,0,971,972,1,0,0,0,972,
		970,1,0,0,0,972,973,1,0,0,0,973,974,1,0,0,0,974,978,5,46,0,0,975,977,2,
		48,57,0,976,975,1,0,0,0,977,980,1,0,0,0,978,976,1,0,0,0,978,979,1,0,0,
		0,979,982,1,0,0,0,980,978,1,0,0,0,981,983,3,243,121,0,982,981,1,0,0,0,
		982,983,1,0,0,0,983,985,1,0,0,0,984,986,5,105,0,0,985,984,1,0,0,0,985,
		986,1,0,0,0,986,1003,1,0,0,0,987,989,5,46,0,0,988,987,1,0,0,0,988,989,
		1,0,0,0,989,991,1,0,0,0,990,992,2,48,57,0,991,990,1,0,0,0,992,993,1,0,
		0,0,993,991,1,0,0,0,993,994,1,0,0,0,994,996,1,0,0,0,995,997,3,243,121,
		0,996,995,1,0,0,0,996,997,1,0,0,0,997,999,1,0,0,0,998,1000,5,105,0,0,999,
		998,1,0,0,0,999,1000,1,0,0,0,1000,1003,1,0,0,0,1001,1003,5,105,0,0,1002,
		970,1,0,0,0,1002,988,1,0,0,0,1002,1001,1,0,0,0,1003,246,1,0,0,0,1004,1005,
		5,67,0,0,1005,1015,5,67,0,0,1006,1007,5,82,0,0,1007,1015,5,82,0,0,1008,
		1009,5,81,0,0,1009,1015,5,81,0,0,1010,1011,5,90,0,0,1011,1015,5,90,0,0,
		1012,1013,5,66,0,0,1013,1015,5,66,0,0,1014,1004,1,0,0,0,1014,1006,1,0,
		0,0,1014,1008,1,0,0,0,1014,1010,1,0,0,0,1014,1012,1,0,0,0,1015,248,1,0,
		0,0,1016,1017,5,116,0,0,1017,1018,5,114,0,0,1018,1019,5,117,0,0,1019,1035,
		5,101,0,0,1020,1021,5,84,0,0,1021,1022,5,114,0,0,1022,1023,5,117,0,0,1023,
		1035,5,101,0,0,1024,1025,5,102,0,0,1025,1026,5,97,0,0,1026,1027,5,108,
		0,0,1027,1028,5,115,0,0,1028,1035,5,101,0,0,1029,1030,5,70,0,0,1030,1031,
		5,97,0,0,1031,1032,5,108,0,0,1032,1033,5,115,0,0,1033,1035,5,101,0,0,1034,
		1016,1,0,0,0,1034,1020,1,0,0,0,1034,1024,1,0,0,0,1034,1029,1,0,0,0,1035,
		250,1,0,0,0,1036,1038,7,2,0,0,1037,1036,1,0,0,0,1038,1039,1,0,0,0,1039,
		1037,1,0,0,0,1039,1040,1,0,0,0,1040,1047,1,0,0,0,1041,1043,5,95,0,0,1042,
		1044,7,3,0,0,1043,1042,1,0,0,0,1044,1045,1,0,0,0,1045,1043,1,0,0,0,1045,
		1046,1,0,0,0,1046,1048,1,0,0,0,1047,1041,1,0,0,0,1047,1048,1,0,0,0,1048,
		252,1,0,0,0,1049,1050,5,47,0,0,1050,1051,5,47,0,0,1051,1055,1,0,0,0,1052,
		1054,8,4,0,0,1053,1052,1,0,0,0,1054,1057,1,0,0,0,1055,1053,1,0,0,0,1055,
		1056,1,0,0,0,1056,1059,1,0,0,0,1057,1055,1,0,0,0,1058,1060,5,13,0,0,1059,
		1058,1,0,0,0,1059,1060,1,0,0,0,1060,1061,1,0,0,0,1061,1074,5,10,0,0,1062,
		1063,5,47,0,0,1063,1064,5,42,0,0,1064,1068,1,0,0,0,1065,1067,9,0,0,0,1066,
		1065,1,0,0,0,1067,1070,1,0,0,0,1068,1069,1,0,0,0,1068,1066,1,0,0,0,1069,
		1071,1,0,0,0,1070,1068,1,0,0,0,1071,1072,5,42,0,0,1072,1074,5,47,0,0,1073,
		1049,1,0,0,0,1073,1062,1,0,0,0,1074,1075,1,0,0,0,1075,1076,6,126,0,0,1076,
		254,1,0,0,0,1077,1079,7,5,0,0,1078,1077,1,0,0,0,1079,1080,1,0,0,0,1080,
		1078,1,0,0,0,1080,1081,1,0,0,0,1081,1082,1,0,0,0,1082,1083,6,127,0,0,1083,
		256,1,0,0,0,24,0,951,956,962,967,972,978,982,985,988,993,996,999,1002,
		1014,1034,1039,1045,1047,1055,1059,1068,1073,1080,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace AngouriMath.Core.Antlr
