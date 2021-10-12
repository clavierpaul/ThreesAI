module ThreesAI.Controls

open ThreesAI
open Board

module Controls =
    type ShiftDirection =
        | Up
        | Down
        | Left
        | Right
    
    let private toShiftVector direction =
        match direction with
        | Up    -> ( 0, -1)
        | Down  -> ( 0,  1)
        | Left  -> (-1,  0)
        | Right -> ( 1,  0)
    
    let private shiftTile (sx, sy) (((tx, ty), tile): Coords * Tile) : Coords * Tile =
        let rx = sx + tx
        let ry = sy + ty
        if rx < 0 || ry < 0 || rx >= 4 || ry >= 4 then
            ((tx, ty), tile)
        else
            ((rx, ry), tile)
        
    
    let shift direction (board: Board): Board =
        let vector = toShiftVector direction
        Map.toList board
            |> List.map (shiftTile vector)
            |> Map.ofList
        