module ThreesAI.Execution.ZeroMQ

open type SDL2.SDL
open ThreesAI.Execution
open ZeroMQGameProcessor

let rec quitLoop () =
    let sdlEvents = CommonSDL.pollEvents ()
    
    let matchQuit (event: SDL_Event) =
        event.``type`` = SDL_EventType.SDL_QUIT
    
    if List.exists matchQuit sdlEvents then
        ()
    else
        quitLoop ()

let init () =
    CommonSDL.init ()
    
let run () =
    let sdlDataOption = init ()
    match sdlDataOption with
    | Ok sdlData ->
        let gameProcessor = zeroMQStateProcessor ()
        
        gameProcessor.StartRender sdlData
        gameProcessor.Start ()
        quitLoop ()
        CommonSDL.quit sdlData
    | Error e -> printfn $"SDL Error: {e}"