module WinTail.Program

open Akka.FSharp

// initialize MyActorSystem
let myActorSystem = 
    Configuration.defaultConfig()
    |> System.create "win-tail"

[<EntryPoint>]
let main argv = 
    
    // time to make your first actors!
    let consoleWriterActor = spawn myActorSystem "console-writer" ConsoleWriterActor.start
    let consoleReaderActor = spawn myActorSystem "console-reader" ConsoleReaderActor.start
    let validationActor = spawn myActorSystem "validator" ValidationActor.start

    // tell console reader to begin
    consoleReaderActor <! System(Start)

    // blocks the main thread from exiting until the actor system is shut down
    myActorSystem.AwaitTermination();

    0 // return an integer exit code
