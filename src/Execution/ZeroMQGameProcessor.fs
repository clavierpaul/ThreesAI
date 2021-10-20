module ThreesAI.Execution.ZeroMQGameProcessor

open type SDL2.SDL
open FsNetMQ
open FSharp.Json
open ThreesAI
open Controls
open System.Text
    
type GameData = {
    Board: int list
    Next: int
    Score: int
    GameOver: bool
}

let stateToGameData (state: Game.State) =
    let board = state.Board
                    |> Seq.cast<Tile>
                    |> Seq.map Board.getTileValue
                    |> List.ofSeq
    
    { Board    = board
      Next     = Board.getTileValue state.NextTile
      Score    = state.Score
      GameOver = state.GameOver }
    
let newStateFromMessage message state =
    match message with
    | 1uy -> state |> Game.shift Up
    | 2uy -> state |> Game.shift Down
    | 3uy -> state |> Game.shift Left
    | 4uy -> state |> Game.shift Right
    | 5uy -> Game.create ()
    | _   -> state
    
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
        Socket.bind rep "tcp://127.0.0.1:5555"
        
        let rec updateLoop () = async {
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
        