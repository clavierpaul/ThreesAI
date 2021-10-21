open ThreesAI
open ThreesAI.Execution

[<EntryPoint>]
let main args =
    if (Array.length args) = 0 then
        User.run ()
        0
    else
        match args.[0] with
        | "--ai" -> ZeroMQ.run ()
        | "--ai-headless" ->
            let gameProcessor = ZeroMQGameProcessor.zeroMQStateProcessor ()
            gameProcessor.Start ()
            let rec loop () =
                loop ()
            loop ()
        | arg    -> printfn $"Unrecognised argument \"{arg}\""
        0
    