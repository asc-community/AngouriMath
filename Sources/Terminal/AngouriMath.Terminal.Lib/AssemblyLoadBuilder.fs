module AngouriMath.Terminal.Lib.AssemblyLoadBuilder

open System
open Microsoft.DotNet.Interactive.FSharp
open AngouriMath.Terminal.Lib.Consts

type AssemblyLoadResult =
    | AssemblyLoadSuccess
    | AssemblyLoadFailure of string

type AssemblyLoadBuilder (execute : FSharpKernel -> string -> ExecutionResult, kernel) =
    member this.Yield x = this.load x
    member _.Run _ = Result.Ok kernel

    member this.Combine (a : AssemblyLoadResult, b) =
        match (a, b) with
        | AssemblyLoadSuccess, Result.Ok kernel -> Result.Ok kernel
        | AssemblyLoadFailure reason, Result.Ok _ -> Result.Error [ reason ]
        | AssemblyLoadSuccess, Result.Error reasons -> Result.Error reasons
        | AssemblyLoadFailure reason, Result.Error reasons -> Result.Error (reason::reasons)

    member this.Delay(f) = f()

    member this.Zero () = Result.Ok ()

    member private this.loadAssembly kernel (path : string) =
        path.Replace("\\", "\\\\")
        |> (fun loc -> execute kernel $"#r \"{loc}\"")

    member private this.load (typeInfo : Type) =
        let assemblyLocation = typeInfo.Assembly.Location
        if System.IO.File.Exists assemblyLocation then
            match this.loadAssembly kernel assemblyLocation with
            | Error error -> AssemblyLoadFailure error
            | _ -> AssemblyLoadSuccess
        else
            AssemblyLoadFailure $"Assembly {assemblyLocation} does not exist"