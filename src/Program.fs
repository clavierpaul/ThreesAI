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

let gameLoop (window: Rendering.Window) (renderer: Rendering.Renderer) textures =
    
    let test = Board.empty
    test.[0, 0] <- Tile 96
    test.[0, 1] <- Tile 6
    test.[0, 2] <- Tile 48
    test.[0, 3] <- Tile 6
    
    test.[1, 0] <- Tile 12
    test.[1, 3] <- Tile 2
    
    test.[2, 0] <- Tile 1
    test.[2, 1] <- Tile 1
    test.[2, 3] <- Tile 3
    
    test.[3, 0] <- Tile 12
    test.[3, 1] <- Tile 3
    test.[3, 2] <- Tile 2
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
    