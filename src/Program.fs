open System
open ThreesAI
open type SDL2.SDL

let screenWidth = 32 * 2 * 4
let screenHeight = 48 * 2 * 4

let pollEvents ()  =
    let rec pollLoop events =
        let mutable event = Unchecked.defaultof<SDL_Event>
        if (SDL_PollEvent &event) <> 0 then
            pollLoop <| event :: events
        else
            events
    pollLoop []

let gameLoop window renderer =
    SDL_SetRenderDrawColor(renderer, 0xB6uy, 0xCDuy, 0xF0uy, 0xFFuy) |> ignore
    match Texture.create renderer "assets/tiles.png" with
    | Ok _ -> ()
    | Error e -> printfn $"SDL Error: {e}"
    
    let rec eventLoop () =
        SDL_RenderClear renderer |> ignore
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

[<EntryPoint>]
let main _ =
    match Rendering.init ("ThreesAI", screenWidth, screenHeight) with
    | Ok (window, renderer) -> gameLoop window renderer
    | Error e -> printfn $"SDL Error: {e}"
    |> ignore 
    0
    