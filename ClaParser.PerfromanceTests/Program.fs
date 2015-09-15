module Program =

    open ClaParser.Models
    open ClaParser.FSharp
    open ClaParser.CSharp
    open FSharpx.TimeMeasurement
    open System
    open System.Threading
    open Chessie
    open Chessie.ErrorHandling

    // create args and arginfos for tests
    let createArgs good bad =
        let rnd = new Random()

        let argInfos = List.init good (fun i -> { command = Guid.NewGuid().ToString(); description = None; required = i % 2 = 0 })
        
        let badArgs = 
            List.init bad (fun i -> [("-" + Guid.NewGuid().ToString(), Guid.NewGuid().ToString())]) 
            |> List.fold List.append []

        let goodArgs = 
            let rec createArgs (defs: ArgInfo list) result =
                match defs with
                | [] -> result
                | head::tail -> 
                    createArgs tail ((("-" + head.command), Guid.NewGuid().ToString()) :: result)
            let reducedArgInfos = (argInfos |> List.skip (bad*2))
            let argInfosWithDuplictaes = reducedArgInfos @ (reducedArgInfos |> List.take bad)
            createArgs argInfosWithDuplictaes []

        let args = 
            goodArgs @ badArgs
            |> List.sortBy (fun x -> rnd.Next())
            |> List.map (fun (t1, t2) -> [t1; t2]) |> List.fold List.append []

        args, argInfos

    let runPerfTest count (f1, name1) (f2, name2) args defs = 
        compareTwoRuntimes count name1 (fun () -> f1 args defs) name2 (fun () -> f2 args defs)

    let runTest good bad = 
        let (args, defs) = createArgs good bad

        printfn "NOTIFICATION"

        let a = new NoteArguments(args, defs)
        let n = a.Validation()

        match n.IsValid with
        | true -> printfn "%A" a.Dictionary
        | false -> printfn "%A" (n.Errors |> Seq.toList)

        printfn ""
        printfn "ROP CSharp"
        
        match RopArguments.ParseArgs(args, defs) with
        | Ok (v, _) -> printfn "%A" v
        | Bad errs -> printfn "%A" errs

        printfn ""
        printfn "ROP FSharp"

        match ClaParser.FSharp.RopArguments.parseArgs args defs with
        | Ok (v, _) -> printfn "%A" v
        | Bad errs -> printfn "%A" errs

    let ropFsharp  = (fun args defs -> ClaParser.FSharp.RopArguments.parseArgs args defs |> ignore), "rop fs"

    let ropCsharp = (fun args defs -> RopArguments.ParseArgs(args, defs) |> ignore), "rop cs"

    let notePattern = 
        let f = fun args defs -> 
            let a = new NoteArguments(args, defs)
            a.Validation()
        f, "note"

    [<EntryPoint>]
    let Main(args) =
        let (args, argInfos) = createArgs 200 0
        runPerfTest 100 ropFsharp notePattern args argInfos |> ignore
        // runTest 10 3 |> ignore
        Console.ReadLine() |> ignore
        0