module ThreesAI.Execution.User

open type SDL2.SDL
open ThreesAI
open ThreesAI.Execution.CommonSDL

let updateGameState (stateProcessor: MailboxProcessor<Message>) (event: SDL_Event) =
    match event.``type`` with
    | SDL_EventType.SDL_KEYDOWN ->
        match event.key.keysym.sym with
        | SDL_Keycode.SDLK_UP    -> stateProcessor.Post ShiftUp
        | SDL_Keycode.SDLK_DOWN  -> stateProcessor.Post ShiftDown
        | SDL_Keycode.SDLK_LEFT  -> stateProcessor.Post ShiftLeft
        | SDL_Keycode.SDLK_RIGHT -> stateProcessor.Post ShiftRight
        | SDL_Keycode.SDLK_r     -> stateProcessor.Post Restart
        | _ -> ()
    | _ -> ()
    
let rec gameLoop (stateProcessor: MailboxProcessor<Message>) =
    let events = pollEvents ()
    
    let matchQuit (event: SDL_Event) = event.typeFSharp = SDL_EventType.SDL_QUIT
    
    if List.exists matchQuit events then
        stateProcessor.Post Exit
        ()
    else
        events |> List.iter (updateGameState stateProcessor)
        gameLoop stateProcessor
        
let run () =
    match init () with
    | Ok sdlData ->
        let stateProcessor = GameProcessor.stateProcessor ()
        let inbox = stateProcessor.Start ()
        stateProcessor.StartRender sdlData
        gameLoop inbox
        quit sdlData
    | Error e -> printfn $"SDL Error: {e}"