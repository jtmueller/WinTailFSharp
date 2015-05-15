namespace WinTail

open System
open Akka.Actor
open Akka.FSharp

/// Actor that validates user input and signals result to others.
module TailCoordinatorActor =
    
    let normalizePath (path: string) =
        path.Replace('\\', '-')

    let private supervisor = 
        let decider (err:exn) =
            match err with
            // Maybe we consider ArithmeticException to not be application critical
            // so we just ignore the error and keep going.
            | :? ArithmeticException -> Directive.Resume

            // Error that we cannot recover from, stop the failing actor
            | :? NotSupportedException -> Directive.Stop

            // In all other cases, just restart the failing actor
            | _ -> Directive.Restart

        Strategy.OneForOne(decider, 10, TimeSpan.FromSeconds 30.0)

    let start (mailbox:Actor<TailCoordinatorMessage>) =
        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                match msg with
                | StartTail(path, output) ->
                    // here we are creating our first parent/child relationship!
                    // the TailActor instance created here is a child
                    // of this instance of TailCoordinatorActor
                    spawnOpt mailbox.Context (normalizePath path) (TailActor.start output path)
                        [ SpawnOption.SupervisorStrategy supervisor ]
                    |> ignore
                | StopTail(path) -> 
                    // Is this the right way to stop a child? Send a custom stop message?
                    mailbox.Context.Child(path).GracefulStop(TimeSpan.FromSeconds 10.0) |> ignore

                return! loop()
            }
        loop()
