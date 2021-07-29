module UserInterface

open System

let private time () = DateTime.Now.ToString("HH:mm:ss")

let init () = 
    Console.Title <- "AngouriMath Terminal."

let private printPrefix prefix =
    Console.ForegroundColor <- ConsoleColor.DarkGray
    Console.Write $"\n{prefix}[{time ()}] = "
    Console.ForegroundColor <- ConsoleColor.Gray

let readLine () =
    "In" |> printPrefix
    Console.ReadLine ()

let writeLine (input : string) =
    "Out" |> printPrefix
    Console.ForegroundColor <- ConsoleColor.Yellow
    input |> Console.WriteLine
    Console.ForegroundColor <- ConsoleColor.Gray

let writeLineError (input : string) =
    "Error" |> printPrefix
    Console.ForegroundColor <- ConsoleColor.Red
    input |> Console.WriteLine
    Console.ForegroundColor <- ConsoleColor.Gray