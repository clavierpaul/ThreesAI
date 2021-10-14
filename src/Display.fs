namespace ThreesAI

module Display =
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

    let renderTile textureAtlas renderer (x, y) tile =
        match tile with
        | Empty -> ()
        | Tile value -> Texture.render textureAtlas renderer (x * 32 * 4, y * 48 * 4) (Some <| clipRectangleFromTileNumber value)
        
    let renderBoard renderer textureAtlas (board: Board) =
        Array2D.iteri (fun x y -> renderTile textureAtlas renderer (x, y)) board