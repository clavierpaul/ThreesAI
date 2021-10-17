module ThreesAI.Game

open System
open Controls

// This would be cleaner with monads
// but this will do for now
type State = {
    Board: Board
    Deck: Deck
    BonusDeck: Deck
    GameOver: bool
}

let random = Random ()

let create () =
    let deck = Deck.create ()
    let board = Board.empty ()
    
    // I could do any number of unoptimized non-mutable methods
    // but this works a lot better
    let rec placementLoop tilesPlaced (deck: Deck) =
        if tilesPlaced = 9 then
            deck
        else
            
        let rx, ry = (random.Next (0, 4), random.Next (0, 4))
        if board.[rx, ry] = Empty then
            let next = List.head deck
            board.[rx, ry] <- next
            placementLoop (tilesPlaced + 1) (List.tail deck)
        else
            placementLoop tilesPlaced deck
    
    let newDeck = placementLoop 0 deck
    { Board = board; Deck = newDeck; BonusDeck = []; GameOver = false }
    
let getPlacementSpots direction state =
    let edge =
       match direction with
       | Up    -> [for x in [0..3] do (x, 3), state.Board.[x, 3]]
       | Down  -> [for x in [0..3] do (x, 0), state.Board.[x, 0]]
       | Left  -> [for y in [0..3] do (3, y), state.Board.[3, y]]
       | Right -> [for y in [0..3] do (0, y), state.Board.[0, y]]
    
    edge |> List.filter (fun (_, space) -> space = Empty)
    
let setBonusDeck state =
    let bestTile = Board.max state.Board |> Board.getTileValue
    if bestTile > 48 && bestTile / 8 <> (List.head state.BonusDeck |> Board.getTileValue) then
        { state with BonusDeck = Tile (bestTile / 8) :: state.BonusDeck }
    else
        state
    
let getNextTile state =
    if List.length state.BonusDeck > 0 && random.Next(0, 21) = 0 then
        state.BonusDeck.[random.Next (0, (List.length state.BonusDeck) - 1)], state
    else
        let deck = if List.length state.Deck > 0 then state.Deck else Deck.create ()
        List.head deck, { state with Deck = List.tail deck }
    
let placeTile direction state =
    let spots = getPlacementSpots direction state
    let x, y = fst <| spots.[random.Next(0, (List.length spots) - 1)]
    
    // Lazy, but Array2D is mutable so shrug
    let newBoard = Array2D.copy state.Board
    let nextTile, state = getNextTile state
    newBoard.[x, y] <- nextTile
    { state with Board = newBoard; }

let didShiftOccur before after =
    let beforeFlat = Seq.cast<Tile> before
    let afterFlat = Seq.cast<Tile> after
    
    Seq.contains false <| Seq.map2 (fun a b -> a = b) beforeFlat afterFlat

let shift direction state =
    let shifted = { state with Board = state.Board |> Board.shift direction }
    if didShiftOccur state.Board shifted.Board then
        placeTile direction shifted
    else
        state