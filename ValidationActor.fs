namespace WinTail

open System
open Akka.Actor
open Akka.FSharp

module ValidationActor =

    let private isValid (message: string) =
        message.Length % 2 = 0

    let private checkValid (writer:#ICanTell) message =
        if String.IsNullOrEmpty(message) then
            writer <! Error(NullInputError "No input received.")
        elif isValid message then
            writer <! Success("Thank you! Message was valid.")
        else
            writer <! Error(ValidationError("Invalid: input had odd number of characters."))

    let start (mailbox:Actor<ValidationMessage>) =
        let writer = select "akka://win-tail/user/console-writer" mailbox.Context.System
        let checkValid = checkValid writer
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()
                
                match msg with
                | CheckValid message -> checkValid message
                // other types of validation, if they existed, would go here

                mailbox.Context.Sender <! System(ContinueProcessing)

                return! loop()
            }
        loop()
