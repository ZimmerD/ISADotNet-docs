open System.Diagnostics

open System.IO
let s = __SOURCE_DIRECTORY__

type Logger(p: string) = 

    member this.Trace(s : string) =
        File.AppendAllText(p,$"\n{System.DateTime.Now}:Trace:{s}")

    member this.Info(s : string) =
        File.AppendAllText(p,$"\n{System.DateTime.Now}:INFO:{s}")

    member this.Error(s : string) =
            File.AppendAllText(p,$"\n{System.DateTime.Now}:Error:{s}")

    static member GitLog = Logger(Path.Combine(s,"GitLog.txt"))

module GitHelper =

    /// Executes Git command and returns git output.
    let executeGitCommandWithResponse (repoDir : string) (command : string) =

        let log = Logger.GitLog

        let procStartInfo = 
            ProcessStartInfo(
                WorkingDirectory = repoDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = "git",
                Arguments = command
            )
        
        let outputs = System.Collections.Generic.List<string>()
        let outputHandler (_sender:obj) (args:DataReceivedEventArgs) = 
            if (args.Data = null |> not) then
                if args.Data.ToLower().Contains ("error") then
                    log.Error($"GIT: {args.Data}")    
                elif args.Data.ToLower().Contains ("trace") then
                    //log.Trace($"GIT: {args.Data}")   
                    ()
                else
                    outputs.Add(args.Data)
                    //log.Info($"GIT: {args.Data}")
        
        let errorHandler (_sender:obj) (args:DataReceivedEventArgs) =  
            if (args.Data = null |> not) then
                let msg = args.Data.ToLower()
                if msg.Contains ("error") || msg.Contains ("fatal") then
                    log.Error($"GIT: {args.Data}")    
                elif msg.Contains ("trace") then
                    //log.Trace($"GIT: {args.Data}")  
                    ()
                else
                    outputs.Add(args.Data)
                    //log.Info($"GIT: {args.Data}")
        
        let p = new Process(StartInfo = procStartInfo)

        p.OutputDataReceived.AddHandler(DataReceivedEventHandler outputHandler)
        p.ErrorDataReceived.AddHandler(DataReceivedEventHandler errorHandler)
        p.Start() |> ignore
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()
        p.WaitForExit()
        outputs

    /// Executes Git command.
    let executeGitCommand (repoDir : string) (command : string) =
        
        executeGitCommandWithResponse repoDir command |> ignore

    let setLocalEmail (dir : string) (email : string) =
        executeGitCommand dir (sprintf "config user.email \"%s\"" email)

    let tryGetLocalEmail (dir : string) =
        let r = executeGitCommandWithResponse dir "config --local --get user.email"
        if r.Count = 0 then None
        else Some r.[0]


    let setLocalName (dir : string) (name : string) =
        executeGitCommand dir (sprintf "config user.name \"%s\"" name)

    let tryGetLocalName (dir : string) =
        let r = executeGitCommandWithResponse dir "config --local --get user.name"
        if r.Count = 0 then None
        else Some r.[0]

    let formatRepoString username pass (url : string) = 
        let comb = username + ":" + pass + "@"
        url.Replace("https://","https://" + comb)

    let clone dir url =
        executeGitCommand dir (sprintf "clone %s" url)

    let cloneNoFilter dir url =        
        let noFilterConfig = "-c \"filter.lfs.smudge = git-lfs smudge --skip -- %f\" -c \"filter.lfs.process = git-lfs filter-process --skip\""
        executeGitCommand dir ($"clone {noFilterConfig} {url}")

    let cloneWithToken dir (name : string) (token : string) url  =
        let url = formatRepoString name token url
        clone dir url 

    let cloneWithTokenNoFilter dir (name : string) (token : string) url  =
        let url = formatRepoString name token url
        cloneNoFilter dir url 

    let pullLfsObject (dir : string) (object : string) = 
        executeGitCommand dir ($"lfs pull --include=\"{object}\"")

    let getFilePointer (dir : string) (object : string) = 
        executeGitCommandWithResponse dir ($"lfs pointer --file=\"{object}\"")
        |> Seq.filter (fun a -> a.StartsWith "version" || a.StartsWith "oid" || a.StartsWith "size")
        |> Seq.reduce (fun a b -> a + "\n" + b)

    let replaceFileByPointer (dir : string) (object : string) =
        let pointer = getFilePointer dir object
        let filePath = Path.Combine(dir,object)
        File.Delete filePath
        File.WriteAllText(filePath,pointer)

    let clearLFSFolders (dir : string) =
        let lfsStorage = Path.Combine [|dir;".git";"lfs";"objects"|]
        Directory.Delete(lfsStorage,true)

    let add dir = 
        executeGitCommand dir "add ."

    let commit dir message =
        executeGitCommand dir (sprintf "commit -m \"%s\"" message)

    let push dir =
        executeGitCommand dir "push"

open GitHelper

pullLfsObject (s + "/../../") @"assays\PSM_DDA_FragPipe_NoNorm\dataset\MS280_3ddanoNorm\combined_protein.tsv"

pullLfsObject (s + "/../../") @"assays\PSM_DDA_FragPipe\dataset\MS280_3ddaNormAcrRun\combined_protein.tsv"
