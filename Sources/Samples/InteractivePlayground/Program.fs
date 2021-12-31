open Plotly.NET 
open Plotly.NET.LayoutObjects
open Plotly.NET.TraceObjects
open AngouriMath.Interactive
open Plot

// withSlider1D polarLinear [ 0.1..0.1..4.0 ] [ 0.0..0.1..1.5 ] "a" "phi + sin(phi) * a" 
// |> Chart.show

let r = [ -1.57..0.1..1.57 ]


// withSlider3D sphericalScatter3D r r [ 1.0..0.1..15.0 ] "a" "phi_1 * (phi_2 + a)"
// |> Chart.show

let d = [ -2.0..0.02..2.0 ]
withSlider3D surface d d [ 2.0..0.1..4.0 ] "a" "(|x2 + y2 - a|)"
// |> Chart.show
|> GenericChart.toChartHTML
|> printfn "%s"

(*
/// Similar to numpy.arrange
let nparange (start: double) (stop:double) (step: double) =
    let stepsCount = ((stop-start) / step) |> int
    seq { for i in 0 .. stepsCount -> start + double(i) * step }
        |> Array.ofSeq

let steps = nparange 0. 5. 0.1
let scattersChart =
    steps
    |> Seq.map
        (fun step -> 
            // Create a scatter plot for every step
            let x = nparange 0. 10. 0.01
            let y = seq { for x_ in x -> sin(step * x_) }
            // Some plot must be visible here or the chart is empty at the beginning
            let chartVisibility = if step = 0. then StyleParam.Visible.True else StyleParam.Visible.False;
            let go =
                Chart.Scatter
                    (
                        x=x, y=y,
                        mode=StyleParam.Mode.Lines,
                        Name="v = " + string(step),
                        Marker = Marker.init(
                            Color = Color.fromHex("#00CED1"),
                            Size = 6
                        )
                    )
                |> Chart.withTraceName(Visible=chartVisibility)
            go
        )
    |> GenericChart.combine

let sliderSteps = 
    steps |> 
    Seq.indexed |>
    Seq.map
        (fun (i, step) ->
            // Create a visibility and a title parameters
            // The visibility parameter includes an array where every parameter
            // is mapped onto the trace visibility
            let visible =
                // Set true only for the current step
                (fun index -> index=i)
                |> Array.init steps.Length
                |> box
            let title =
                sprintf "Slider switched to step: %f" step
                |> box
            SliderStep.init(
                    Args = ["visible", visible; "title", title],
                    Method = StyleParam.Method.Update,
                    Label="v = " + string(step)
                )
        )

let slider =
    Slider.init(
            CurrentValue=SliderCurrentValue.init(Prefix="Frequency: "),
            Padding=Padding.init(T=50),
            Steps=sliderSteps
        )

let chart =
    scattersChart
    |> Chart.withSlider slider

chart |> Chart.show*)