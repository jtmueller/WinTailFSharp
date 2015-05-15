namespace WinTail

open System
open Akka.Actor
open Akka.FSharp

/// Actor responsible for reading FROM the console. 
/// Also responsible for calling ActorSystem.Shutdown.
module ConsoleReaderActor =
    let private exitCommand = "exit"

    let private printInstructions (writer: #ICanTell) =
        writer <! Display("Please provide the URI of a log file on disk.\n")
 
    let private getAndValidateInput (self: #ICanTell) (validator:#ICanTell) =
        let message = Console.ReadLine()

        if String.Equals(message, exitCommand, StringComparison.OrdinalIgnoreCase) then
            self <! System(Exit)
        else
            validator <! message

    let start (mailbox:Actor<ReaderMessage>) =
        let writer = select "akka://win-tail/user/console-writer" mailbox.Context.System
        let validator = select "akka://win-tail/user/file-validator" mailbox.Context.System
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                match msg with
                | System(Start) -> 
                    printInstructions writer
                    mailbox.Self <! System(ContinueProcessing)
                | System(Exit) -> mailbox.Context.System.Shutdown()
                | System(ContinueProcessing) ->
                    getAndValidateInput mailbox.Self validator

                return! loop()
            }
        loop()
