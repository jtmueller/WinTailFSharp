namespace WinTail

open System
open Akka.Actor
open Akka.FSharp

/// Actor responsible for reading FROM the console. 
/// Also responsible for calling ActorSystem.Shutdown.
module ConsoleReaderActor =
    let private exitCommand = "exit"

    // I didn't think it was right for the Reader to be writing directly to the console, so...
    let private printInstructions (writer: #ICanTell) =
        writer <!
            Display("Write whatever you want into the console! \n\
                     Some entries will pass validation, and some won't...\n\n\
                     Type 'exit' to quit this application at any time.\n")
                     
    let private isValid (message: string) =
        message.Length % 2 = 0  

    let private getAndValidateInput (self: #ICanTell) (writer:#ICanTell) =
        let message = Console.ReadLine()
        if String.IsNullOrEmpty(message) then
            writer <! Error(NullInputError "No input received.")
            self <! System(ContinueProcessing)
        elif String.Equals(message, exitCommand, StringComparison.OrdinalIgnoreCase) then
            self <! System(Exit)
        else
            if isValid message then
                writer <! Success("Thank you! Message was valid.")
            else
                writer <! Error(ValidationError("Invalid: input had odd number of characters."))
            self <! System(ContinueProcessing)

    let start (mailbox:Actor<ReaderMessage>) =
        // selecting the other actor instead of requiring a hard reference to be passed in
        let writer = select "akka://win-tail/user/console-writer" mailbox.Context.System
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                match msg with
                | System(Start) -> printInstructions writer
                | System(Exit) -> mailbox.Context.System.Shutdown()
                | _ -> ()

                getAndValidateInput mailbox.Self writer

                return! loop()
            }
        loop()