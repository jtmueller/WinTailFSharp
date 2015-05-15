namespace WinTail

open System
open Akka.FSharp

/// Actor responsible for serializing message writes to the console.
/// (write one message at a time, champ :)
module ConsoleWriterActor =
    
    let start (mailbox:Actor<WriterMessage>) =
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                match msg with
                | Success message ->
                    Console.ForegroundColor <- ConsoleColor.Green
                    Console.WriteLine(message)
                | Error(NullInputError reason) ->
                    Console.ForegroundColor <- ConsoleColor.Magenta
                    Console.WriteLine(reason)
                | Error(ValidationError reason) ->
                    Console.ForegroundColor <- ConsoleColor.Red
                    Console.WriteLine(reason)
                | Error(FileReadError reason) ->
                    Console.ForegroundColor <- ConsoleColor.Magenta
                    Console.WriteLine(reason)
                | Display message ->
                    Console.WriteLine(message)

                Console.ResetColor()

                return! loop()
            }
        loop()