module ThreesAI.Execution.ZeroMQGameProcessor

open type SDL2.SDL
open FsNetMQ
open FSharp.Json
open ThreesAI
open Controls
open System.Text
    
type GameData = {
    Board: int list
    ValidMoves: byte list
    Next: int
    Score: int
    GameOver: bool
}
    
let newStateFromMessage message state =
    match message with
    | 1uy -> state |> Game.shift Up
    | 2uy -> state |> Game.shift Down
    | 3uy -> state |> Game.shift Left
    | 4uy -> state |> Game.shift Right
    | 5uy -> Game.create ()
    | m   -> failwith $"Attempted an invalid move {m}"

let testShift direction before board =
    board
        |> Board.shift direction
        |> Game.didShiftOccur before

let testDirection board direction  =
    match direction with
    | 1uy -> board |> testShift ShiftDirection.Up board
    | 2uy -> board |> testShift ShiftDirection.Down board
    | 3uy -> board |> testShift ShiftDirection.Left board
    | 4uy -> board |> testShift ShiftDirection.Right board
    | m   -> failwith $"Attempted an invalid move {m}"
    
let findValidMoves (board: Board) =
    [ 1uy; 2uy; 3uy; 4uy ]
        |> List.filter (testDirection board)
        
let stateToGameData (state: Game.State) =
    let board = state.Board
                    |> Seq.cast<Tile>
                    |> Seq.map Board.getTileValue
                    |> List.ofSeq
    
    { Board      = board
      ValidMoves = findValidMoves state.Board
      Next       = Board.getTileValue state.NextTile
      Score      = state.Score
      GameOver   = state.GameOver }
    
// If I had more time for this project I would find a more functional way to do this,
// but I'm going to do it this way for now
type zeroMQStateProcessor() =
    let mutable state = Game.create ()
    let mutable exit = false
    
    member this.AsyncRenderLoop (sdlData: Display.SDLData) = async {
        let rec renderLoop () =
            if not exit then 
                SDL_RenderClear sdlData.Renderer |> ignore
                Display.render sdlData.Renderer sdlData.Textures state
                SDL_RenderPresent sdlData.Renderer
                renderLoop ()
            else
                ()
        
        renderLoop ()
    }
    
    member this.StartRender (sdlData: Display.SDLData) =
        Async.Start(this.AsyncRenderLoop sdlData)
    
    member this.Start () =
        let rep = Socket.rep ()
        Socket.bind rep "tcp://*:5555"
        
        let rec updateLoop () = async {
            printfn "Waiting for input..."
            let message, _ = Frame.recv rep
            printfn $"Received {message.[0]}"
            
            state <- state |> newStateFromMessage message.[0]
            
            let gameData = state |> stateToGameData
            let serialized = Json.serialize gameData
                             |> Encoding.ASCII.GetBytes
            
            Frame.send rep serialized
            return! updateLoop ()
        }
        
        Async.Start(updateLoop ())
        