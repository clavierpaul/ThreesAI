from tf_agents.environments import py_environment
from tf_agents.specs import array_spec
from tf_agents.trajectories import time_step as ts
from communication import GameConnector
import numpy as np
from typing import Dict, List, Union, cast


class ThreesEnv(py_environment.PyEnvironment):
    connector: GameConnector
    last_state: Dict

    def __init__(self):
        self.connector = GameConnector("127.0.0.1", 5555)
        self._action_spec = array_spec.BoundedArraySpec(
            shape=(), dtype=np.int32, minimum=0, maximum=3, name='action')

        self._observation_spec = array_spec.BoundedArraySpec(
            shape=(17,), dtype=np.int32, minimum=0, maximum=6144, name='observation')

    def __state_to_observation(self, state: Dict):
        board = state['Board']
        board.append(state['Next'])
        return np.array(board, dtype=np.int32)

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
    def __score_board(self, state: Dict):
        board = np.array(state['Board'][0:16])
        board = np.reshape(board, (4, 4))

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

    def action_spec(self):
        return self._action_spec

    def observation_spec(self):
        return self._observation_spec

    def _reset(self):
        self.last_state = self.connector.restart()
        return ts.restart(self.__state_to_observation(self.last_state))

    def _step(self, action):
        action = action.item(0) + 1
        # No reward for invalid moves
        if (action not in self.last_state['ValidMoves']):
            state = self.connector.shift(action)
            self.last_state = state
            return ts.transition(self.__state_to_observation(self.last_state), reward=0, discount=1.0)

        state = self.connector.shift(action)
        self.last_state = state

        if state['GameOver']:
            return ts.termination(self.__state_to_observation(state), reward=state['Score'])
        else:
            return ts.transition(self.__state_to_observation(state), reward=self.__score_board(state), discount=1.0)
