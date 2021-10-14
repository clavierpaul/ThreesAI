module ThreesAI.Rendering
    open System
    open ResultBuilder
    open type SDL2.SDL
    
    type WindowProperties = string * int * int
    type Window = nativeint
    type Renderer = nativeint
    
    
    let private sdlInit () =
        if (SDL_Init SDL_INIT_VIDEO) <> 0 then
            Error <| SDL_GetError ()
        else
            Ok ()
            
    let private createWindow ((title, width, height): WindowProperties): Result<Window, string> =
        let window = SDL_CreateWindow (title,
                                       SDL_WINDOWPOS_UNDEFINED,
                                       SDL_WINDOWPOS_UNDEFINED,
                                       width,
                                       height,
                                       SDL_WindowFlags.SDL_WINDOW_SHOWN)
        
        if window = IntPtr.Zero then
            Error <| SDL_GetError ()
        else
            Ok window
            
    let private createRenderer window: Result<Renderer, string> =
        let renderer = SDL_CreateRenderer (window,
                                           -1,
                                           SDL_RendererFlags.SDL_RENDERER_ACCELERATED ||| SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC)
            
        if renderer = IntPtr.Zero then
           Error <| SDL_GetError ()
        else
            Ok renderer
    
    let init (windowProperties: WindowProperties): Result<Window * Renderer, string> = resultBuilder {
        do! sdlInit ()
        let! window = createWindow windowProperties
        let! renderer = createRenderer window
        
        return (window, renderer)
    }
    