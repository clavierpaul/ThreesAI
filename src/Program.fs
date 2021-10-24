open ThreesAI
open type SDL2.SDL

type SDLData =
    { Window: Rendering.Window
      Renderer: Rendering.Renderer
      Textures: Display.Textures }

// Dumb type name is dumb
type EventReturn =
    | Quit
    | State of Game.State

let render sdlData board =
    SDL_RenderClear sdlData.Renderer |> ignore
    Display.render sdlData.Renderer sdlData.Textures board
    SDL_RenderPresent sdlData.Renderer

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

// Avoids having to wrap every return in State
let updateState state (event: SDL_Event) =
    match event.``type`` with
    | SDL_EventType.SDL_KEYDOWN ->
        match event.key.keysym.sym with
        | SDL_Keycode.SDLK_LEFT  -> state |> Game.shift Controls.Left
        | SDL_Keycode.SDLK_RIGHT -> state |> Game.shift Controls.Right
        | SDL_Keycode.SDLK_UP    -> state |> Game.shift Controls.Up
        | SDL_Keycode.SDLK_DOWN  -> state |> Game.shift Controls.Down
        | SDL_Keycode.SDLK_r     -> Game.create ()
        | _ -> state
    | _ -> state
    
let update state (event: SDL_Event) =
    match state with
    | Quit -> state // If we're quitting, there's no more state to handle
    | State state ->
        match event.``type`` with
        | SDL_EventType.SDL_QUIT -> Quit
        | _                      -> State <| updateState state event
    
let rec gameLoop sdlData state =
    // Fold events together into a final state (or quit)
    match List.fold update (State state) <| pollEvents () with
    | Quit           -> ()
    | State newState ->
        if newState.GameOver then
            printfn "Game over!"
            ()
        else 
            render sdlData newState
            gameLoop sdlData newState
    
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
        gameLoop sdlData <| Game.create ()
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
    