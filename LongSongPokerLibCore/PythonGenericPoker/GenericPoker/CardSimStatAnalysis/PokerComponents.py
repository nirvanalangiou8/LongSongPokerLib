from enum import Enum
from typing import List, Optional, Set, Any, Iterable
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardsCompType
from GenericPoker.ICardRule import ICardRule


class ComponentCategory(Enum):
    Kind = "Kind"
    FlushStraight = "FlushStraight"
    Flush = "Flush"
    Straight = "Straight"
    Other = "Other"


class PokerComponents:
    def __init__(self, comp_type: SimCardsCompType = SimCardsCompType.Nothing, card_count: Optional[int] = None, rule: Optional[ICardRule] = None):
        self.comp_type: SimCardsCompType = comp_type
        self.card_count: int = card_count if card_count is not None else self.get_default_card_count(comp_type)
        self.rule: Optional[ICardRule] = rule

    @property
    def power(self) -> int:
        return self.get_comp_power(self.comp_type)

    @staticmethod
    def get_comp_power(comp: SimCardsCompType) -> int:
        # In C# enum value
        members = list(SimCardsCompType)
        try:
            return members.index(comp)
        except ValueError:
            return 0

    @staticmethod
    def get_default_card_count(comp: SimCardsCompType) -> int:
        mapping = {
            SimCardsCompType.Pair: 2,
            SimCardsCompType.ThreeOfKind: 3,
            SimCardsCompType.FourOfKind: 4,
            SimCardsCompType.FiveOfKind: 5,
            SimCardsCompType.SixOfKind: 6,
            SimCardsCompType.SevenOfKind: 7,
            SimCardsCompType.EightOfKind: 8,
            SimCardsCompType.NineOfKind: 9,
            SimCardsCompType.TenOfKind: 10,

            SimCardsCompType.ThreeCardsFlush: 3,
            SimCardsCompType.FourCardsFlush: 4,
            SimCardsCompType.FiveCardsFlush: 5,
            SimCardsCompType.SixCardsFlush: 6,
            SimCardsCompType.SevenCardsFlush: 7,
            SimCardsCompType.EightCardsFlush: 8,
            SimCardsCompType.NineCardsFlush: 9,
            SimCardsCompType.TenCardsFlush: 10,

            SimCardsCompType.ThreeCardsStraight: 3,
            SimCardsCompType.FourCardStraight: 4,
            SimCardsCompType.FiveCardsStraight: 5,
            SimCardsCompType.SixCardsStraight: 6,
            SimCardsCompType.SevenCardsStraight: 7,
            SimCardsCompType.EightCardsStraight: 8,
            SimCardsCompType.NineCardsStraight: 9,
            SimCardsCompType.TenCardsStraight: 10,

            SimCardsCompType.ThreeCardsFlushStraight: 3,
            SimCardsCompType.FourCardsFlushStraight: 4,
            SimCardsCompType.FiveCardsFlushStraight: 5,
            SimCardsCompType.SixCardsFlushStraight: 6,
            SimCardsCompType.SevenCardsFlushStraight: 7,
            SimCardsCompType.EightCardsFlushStraight: 8,
            SimCardsCompType.NineCardsFlushStraight: 9,
            SimCardsCompType.TenCardsFlushStraight: 10,
        }
        return mapping.get(comp, 0)

    @staticmethod
    def get_component_category(comp_type: SimCardsCompType) -> ComponentCategory:
        kind_set = {
            SimCardsCompType.Pair, SimCardsCompType.ThreeOfKind, SimCardsCompType.FourOfKind,
            SimCardsCompType.FiveOfKind, SimCardsCompType.SixOfKind, SimCardsCompType.SevenOfKind,
            SimCardsCompType.EightOfKind, SimCardsCompType.NineOfKind, SimCardsCompType.TenOfKind
        }
        if comp_type in kind_set:
            return ComponentCategory.Kind

        flush_straight_set = {
            SimCardsCompType.ThreeCardsFlushStraight, SimCardsCompType.FourCardsFlushStraight,
            SimCardsCompType.FiveCardsFlushStraight, SimCardsCompType.SixCardsFlushStraight,
            SimCardsCompType.SevenCardsFlushStraight, SimCardsCompType.EightCardsFlushStraight,
            SimCardsCompType.NineCardsFlushStraight, SimCardsCompType.TenCardsFlushStraight
        }
        if comp_type in flush_straight_set:
            return ComponentCategory.FlushStraight

        flush_set = {
            SimCardsCompType.ThreeCardsFlush, SimCardsCompType.FourCardsFlush,
            SimCardsCompType.FiveCardsFlush, SimCardsCompType.SixCardsFlush,
            SimCardsCompType.SevenCardsFlush, SimCardsCompType.EightCardsFlush,
            SimCardsCompType.NineCardsFlush, SimCardsCompType.TenCardsFlush
        }
        if comp_type in flush_set:
            return ComponentCategory.Flush

        straight_set = {
            SimCardsCompType.ThreeCardsStraight, SimCardsCompType.FourCardStraight,
            SimCardsCompType.FiveCardsStraight, SimCardsCompType.SixCardsStraight,
            SimCardsCompType.SevenCardsStraight, SimCardsCompType.EightCardsStraight,
            SimCardsCompType.NineCardsStraight, SimCardsCompType.TenCardsStraight
        }
        if comp_type in straight_set:
            return ComponentCategory.Straight

        return ComponentCategory.Other

    @staticmethod
    def get_component_type(category: ComponentCategory, count: int) -> SimCardsCompType:
        if category == ComponentCategory.Kind:
            mapping = {
                2: SimCardsCompType.Pair, 3: SimCardsCompType.ThreeOfKind, 4: SimCardsCompType.FourOfKind,
                5: SimCardsCompType.FiveOfKind, 6: SimCardsCompType.SixOfKind, 7: SimCardsCompType.SevenOfKind,
                8: SimCardsCompType.EightOfKind, 9: SimCardsCompType.NineOfKind, 10: SimCardsCompType.TenOfKind
            }
            return mapping.get(count, SimCardsCompType.Nothing)
        elif category == ComponentCategory.FlushStraight:
            mapping = {
                3: SimCardsCompType.ThreeCardsFlushStraight, 4: SimCardsCompType.FourCardsFlushStraight,
                5: SimCardsCompType.FiveCardsFlushStraight, 6: SimCardsCompType.SixCardsFlushStraight,
                7: SimCardsCompType.SevenCardsFlushStraight, 8: SimCardsCompType.EightCardsFlushStraight,
                9: SimCardsCompType.NineCardsFlushStraight, 10: SimCardsCompType.TenCardsFlushStraight
            }
            return mapping.get(count, SimCardsCompType.Nothing)
        elif category == ComponentCategory.Flush:
            mapping = {
                3: SimCardsCompType.ThreeCardsFlush, 4: SimCardsCompType.FourCardsFlush,
                5: SimCardsCompType.FiveCardsFlush, 6: SimCardsCompType.SixCardsFlush,
                7: SimCardsCompType.SevenCardsFlush, 8: SimCardsCompType.EightCardsFlush,
                9: SimCardsCompType.NineCardsFlush, 10: SimCardsCompType.TenCardsFlush
            }
            return mapping.get(count, SimCardsCompType.Nothing)
        elif category == ComponentCategory.Straight:
            mapping = {
                3: SimCardsCompType.ThreeCardsStraight, 4: SimCardsCompType.FourCardStraight,
                5: SimCardsCompType.FiveCardsStraight, 6: SimCardsCompType.SixCardsStraight,
                7: SimCardsCompType.SevenCardsStraight, 8: SimCardsCompType.EightCardsStraight,
                9: SimCardsCompType.NineCardsStraight, 10: SimCardsCompType.TenCardsStraight
            }
            return mapping.get(count, SimCardsCompType.Nothing)
        return SimCardsCompType.Nothing

    def break_down(self, rule: Optional[ICardRule] = None,
                   min_flush_straight_cards: int = -1,
                   min_flush_cards: int = -1,
                   min_straight_cards: int = -1,
                   min_kind_cards: int = -1) -> List[List['PokerComponents']]:
        from GenericPoker.EightCardRule import EightCardRule

        category = self.get_component_category(self.comp_type)
        total_cards = self.card_count if self.card_count > 0 else self.get_default_card_count(self.comp_type)
        effective_rule = rule if rule is not None else (self.rule if self.rule is not None else EightCardRule.default())

        if category == ComponentCategory.Other or total_cards <= 0:
            return [[PokerComponents(self.comp_type, total_cards, effective_rule)]]

        if category == ComponentCategory.FlushStraight:
            min_cards = min_flush_straight_cards if min_flush_straight_cards > 0 else effective_rule.min_flush_straight_count
        elif category == ComponentCategory.Flush:
            min_cards = min_flush_cards if min_flush_cards > 0 else effective_rule.min_flush_count
        elif category == ComponentCategory.Straight:
            min_cards = min_straight_cards if min_straight_cards > 0 else effective_rule.min_straight_count
        elif category == ComponentCategory.Kind:
            min_cards = min_kind_cards if min_kind_cards > 0 else effective_rule.min_kind_count
        else:
            min_cards = 1

        partitions: List[List[int]] = []
        self._generate_partitions(total_cards, total_cards, [], partitions, min_cards)

        result: List[List[PokerComponents]] = []
        seen: Set[str] = set()

        for partition in partitions:
            valid_parts = [p for p in partition if p >= min_cards]
            if len(valid_parts) == 0:
                continue

            key = ",".join(str(p) for p in valid_parts)
            if key not in seen:
                seen.add(key)
                breakdown_option = [
                    PokerComponents(self.get_component_type(category, p), p, effective_rule)
                    for p in valid_parts
                ]
                result.append(breakdown_option)

        if len(result) == 0:
            result.append([PokerComponents(self.comp_type, total_cards, effective_rule)])

        return result

    @staticmethod
    def _generate_partitions(remaining: int, max_val: int, current: List[int], partitions: List[List[int]], min_cards: int = 1) -> None:
        if remaining < min_cards:
            partitions.append(list(current))
            return

        for i in range(min(remaining, max_val), min_cards - 1, -1):
            current.append(i)
            PokerComponents._generate_partitions(remaining - i, i, current, partitions, min_cards)
            current.pop()

    @staticmethod
    def cartesian_product(sequences: List[List[List['PokerComponents']]]) -> List[List[List['PokerComponents']]]:
        result: List[List[List['PokerComponents']]] = [[]]
        for sequence in sequences:
            temp: List[List[List['PokerComponents']]] = []
            for existing in result:
                for item in sequence:
                    copy = list(existing)
                    copy.append(item)
                    temp.append(copy)
            result = temp
        return result

    def compare_to(self, other: Optional['PokerComponents']) -> int:
        if other is None:
            return 1
        if self.power > other.power:
            return 1
        elif self.power < other.power:
            return -1
        return 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, PokerComponents):
            return False
        return self.comp_type == other.comp_type and self.card_count == other.card_count

    def __repr__(self) -> str:
        return f"{self.comp_type.value}({self.card_count})"
