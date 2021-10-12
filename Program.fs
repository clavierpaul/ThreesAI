open System
open System.Runtime.InteropServices //for OutAttribute
open SDL2
open type SDL2.SDL
open ThreesAI
//
//let displayTile (tx, ty) tile =
//    match tx with 
//
//let displayTile (tx, ty) tile =
//    let display = 
//    match tx with
//    | 0 -> " . "
//    | _ -> if (tx + 1) % 4 = 0 then ""
//
//let displayBoard (board: Board) =
//    let sortedList = boarMap.toList
//        
//    sortedList
//

let screenWidth = 640
let screenHeight = 480

let pollEvents ()  =
    let rec pollLoop events =
        let mutable event = Unchecked.defaultof<SDL_Event>
        if (SDL_PollEvent &event) <> 0 then
            pollLoop <| event :: events
        else
            events
    pollLoop []

[<EntryPoint>]
let main argv =
    if SDL_Init(SDL_INIT_VIDEO) < 0 then
        printfn $"Error initializing SDL: {SDL_GetError()}"
        
    let window = SDL_CreateWindow ("SDL Test", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, screenWidth, screenHeight, SDL_WindowFlags.SDL_WINDOW_SHOWN)
    
    if window = IntPtr.Zero then
        printfn $"Error creating window: {SDL_GetError()}"
    
    let renderer = SDL_CreateRenderer (window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED ||| SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC)
    
    if renderer = IntPtr.Zero then
        printfn $"Error creating renderer: {SDL_GetError()}"
    
    SDL_SetRenderDrawColor(renderer, 0x00uy, 0x00uy, 0xFFuy, 0xFFuy) |> ignore
    
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
    0