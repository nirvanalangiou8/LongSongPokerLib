from typing import List, Optional, Iterable, Any
from GenericPoker.ICardRule import ICardRule
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardOverAllHandRank, SimCardsCompType


class BaseCardRule(ICardRule):
    def __init__(self):
        self._min_straight_count = 5
        self._min_flush_count = 5
        self._min_flush_straight_count = 3
        self._min_kind_count = 2
        self._card_count = 8

    @property
    def min_straight_count(self) -> int:
        return self._min_straight_count

    @min_straight_count.setter
    def min_straight_count(self, value: int):
        self._min_straight_count = value

    @property
    def min_flush_count(self) -> int:
        return self._min_flush_count

    @min_flush_count.setter
    def min_flush_count(self, value: int):
        self._min_flush_count = value

    @property
    def min_flush_straight_count(self) -> int:
        return self._min_flush_straight_count

    @min_flush_straight_count.setter
    def min_flush_straight_count(self, value: int):
        self._min_flush_straight_count = value

    @property
    def min_kind_count(self) -> int:
        return self._min_kind_count

    @min_kind_count.setter
    def min_kind_count(self, value: int):
        self._min_kind_count = value

    @property
    def card_count(self) -> int:
        return self._card_count

    @card_count.setter
    def card_count(self, value: int):
        self._card_count = value

    def assemble_hand_rank(self, components_or_types: Optional[Iterable[Any]] = None) -> SimCardOverAllHandRank:
        if components_or_types is None:
            return SimCardOverAllHandRank.Nothing

        raw_list = list(components_or_types)
        if len(raw_list) == 0:
            return SimCardOverAllHandRank.Nothing

        # Check if list elements are PokerComponents or SimCardsCompType
        comp_types: List[SimCardsCompType] = []
        for item in raw_list:
            if hasattr(item, 'comp_type'):
                comp_types.append(item.comp_type)
            elif isinstance(item, SimCardsCompType):
                comp_types.append(item)

        valid_list = [
            c for c in comp_types
            if c != SimCardsCompType.Nothing and c != SimCardsCompType.None_
        ]
        if len(valid_list) == 0:
            return SimCardOverAllHandRank.Nothing

        # Sort high power to low power (using enum order)
        members = list(SimCardsCompType)
        valid_list.sort(key=lambda x: members.index(x), reverse=True)

        if len(valid_list) == 1:
            name = valid_list[0].value
            for rank in SimCardOverAllHandRank:
                if rank.value == name or rank.name == name:
                    return rank
            return SimCardOverAllHandRank.None_

        if len(valid_list) == 2:
            c1 = valid_list[0]
            c2 = valid_list[1]

            if c1 == SimCardsCompType.ThreeOfKind and c2 == SimCardsCompType.Pair:
                return SimCardOverAllHandRank.FullHouse
            if c1 == SimCardsCompType.ThreeCardsFlushStraight and c2 == SimCardsCompType.Pair:
                return SimCardOverAllHandRank.Mansion
            if c1 == SimCardsCompType.Pair and c2 == SimCardsCompType.Pair:
                return SimCardOverAllHandRank.TwoPairs
            if c1 == SimCardsCompType.ThreeCardsFlushStraight and c2 == SimCardsCompType.ThreeCardsFlushStraight:
                return SimCardOverAllHandRank.ThreeCardsFlushStraight

            return SimCardOverAllHandRank.None_

        return SimCardOverAllHandRank.None_
