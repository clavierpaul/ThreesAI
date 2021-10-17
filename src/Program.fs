open ThreesAI
open ThreesAI.Game
open type SDL2.SDL

let screenWidth = 140
let screenHeight = 204

type SDLData =
    { Window: Rendering.Window
      Renderer: Rendering.Renderer
      Textures: Display.Textures }

// Dumb type name is dumb
type EventReturn =
    | Quit
    | State of State

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
        | SDL_Keycode.SDLK_LEFT  -> state |> shift Controls.Left
        | SDL_Keycode.SDLK_RIGHT -> state |> shift Controls.Right
        | SDL_Keycode.SDLK_UP    -> state |> shift Controls.Up
        | SDL_Keycode.SDLK_DOWN  -> state |> shift Controls.Down
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
        render sdlData newState.Board
        gameLoop sdlData newState
    
let init () = ResultBuilder.resultBuilder {
    // If there are any errors, init will end early and return Error
    let! window, renderer = Rendering.init ("ThreesAI", screenWidth * Display.scale, screenHeight * Display.scale)
    let! tiles = Texture.create renderer "assets/tiles.png" Display.scale
    let! background = Texture.create renderer "assets/background.png" Display.scale
    
    return { Window = window; Renderer = renderer; Textures = { Tiles = tiles; Background = background } }
}

[<EntryPoint>]
let main _ =
    match init () with
    | Ok sdlData ->
        gameLoop sdlData <| Game.init ()
        SDL_DestroyTexture  sdlData.Textures.Tiles.Handle
        SDL_DestroyTexture  sdlData.Textures.Background.Handle
        SDL_DestroyRenderer sdlData.Renderer
        SDL_DestroyWindow   sdlData.Window
        SDL_Quit ()
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    