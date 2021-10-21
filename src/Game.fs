module ThreesAI.Game

open System
open Controls

let private random = Random ()
    
type State = {
    Board: Board
    NextTile: Tile
    Deck: Deck
    BonusDeck: Deck
    Score: int
    GameOver: bool
}
    
let private getPlacementSpots direction state =
    let edge =
       match direction with
       | Up    -> [for x in [0..3] do (x, 3), state.Board.[x, 3]]
       | Down  -> [for x in [0..3] do (x, 0), state.Board.[x, 0]]
       | Left  -> [for y in [0..3] do (3, y), state.Board.[3, y]]
       | Right -> [for y in [0..3] do (0, y), state.Board.[0, y]]
    
    edge |> List.filter (fun (_, space) -> space = Empty)
    
let private setBonusDeck state =
    let bestTile = Board.max state.Board |> Board.getTileValue
    if bestTile >= 48 then
        if List.length state.BonusDeck = 0 || bestTile / 8 <> (List.head state.BonusDeck |> Board.getTileValue) then
            { state with BonusDeck = Tile (bestTile / 8) :: state.BonusDeck }
        else
            state
    else
        state
    
let private getNextTile state =
    if List.length state.BonusDeck > 0 && random.Next(0, 21) = 0 then
        { state with NextTile = state.BonusDeck.[random.Next (0, (List.length state.BonusDeck) - 1)] }
    else
        let deck = if List.length state.Deck > 0 then state.Deck else Deck.create ()
        { state with NextTile = List.head deck; Deck = List.tail deck }
    
let private placeTile direction state =
    let spots = getPlacementSpots direction state
    let x, y = fst <| spots.[random.Next(0, (List.length spots) - 1)]
    
    // Lazy, but Array2D is mutable so shrug
    let newBoard = Array2D.copy state.Board
    newBoard.[x, y] <- state.NextTile
    
    { getNextTile state with Board = newBoard; }

let didShiftOccur before after =
    let beforeFlat = Seq.cast<Tile> before
    let afterFlat = Seq.cast<Tile> after
    
    Seq.contains false <| Seq.map2 (fun a b -> a = b) beforeFlat afterFlat

let private canTileMerge (board: Board) x y tile =
    let adjacent =
        [ Board.tryGet (x - 1, y) board
          Board.tryGet (x + 1, y) board
          Board.tryGet (x, y - 1) board
          Board.tryGet (x, y + 1) board ] |> List.filter (fun result -> result <> None) |> List.map unwrap

    let mergeResults = adjacent |> List.map (Board.canMerge tile)
    List.contains true mergeResults

let private detectGameOver state =
    let canTilesMerge = state.Board |> Array2D.mapi (canTileMerge state.Board) |> Seq.cast<bool> |> Seq.contains true
    if canTilesMerge then
        state
    else
        { state with GameOver = true }

let calculateScore state =
    let scoreTile totalScore tile =
        match tile with
        | Empty  -> totalScore
        | Tile 1 -> totalScore
        | Tile 2 -> totalScore
        | Tile t -> totalScore + (int <| 3.0 ** (Math.Log2(float t / 3.0) + 1.0))
    
    { state with Score = Seq.cast<Tile> state.Board |> Seq.fold scoreTile 0 }
    
let shift direction state =
    let shifted = { state with Board = state.Board |> Board.shift direction }
    if didShiftOccur state.Board shifted.Board then
        setBonusDeck shifted
        |> placeTile direction
        |> calculateScore
        |> detectGameOver
    else
        state

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
    { Board = board
      NextTile = List.head newDeck
      Deck = List.tail newDeck
      BonusDeck = []; Score = 0
      GameOver = false } |> calculateScore