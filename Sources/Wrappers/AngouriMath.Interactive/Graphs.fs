module AngouriMath.Interactive.Graphs

open AngouriMath.FSharp.Compilation
open Plotly.NET
open AngouriMath.FSharp.Core

exception InvalidInput


let displayFunc (range : 'T seq) (func : obj) =
    let entity = parsed func
    let vars = entity.Vars |> List.ofSeq
    match vars with
    | theOnlyVar::[] ->
        let compiled = compiled1In<'T, double> theOnlyVar func
        let xData = List.ofSeq range
        let yData = List.map compiled xData
        Chart.Point (xData, yData)
    | _ -> raise InvalidInput


let displayFunc2D (xRange : 'T1 seq) (yRange : 'T2 seq) (func : obj) =
    let entity = parsed func
    let vars = entity.Vars |> List.ofSeq
    match vars with
    | firstVar::tail ->
        match tail with
        | secondVar::[] ->
            let compiled = compiled2In<'T1, 'T2, double> firstVar secondVar func
            let xData = List.ofSeq xRange
            let yData = List.ofSeq yRange
            let zData = List.map (fun x -> List.map (fun y -> compiled x y) yData) xData
            Chart.Surface (zData, xData, yData)
        | _ -> raise InvalidInput
    | _ -> raise InvalidInput