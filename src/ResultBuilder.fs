namespace ThreesAI

type ResultBuilder () =
    member this.Bind(v, f) =
        match v with
        | Ok v -> f v
        | Error e -> Error e
    
    member this.Return value = Ok value

module ResultBuilder =
    let resultBuilder = ResultBuilder ()