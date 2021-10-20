module ThreesAI.Execution.CommonSDL

open type SDL2.SDL
open ThreesAI
open Display

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

let quit data =
    SDL_DestroyTexture  data.Textures.Tiles.Handle
    SDL_DestroyTexture  data.Textures.Background.Handle
    SDL_DestroyTexture  data.Textures.NextTiles.Handle
    SDL_DestroyTexture  data.Textures.ScoreLabel.Handle
    SDL_DestroyTexture  data.Textures.Numbers.Handle
    SDL_DestroyRenderer data.Renderer
    SDL_DestroyWindow   data.Window
    SDL_Quit ()