from numpy.core.defchararray import array
from tensorflow.python.util.tf_decorator import rewrap
from tf_agents.environments import py_environment
from tf_agents.specs import array_spec
import tensorflow as tf
from tensorflow import convert_to_tensor
from tf_agents.trajectories import time_step as ts
from board import Direction
from game import Threes
import numpy as np
import board
from typing import Dict, List, Union, cast


class ThreesEnv(py_environment.PyEnvironment):
    game: Threes
    last_board: np.ndarray
    moves = [Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT]
    invalid_moves = 0

    def __init__(self):
        self.game = Threes()
        self.reset()

        self._action_spec = array_spec.BoundedArraySpec(
            shape=(), dtype=np.int32, minimum=0, maximum=3, name='action')

        self._observation_spec = {'observation':  array_spec.BoundedArraySpec(
            shape=(17,), dtype=np.int32, minimum=0, maximum=6144, name='observation'),
            'actions': array_spec.ArraySpec(shape=(4,), dtype=np.int32)}

    def __get_observation(self):
        board = self.game.board.flatten()

        board = np.append(board, self.game.next_tile)
        return {'observation': np.array(board, dtype=np.int32), 'actions': self.__get_valid_moves_mask() }

    def __try_get(self, board, x: int, y: int) -> Union[int, None]:
        if 0 <= x < 4 and 0 <= y < 4:
            return board[x, y]
        else:
            return None

    def __get_adjacent_tiles(self, board, x: int, y: int) -> List[int]:
        adjacent = [
            self.__try_get(board, x - 1, y),
            self.__try_get(board, x + 1, y),
            self.__try_get(board, x, y - 1),
            self.__try_get(board, x, y + 1),
        ]

        return cast(List[int], list(filter(lambda x: x != None, adjacent)))

    def __does_tile_match(self, a: int, b: int) -> bool:
        return (a == 1 and b == 2) or (a == 2 and b == 1) or (a == b)

    def __is_tile_double(self, src: int, dest: int) -> bool:
        return (src == 1 and dest == 2) or (dest == 2 * src)

    def __is_higher_value(self, src: int, dest: Union[int, None]) -> bool:
        return (dest == None) or (dest >= 3 and src < dest)

    # Reward algorithm from Threesus
    def __score_board(self):
        board = self.game.board

        score = 0

        for x in range(4):
            for y in range(4):
                tile = board[x, y]
                # An empty tile is worth 2 points
                if tile == 0:
                    score += 2
                    continue

                # Get adjacent tiles
                adjacent = self.__get_adjacent_tiles(board, x, y)

                matching = len(list(
                    filter(lambda t: self.__does_tile_match(t, tile), adjacent)))
                twice = len(list(
                    filter(lambda t: self.__is_tile_double(tile, t), adjacent)))

                # Matching tiles are worth twice as much as tiles that are double
                score += matching * 2 + twice

                # -1 for each pair of higher value tiles we're stuck between
                adjacent_horizontal_pair = [self.__try_get(
                    board, x - 1, y), self.__try_get(board, x + 1, y)]
                adjacent_vertical_pair = [self.__try_get(
                    board, x, y - 1), self.__try_get(board, x, y + 1)]

                if len(list(filter(lambda t: self.__is_higher_value(tile, t), adjacent_horizontal_pair))) == 2:
                    score -= 1

                if len(list(filter(lambda t: self.__is_higher_value(tile, t), adjacent_vertical_pair))) == 2:
                    score -= 1

        return score

    def __get_valid_moves_mask(self):
        move_mask = [0, 0, 0, 0]
        for i in range(4):
            move = self.moves[i]

            result = board.shift(move, self.game.board.copy())
            if self.game.didShiftOccur(self.game.board, result):
                move_mask[i] = 1
        
        return np.array(move_mask, dtype=np.int32)

    def action_spec(self):
        return self._action_spec

    def observation_spec(self):
        return self._observation_spec

    def _reset(self):
        self.game = Threes()
        self.game.reset()
        self.invalid_moves = 0
        return ts.restart(self.__get_observation())


    def _step(self, action):
        action = action.item(0)
        self.last_board = self.game.board.copy()
        self.game.shift(self.moves[action])
        
        if self.game.game_over:
            termination = ts.termination(self.__get_observation(), reward=self.game.score)
            self.reset()
            return termination
        else:
            return ts.transition(self.__get_observation(), reward=self.__score_board(), discount=1.0)