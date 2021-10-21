namespace ThreesAI

[<AutoOpen>]
module Prelude =
    [<Literal>]
    let renderScale = 1

    [<Literal>]
    let screenWidth = 312

    [<Literal>]
    let screenHeight = 506
    
    let unwrap result =
        match result with
        | None   -> failwith "Attempted to unwrap a None"
        | Some n -> n