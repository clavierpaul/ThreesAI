# Port of F# code to python

from functools import reduce
from math import log2
from typing import List, cast
import numpy as np
from random import shuffle, randrange, choice

import board

class Threes():
    board: np.ndarray
    next_tile: int
    deck: List[int]
    bonus_deck: List[int]
    score: int
    game_over: bool

    def getPlacementSpots(self, direction: np.ndarray) -> List[tuple[tuple[int, int], int]]:
        edge = []
        
        if direction == board.Direction.UP:
            edge = [((3, x), self.board[3, x]) for x in range(4)]
        elif direction == board.Direction.DOWN:
            edge = [((0, x), self.board[0, x]) for x in range(4)]
        elif direction == board.Direction.LEFT:
            edge = [((y, 3), self.board[y, 3]) for y in range(4)]
        else:
            edge = [((y, 0), self.board[y, 0]) for y in range(4)]
        
        return list(filter(lambda e: e[1] == 0, edge))
    
    def setBonusDeck(self):
        bestTile = np.max(self.board)

        if bestTile < 48:
            return
        
        if len(self.bonus_deck) == 0 or bestTile / 8 != self.bonus_deck[-1]:
            self.bonus_deck.append(bestTile / 8)

    def getNextTIle(self):
        if len(self.bonus_deck) > 0 and randrange(0, 21) == 0:
            self.next_tile = choice(self.bonus_deck)
        else:
            if len(self.deck) == 0:
                self.createDeck()
            
            self.next_tile = self.deck.pop()
    
    def createDeck(self):
        self.deck = [1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3]
        shuffle(self.deck)
    
    def placeTile(self, direction):
        spots = self.getPlacementSpots(direction)
        x, y = choice(spots)[0]

        self.board[x, y] = self.next_tile
    
    def didShiftOccur(self, before: np.ndarray, after: np.ndarray) -> bool:
        beforeFlat = before.flatten()
        afterFlat = after.flatten()

        return False in [a == b for a, b in zip(beforeFlat, afterFlat)]
    
    def canTileMerge(self, x, y, tile):
        adjacent = [
            board.tryGet(x - 1, y, self.board),
            board.tryGet(x + 1, y, self.board),
            board.tryGet(x, y - 1, self.board),
            board.tryGet(x, y + 1, self.board) 
        ]

        adjacent = list(filter(lambda result: result != None, adjacent))
        mergeResults = list(map(lambda t: board.canMerge(tile, cast(int, t)), adjacent))

        return True in mergeResults
    
    def detectGameOver(self):
        canTilesMerge = False
        for x in range(4):
            for y in range(4):
                if self.canTileMerge(x, y, self.board[x, y]):
                    canTilesMerge = True
                    break
            
            if canTilesMerge:
                break
        
        self.game_over = not canTilesMerge
    
    def getTileScore(self, tile: int):
        if tile >= 3:
            return int(3 ** (log2(tile / 3) + 1))
        else:
            return 0

    def calculateScore(self):
        flat = self.board.flatten()

        self.score = 0
        for tile in flat:
            self.score += self.getTileScore(tile)
    
    def shift(self, direction):
        shifted = board.shift(direction, self.board)
        if self.didShiftOccur(self.board, shifted):
            self.board = shifted
            self.setBonusDeck()
            self.placeTile(direction)
            self.calculateScore()
            self.detectGameOver()

    def reset(self):
        self.createDeck()
        self.board = board.empty()

        tilesPlaced = 0

        while tilesPlaced < 9:
            rx, ry = (randrange(0, 4), randrange(0, 4))
            if self.board[rx, ry] == 0:
                self.board[rx, ry] = self.deck.pop()
                tilesPlaced += 1
        
        self.next_tile = self.deck.pop()
        self.bonus_deck = []
        self.calculateScore()
        self.game_over = False

    def __init__(self) -> None:
        self.reset()