namespace WinTail

open System
open System.IO
open Akka.FSharp

/// Actor that validates user input and signals result to others.
module FileValidatorActor =

    let isFileUri path = File.Exists(path)
    
    let start (mailbox:Actor<string>) =
        let writer = select "akka://win-tail/user/console-writer" mailbox.Context.System
        let tailCoordinator = select "akka://win-tail/user/tail-coord" mailbox.Context.System
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                if String.IsNullOrEmpty(msg) then
                    writer <! Error(NullInputError "Input was blank. Please try again.\n")
                    mailbox.Sender() <! System(ContinueProcessing)
                else
                    if isFileUri msg then
                        // signal successful input
                        writer <! Success(sprintf "Starting processing for %s." msg)

                        // start coordinator
                        tailCoordinator <! StartTail(msg, writer)
                    else
                        // signal that input was bad
                        writer <! Error(ValidationError(sprintf "%s is not an existing URI on disk." msg))

                        // tell sender to continue doing its thing
                        mailbox.Sender() <! System(ContinueProcessing)

                return! loop()
            }
        loop()
