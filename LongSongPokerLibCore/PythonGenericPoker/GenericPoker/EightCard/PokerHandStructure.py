from typing import List, Optional, Any
import functools
from GenericPoker.PokerEnumAndDicts import BaseCompType
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.BaseSubBattleHand import BaseSubBattleHand, BattleHandEnum, EightCardsBattleHandRank
from GenericPoker.EightCard.EightCardHands import EightCardHands


class PokerHandStructure:
    def __init__(self, other: Optional['PokerHandStructure'] = None):
        if other is not None:
            self.components: List[PokerCardComponent[BaseCompType, EightCardPokerCard]] = [
                PokerCardComponent(c.comp_rank, list(c.cards)) for c in other.components
            ]
            self.remaining_cards: List[EightCardPokerCard] = list(other.remaining_cards)
            self.final_comps_str: str = other.final_comps_str
        else:
            self.components = []
            self.remaining_cards = []
            self.final_comps_str = ""

    def clear(self) -> None:
        self.components.clear()
        self.remaining_cards.clear()
        self.final_comps_str = ""

    def add_comp(self, new_component: PokerCardComponent[BaseCompType, EightCardPokerCard]) -> None:
        self.components.append(new_component)

    def remove_last(self, count: int = 1) -> None:
        for _ in range(count):
            if self.components:
                self.components.pop()

    def sort_comps_and_classify(self) -> None:
        self.components.sort(key=functools.cmp_to_key(lambda c1, c2: c2.compare_to(c1)))
        groups = []
        for comp in self.components:
            rank_name = comp.comp_rank.value if hasattr(comp.comp_rank, 'value') else str(comp.comp_rank)
            if groups and groups[-1][0] == rank_name:
                groups[-1][1] += 1
            else:
                groups.append([rank_name, 1])

        comp_type_counts_list = [
            f"{key}" if count <= 1 else f"{key}*{count}"
            for key, count in groups
        ]
        self.final_comps_str = "_".join(comp_type_counts_list)

    @staticmethod
    def convert_comp_rank_to_battle_rank(comp_type: BaseCompType) -> EightCardsBattleHandRank:
        name = comp_type.value if hasattr(comp_type, 'value') else str(comp_type)
        for rank in EightCardsBattleHandRank:
            if rank.value == name or rank.name == name:
                return rank
        return EightCardsBattleHandRank.Nothing

    def set_remaining_cards(self, input_remaining: List[EightCardPokerCard]) -> None:
        self.remaining_cards.extend(input_remaining)

    def arrange_hands(self, strategy: Any) -> EightCardHands:
        from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator

        sorted_remaining_cards = sorted(self.remaining_cards, key=lambda item: item.poker_card_power, reverse=True)

        if len(self.components) == 4:
            new_battle_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict.get(
                (BaseCompType.Pair, BaseCompType.Pair), EightCardsBattleHandRank.TwoPairs
            )
            first_hand = BaseSubBattleHand(BattleHandEnum.FirstHand, new_battle_rank, self.components[1], self.components[2])
            second_hand = BaseSubBattleHand(BattleHandEnum.SecondHand, new_battle_rank, self.components[0], self.components[3])
        elif len(self.components) in (2, 3):
            first_hand, second_hand = strategy.arrange_comps(self.components)
        elif len(self.components) == 1:
            first_hand = BaseSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing)
            second_hand = BaseSubBattleHand(BattleHandEnum.SecondHand, self.convert_comp_rank_to_battle_rank(self.components[0].comp_rank), self.components[0])
        else:
            first_hand = BaseSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing)
            second_hand = BaseSubBattleHand(BattleHandEnum.SecondHand, EightCardsBattleHandRank.Nothing)

        new_remaining = second_hand.add_one_minor_card(sorted_remaining_cards)
        new_remaining = first_hand.add_minor_cards(new_remaining)
        new_remaining = second_hand.add_minor_cards(new_remaining)

        if len(new_remaining) > 0:
            print("Fatal errors")

        return EightCardHands(first_hand, second_hand)

    def compare_to(self, other: 'PokerHandStructure') -> int:
        for comp1, comp2 in zip(self.components, other.components):
            cmp = comp1.compare_to(comp2)
            if cmp != 0:
                return cmp
        if len(self.components) > len(other.components):
            return 1
        elif len(self.components) < len(other.components):
            return -1
        return 0

    def __lt__(self, other: 'PokerHandStructure') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'PokerHandStructure') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'PokerHandStructure') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'PokerHandStructure') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, PokerHandStructure):
            return False
        if len(self.components) != len(other.components):
            return False
        for comp1, comp2 in zip(self.components, other.components):
            if comp1 != comp2:
                return False
        return True

    def __ne__(self, other: Any) -> bool:
        return not (self == other)
