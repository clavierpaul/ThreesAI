namespace ThreesAI

open type SDL2.SDL
open ThreesAI.Game

module Display =
    type Textures =
        { Background: Texture
          Tiles: Texture
          NextTiles: Texture
          Numbers: Texture
          ScoreLabel: Texture }
        
    type SDLData =
        { Window: Rendering.Window
          Renderer: Rendering.Renderer
          Textures: Textures }
    
    let tileAtlas = Map.empty
                        .Add(1,    (0, 0))
                        .Add(2,    (1, 0))
                        .Add(3,    (2, 0))
                        .Add(6,    (0, 1))
                        .Add(12,   (1, 1))
                        .Add(24,   (2, 1))
                        .Add(48,   (0, 2))
                        .Add(96,   (1, 2))
                        .Add(192,  (2, 2))
                        .Add(384,  (0, 3))
                        .Add(768,  (1, 3))
                        .Add(1536, (2, 3))
                        .Add(3072, (0, 4))
                        .Add(6144, (1, 4))
   
    let renderNextTile texture state renderer =
        let w = texture.Width / 4
        let h = texture.Height
        
        let tileClipPosition =
            match state.NextTile with
            | Tile 1 -> 0
            | Tile 2 -> w
            | Tile 3 -> w * 2
            | Tile _ -> w * 3
            | Empty  -> failwith "Next tile is empty"
        
        Texture.renderer texture renderer
            |> Texture.renderAt (258, 12)
            |> Texture.clip (SDL_Rect ( x = tileClipPosition, y = 0, w = w, h = h ))
            |> Texture.render
        
        renderer
    
    let renderTiles texture state renderer =
        let w = texture.Width / 3
        let h = texture.Height / 5
        
        let clipRectangleFromTileNumber value =
            let x, y = tileAtlas.[value]
            SDL_Rect ( x = x * w, y = y * h, w = w, h = h )
            
        let renderTile x y tile =
            let tileX = x * (w + 2)
            let tileY = y * (h + 2)
            match tile with
            | Empty -> ()
            | Tile value ->
                Texture.renderer texture renderer
                    |> Texture.renderAt (11 + tileX, 43 + tileY)
                    |> Texture.clip (clipRectangleFromTileNumber value)
                    |> Texture.render
        
        state.Board |> Array2D.iteri renderTile
        
        renderer
    
    let renderBoard texture renderer =
        Texture.renderer texture renderer
            |> Texture.renderAt (8, 38)
            |> Texture.render
            
        renderer
        
    let renderScore texture state renderer =
        let w = texture.Width / 10
        let h = texture.Height
        
        let digits = state.Score
                         |> string
                         |> Seq.toList
                         |> Seq.map (string >> int)
            
        let renderDigit i digit =
            let x = 12 + i * w
            
            Texture.renderer texture renderer
                |> Texture.renderAt (x, 27)
                |> Texture.clip (SDL_Rect ( x = w * digit, y = 0, w = w, h = h ))
                |> Texture.render
        
        digits |> Seq.iteri renderDigit
        renderer
        
    let renderScoreLabel texture renderer =
        Texture.renderer texture renderer
            |> Texture.renderAt (16, 15)
            |> Texture.render
        
        renderer
    
    let render renderer textures state =
        renderer
            |> renderBoard textures.Background
            |> renderTiles textures.Tiles state
            |> renderNextTile textures.NextTiles state
            |> renderScoreLabel textures.ScoreLabel
            |> renderScore textures.Numbers state
            |> ignore