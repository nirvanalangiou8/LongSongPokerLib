from enum import Enum
from typing import List, Sequence


class SpaceType(Enum):
    Cartesian = "Cartesian"
    Combination = "Combination"


class SpaceDef:
    def __init__(self, space_type: SpaceType, pool_size: int, dimensions: int):
        self.type: SpaceType = space_type
        self.pool_size: int = pool_size
        self.dimensions: int = dimensions


class PokerMath:
    PascalTable = [
        [1, 0, 0, 0, 0, 0, 0, 0, 0],
        [1, 1, 0, 0, 0, 0, 0, 0, 0],
        [1, 2, 1, 0, 0, 0, 0, 0, 0],
        [1, 3, 3, 1, 0, 0, 0, 0, 0],
        [1, 4, 6, 4, 1, 0, 0, 0, 0],
        [1, 5, 10, 10, 5, 1, 0, 0, 0],
        [1, 6, 15, 20, 15, 6, 1, 0, 0],
        [1, 7, 21, 35, 35, 21, 7, 1, 0],
        [1, 8, 28, 56, 70, 56, 28, 8, 1],
        [1, 9, 36, 84, 126, 126, 84, 36, 9],
        [1, 10, 45, 120, 210, 252, 210, 120, 45],
        [1, 11, 55, 165, 330, 462, 462, 330, 165],
        [1, 12, 66, 220, 495, 792, 924, 792, 495],
        [1, 13, 78, 286, 715, 1287, 1716, 1716, 1287],
        [1, 14, 91, 364, 1001, 2002, 3003, 3432, 3003]
    ]

    @staticmethod
    def get_unified_win_rate(parsed_offsets: Sequence[int], schema: Sequence[SpaceDef], prob_min: float, prob_max: float) -> float:
        global_index = 0
        current_multiplier = 1
        offset_pointer = len(parsed_offsets) - 1

        for i in range(len(schema) - 1, -1, -1):
            space = schema[i]
            block_index = 0
            block_total = 0

            if space.type == SpaceType.Cartesian:
                block_index = parsed_offsets[offset_pointer]
                offset_pointer -= 1
                block_total = space.pool_size
            elif space.type == SpaceType.Combination:
                block_total = PokerMath.PascalTable[space.pool_size][space.dimensions]
                vars_left = space.dimensions
                for v in range(space.dimensions):
                    current_rank_offset = parsed_offsets[offset_pointer - (space.dimensions - 1) + v]
                    if current_rank_offset >= vars_left:
                        block_index += PokerMath.PascalTable[current_rank_offset][vars_left]
                    vars_left -= 1
                offset_pointer -= space.dimensions

            global_index += block_index * current_multiplier
            current_multiplier *= block_total

        total_combinations = current_multiplier
        # Relative rank index 0 to total_combinations - 1
        rank_ratio = global_index / (total_combinations - 1) if total_combinations > 1 else 0.0
        return prob_min + rank_ratio * (prob_max - prob_min)
