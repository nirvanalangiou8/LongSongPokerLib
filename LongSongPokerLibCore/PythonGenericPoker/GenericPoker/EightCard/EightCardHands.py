from typing import Optional, Any
from GenericPoker.BaseSubBattleHand import BaseSubBattleHand


class EightCardHands:
    def __init__(self, first_hand: BaseSubBattleHand, second_hand: BaseSubBattleHand):
        self._firstHand = first_hand
        self._secondHand = second_hand

    @property
    def total_power(self) -> int:
        return self._firstHand.hand_power + self._secondHand.hand_power

    @property
    def front_hand(self) -> BaseSubBattleHand:
        return self._firstHand

    @property
    def back_hand(self) -> BaseSubBattleHand:
        return self._secondHand

    def compare_to(self, other: Optional['EightCardHands']) -> int:
        if other is None:
            return 1
        if self._firstHand == other._firstHand and self._secondHand == other._secondHand:
            return 0
        if self.total_power == other.total_power:
            return 0
        elif self.total_power > other.total_power:
            return 1
        else:
            return -1

    def __lt__(self, other: 'EightCardHands') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'EightCardHands') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'EightCardHands') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'EightCardHands') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, EightCardHands):
            return False
        return self.total_power == other.total_power

    def __ne__(self, other: Any) -> bool:
        return not (self == other)
