module ThreesAI.Game

open System

// This would be cleaner with monads
// but this will do for now
type State = {
    Board: Board
    Deck: Deck
}

let random = Random ()

let create () =
    let deck = Deck.create ()
    let board = Board.empty ()
    
    // I could do any number of unoptimized non-mutable methods
    // but this works a lot better
    let rec placementLoop tilesPlaced (deck: Deck) =
        if tilesPlaced = 9 then
            ()
        else
            
        let rx, ry = (random.Next (0, 4), random.Next (0, 4))
        if board.[rx, ry] = Empty then
            let next = List.head deck
            board.[rx, ry] <- next
            placementLoop (tilesPlaced + 1) (List.tail deck)
        else
            placementLoop tilesPlaced deck
    
    placementLoop 0 deck
    { Deck = deck; Board = board }

let shift direction state = { state with Board = state.Board |> Board.shift direction }