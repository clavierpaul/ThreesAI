open ThreesAI
open Display
open type SDL2.SDL

// Dumb type name is dumb
type EventReturn =
    | Quit
    | State of Game.State

let pollEvents ()  =
    let rec pollLoop events =
        // Required to be mutable :(
        // Could be done using a non-recursive method?
        let mutable event = Unchecked.defaultof<SDL_Event>
        if (SDL_PollEvent &event) <> 0 then
            pollLoop <| event :: events
        else
            events
    pollLoop []

let updateGameState (stateProcessor: MailboxProcessor<Message>) (event: SDL_Event) =
    match event.``type`` with
    | SDL_EventType.SDL_KEYDOWN ->
        match event.key.keysym.sym with
        | SDL_Keycode.SDLK_LEFT  -> stateProcessor.Post(ShiftLeft)
        | SDL_Keycode.SDLK_RIGHT -> stateProcessor.Post(ShiftRight)
        | SDL_Keycode.SDLK_UP    -> stateProcessor.Post(ShiftUp)
        | SDL_Keycode.SDLK_DOWN  -> stateProcessor.Post(ShiftDown)
        | SDL_Keycode.SDLK_r     -> stateProcessor.Post(Restart)
        | _ -> ()
    | _ -> ()
    
let rec gameLoop (stateProcessor: MailboxProcessor<Message>) =
    // Fold events together into a final state (or quit)
    let events = pollEvents ()
    
    let matchQuit (event: SDL_Event) = event.``type`` = SDL_EventType.SDL_QUIT
    
    if List.exists matchQuit events then
        stateProcessor.Post(Exit)
        ()
    else
        events |> List.iter (updateGameState stateProcessor)
        gameLoop stateProcessor
    
let init () = ResultBuilder.resultBuilder {
    // If there are any errors, init will end early and return Error
    let! window, renderer = Rendering.init ("ThreesAI", screenWidth, screenHeight)
    let! tiles      = Texture.create renderer "assets/tiles.png" 2
    let! background = Texture.create renderer "assets/background.png" 2
    let! nextTiles  = Texture.create renderer "assets/next_tiles.png" 1
    let! numbers    = Texture.create renderer "assets/numbers.png" 1
    let! scoreLabel = Texture.create renderer "assets/score.png" 1
    
    do! Rendering.setDrawColor renderer (0xFAuy, 0xFAuy, 0xFAuy, 0xFFuy)
    
    return { Window = window
             Renderer = renderer
             Textures =
                 { Tiles      = tiles
                   Background = background
                   NextTiles  = nextTiles
                   Numbers    = numbers
                   ScoreLabel = scoreLabel } }
}

[<EntryPoint>]
let main _ =
    match init () with
    | Ok sdlData ->
        let stateProcessor = GameProcessor.stateProcessor sdlData
        let inbox = stateProcessor.Start ()
        gameLoop inbox
        // There needs to be a system to signal when the renderer is done to avoid crash on exit,
        // however I don't have time anymore :/
        SDL_DestroyTexture  sdlData.Textures.Tiles.Handle
        SDL_DestroyTexture  sdlData.Textures.Background.Handle
        SDL_DestroyTexture  sdlData.Textures.NextTiles.Handle
        SDL_DestroyTexture  sdlData.Textures.ScoreLabel.Handle
        SDL_DestroyTexture  sdlData.Textures.Numbers.Handle
        SDL_DestroyRenderer sdlData.Renderer
        SDL_DestroyWindow   sdlData.Window
        SDL_Quit ()
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    
    