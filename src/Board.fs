namespace ThreesAI

open ThreesAI.Controls

type Tile =
    | Empty
    | Tile of int
    
type Coords = int * int
type Board = Tile [,]

module Board =
    let empty: Board = Array2D.create 4 4 Empty
    
    let private transpose (board: Board) =
        Array2D.init 4 4 (fun r c -> board.[c, r])
        
    let private reverseRows (board: Board) =
        Array2D.init 4 4 (fun r c -> board.[3 - r, c])

    // Rotate the board so that shifting code is simpler
    let private rotateForDirection (direction: ShiftDirection) board =
        match direction with
        | Up    -> board |> reverseRows |> transpose
        | Down  -> board |> transpose   |> reverseRows
        | Left  -> board
        | Right -> board |> transpose   |> reverseRows |> transpose |> reverseRows
    
    let private rotateForDirectionInverse (direction: ShiftDirection) =
        match direction with
        | Up    -> rotateForDirection Down
        | Down  -> rotateForDirection Up
        | Left  -> rotateForDirection Left
        | Right -> rotateForDirection Right
        
    let private canMerge src dest =
        match src, dest with
        | Empty  , Empty   -> false
        | Empty  , Tile _  -> false
        | Tile _ , Empty   -> true
        | Tile 1 , Tile 2  -> true
        | Tile 2 , Tile 1  -> true
        | Tile at, Tile bt -> at = bt
        
    let private mergeLeft tile destTile  =
        // I don't really like this but oh well
        if canMerge tile destTile then
            match tile, destTile with
            | Tile n, Empty  -> Tile n
            | Tile s, Tile n -> Tile (s + n)
            | _, _ -> failwith "Merge left called with invalid tiles"
        else
            destTile
    
    let private mergeColumnLeft column destColumn =
        List.map2 mergeLeft column destColumn
        
    let private getColumn col (board: Board) =
        [for y in [0..3] do board.[col, y]]
    
    let private createNextBoard (oldBoard: Board) =
        Array2D.init 4 4 (fun x y -> if x = 0 then oldBoard.[x, y] else Empty)
    
    let shift direction board =
        // Hacky, hopefully temp solution
        let rotated = board |> rotateForDirection direction
        let rec shiftColumn ci previousColumn newBoardList =
            if ci = 4 then
                newBoardList @ previousColumn
            else
                let currentColumn = rotated |> getColumn ci
                let merged = mergeColumnLeft currentColumn previousColumn
                let remainder = List.map2 (fun src dest -> if canMerge src dest then Empty else src) currentColumn previousColumn
                shiftColumn (ci + 1) remainder <| newBoardList @ merged
        
        let newBoardList = shiftColumn 1 (rotated |> getColumn 0) []
        
        let newBoard = Array2D.init 4 4 (fun x y -> newBoardList.[x * 4 + y])
        newBoard |> rotateForDirectionInverse direction