namespace ThreesAI

open System

type Deck = Tile list

module Deck =
    let private deck = [| Tile 1; Tile 1; Tile 1; Tile 1; Tile 2; Tile 2; Tile 2; Tile 2; Tile 3; Tile 3; Tile 3; Tile 3 |]
    
    let private random = Random ()
    
    // Fisher-Yates shuffle stolen and modified from Rosetta Code
    // Returns a new shuffled deck
    let create (): Deck =
        let newDeck = Array.copy deck
        let swap i j =
            let item = newDeck.[i]
            newDeck.[i] <- newDeck.[j]
            newDeck.[j] <- item
        let len = Array.length newDeck
        [0 .. (len - 2)] |> List.iter (fun i -> swap i (random.Next(i, len)))
        List.ofArray newDeck
    