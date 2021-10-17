module ThreesAI.Game

// This would be cleaner with monads
// but this will do for now
type State = {
    Board: Board
}

// Testing code
let createDummyBoard () =
    let mutable test = Board.empty
    test.[0, 0] <- Tile 1
    test.[1, 0] <- Tile 3
    test.[2, 0] <- Tile 3
    
    test.[0, 1] <- Tile 2
    
    test.[1, 2] <- Tile 1
    test.[2, 2] <- Tile 3
    test.[3, 2] <- Tile 1
    
    test.[2, 3] <- Tile 3
    test.[3, 3] <- Tile 2
    test

let init () = { Board = createDummyBoard () }

let shift direction state = { state with Board = state.Board |> Board.shift direction }