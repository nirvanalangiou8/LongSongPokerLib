from abc import ABC, abstractmethod
from GenericPoker.PokerEnumAndDicts import PokerSuit


class IJoker(ABC):
    @property
    @abstractmethod
    def joker_power(self) -> int:
        pass


class IJokerFlushable(IJoker):
    @abstractmethod
    def set_suit_sub(self, input_suit: PokerSuit) -> None:
        pass


class IJokerStraightable(IJoker):
    @abstractmethod
    def set_straight_sub(self, number: int) -> None:
        pass


class INumberReplaceable(ABC):
    @abstractmethod
    def check_ace(self) -> None:
        pass


class IAceable(INumberReplaceable):
    pass


class IAKable(INumberReplaceable):
    pass
