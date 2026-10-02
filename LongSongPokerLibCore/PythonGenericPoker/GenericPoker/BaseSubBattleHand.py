from typing import List, Optional, Any
from GenericPoker.BasePokerCard import BasePokerCard
from GenericPoker.EightCard.EightCardSubBattleHand import EightCardsBattleHandRank


class BaseSubBattleHand:
    def __init__(self, cards: Optional[List[Any]] = None):
        self._cards: List[Any] = list(cards) if cards is not None else []
        self._battle_hand_rank: EightCardsBattleHandRank = EightCardsBattleHandRank.Nothing
        self._hand_power: int = 0
        self._hand_name: str = ""

    @property
    def cards(self) -> List[Any]:
        return self._cards

    @property
    def hand_power(self) -> int:
        return self._hand_power

    @property
    def hand_name(self) -> str:
        return self._hand_name

    @property
    def battle_hand_rank(self) -> EightCardsBattleHandRank:
        return self._battle_hand_rank

    def get_hand_string(self, separator: str = "_") -> str:
        return separator.join(card.card_str for card in self._cards)

    def compare_to(self, other: 'BaseSubBattleHand') -> int:
        return 0

    def __lt__(self, other: 'BaseSubBattleHand') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'BaseSubBattleHand') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'BaseSubBattleHand') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'BaseSubBattleHand') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, BaseSubBattleHand):
            return False
        return self.compare_to(other) == 0

    def __ne__(self, other: Any) -> bool:
        return not (self == other)
