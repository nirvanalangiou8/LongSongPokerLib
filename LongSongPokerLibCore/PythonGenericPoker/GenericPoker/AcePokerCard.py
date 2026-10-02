from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.IPokerCardInterfaces import IJokerStraightable


class AcePokerCard(SimPokerCard, EightCardPokerCard, IJokerStraightable):
    def __init__(self, card_id: int = 0, number: int = 0, suit=None, object_id: int = 0, deck_id: int = 1):
        super().__init__(card_id, number, suit, object_id, deck_id)
        self._replaced_number = 0

    @property
    def joker_power(self) -> int:
        return 100

    @property
    def number(self) -> int:
        return self._replaced_number if self._replaced_number != 0 else self._number

    def set_ace_fourteen_number(self, number: int) -> None:
        self._number = number
        self._replaced_number = number

    def set_straight_sub(self, number: int) -> None:
        self._number = number
        self._replaced_number = number

    def check_ace(self) -> None:
        pass
