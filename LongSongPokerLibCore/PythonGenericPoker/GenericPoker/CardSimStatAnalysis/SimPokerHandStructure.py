from typing import List, Optional, Any
import functools
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardsCompType
from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard


class SimPokerHandStructure:
    def __init__(self, other: Optional['SimPokerHandStructure'] = None):
        if other is not None:
            self.components: List[PokerCardComponent[SimCardsCompType, SimPokerCard]] = [
                PokerCardComponent(c.comp_rank, list(c.cards)) for c in other.components
            ]
            self.remaining_cards: List[SimPokerCard] = list(other.remaining_cards)
            self.final_comps_str: str = other.final_comps_str
        else:
            self.components = []
            self.remaining_cards = []
            self.final_comps_str = ""

    def clear(self) -> None:
        self.components.clear()
        self.remaining_cards.clear()
        self.final_comps_str = ""

    def add_comp(self, new_component: PokerCardComponent[SimCardsCompType, SimPokerCard]) -> None:
        self.components.append(new_component)

    def remove_last(self, count: int = 1) -> None:
        for _ in range(count):
            if self.components:
                self.components.pop()

    def sort_comps_and_classify(self) -> None:
        # Sort descending by component power / rank
        self.components.sort(key=functools.cmp_to_key(lambda a, b: b.compare_to(a)))

        # Group by comp_rank preserving sort order
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

    def set_remaining_cards(self, input_remaining: List[SimPokerCard]) -> None:
        self.remaining_cards.extend(input_remaining)

    def compare_to(self, other: 'SimPokerHandStructure') -> int:
        for comp1, comp2 in zip(self.components, other.components):
            cmp = comp1.compare_to(comp2)
            if cmp != 0:
                return cmp
        if len(self.components) > len(other.components):
            return 1
        elif len(self.components) < len(other.components):
            return -1
        return 0

    def __lt__(self, other: 'SimPokerHandStructure') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'SimPokerHandStructure') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'SimPokerHandStructure') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'SimPokerHandStructure') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, SimPokerHandStructure):
            return False
        if len(self.components) != len(other.components):
            return False
        for comp1, comp2 in zip(self.components, other.components):
            if comp1 != comp2:
                return False
        return True

    def __ne__(self, other: Any) -> bool:
        return not (self == other)

    def __hash__(self) -> int:
        return hash(self.final_comps_str)

    def __repr__(self) -> str:
        return f"SimPokerHandStructure({self.final_comps_str})"
