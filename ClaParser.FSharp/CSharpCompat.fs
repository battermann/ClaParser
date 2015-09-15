namespace ClaParser.FSharp

open System
open Chessie.ErrorHandling
open ExtraTopLevelOperators
open System.Text.RegularExpressions
open ClaParser.Models
open RopArguments
open System.Runtime.CompilerServices
open Microsoft.FSharp.Core

[<Extension>]
type RopArgumentsExtensions () =
    
    [<Extension>]
    static member Parse args =
        RopArguments.parse(args |> List.ofSeq)

    [<Extension>]
    static member ParseArgs(args:string seq, argInfos: ArgInfo seq) =
        RopArguments.parseArgs (args |> List.ofSeq) (argInfos |> List.ofSeq)

    [<Extension>]
    static member Try((parser:Func<'TSource, 'TSuccess>), source, msg) =
        try
            Ok(parser.Invoke(source),[])
        with
        | exn -> Bad[msg]

    [<Extension>]
    static member ToResult(option, msg) =
        match option with
        | Some v -> Ok (v,[])
        | _ -> Bad [msg]

