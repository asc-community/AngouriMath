using AngouriMath;
using AngouriMath.Extensions;
using System.CommandLine;

var rootCommand = new RootCommand("Sample app for System.CommandLine");

var exprArgument = new Argument<string?>(
    name: "expr",
    description: "Expression to evaluate, or none to read from standard input",
    getDefaultValue: () => null
);

var varOption = new Option<string?>(
    name: "--var",
    description: "Variable to use in the command",
    getDefaultValue: () => null
);

var evalCommand = new Command("eval", "Evaluate to a single number, boolean, or two numbers in case of complex");
evalCommand.AddArgument(exprArgument);
evalCommand.SetHandler(expr => {
    var ent = GetExpr(expr).ToEntity().Evaled;
    var res = ent.ToString();
    if (ent is Entity.Number.Rational rat)
        res = rat.RealPart.EDecimal.ToString();
    Console.WriteLine(res);
}, exprArgument);

var simplifyCommand = new Command("simplify", "Simplify an expression");
simplifyCommand.AddArgument(exprArgument);
simplifyCommand.SetHandler(expr => {
    Console.WriteLine(GetExpr(expr).ToEntity().Simplify());
}, exprArgument);

var solveCommand = new Command("solve", "Solve an equation or inequality");
solveCommand.AddArgument(exprArgument);
solveCommand.AddOption(varOption);
solveCommand.SetHandler((expr, v) => {
    expr = GetExpr(expr);
    var ent = expr.ToEntity();
    var vari = GetVar(v, ent);
    var sols = ent.Solve(vari);
    if (sols is Entity.Set.FiniteSet fs)
        foreach (var sol in fs)
        {
            Console.WriteLine(sol);
        }
    else
        Console.WriteLine(sols);
}, exprArgument, varOption);

var latexCommand = new Command("latex", "Convert an expression to LaTeX");
latexCommand.AddArgument(exprArgument);
latexCommand.SetHandler(expr => {
    Console.WriteLine(GetExpr(expr).ToEntity().Latexise());
}, exprArgument);

rootCommand.AddCommand(evalCommand);
rootCommand.AddCommand(simplifyCommand);
rootCommand.AddCommand(solveCommand);
rootCommand.AddCommand(latexCommand);

rootCommand.Invoke(System.Environment.GetCommandLineArgs());

static string GetExpr(string? opt)
{
    if (opt is null)
        return Console.ReadLine()!;
    return opt;
}

static Entity.Variable GetVar(string? v, Entity expr)
{
    if (v is null)
    {
        if (expr.Vars.Count() is 1) 
            return expr.Vars.First();
        throw new Exception("Cannot detect a variable, please, specify one");
    }
    return v;
}
