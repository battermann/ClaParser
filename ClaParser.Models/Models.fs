namespace ClaParser.Models

type ArgInfo = { command:string; description: string option; required: bool}

type ClaError =
    | InvalidArgument of string
    | DuplicateCommand of string
    | UnknownCommand of string
    | RequiredCommandMissing of string
    | ValueMissing of string
    | CannotParseValue of string

open System
open System.Runtime.CompilerServices

[<Extension>]
type ClaParserExtensions () =
    
    [<Extension>]
    static member Match(error, (onInvalidArgument: Func<string, 'T>) , (onDuplicateCommand: Func<string, 'T>), (onUnknownCommand: Func<string, 'T>), (onRequiredCommandMissing: Func<string, 'T>), (onValueMissing: Func<string, 'T>), (onCannotParseValue: Func<string, 'T>)) =
        match error with
        | InvalidArgument msg        -> onInvalidArgument.Invoke(msg)
        | DuplicateCommand msg       -> onDuplicateCommand.Invoke(msg)
        | UnknownCommand msg         -> onUnknownCommand.Invoke(msg)
        | RequiredCommandMissing msg -> onRequiredCommandMissing.Invoke(msg)
        | ValueMissing msg           -> onValueMissing.Invoke(msg)
        | CannotParseValue msg       -> onCannotParseValue.Invoke(msg)

    [<Extension>]
    static member SPrint(msg) =
        sprintf "%A" msg

    [<Extension>]
    static member SPrintClaError(msg) =
        match msg with
        | InvalidArgument msg        -> sprintf "Expected a command but got \'%s'." msg
        | DuplicateCommand msg       -> sprintf "Duplicate command \'%s'." msg
        | UnknownCommand msg         -> sprintf "Command \'%s' not defined." msg
        | RequiredCommandMissing msg -> sprintf "Required command \'%s' not in argument list." msg
        | ValueMissing msg           -> sprintf "Command \'%s' has no value." msg
        | CannotParseValue msg       -> sprintf "Cannot parse value \'%s'." msg