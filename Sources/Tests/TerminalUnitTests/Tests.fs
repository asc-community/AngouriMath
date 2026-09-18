module AngouriMath.Terminal.Tests

open System
open Xunit
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.PreRunCode
open AngouriMath.Terminal.Lib.Consts
open AngouriMath.Terminal.Lib.AssemblyLoadBuilder

let ``Create kernel or fail`` () =
    match createKernel () with
    | Result.Error reasons ->
        Assert.False(true, "Kernel failed to load")
        raise (Exception())
    | Result.Ok k -> k

let executeNew = ``Create kernel or fail`` () |> execute 

[<Fact>]
let ``Test no terminal 1`` () =
    executeNew "1 + 1"
    |> (fun actual -> PlainTextSuccess "2", actual)
    |> Assert.Equal

[<Fact>]
let ``Test no terminal 2`` () =
    executeNew "8 / 6"
    |> (fun actual -> PlainTextSuccess "1", actual)
    |> Assert.Equal

[<Fact>]
let ``Test no terminal 4`` () =
    executeNew "let x = 4"
    |> (fun actual -> VoidSuccess, actual)
    |> Assert.Equal

[<Fact>]
let ``Test no terminal 5`` () =
    executeNew "let x ="
    |> (fun actual -> 
        match actual with
        | Error error -> Assert.Contains("(1,1)-(1,8) parse", error)
        | _ -> Assert.True(false, $"Expected an error. Got {actual} instead")
        )


let executeNewWithAm code =
    let kernel = ``Create kernel or fail`` ()
    enableAngouriMath kernel |> (fun res -> 
        match res with
        | Error error -> false, error
        | _ -> true, "")
        |> Assert.True
    execute kernel code


// Every value a cell produces is kept in `run`, oldest first, and `back n` counts from the end;
// a cell without a value (a `let`) adds nothing. https://github.com/asc-community/AngouriMath/issues/601
[<Fact>]
let ``The values of earlier cells are kept in run`` () =
    let kernel = ``Create kernel or fail`` ()
    enableAngouriMath kernel |> ignore
    execute kernel "1 + 10" |> ignore
    execute kernel "let unused = 4" |> ignore
    execute kernel "2 * 10" |> ignore
    Assert.Equal(PlainTextSuccess "2", execute kernel "run.Count")
    // Asking is itself a cell with a value, so each question is remembered too.
    Assert.Equal(LatexSuccess ("11", "11"), execute kernel "run.[0]")
    Assert.Equal(LatexSuccess ("20", "20"), execute kernel "run.[1]")
    Assert.Equal(LatexSuccess ("11", "11"), execute kernel "back 5")
    Assert.Equal(PlainTextSuccess "6", execute kernel "run.Count")

[<Fact>]
let ``Test 1`` () =
    executeNewWithAm "1 + 1"
    |> (fun actual -> LatexSuccess ("2", "2"), actual)
    |> Assert.Equal

[<Fact>]
let ``Test 2`` () =
    executeNewWithAm "8 / 6"
    |> (fun actual -> LatexSuccess ("\\frac{4}{3}", "4/3"), actual)
    |> Assert.Equal

    
[<Fact>]
let ``Test 3`` () =
    executeNewWithAm "solutions \"x\" \"x2 = 4\""
    |> (fun actual -> LatexSuccess (@"\left\{ 2, -2 \right\}", "{ 2, -2 }"), actual)
    |> Assert.Equal
    