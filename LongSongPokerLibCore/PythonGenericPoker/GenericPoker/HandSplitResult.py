from typing import Optional
from GenericPoker.EightCard.EightCardSubBattleHand import EightCardSubBattleHand


class HandSplitResult:
    def __init__(self, front_hand: EightCardSubBattleHand, back_hand: EightCardSubBattleHand,
                 front_win_rate: float, back_win_rate: float, total_score: float):
        self.front_hand: EightCardSubBattleHand = front_hand
        self.back_hand: EightCardSubBattleHand = back_hand
        self.front_win_rate: float = front_win_rate
        self.back_win_rate: float = back_win_rate
        self.total_score: float = total_score
