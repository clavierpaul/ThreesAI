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
        