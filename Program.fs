module WinTail.Program

open System
open Akka.FSharp

// initialize MyActorSystem
let myActorSystem = 
    Configuration.defaultConfig()
    |> System.create "MyActorSystem"

let printInstructions() =
    Console.WriteLine("Write whatever you want into the console!")
    Console.Write("Some lines will appear as")
    Console.ForegroundColor <- ConsoleColor.DarkRed
    Console.Write(" red ")
    Console.ResetColor()
    Console.Write("and others will appear as")
    Console.ForegroundColor <- ConsoleColor.Green
    Console.Write(" green! ")
    Console.ResetColor()
    Console.WriteLine()
    Console.WriteLine()
    Console.WriteLine("Type 'exit' to quit this application at any time.\n")

[<EntryPoint>]
let main argv = 
    
    printInstructions()

    // time to make your first actors!
    let consoleWriterActor = spawn myActorSystem "console-writer" ConsoleWriterActor.start
    let consoleReaderActor = spawn myActorSystem "console-reader" (ConsoleReaderActor.start consoleWriterActor)

    // tell console reader to begin
    consoleReaderActor <! "start"

    // blocks the main thread from exiting until the actor system is shut down
    myActorSystem.AwaitTermination();

    0 // return an integer exit code
