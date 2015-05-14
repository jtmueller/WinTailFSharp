namespace WinTail

open System
open Akka.Actor
open Akka.FSharp

/// Actor responsible for reading FROM the console. 
/// Also responsible for calling ActorSystem.Shutdown.
module ConsoleReaderActor =
    let private exitCommand = "exit"
    
    let start (consoleWriterActor: IActorRef) (mailbox:Actor<string>) =
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                let read = Console.ReadLine()
                if String.Equals(read, exitCommand, StringComparison.OrdinalIgnoreCase) then
                    mailbox.Context.System.Shutdown()
                else
                    // send input to the console writer to process and print
                    consoleWriterActor <! read

                    // continue reading messages from the console
                    mailbox.Self <! "continue"

                return! loop()
            }
        loop()