# Port of F# code to python

import numpy as np
from enum import Enum

class Direction(Enum):
    UP = 0,
    DOWN = 1,
    LEFT = 2,
    RIGHT = 3

def empty() -> np.ndarray:
    return np.zeros((4,4), dtype=np.int32)

def transpose(board):
    return np.transpose(board)

def reverseRows(board):
    return np.flip(board, 1)

def rotateForDirection(direction: Direction, board):
    if direction == Direction.UP:
        return transpose(reverseRows(board))
    elif direction == Direction.DOWN:
        return reverseRows(transpose(board))
    elif direction == Direction.LEFT:
        return board
    else:
        return board[::-1,::-1]

def rotateForDirectionInverse(direction: Direction, board):
    if direction == Direction.UP:
        return rotateForDirection(Direction.DOWN, board)
    elif direction == Direction.DOWN:
        return rotateForDirection(Direction.UP, board)
    elif direction == Direction.LEFT:
        return board
    else:
        return rotateForDirection(Direction.RIGHT, board)

def canMerge(src: int, dest: int) -> bool:
    if src == 0:
        return False
    elif dest == 0:
        return True
    elif (src == 1 and dest == 2) or (src == 2 and dest == 1):
        return True
    elif (src == 1 and dest == 1) or (src == 2 and dest == 2):
        return False
    else:
        return src == dest


def mergeLeft(tile: int, destTile: int) -> int:
    if canMerge(tile, destTile):
        return tile + destTile
    else:
        return destTile

def mergeColumnLeft(column, destColumn):
    return [mergeLeft(src, dest) for src, dest in zip(column, destColumn)]

def getColumn(col: int, board):
    return board[:,col]

def shift(direction, board):
    rotated = rotateForDirection(direction, board)
    newBoardList = []
    previousColumn = getColumn(0, rotated)
    for ci in range(1, 4):
        currentColumn = getColumn(ci, rotated)
        merged = mergeColumnLeft(currentColumn, previousColumn)
        previousColumn = [0 if canMerge(src, dest) else src for src, dest in zip(currentColumn, previousColumn)]
        newBoardList += merged

    newBoardList += previousColumn
    newBoard = np.transpose(np.reshape(newBoardList, (4, 4)))
    return rotateForDirectionInverse(direction, newBoard)

def tryGet(x, y, board):
    if (0 <= x < 4) and (0 <= y < 4):
        return board[x, y]
    else:
        return None