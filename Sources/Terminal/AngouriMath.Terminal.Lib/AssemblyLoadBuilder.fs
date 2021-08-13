module AngouriMath.Terminal.Lib.AssemblyLoadBuilder

open System
open Microsoft.DotNet.Interactive.FSharp
open AngouriMath.Terminal.Lib.Consts

type AssemblyLoadResult =
    | AssemblyLoadSuccess
    | AssemblyLoadFailure of string

type AssembliesLoadResult =
    | Success of FSharpKernel
    | Failure of string list

type AssemblyLoadBuilder (execute : FSharpKernel -> string -> ExecutionResult, kernel) =
    member this.YieldFrom x = this.load x
    member this.ReturnFrom x = Success kernel

    member this.Combine (a : AssemblyLoadResult, b : AssembliesLoadResult) =
        match (a, b) with
        | AssemblyLoadSuccess, Success kernel -> Success kernel
        | AssemblyLoadFailure reason, Success _ -> Failure [ reason ]
        | AssemblyLoadSuccess, Failure reasons -> Failure reasons
        | AssemblyLoadFailure reason, Failure reasons -> Failure (reason::reasons)

    member this.Delay(f) = f()

    member this.Zero () = Success

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