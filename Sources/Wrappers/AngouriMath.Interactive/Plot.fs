module AngouriMath.Interactive.Plot

open AngouriMath.FSharp.Compilation
open Plotly.NET
open AngouriMath.FSharp.Core

exception InvalidInput of string


let private getVarList (expr : AngouriMath.Entity) =
    expr.Vars |> List.ofSeq |> List.sortBy (fun v -> v.Name)


let private getSingleVariable (func : AngouriMath.Entity) =
    let vars = getVarList func
    match vars with
    | [ theOnlyVar ] -> theOnlyVar
    | _ -> raise (InvalidInput $"There should be exactly one variable, found: {vars}")


let private getOnlyTwoVariables (func : AngouriMath.Entity) =
    let vars = getVarList func
    match vars with
    | [ firstVar; secondVar ] -> firstVar, secondVar
    | _ -> raise (InvalidInput $"There should be exactly one variable, found: {vars}")


let private prepareLinearData (range : 'T seq) (func : obj) =
    let entity = parsed func
    let theOnlyVar = getSingleVariable entity
    let compiled = compiled1In<'T, double> theOnlyVar func
    let xData = List.ofSeq range
    let yData = List.map compiled xData
    (xData, yData)


let private prepareSurface3DData (xRange : 'T1 seq) (yRange : 'T2 seq) (func : obj) =
    let entity = parsed func
    let (firstVar, secondVar) = getOnlyTwoVariables entity
    let compiled = compiled2In<'T1, 'T2, double> firstVar secondVar func
    let xData = List.ofSeq xRange
    let yData = List.ofSeq yRange
    let zData = List.map (fun x -> List.map (fun y -> compiled x y) yData) xData
    (zData, xData, yData)
        

let private prepareQuadraticData (xRange : 'T1 seq) (yRange : 'T2 seq) (func : obj) =
    let entity = parsed func
    let (firstVar, secondVar) = getOnlyTwoVariables entity
    let compiled = compiled2In<'T1, 'T2, double> firstVar secondVar func
    let xDataRaw = List.ofSeq xRange
    let yDataRaw = List.ofSeq yRange
    let xy = List.allPairs xDataRaw yDataRaw
    let xData = List.map fst xy
    let yData = List.map snd xy
    let zData = List.map (fun (x, y) -> compiled x y) xy
    (zData, xData, yData)
        

let private withTransparency chart =
    chart
    |> Chart.withTemplate ChartTemplates.transparent

let linear (range : 'T seq) (func : obj) =
    let (xData, yData) = prepareLinearData range func
    Chart.Line (xData, yData)
    |> withTransparency

let scatter2D (range : 'T seq) (func : obj) =
    let (xData, yData) = prepareLinearData range func
    Chart.Point (xData, yData)
    |> withTransparency

let surface (xRange : 'T1 seq) (yRange : 'T2 seq) (func : obj) =
    let (zData, xData, yData) = prepareSurface3DData xRange yRange func
    Chart.Surface (zData, xData, yData)
    |> withTransparency

let scatter3D (xRange : 'T1 seq) (yRange : 'T2 seq) (func : obj) =
    let (zData, xData, yData) = prepareQuadraticData xRange yRange func
    Chart.Scatter3d (xData, yData, zData, StyleParam.Mode.Lines)
    |> withTransparency