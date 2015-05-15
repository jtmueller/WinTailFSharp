namespace WinTail

open System
open System.IO
open Akka.Actor

type FileObserver(tailActor: IActorRef, filePath: string) =

    let fileDir = Path.GetDirectoryName(filePath)
    let fileName = Path.GetFileName(filePath)
    let watcher = new FileSystemWatcher(fileDir, fileName)

    do
        // watch our file for changes to the file name, or new messages being written to file
        watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite

        watcher.Changed 
        |> Event.add (fun e -> 
            if e.ChangeType = WatcherChangeTypes.Changed then
                // here we use a special ActorRefs.NoSender
                // since this event can happen many times, this is a little microoptimization
                tailActor.Tell(FileChange(e.Name), ActorRefs.NoSender)
        )

        watcher.Error
        |> Event.add (fun e ->
            tailActor.Tell(FileError(fileName, e.GetException().Message), ActorRefs.NoSender)
        )

        watcher.EnableRaisingEvents <- true

    interface IDisposable with
        member x.Dispose() =
            watcher.Dispose()
