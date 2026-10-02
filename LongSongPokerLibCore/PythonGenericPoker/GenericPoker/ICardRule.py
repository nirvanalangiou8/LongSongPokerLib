from abc import ABC, abstractmethod
from typing import List, Optional, Iterable, Any
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardOverAllHandRank, SimCardsCompType


class ICardRule(ABC):
    @property
    @abstractmethod
    def min_straight_count(self) -> int:
        pass

    @min_straight_count.setter
    @abstractmethod
    def min_straight_count(self, value: int):
        pass

    @property
    @abstractmethod
    def min_flush_count(self) -> int:
        pass

    @min_flush_count.setter
    @abstractmethod
    def min_flush_count(self, value: int):
        pass

    @property
    @abstractmethod
    def min_flush_straight_count(self) -> int:
        pass

    @min_flush_straight_count.setter
    @abstractmethod
    def min_flush_straight_count(self, value: int):
        pass

    @property
    @abstractmethod
    def min_kind_count(self) -> int:
        pass

    @min_kind_count.setter
    @abstractmethod
    def min_kind_count(self, value: int):
        pass

    @property
    @abstractmethod
    def card_count(self) -> int:
        pass

    @card_count.setter
    @abstractmethod
    def card_count(self, value: int):
        pass

    @abstractmethod
    def assemble_hand_rank(self, components_or_types: Optional[Iterable[Any]] = None) -> SimCardOverAllHandRank:
        pass
