namespace azure_function_fsharp

[<RequireQualifiedAccess>]
module DotEnv =
    open System
    open System.IO

    let private parseLine(line : string) =        
        match line.Split('=', StringSplitOptions.RemoveEmptyEntries) with
        | args when args.Length = 2 ->
            Environment.SetEnvironmentVariable(
                args.[0],
                args.[1])
        | _ -> ()

    let private load() =
        lazy (            
            let directory = Directory.GetCurrentDirectory()
            let filePath = Path.Combine(directory, ".env")
            filePath
            |> File.Exists
            |> function
                | false -> Console.WriteLine "No .env file found."
                | true  ->
                    filePath
                    |> File.ReadAllLines
                    |> Seq.iter parseLine
        )

    let init () = load().Force()