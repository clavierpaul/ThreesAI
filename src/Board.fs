namespace ThreesAI

type Tile = int
type Coords = int * int
type Board = Map<Coords, Tile>

module Board =
    let empty: Board = Map.empty