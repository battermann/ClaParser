﻿namespace ClaParser.FSharp

open System
open System.Runtime.CompilerServices
open Chessie.ErrorHandling
open ExtraTopLevelOperators
open System.Text.RegularExpressions
open ClaParser.Models

module RopArguments =
    let inList xs = fun x -> xs |> List.contains x

    let validate x validator =
        let errs = validator x
        match errs with 
        | [] -> ok x
        | _ -> Bad errs

    let parse args =
        let (| Command | Value |) arg =
            let m = Regex.Match(arg, @"^(?:-{1,2}|\/)(?<command>[a-zA-Z0-9]+.*)$", RegexOptions.IgnoreCase);
            match m.Success with
            | true -> Command (m.Groups.["command"].Value.ToLower())
            | _ -> Value (arg.ToLower())

        let (| ValueAndTail |) (head2, tail, tail2) =
            match head2 with
            | Command _ -> None, tail
            | Value v -> Some v, tail2

        let rec parseRec result remaining =
            match result, remaining with
            | Bad _, _ -> result
            | _, [] -> result
            | Ok (parsed,_), head::tail -> 
                match head, tail with
                | Value v, _ -> v |> InvalidArgument |> fail
                | Command cmd, [] -> ok (parsed @ [(cmd, None)])
                | Command cmd, head2::tail2 -> 
                    let (ValueAndTail (value, tail')) = (head2, tail, tail2) 
                    parseRec (ok (parsed @ [(cmd, value)])) tail'

        parseRec (ok []) args

    let checkArgsContainNoDuplicates definedCmds args =
        args
        |> List.filter (fst >> inList definedCmds)
        |> List.groupBy fst
        |> List.filter (fun (_, x) -> (List.length x) > 1)
        |> List.map fst
        |> List.distinct
        |> List.map DuplicateCommand

    let checkAllArgsAreDefined validCmds args =
        args 
        |> List.map fst
        |> List.distinct
        |> List.filter (not << inList validCmds)
        |> List.map UnknownCommand

    let checkAllRequiredArgsExist requiredCmds args =
        requiredCmds
        |> List.filter (not << inList (args |> List.map fst))
        |> List.map RequiredCommandMissing

    let parseArgs args argInfos =
        let required = argInfos |> List.filter (fun x -> x.required) |> List.map (fun x -> x.command)
        let definedCommands = argInfos |> List.map (fun x -> x.command)

        let createDict a b c = a |> List.map (fun x -> fst x, snd x) |> dict

        parse args
        >>= fun parsedArgs -> 
            createDict
            <!> validate parsedArgs (checkArgsContainNoDuplicates definedCommands) 
            <*> validate parsedArgs (checkAllRequiredArgsExist required)
            <*> validate parsedArgs (checkAllArgsAreDefined definedCommands)