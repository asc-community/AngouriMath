module UserInterface

open System
open Spectre.Console
open RadLine

let init () = 
    Console.Title <- "AngouriMath Terminal."

let private getPrefix prefix =
    let time = DateTime.Now.ToString("HH:mm:ss")
    $"\n{prefix}[{time}] = "


let readLine (lineEditor : LineEditor) =
    lineEditor.ReadLine(System.Threading.CancellationToken.None).Result


let writeLine (ansiConsole : IAnsiConsole) (input : string) =
    printf "\n"
    Panel input
    |> ExpandableExtensions.Expand
    |> HasBoxBorderExtensions.RoundedBorder
    |> (fun p -> HasBorderExtensions.BorderColor(p, Color.Green))
    |> ansiConsole.Write


let writeLineError (ansiConsole : IAnsiConsole) (input : string) =
    printf "\n"
    Panel input
    |> (fun p -> PanelExtensions.Header(p, "Error"))
    |> ExpandableExtensions.Expand
    |> HasBoxBorderExtensions.RoundedBorder
    |> (fun p -> HasBorderExtensions.BorderColor(p, Color.Red))
    |> ansiConsole.Write

    
let getWordHighlighter () =
    let rec addWord words style (highlighter : WordHighlighter) =
        match words with
        | hd::tl -> 
            highlighter.AddWord(hd, style) |> ignore
            addWord tl style highlighter
        | _ -> ()

    let highlighter = WordHighlighter ()

    let keywordStyle = new Style(Color.LightSlateBlue);

    let keywords = ["let"; "int"]

    addWord keywords keywordStyle highlighter

    highlighter

type TimePrefixEditorPromt() =
    interface ILineEditorPrompt with
        member this.GetPrompt(state : ILineEditorState, line : int) = 
            let escaped = getPrefix "In" |> Markup.Escape |> (fun c -> c.Trim())
            let markup = Markup (escaped, Style Color.White)
            markup, 1
    

let getLineEditor (ansiConsole : IAnsiConsole) =
    let lineEditor = LineEditor (ansiConsole, null)
    lineEditor.Highlighter <- getWordHighlighter ()
    lineEditor.Prompt <- TimePrefixEditorPromt ()
    lineEditor