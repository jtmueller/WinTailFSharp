namespace WinTail

type SystemMessage =
    | ContinueProcessing
    | Start
    | Exit

type ErrorMessage =
    | NullInputError of reason:string
    | ValidationError of reason:string

type ReaderMessage =
    | System of SystemMessage
    
type WriterMessage =
    | Success of string
    | Error of ErrorMessage
    | Display of string

type ValidationMessage =
    | CheckValid of string
