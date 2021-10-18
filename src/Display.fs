namespace ThreesAI

open ThreesAI.Game

module Display =
    type Textures =
        { Background: Texture
          Tiles: Texture
          NextTiles: Texture
          Numbers: Texture
          ScoreLabel: Texture }

    [<Literal>]
    let renderScale = 1
    
    [<Literal>]
    let tileW = 32
    
    [<Literal>]
    let tileH = 48
    
    [<Literal>]
    let screenWidth = 156

    [<Literal>]
    let screenHeight = 253
    
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
                        
    let private clipRectangleFromTileNumber value =
        let x, y = tileAtlas.[value]
        { X = x * 32; Y = y * 48; W = 32; H = 48 }
    
    let renderTile textureAtlas renderer (boardX, boardY) (x, y) scale tile =
        let gridX = boardX + x * (tileW + 2) + 3
        let gridY = boardY + 2 + y * (tileH + 2) + 3
        match tile with
        | Empty -> ()
        | Tile value -> Texture.render textureAtlas renderer (scale * gridX, scale * gridY) (Some <| clipRectangleFromTileNumber value)
        
    let renderNextTile textures state renderer =
        let nextTileAtlas = textures.NextTiles
        let scale = nextTileAtlas.Scale
        let x = (screenWidth * renderScale * 2 - scale * (nextTileAtlas.Width / 4 + 16))
        let y = 12 * renderScale
        
        let w = (nextTileAtlas.Width / 4)
        let h = nextTileAtlas.Height
        
        match state.NextTile with
        | Tile 1 -> Texture.render nextTileAtlas renderer (x, y) (Some <| { X = 0; Y = 0; W = w; H = h })
        | Tile 2 -> Texture.render nextTileAtlas renderer (x, y) (Some <| { X = w; Y = 0; W = w; H = h })
        | Tile 3 -> Texture.render nextTileAtlas renderer (x, y) (Some <| { X = w * 2; Y = 0; W = w; H = h })
        | Tile _ -> Texture.render nextTileAtlas renderer (x, y) (Some <| { X = w * 3; Y = 0; W = w; H = h })
        | Empty -> failwith "Next tile is empty"
        
    let renderNumber textures value renderer =
        let numberAtlas = textures.Numbers
        let w = (numberAtlas.Width / 10)
        let h = numberAtlas.Height
        
        let digits = value
                     |> string
                     |> Seq.toList
                     |> Seq.map (string >> int)
                     
        
        let renderDigit i digit =
            let x = (renderScale * 12) + i * w * numberAtlas.Scale
            let y = (renderScale * 27)
            
            Texture.render numberAtlas renderer (x, y) (Some <| { X = w * digit; Y = 0; W = w; H = h })
        
        digits |> Seq.iteri renderDigit
        
    let renderScore textures state renderer =
        let scoreLabel = textures.ScoreLabel
        
        Texture.render scoreLabel renderer (renderScale * 16, renderScale * 15) None
        renderNumber textures state.Score renderer
        
    let render renderer textures state =
        let board = state.Board
        
        let boardY = screenHeight - textures.Background.Height - 8
        Texture.render textures.Background renderer (textures.Background.Scale * 8, textures.Background.Scale * boardY) None
        Array2D.iteri (fun x y -> renderTile textures.Tiles renderer (8, boardY) (x, y) textures.Tiles.Scale) board
        renderer |> renderNextTile textures state
        renderer |> renderScore textures state