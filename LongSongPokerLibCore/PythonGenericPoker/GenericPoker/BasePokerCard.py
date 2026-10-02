from typing import Any
from GenericPoker.PokerEnumAndDicts import PokerSuit, PokerConst


class BasePokerCard:
    RegularSuitClubIndex = 1
    RegularSuitSpadeIndex = 4

    PokerCardPokerSuitModulationRatio = 1.0 / 32.0

    def __init__(self, card_id: int = 0, number: int = 0, suit: PokerSuit = PokerSuit.NoSuit, object_id: int = 0, deck_id: int = 1):
        self._cardID = card_id
        self._number = number
        self._suit = suit
        self._objectID = object_id
        self._deckID = deck_id

    @property
    def suit(self) -> PokerSuit:
        return self._suit

    @property
    def number(self) -> int:
        return self._number

    @property
    def number_str(self) -> str:
        return PokerConst.PokerNumberNameDict.get(self.number, str(self.number))

    @property
    def suit_str(self) -> str:
        return PokerConst.PokerSuitToSymbol.get(self._suit, "✖️")

    @property
    def card_str(self) -> str:
        return f"{self.number_str}{self.suit_str}"

    @property
    def card_str_num_only(self) -> str:
        return self.number_str

    @property
    def card_unit_test_str(self) -> str:
        return self.card_str

    @property
    def object_id(self) -> int:
        return self._objectID

    @object_id.setter
    def object_id(self, value: int):
        self._objectID = value

    @property
    def deck_id(self) -> int:
        return self._deckID

    @deck_id.setter
    def deck_id(self, value: int):
        self._deckID = value

    def init(self, card_id: int, number: int, suit: PokerSuit, object_id: int, deck_id: int):
        self._cardID = card_id
        self._number = number
        self._suit = suit
        self._objectID = object_id
        self._deckID = deck_id

    @property
    def poker_card_power(self) -> float:
        return self.number + float(int(self._suit)) * self.PokerCardPokerSuitModulationRatio

    def is_next_neighbor_number(self, next_card: 'BasePokerCard') -> bool:
        left_number = PokerConst.AceBigNumber if self.number == 1 else self.number
        right_number = next_card.number
        return left_number - right_number == 1

    def compare_to(self, b: 'BasePokerCard') -> int:
        if self > b:
            return 1
        elif self == b:
            return 0
        else:
            return -1

    def compare_to_dont_care_suit(self, b: 'BasePokerCard') -> int:
        if self.number > b.number:
            return 1
        elif self.number == b.number:
            return 0
        else:
            return -1

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, BasePokerCard):
            return False
        num_a = PokerConst.AceBigNumber if self.__class__.__name__ == "AcePokerCard" or self.__class__.__name__ == "AceCard" else self.number
        num_b = PokerConst.AceBigNumber if other.__class__.__name__ == "AcePokerCard" or other.__class__.__name__ == "AceCard" else other.number
        return (num_a == num_b) and (self._suit == other._suit)

    def __ne__(self, other: Any) -> bool:
        return not (self == other)

    def __gt__(self, other: 'BasePokerCard') -> bool:
        if self.number > other.number:
            return True
        elif self.number < other.number:
            return False
        else:
            return int(self._suit) > int(other._suit)

    def __ge__(self, other: 'BasePokerCard') -> bool:
        return (self > other) or (self == other)

    def __lt__(self, other: 'BasePokerCard') -> bool:
        if self.number < other.number:
            return True
        elif self.number > other.number:
            return False
        else:
            return int(self._suit) < int(other._suit)

    def __le__(self, other: 'BasePokerCard') -> bool:
        return (self < other) or (self == other)

    def __hash__(self) -> int:
        ret_number = PokerConst.AceBigNumber if (self.__class__.__name__ in ("AcePokerCard", "AceCard")) else self.number
        return hash((ret_number, self._suit))

    def __repr__(self) -> str:
        return self.card_str
