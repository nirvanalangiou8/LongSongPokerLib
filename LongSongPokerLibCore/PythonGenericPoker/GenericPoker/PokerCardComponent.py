from typing import List, TypeVar, Generic, Any
from GenericPoker.IPokerCardInterfaces import IJoker
from GenericPoker.BasePokerCard import BasePokerCard

TEnum = TypeVar('TEnum')
TCard = TypeVar('TCard', bound=BasePokerCard)


class PokerCardComponent(Generic[TEnum, TCard]):
    def __init__(self, comp_rank: TEnum = None, cards: List[TCard] = None):
        self.comp_rank: TEnum = comp_rank
        self._cards: List[TCard] = list(cards) if cards is not None else []

    @property
    def cards(self) -> List[TCard]:
        return self._cards

    @cards.setter
    def cards(self, value: List[TCard]):
        self._cards = list(value)

    @property
    def card_count(self) -> int:
        return len(self._cards)

    @property
    def comp_string(self) -> str:
        return "#".join(c.card_unit_test_str for c in self._cards)

    def comp_unique_key(self) -> str:
        return "_".join(c.card_str for c in self._cards)

    def compare_to(self, other: 'PokerCardComponent[TEnum, TCard]') -> int:
        if self.comp_rank != other.comp_rank:
            # Enum comparison in C#: compare enum values/names or underlying int if IntEnum
            val_self = self.comp_rank.value if hasattr(self.comp_rank, 'value') else self.comp_rank
            val_other = other.comp_rank.value if hasattr(other.comp_rank, 'value') else other.comp_rank
            # For SimCardsCompType / BaseCompType in C#, it's enum index / order
            # To match C# Enum.CompareTo, we can compare enum names or values
            # But in C# Enum CompareTo compares underlying int values!
            if hasattr(self.comp_rank, '__class__') and issubclass(self.comp_rank.__class__, object):
                # Check enum member order or enum int
                try:
                    members = list(self.comp_rank.__class__)
                    idx_self = members.index(self.comp_rank)
                    idx_other = members.index(other.comp_rank)
                    if idx_self != idx_other:
                        return 1 if idx_self > idx_other else -1
                except Exception:
                    pass
            if val_self > val_other:
                return 1
            elif val_self < val_other:
                return -1

        for card_a, card_b in zip(self._cards, other._cards):
            cmp = card_a.compare_to_dont_care_suit(card_b)
            if cmp != 0:
                return cmp

        for card_a, card_b in zip(self._cards, other._cards):
            is_a_joker = isinstance(card_a, IJoker)
            is_b_joker = isinstance(card_b, IJoker)

            if is_a_joker and is_b_joker:
                jp_a = card_a.joker_power
                jp_b = card_b.joker_power
                if jp_a > jp_b:
                    return 1
                elif jp_a < jp_b:
                    return -1
            elif is_a_joker:
                return -1
            elif is_b_joker:
                return 1

        return 0

    def __lt__(self, other: 'PokerCardComponent[TEnum, TCard]') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'PokerCardComponent[TEnum, TCard]') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'PokerCardComponent[TEnum, TCard]') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'PokerCardComponent[TEnum, TCard]') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, PokerCardComponent):
            return False
        if self.comp_rank != other.comp_rank:
            return False
        if len(self._cards) != len(other._cards):
            return False
        for c1, c2 in zip(self._cards, other._cards):
            if c1 != c2:
                return False
        return True

    def __ne__(self, other: Any) -> bool:
        return not (self == other)

    def __repr__(self) -> str:
        return f"{self.comp_rank}: {self.comp_string}"
