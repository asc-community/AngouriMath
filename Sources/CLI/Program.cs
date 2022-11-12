using AngouriMath;
using AngouriMath.Extensions;

var cliArgs = System.Environment.GetCommandLineArgs();
var reader = new ArgReader(cliArgs);

Entity expr;
Entity.Variable v;
string res;

var cmd = reader.Next();
switch (cmd)
{
    case "help":
        Console.WriteLine("""
        amcli (c) 2019-2022 Angouri
        This is free software. You're free to use,
        modify and redistribute it. MIT (Expat) license.
        
        Any argument can be received either as a CLI
        argument or through standard input. For example,

            amcli eval "1 + 1"
        
        is equivalent to

            echo "1 + 1" | amcli eval


        Possible options:

        amcli eval - to evaluate into a single number,
        boolean, or a+bi format for complex numbers.
        Expects one argument.

        Example:
            $ amcli "1 / 2"
            0.5
            $ amcli "e ^ pi > pi ^ e"
            true

        amcli diff - to differentiate the expression
        over the given variable (the first argument).
        Expects two arguments.

        Example:
            $ amcli diff "x" "sin(x)"
            cos(x)
            $ amcli diff "x" "1 + x^2"
            2 * x
            $ echo "1 + x^2" | amcli diff "x"
            2 * x

        amcli simplify - to simplify the expression.
        Expects one argument.

        Example:
            $ amcli simplify "sin(x)^2 + cos(x)^2"
            1

        amcli solve - to solve a *statement* over the
        given variable. A *statement* is an expression,
        otherwise evaluable to true or false (e. g. 
        "x > 3" is a statement, but "x ^ 2" is not).

        When the solution set is a finite solution, all
        solutions are written line-by-line. Otherwise,
        it's written as one line.

        Example:
            $ amcli solve "x" "x2 - 1 = 0"
            -1
            1

        """);
        break;

    case "eval":
        expr = reader.Next().ToEntity().Evaled;
        res = expr.ToString();
        if (expr is Entity.Number.Rational rat)
            res = rat.RealPart.EDecimal.ToString();
        Console.WriteLine(res);
        break;

    case "diff":
        v = (Entity.Variable)reader.Next();
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Differentiate(v));
        break;

    case "solve":
        v = (Entity.Variable)reader.Next();
        expr = reader.Next().ToEntity();
        var sols = expr.Solve(v);
        if (sols is Entity.Set.FiniteSet fs)
            foreach (var sol in fs)
                Console.WriteLine(sol);
        else
            Console.WriteLine(sols);
        break;

    case "latex":
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Latexise());
        break;

    case "simplify":
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Simplify());
        break;

    default:
        Console.WriteLine($"Unrecognized command `{cmd}`");
        break;
}


public sealed class ArgReader
{
    private readonly string[] args;
    private int curr = 1;
    public ArgReader(string[] args)
        => this.args = args;
    public string Next()
    {
        if (curr < args.Length)
        {
            var res = args[curr];
            curr++;
            return res;
        }
        return Console.ReadLine()!;
    }
}
