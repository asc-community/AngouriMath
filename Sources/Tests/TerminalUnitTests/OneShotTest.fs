/// The commands amcli answers once and exits on, without opening the terminal.
/// https://github.com/asc-community/AngouriMath/issues/1649
module AngouriMath.Terminal.OneShotTest

open System.IO
open Xunit
open AngouriMath.Terminal.Lib

/// The exit code, what was printed, and what was printed to stderr.
let private run (args: string list) (stdin: string) =
    use input = new StringReader(stdin)
    use output = new StringWriter()
    use error = new StringWriter()
    let code = OneShot.run args input output error
    code, output.ToString().Replace("\r\n", "\n").TrimEnd('\n'), error.ToString()

let private answers (args: string list) (expected: string) =
    let code, printed, error = run args ""
    Assert.Equal("", error)
    Assert.Equal(0, code)
    Assert.Equal(expected, printed)

[<Fact>]
let ``simp simplifies`` () = answers [ "simp"; "sin(x)^2 + cos(x)^2" ] "1"

[<Fact>]
let ``fsimp normalises without the search`` () = answers [ "fsimp"; "x + 0" ] "x"

/// A rational is printed as a decimal, which is what the command has always promised.
[<Fact>]
let ``eval writes a rational as a decimal`` () = answers [ "eval"; "1 / 2" ] "0.5"

[<Fact>]
let ``eval decides a comparison`` () = answers [ "eval"; "e ^ pi > pi ^ e" ] "True"

[<Fact>]
let ``diff takes the variable first`` () = answers [ "diff"; "x"; "sin(x)" ] "cos(x)"

[<Fact>]
let ``latex writes LaTeX`` () = answers [ "latex"; "x ^ 2" ] "{x}^{2}"

[<Fact>]
let ``sub substitutes the value for the variable`` () = answers [ "sub"; "x"; "y"; "x + 1" ] "y + 1"

/// One root per line, so that a script can read them with a loop.
[<Fact>]
let ``solve writes finitely many roots one per line`` () =
    let code, printed, _ = run [ "solve"; "x"; "x ^ 2 - 1 = 0" ] ""
    Assert.Equal(0, code)
    Assert.Equal<string Set>(set [ "1"; "-1" ], printed.Split('\n') |> Set.ofArray)

[<Fact>]
let ``solve writes infinitely many roots on one line`` () =
    let code, printed, _ = run [ "solve"; "x"; "x ^ 2 > 1" ] ""
    Assert.Equal(0, code)
    Assert.DoesNotContain("\n", printed)

[<Fact>]
let ``an argument left out is read from stdin`` () =
    let code, printed, _ = run [ "diff"; "x" ] "1 + x ^ 2"
    Assert.Equal(0, code)
    Assert.Equal("2 * x", printed)

[<Fact>]
let ``an argument written as an underscore is read from stdin`` () =
    let code, printed, _ = run [ "sub"; "x"; "_"; "x + 1" ] "y"
    Assert.Equal(0, code)
    Assert.Equal("y + 1", printed)

[<Fact>]
let ``a missing argument with nothing on stdin is a usage error`` () =
    let code, printed, error = run [ "diff"; "x" ] ""
    Assert.Equal(2, code)
    Assert.Equal("", printed)
    Assert.Contains("amcli diff needs more arguments", error)

[<Fact>]
let ``a command that does not exist is a usage error, and names itself`` () =
    let code, _, error = run [ "frobnicate"; "x" ] ""
    Assert.Equal(2, code)
    Assert.Contains("amcli has no command frobnicate", error)

[<Fact>]
let ``input that does not parse fails, and says why`` () =
    let code, printed, error = run [ "simp"; "x + * 2" ] ""
    Assert.Equal(1, code)
    Assert.Equal("", printed)
    Assert.NotEqual<string>("", error.Trim())

[<Fact>]
let ``help lists every command`` () =
    let code, printed, _ = run [ "help" ] ""
    Assert.Equal(0, code)
    for command in [ "eval"; "simp"; "fsimp"; "diff"; "solve"; "sub"; "latex" ] do
        Assert.Contains($"amcli {command} ", printed)
