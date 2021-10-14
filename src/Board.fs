namespace ThreesAI

type Tile =
    | Empty
    | Tile of int
    
type Coords = int * int
type Board = Tile [,]

module Board =
    let empty: Board = Array2D.zeroCreate 4 4