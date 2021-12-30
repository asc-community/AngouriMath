module AngouriMath.Interactive.Plot

open AngouriMath.FSharp.Compilation
open Plotly.NET
open AngouriMath.FSharp.Core
open Plotly.NET
open Plotly.NET.TraceObjects
open Plotly.NET.LayoutObjects

exception InvalidInput of string

(*
    For example assume I have n x m rectangle, then for diagonal d it'll be
    start: (0, d) provided d <= m, (n + m - d, n) otherwise
    end: (d, 0) provided d <= n, (m, n + m - d) otherwise 
*)
let span start finish =
    seq {
        let mutable x, y = start
        let endX, endY = finish
        while x <= endX && y >= endY do
            yield x, y
            x <- x + 1
            y <- y - 1
    } |> List.ofSeq


let diagnonalTraverse (seq1 : list<double>) (seq2 : list<double>) =
    let arr1 = Array.ofSeq seq1
    let arr2 = Array.ofSeq seq2
    let n = arr1.Length
    let m = arr2.Length
    seq {
        for d in 1 .. n + m - 1 do
            let start = if d <= m then 0, (d - 1) else (d - m, m - 1)
            let finish = if d <= n then (d - 1), 0 else (n - 1, d - n)
            for (x, y) in span start finish do
                yield arr1.[x], arr2.[y]
    } |> List.ofSeq

let private getVarList (expr : AngouriMath.Entity) =
    expr.Vars |> List.ofSeq |> List.sortBy (fun v -> v.Name)


let private getSingleVariable (func : AngouriMath.Entity) =
    let vars = getVarList func
    match vars with
    | [ ] -> symbol "x" // any variable is ok
    | [ theOnlyVar ] -> theOnlyVar
    | _ -> raise (InvalidInput $"There should be exactly one variable, found: {vars}")


let private getOnlyTwoVariables (func : AngouriMath.Entity) =
    let vars = getVarList func
    match vars with
    | [] -> symbol "x", symbol "y" // any variables are ok
    | [ oneVar ] -> oneVar, symbol ("t" + oneVar.Name)
    | [ firstVar; secondVar ] -> firstVar, secondVar
    | _ -> raise (InvalidInput $"There should be exactly two variables, found: {vars}")

let private getOnlyThreeVariables (func : AngouriMath.Entity) =
    let vars = getVarList func
    match vars with
    | [] -> symbol "x", symbol "y", symbol "z"     // any variables are ok
    | [ oneVar ] -> oneVar, symbol ("t" + oneVar.Name), symbol ("tt" + oneVar.Name)
    | [ firstVar; secondVar ] -> firstVar, secondVar, symbol ("tt" + (if firstVar.Name.Length > secondVar.Name.Length then firstVar else secondVar).Name)
    | [ firstVar; secondVar; thirdVar ] -> firstVar, secondVar, thirdVar
    | _ -> raise (InvalidInput $"There should be exactly three variables, found: {vars}")


let private compile1 (func : obj) =
    match func with
    | :? (double -> double) as res -> res
    | _  ->
        let var = getSingleVariable (parsed func)
        compiled1In<double, double> var func

let private compile2 (func : obj) =
    match func with
    | :? (double * double -> double) as res -> res
    | _ ->
        let (var1, var2) = getOnlyTwoVariables (parsed func)
        let f = compiled2In<double, double, double> var1 var2 func
        (fun (x, y) -> f x y)

let private prepareLinearData (range : double seq) (func : double -> double) =
    let xData = List.ofSeq range
    let yData = List.map func xData
    (xData, yData)

let private preparePolarData (range: double seq) (func : double -> double) =
    let (phiData, rData) = prepareLinearData range func
    let polar = List.zip phiData rData
    let (phi, r) = (symbol "phi", symbol "r")
    let compiledX = compiled2In<double, double, double> phi r "r * cos(phi)"
    let compiledY = compiled2In<double, double, double> phi r "r * sin(phi)"
    let xData = List.map (fun (phi, r) -> compiledX phi r) polar
    let yData = List.map (fun (phi, r) -> compiledY phi r) polar
    (xData, yData)

let private prepareSurface3DData (xRange : double seq) (yRange : double seq) (func : (double * double) -> double) =
    let xData = List.ofSeq xRange
    let yData = List.ofSeq yRange
    let zData = List.map (fun x -> List.map (fun y -> func(x, y)) yData) xData
    (zData, xData, yData)
        

let private getPolarToRegular3D () =
    compiled3In<double, double, double, double> "phi_1" "phi_2" "r" "r * sin(phi_2)",
    compiled3In<double, double, double, double> "phi_1" "phi_2" "r" "r * sin(phi_1) * cos(phi_2)",
    compiled3In<double, double, double, double> "phi_1" "phi_2" "r" "r * cos(phi_1) * cos(phi_2)"


let private prepareQuadraticData (xRange : double seq) (yRange : double seq) (func : (double * double) -> double) traverse =
    let compiled = compile2 func
    let xDataRaw = List.ofSeq xRange
    let yDataRaw = List.ofSeq yRange
    let xy = traverse xDataRaw yDataRaw
    let xData = List.map fst xy
    let yData = List.map snd xy
    let zData = List.map compiled xy
    (zData, xData, yData)


let private preparePolarSurfaceDataFromAngles (phi1Range : double seq) (phi2Range : double seq) (func : (double * double) -> double) =
    let (rData, phi1Data, phi2Data) = prepareQuadraticData phi1Range phi2Range func List.allPairs
    let (xOfPoint, yOfPoint, zOfPoint) = getPolarToRegular3D ()

    let data =
        (rData, phi1Data, phi2Data)
        |||> List.zip3

    let (x, y, z) =
        data
        |> List.map (fun (r, p1, p2) ->
            let x = xOfPoint p1 p2 r
            let y = yOfPoint p1 p2 r
            let z = zOfPoint p1 p2 r
            (x, y, z)
        )
        |> List.unzip3

    // let z = List.map (fun (r, p1, p2) -> List.map (fun (r, p1, p2) -> zOfPoint p1 p2 r) data) data
    (z, x, y)
        
let private preparePolarScatter3DFromAngles (phi1Range : double seq) (phi2Range : double seq) (func : double * double -> double) =
    let (rData, phi1Data, phi2Data) = prepareQuadraticData phi1Range phi2Range func diagnonalTraverse

    let (xOfPoint, yOfPoint, zOfPoint) = getPolarToRegular3D ()

    List.zip3 phi1Data phi2Data rData
    |> List.map (fun (phi1, phi2, r) ->
        xOfPoint phi1 phi2 r,
        yOfPoint phi1 phi2 r,
        zOfPoint phi1 phi2 r
    )
    |> List.unzip3


let private withTransparency chart =
    chart
    |> Chart.withTemplate ChartTemplates.transparent

let private getSlider (range : seq<float>) =
    let sliderSteps =
        range |> 
        Seq.indexed |>
        Seq.map
            (fun (i, step) ->
                let visible =
                    (fun index -> index=i)
                    |> Array.init (Seq.length range)
                    |> box
                SliderStep.init(
                        Args = [ "visible", visible ],
                        Method = StyleParam.Method.Update,
                        Label = $"v = {step:F3}"
                    )
            )
    Slider.init(
        CurrentValue=SliderCurrentValue.init(Prefix="Parameter value: "),
        Padding=Padding.init(T=50),
        Steps=sliderSteps
    )

let private castAs<'a, 'b> (a : 'a) : 'b =
    let up : obj = a :> obj
    up :?> 'b

let private withSliderND<'a, 'b> n (chartPlotter : 'a -> obj -> GenericChart.GenericChart) (ranges : 'a) (paramRange : double seq) (param : obj) (func : obj) =
    let var = symbol param
    let compiled =
        match n with
        | 1 ->
            let (v1, v2) = getOnlyTwoVariables (parsed func)
            let x = if v1 = var then v2 else v1
            let f = compiled2In<double, double, double> var x func
            castAs<_, double -> 'b -> double> f
        | 2 ->
            let (v1, v2, v3) = getOnlyThreeVariables (parsed func)
            let (x, y) =
                if var = v1 then (v2, v3)
                else if var = v2 then (v1, v3)
                else (v1, v2)
            let f = compiled3In<double, double, double, double> var x y func
            castAs<_, double -> 'b -> double> (fun p (x, y) -> f p x y)
        | _ -> raise (System.Exception $"Can't have {n}-dimensional slider")
    let charts = 
        paramRange
        |> Seq.map (fun step ->
            let newFunc = compiled step
            chartPlotter ranges newFunc
            |> Chart.withTraceName(Visible = if step = Seq.head paramRange then StyleParam.Visible.True else StyleParam.Visible.False)
        )
        |> GenericChart.combine
    let slider = getSlider paramRange

    charts
    |> Chart.withSlider slider
    |> withTransparency

let withSlider2D (chartPlotter : double seq -> obj -> GenericChart.GenericChart) (range : double seq) (paramRange : double seq) (param : obj) (func : obj) =
    withSliderND<double seq, double> 1 chartPlotter range paramRange param func

let withSlider3D (chartPlotter : double seq -> double seq -> obj -> GenericChart.GenericChart) (range1 : double seq) (range2 : double seq) (paramRange : double seq) (param : obj) (func : obj) =
    withSliderND<double seq * double seq, (double * double)> 2 (fun (r1, r2) f -> chartPlotter r1 r2 f) (range1, range2) paramRange param func

let linear (range : double seq) (func : obj) =
    let (xData, yData) = prepareLinearData range (compile1 func)
    Chart.Line (xData, yData)
    |> withTransparency


let polarLinear (range : double seq) (func : obj) =
    let (xData, yData) = preparePolarData range (compile1 func)
    Chart.Line (xData, yData)
    |> withTransparency
   
let polarScatter2D (range : double seq) (func : obj) =
    let (xData, yData) = preparePolarData range (compile1 func)
    Chart.Point (xData, yData)
    |> withTransparency


let private blend c (r1: int, g1: int, b1: int) (r2: int, g2: int, b2: int) =
    let blendChannel (coef : double) (channel1 : int) (channel2 : int) =
        (1. - coef) * float channel1 + coef * float channel2
        |> int
    blendChannel c r1 r2,
    blendChannel c g1 g2,
    blendChannel c b1 b2

let private colorPoints3D points colorA colorB =
    let distances =
        points
        |> List.map (fun (x, y, z) -> x * x + y * y + z * z |> sqrt)
    let maxDist =
        distances
        |> Seq.filter System.Double.IsFinite
        |> Seq.max
    distances
    |> List.map (fun d -> d / maxDist)
    |> List.map (fun c ->
        if System.Double.IsFinite c then
            blend c colorA colorB |||> Color.fromRGB
        else Color.fromKeyword ColorKeyword.Gray)

    
let sphericalScatter3D (phi1Range : double seq) (phi2Range : double seq) (func : obj) =
    let points = preparePolarScatter3DFromAngles phi1Range phi2Range (compile2 func)
    let zipped = points |||> List.zip3
    let colors = colorPoints3D zipped (255, 75, 75) (75, 75, 255)
    zipped
    |> (fun l -> Chart.Point3D(l, MarkerColor = Color.fromColors colors) )
    |> withTransparency

(*
let sphericalSurface (phi1Range : double seq) (phi2Range : double seq) (func : obj) =
    // preparePolarSurfaceDataFromAngles phi1Range phi2Range func
    // |> (fun (z, x, y) -> Chart.Surface(z, x, y))
    preparePolarScatter3DFromAngles phi1Range phi2Range (compile2 func)
    |> (fun (x, y, z) -> Chart.Mesh3D(x, y, z))
    |> withTransparency
*)

let scatter2D (range : double seq) (func : obj) =
    let (xData, yData) = prepareLinearData range (compile1 func)
    Chart.Point (xData, yData)
    |> withTransparency

let surface (xRange : double seq) (yRange : double seq) (func : obj) =
    let (zData, xData, yData) = prepareSurface3DData xRange yRange (compile2 func)
    Chart.Surface (zData, xData, yData)
    |> withTransparency

let scatter3D (xRange : double seq) (yRange : double seq) (func : obj) =
    prepareQuadraticData xRange yRange (compile2 func) List.allPairs
    |||> List.zip3
    |> Chart.Point3D
    |> withTransparency
