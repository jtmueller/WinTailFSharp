namespace WinTail

open System
open Akka.FSharp

/// Actor responsible for serializing message writes to the console.
/// (write one message at a time, champ :)
module ConsoleWriterActor =
    
    let start (mailbox:Actor<string>) =
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                // make sure we got a message
                if String.IsNullOrEmpty(msg) then
                    Console.ForegroundColor <- ConsoleColor.DarkYellow
                    Console.WriteLine("Please provide an input.\n")
                    Console.ResetColor()
                else
                    // if message has even # characters, display in red; else, green
                    let even = msg.Length % 2 = 0
                    let color = if even then ConsoleColor.Red else ConsoleColor.Green
                    let alert = sprintf "Your string had an %s # of characters.\n" (if even then "even" else "odd")
                    Console.ForegroundColor <- color
                    Console.WriteLine(alert)
                    Console.ResetColor()

                return! loop()
            }
        loop()