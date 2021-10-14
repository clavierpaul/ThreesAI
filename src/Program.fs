open System
open ThreesAI
open type SDL2.SDL

type Textures =
    { Tiles: Texture }

let screenWidth = 32 * 2 * 4 * 2
let screenHeight = 48 * 2 * 4 * 2

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
    test.[1, 1] <- Tile 96
    test.[1, 2] <- Tile 3
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
        Display.renderBoard renderer textures.Tiles test
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
    let! window, renderer = Rendering.init ("ThreesAI", screenWidth, screenHeight)
    let! tiles = Texture.create renderer "assets/tiles.png" 4
    
    do! Rendering.setDrawColor renderer (0xB6uy, 0xCDuy, 0xF0uy, 0xFFuy)
    
    return (window, renderer, { Tiles = tiles })
}

[<EntryPoint>]
let main _ =
    match init () with
    | Ok (window, renderer, textures) -> gameLoop window renderer textures
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    