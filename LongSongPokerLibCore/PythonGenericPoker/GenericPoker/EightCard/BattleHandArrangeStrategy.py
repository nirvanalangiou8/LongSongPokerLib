from abc import ABC, abstractmethod
from typing import List, Tuple, Dict, Optional, Any
from GenericPoker.UtilFunc import UtilFunc
from GenericPoker.PokerEnumAndDicts import EightCardsCompType
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.EightCard.EightCardSubBattleHand import EightCardSubBattleHand, BattleHandEnum, EightCardsBattleHandRank
from GenericPoker.EightCard.PokerMath import PokerMath, SpaceDef, SpaceType


class IBattleHandArrangeStrategy(ABC):
    def calc_hand_win_rate(self, first_battle_hand: EightCardSubBattleHand, second_battle_hand: EightCardSubBattleHand) -> float:
        return 0.5

    @abstractmethod
    def arrange_comps(self, comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]]) -> Tuple[EightCardSubBattleHand, EightCardSubBattleHand]:
        pass


class BalancedStrategy(IBattleHandArrangeStrategy):
    def arrange_comps(self, comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]]) -> Tuple[EightCardSubBattleHand, EightCardSubBattleHand]:
        from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
        from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure

        first_hand = None
        second_hand = None

        if len(comps) == 3:
            key = (comps[1].comp_rank, comps[2].comp_rank)
            if key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                new_battle_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[key]
                first_hand = EightCardSubBattleHand(
                    BattleHandEnum.FirstHand,
                    PokerHandStructure.convert_comp_rank_to_battle_rank(comps[0].comp_rank),
                    comps[0]
                )
                second_hand = EightCardSubBattleHand(
                    BattleHandEnum.SecondHand,
                    new_battle_rank,
                    comps[1], comps[2]
                )
                if first_hand > second_hand:
                    first_hand, second_hand = second_hand, first_hand
            else:
                print("Fatal error in Battle Hand arrange of strategy (3 comps).")
        elif len(comps) == 2:
            first_hand = EightCardSubBattleHand(
                BattleHandEnum.FirstHand,
                PokerHandStructure.convert_comp_rank_to_battle_rank(comps[1].comp_rank),
                comps[1]
            )
            second_hand = EightCardSubBattleHand(
                BattleHandEnum.SecondHand,
                PokerHandStructure.convert_comp_rank_to_battle_rank(comps[0].comp_rank),
                comps[0]
            )

        return first_hand, second_hand


class RuleTableStrategy(IBattleHandArrangeStrategy):
    def arrange_comps(self, comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]]) -> Tuple[EightCardSubBattleHand, EightCardSubBattleHand]:
        from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
        from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure

        two_comp_permutations = UtilFunc.get_permutation(comps, 2)
        for selected_comps in two_comp_permutations:
            remaining_comps = UtilFunc.get_exclude_list(comps, selected_comps)
            if len(remaining_comps) > 2:
                continue

            key = (selected_comps[0].comp_rank, selected_comps[1].comp_rank)
            if key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                first_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[key]
                first_hand = EightCardSubBattleHand(BattleHandEnum.FirstHand, first_rank, selected_comps[0], selected_comps[1])

                if len(remaining_comps) == 2:
                    rem_key = (remaining_comps[0].comp_rank, remaining_comps[1].comp_rank)
                    if rem_key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                        second_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[rem_key]
                        second_hand = EightCardSubBattleHand(BattleHandEnum.SecondHand, second_rank, remaining_comps[0], remaining_comps[1])
                        return self._finalize_hands(first_hand, second_hand)
                elif len(remaining_comps) == 1:
                    second_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(remaining_comps[0].comp_rank)
                    second_hand = EightCardSubBattleHand(BattleHandEnum.SecondHand, second_rank, remaining_comps[0])
                    return self._finalize_hands(first_hand, second_hand)

        one_comp_permutations = UtilFunc.get_permutation(comps, 1)
        for selected_comps in one_comp_permutations:
            selected_comp = selected_comps[0]
            remaining_comps = UtilFunc.get_exclude_list(comps, selected_comps)
            if len(remaining_comps) > 2:
                continue

            first_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(selected_comp.comp_rank)
            first_hand = EightCardSubBattleHand(BattleHandEnum.FirstHand, first_rank, selected_comp)

            if len(remaining_comps) == 2:
                rem_key = (remaining_comps[0].comp_rank, remaining_comps[1].comp_rank)
                if rem_key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                    second_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[rem_key]
                    second_hand = EightCardSubBattleHand(BattleHandEnum.SecondHand, second_rank, remaining_comps[0], remaining_comps[1])
                    return self._finalize_hands(first_hand, second_hand)
            elif len(remaining_comps) == 1:
                second_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(remaining_comps[0].comp_rank)
                second_hand = EightCardSubBattleHand(BattleHandEnum.SecondHand, second_rank, remaining_comps[0])
                return self._finalize_hands(first_hand, second_hand)

        return None, None

    def _finalize_hands(self, first: EightCardSubBattleHand, second: EightCardSubBattleHand) -> Tuple[EightCardSubBattleHand, EightCardSubBattleHand]:
        if first > second:
            return second, first
        return first, second


class WinRateStrategy(IBattleHandArrangeStrategy):
    StatProbLadder = [
        (EightCardsBattleHandRank.Nothing,                0.171982),
        (EightCardsBattleHandRank.Pair,                   0.138407),
        (EightCardsBattleHandRank.TwoPairs,               0.256321),
        (EightCardsBattleHandRank.ThreeCardsPairInFlush,  0.034262),
        (EightCardsBattleHandRank.ThreeOfKind,            0.025718),
        (EightCardsBattleHandRank.TownHouse,              0.001622),
        (EightCardsBattleHandRank.FiveCardsStraight,      0.106110),
        (EightCardsBattleHandRank.FullHouse,              0.038412),
        (EightCardsBattleHandRank.ThreeCardsFlushStraight,0.024445),
        (EightCardsBattleHandRank.FiveCardsFlush,         0.051904),
        (EightCardsBattleHandRank.Mansion,                0.034262),
        (EightCardsBattleHandRank.SixCardsStraight,       0.028945),
        (EightCardsBattleHandRank.FourOfKind,             0.001126),
        (EightCardsBattleHandRank.FourCardsFlushStraight, 0.003965),
        (EightCardsBattleHandRank.SixCardsFlush,          0.005314),
        (EightCardsBattleHandRank.SevenCardsStraight,     0.004747),
        (EightCardsBattleHandRank.FiveCardsFlushStraight, 0.000421),
        (EightCardsBattleHandRank.EightCardsStraight,     0.000367),
        (EightCardsBattleHandRank.SevenCardsFlush,        0.000247),
        (EightCardsBattleHandRank.SixCardsFlushStraight,  0.000029),
        (EightCardsBattleHandRank.EightCardsFlush,        0.000004),
    ]

    _CdfBandDict: Optional[Dict[EightCardsBattleHandRank, Tuple[float, float]]] = None

    @classmethod
    def _build_cdf_bands(cls) -> Dict[EightCardsBattleHandRank, Tuple[float, float]]:
        if cls._CdfBandDict is not None:
            return cls._CdfBandDict
        total = sum(prob for _, prob in cls.StatProbLadder)
        if total <= 0:
            total = 1.0

        bands: Dict[EightCardsBattleHandRank, Tuple[float, float]] = {}
        cum = 0.0
        for rank, prob in cls.StatProbLadder:
            min_val = cum / total
            cum += prob
            max_val = cum / total
            if rank in bands:
                old_min, old_max = bands[rank]
                bands[rank] = (min(old_min, min_val), max(old_max, max_val))
            else:
                bands[rank] = (min_val, max_val)
        cls._CdfBandDict = bands
        return bands

    @classmethod
    def get_band(cls, rank: EightCardsBattleHandRank) -> Tuple[float, float]:
        bands = cls._build_cdf_bands()
        if rank in bands:
            return bands[rank]
        return (0.0, bands[EightCardsBattleHandRank.Nothing][1] if EightCardsBattleHandRank.Nothing in bands else 0.1)

    @staticmethod
    def _safe_unified_win_rate(offsets: List[int], schema: List[SpaceDef], min_val: float, max_val: float) -> float:
        offsets_copy = list(offsets)
        ptr = 0
        for space in schema:
            upper = space.pool_size - 1
            for d in range(space.dimensions):
                if ptr < len(offsets_copy):
                    if offsets_copy[ptr] < 0:
                        offsets_copy[ptr] = 0
                    if offsets_copy[ptr] > upper:
                        offsets_copy[ptr] = upper
                    ptr += 1
        return PokerMath.get_unified_win_rate(offsets_copy, schema, min_val, max_val)

    @staticmethod
    def _rep_rank(comp: PokerCardComponent[EightCardsCompType, EightCardPokerCard]) -> int:
        best = 2
        for card in comp.cards:
            if card.number > best:
                best = card.number
        return best

    @classmethod
    def get_sub_hand_win_rate(cls, hand: Optional[EightCardSubBattleHand]) -> float:
        if hand is None:
            return 0.0

        min_val, max_val = cls.get_band(hand.battle_hand_rank)
        comps = hand.components

        if hand.battle_hand_rank == EightCardsBattleHandRank.Nothing:
            ranks = sorted([c.number for c in hand.cards], reverse=True)[:3]
            if len(ranks) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Combination, 13, len(ranks))]
            offsets = [r - 2 for r in ranks]
            return cls._safe_unified_win_rate(offsets, schema, min_val, max_val)

        elif hand.battle_hand_rank == EightCardsBattleHandRank.Pair:
            if len(comps) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Cartesian, 13, 1)]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 2], schema, min_val, max_val)

        elif hand.battle_hand_rank == EightCardsBattleHandRank.TwoPairs:
            if len(comps) < 2:
                return min_val
            schema = [
                SpaceDef(SpaceType.Cartesian, 13, 1),
                SpaceDef(SpaceType.Cartesian, 13, 1)
            ]
            hi = max(cls._rep_rank(comps[0]), cls._rep_rank(comps[1]))
            lo = min(cls._rep_rank(comps[0]), cls._rep_rank(comps[1]))
            return cls._safe_unified_win_rate([hi - 2, lo - 2], schema, min_val, max_val)

        elif hand.battle_hand_rank == EightCardsBattleHandRank.ThreeOfKind:
            if len(comps) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Cartesian, 13, 1)]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 2], schema, min_val, max_val)

        elif hand.battle_hand_rank in (EightCardsBattleHandRank.FullHouse, EightCardsBattleHandRank.TownHouse):
            if len(comps) < 2:
                return min_val
            schema = [
                SpaceDef(SpaceType.Cartesian, 13, 1),
                SpaceDef(SpaceType.Cartesian, 13, 1)
            ]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 2, cls._rep_rank(comps[1]) - 2], schema, min_val, max_val)

        elif hand.battle_hand_rank in (EightCardsBattleHandRank.ThreeCardsFlushStraight,
                                       EightCardsBattleHandRank.FourCardsFlushStraight,
                                       EightCardsBattleHandRank.FiveCardsFlushStraight):
            if len(comps) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Cartesian, 11, 1)]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 4], schema, min_val, max_val)

        elif hand.battle_hand_rank == EightCardsBattleHandRank.Mansion:
            if len(comps) < 2:
                return min_val
            schema = [
                SpaceDef(SpaceType.Cartesian, 11, 1),
                SpaceDef(SpaceType.Cartesian, 13, 1)
            ]
            straight_high = cls._rep_rank(comps[0])
            pair_rank = cls._rep_rank(comps[1])
            return cls._safe_unified_win_rate([straight_high - 4, pair_rank - 2], schema, min_val, max_val)

        elif hand.battle_hand_rank in (EightCardsBattleHandRank.FiveCardsStraight,
                                       EightCardsBattleHandRank.SixCardsStraight,
                                       EightCardsBattleHandRank.SevenCardsStraight,
                                       EightCardsBattleHandRank.EightCardsStraight):
            if len(comps) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Cartesian, 11, 1)]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 4], schema, min_val, max_val)

        elif hand.battle_hand_rank in (EightCardsBattleHandRank.FiveCardsFlush,
                                       EightCardsBattleHandRank.SixCardsFlush,
                                       EightCardsBattleHandRank.SevenCardsFlush,
                                       EightCardsBattleHandRank.EightCardsFlush,
                                       EightCardsBattleHandRank.FourOfKind):
            if len(comps) == 0:
                return min_val
            schema = [SpaceDef(SpaceType.Cartesian, 13, 1)]
            return cls._safe_unified_win_rate([cls._rep_rank(comps[0]) - 2], schema, min_val, max_val)

        else:
            return (min_val + max_val) * 0.5

    def calc_hand_win_rate(self, first_battle_hand: EightCardSubBattleHand, second_battle_hand: EightCardSubBattleHand) -> float:
        return float(self.get_sub_hand_win_rate(first_battle_hand) + self.get_sub_hand_win_rate(second_battle_hand))

    def arrange_comps(self, comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]]) -> Tuple[EightCardSubBattleHand, EightCardSubBattleHand]:
        from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
        from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure

        best_first = None
        best_second = None
        best_score = float('-inf')

        def consider(a: EightCardSubBattleHand, b: EightCardSubBattleHand):
            nonlocal best_first, best_second, best_score
            if a is None or b is None:
                return
            if a > b:
                a, b = b, a
            score = self.get_sub_hand_win_rate(a) + self.get_sub_hand_win_rate(b)
            if score > best_score:
                best_score = score
                best_first = a
                best_second = b

        two_comp_permutations = UtilFunc.get_permutation(comps, 2)
        for selected_comps in two_comp_permutations:
            remaining_comps = UtilFunc.get_exclude_list(comps, selected_comps)
            if len(remaining_comps) > 2:
                continue

            key = (selected_comps[0].comp_rank, selected_comps[1].comp_rank)
            if key not in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                continue

            paired_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[key]
            hand_a = EightCardSubBattleHand(BattleHandEnum.SecondHand, paired_rank, selected_comps[0], selected_comps[1])

            if len(remaining_comps) == 2:
                rem_key = (remaining_comps[0].comp_rank, remaining_comps[1].comp_rank)
                if rem_key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                    other_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[rem_key]
                    hand_b = EightCardSubBattleHand(BattleHandEnum.SecondHand, other_rank, remaining_comps[0], remaining_comps[1])
                    consider(hand_a, hand_b)
            elif len(remaining_comps) == 1:
                other_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(remaining_comps[0].comp_rank)
                hand_b = EightCardSubBattleHand(BattleHandEnum.SecondHand, other_rank, remaining_comps[0])
                consider(hand_a, hand_b)
            elif len(remaining_comps) == 0:
                hand_b = EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing)
                consider(hand_a, hand_b)

        one_comp_permutations = UtilFunc.get_permutation(comps, 1)
        for selected_comps in one_comp_permutations:
            selected_comp = selected_comps[0]
            remaining_comps = UtilFunc.get_exclude_list(comps, selected_comps)
            if len(remaining_comps) > 2:
                continue

            a_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(selected_comp.comp_rank)
            hand_a = EightCardSubBattleHand(BattleHandEnum.SecondHand, a_rank, selected_comp)

            if len(remaining_comps) == 2:
                rem_key = (remaining_comps[0].comp_rank, remaining_comps[1].comp_rank)
                if rem_key in PokerHandCalculator.EightCardsCompComboToBattleRankDict:
                    other_rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[rem_key]
                    hand_b = EightCardSubBattleHand(BattleHandEnum.SecondHand, other_rank, remaining_comps[0], remaining_comps[1])
                    consider(hand_a, hand_b)
            elif len(remaining_comps) == 1:
                other_rank = PokerHandStructure.convert_comp_rank_to_battle_rank(remaining_comps[0].comp_rank)
                hand_b = EightCardSubBattleHand(BattleHandEnum.SecondHand, other_rank, remaining_comps[0])
                consider(hand_a, hand_b)
            elif len(remaining_comps) == 0:
                hand_b = EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing)
                consider(hand_a, hand_b)

        if best_first is not None and best_second is not None:
            return best_first, best_second

        return RuleTableStrategy().arrange_comps(comps)
