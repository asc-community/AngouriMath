using AngouriMath.Terminal;
using static AngouriMath.Terminal.Shared.ExecutionResult;
using AngouriMath.Terminal.Shared;
using System;

Console.WriteLine(
$@"
══════════════════════════════════════════════════════════════════════
                Welcome to AngouriMath.Terminal.

It is an interface to AngouriMath, open source symbolic algebra
library. The terminal uses F# Interactive inside, so that you can
run any command you could in normal F#. AngouriMath.FSharp is
being installed every start, so you are guaranteed to be on the
latest version of it. Type 'preRunCode' to see, what code
was pre-ran before you were able to type.
══════════════════════════════════════════════════════════════════════
".Trim());

Console.Write("Starting the kernel... ");
var ui = new UserInterface();

var ran = await FSharpInteractive.Create();
if (ran is DUnion.Item<Error>(var e))
    HandleResult(e);

var interactive = ((DUnion.Item<FSharpInteractive>)ran).Value;
await interactive.Execute("1 + 1");
var preRunCode = CodeSnippets.OpensAndOperators;
preRunCode += $"let preRunCode = \"{preRunCode.Replace("\"", "\\\"")}\"";
if (!HandleResult(await interactive.Execute(preRunCode))) return;
Console.WriteLine("started. You can start working.");

while (true)
{
    var input = ui.ReadLine();
    if (input is null)
        continue;
    switch (await interactive.Execute(input))
    {
        case PlainTextSuccess(var text):
            ui.WriteLine(text);
            break;
        case LatexSuccess(_, var text):
            ui.WriteLine(text);
            break;
        case Error(var message):
            ui.WriteLineError(message);
            break;
    }
}

static bool HandleResult(ExecutionResult res)
{
    if (res is Error(var message))
    {
        Console.WriteLine($"Error: {message}");
        Console.WriteLine($"Report about it to the official repo. The terminal will be closed.");
        Console.ReadLine();
        return false;
    }
    return true;
}