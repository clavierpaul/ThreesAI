open System
open ThreesAI
open type SDL2.SDL

let screenWidth = 140
let screenHeight = 204

let pollEvents ()  =
    let rec pollLoop events =
        let mutable event = Unchecked.defaultof<SDL_Event>
        if (SDL_PollEvent &event) <> 0 then
            pollLoop <| event :: events
        else
            events
    pollLoop []

let handleEvent (event: SDL_Event) board =
    match event.``type`` with
    | SDL_EventType.SDL_KEYDOWN -> match event.key.keysym.sym with
                                   | SDL_Keycode.SDLK_LEFT  -> Board.shift Controls.Left board
                                   | SDL_Keycode.SDLK_RIGHT -> Board.shift Controls.Right board
                                   | SDL_Keycode.SDLK_UP    -> Board.shift Controls.Up board
                                   | SDL_Keycode.SDLK_DOWN  -> Board.shift Controls.Down board
                                   | _ -> board
    | _ -> board

let rec handleEvents (events: SDL_Event list) board =
    match events with
    | [] -> board
    | event :: events -> handleEvents events <| handleEvent event board

let gameLoop (window: Rendering.Window) (renderer: Rendering.Renderer) textures =
    let mutable test = Board.empty
    test.[0, 0] <- Tile 1
    test.[1, 0] <- Tile 3
    test.[2, 0] <- Tile 3
    
    test.[0, 1] <- Tile 2
    
    test.[1, 2] <- Tile 1
    test.[2, 2] <- Tile 3
    test.[3, 2] <- Tile 1
    
    test.[2, 3] <- Tile 3
    test.[3, 3] <- Tile 2
    
    let rec eventLoop () =
        SDL_RenderClear renderer |> ignore
        Display.render renderer textures test
        SDL_RenderPresent renderer
        
        let events = pollEvents ()
        
        let matchQuit (event: SDL_Event) =
            event.``type`` = SDL_EventType.SDL_QUIT
       
        if List.exists matchQuit events then
            ()
        else
            test <- handleEvents events test
            eventLoop ()
        
    eventLoop ()
    SDL_DestroyRenderer renderer
    SDL_DestroyWindow window
    SDL_Quit ()
    
let init () = ResultBuilder.resultBuilder {
    let! window, renderer = Rendering.init ("ThreesAI", screenWidth * Display.scale, screenHeight * Display.scale)
    let! tiles = Texture.create renderer "assets/tiles.png" Display.scale
    let! background = Texture.create renderer "assets/background.png" Display.scale
    
    return (window, renderer, ({ Background = background; Tiles = tiles }: Display.Textures) )
}

[<EntryPoint>]
let main _ =
    match init () with
    | Ok (window, renderer, textures) -> gameLoop window renderer textures
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    