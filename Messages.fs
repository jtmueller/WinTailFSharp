namespace WinTail

open System.IO
open Akka.Actor

type SystemMessage =
    | ContinueProcessing
    | Start
    | Exit

type ErrorMessage =
    | NullInputError of reason:string
    | ValidationError of reason:string
    | FileReadError of reason:string

type ReaderMessage =
    | System of SystemMessage
    
type WriterMessage =
    | Success of string
    | Error of ErrorMessage
    | Display of string

type ValidationMessage =
    | CheckValid of string

type TailCoordinatorMessage =
    | StartTail of path:string * output:ICanTell 
    | StopTail of path:string

type TailActorMessage =
    | FileChange of fileName:string
    | FileError of fileName:string * reason:string
    | FileRead of fileName:string * text:string
