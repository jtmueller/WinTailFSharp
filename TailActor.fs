namespace WinTail

open System
open System.IO
open System.Text
open Akka.Actor
open Akka.FSharp

/// Actor that validates user input and signals result to others.
module TailActor =

    let start (output:ICanTell) filePath (mailbox:Actor<TailActorMessage>) =
        let fullPath = Path.GetFullPath(filePath)

        // start watching file for changes
        let observer = new FileObserver(mailbox.Self, fullPath)
        mailbox.Defer((observer :> IDisposable).Dispose)

        // open the file stream with shared read/write permissions (so file can be written to while open)
        let fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        let fsReader = new StreamReader(fileStream, Encoding.UTF8)
        mailbox.Defer(fsReader.Dispose)
        mailbox.Defer(fileStream.Dispose)

        // read the initial contents of the file and send it to console as first message
        async {
            let! text = fsReader.AsyncReadToEnd()
            return FileRead(filePath, text)
        } |!> mailbox.Self

        let rec loop () =
            actor {
                let! msg = mailbox.Receive()

                match msg with
                | FileRead(path, text) ->
                    if not (String.IsNullOrEmpty(text)) then
                        output <! Display(text)
                | FileChange _ ->
                    // move file cursor forward
                    // pull results from cursor to end of file and write to output
                    // (this is assuming a log file type format that is append-only)
                    async {
                        let! text = fsReader.AsyncReadToEnd()
                        return FileRead(filePath, text.Trim())
                    } |!> mailbox.Self
                | FileError(path, reason) ->
                    output <! Error(FileReadError reason)

                return! loop()
            }
        loop()