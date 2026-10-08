from typing import Optional
from GenericPoker.BaseSubBattleHand import BaseSubBattleHand


class HandSplitResult:
    def __init__(self, front_hand: BaseSubBattleHand, back_hand: BaseSubBattleHand,
                 front_win_rate: float, back_win_rate: float, total_score: float):
        self.front_hand: BaseSubBattleHand = front_hand
        self.back_hand: BaseSubBattleHand = back_hand
        self.front_win_rate: float = front_win_rate
        self.back_win_rate: float = back_win_rate
        self.total_score: float = total_score
