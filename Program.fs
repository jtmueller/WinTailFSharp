module WinTail.Program

open Akka.FSharp

// initialize MyActorSystem
let myActorSystem = 
    Configuration.defaultConfig()
    |> System.create "win-tail"

[<EntryPoint>]
let main argv = 
    
    // create the actors
    let consoleWriterActor = spawn myActorSystem "console-writer" ConsoleWriterActor.start
    let consoleReaderActor = spawn myActorSystem "console-reader" ConsoleReaderActor.start
    let tailCoordinatorActor = spawn myActorSystem "tail-coord" TailCoordinatorActor.start
    let validationActor = spawn myActorSystem "file-validator" FileValidatorActor.start

    // tell console reader to begin
    consoleReaderActor <! System(Start)

    // blocks the main thread from exiting until the actor system is shut down
    myActorSystem.AwaitTermination();

    0 // return an integer exit code
