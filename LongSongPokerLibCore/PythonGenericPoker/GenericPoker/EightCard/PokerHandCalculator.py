from typing import List, Dict, Tuple, Optional, Any
import functools
from GenericPoker.PokerEnumAndDicts import PokerSuit, CompType, BaseCompType, PokerRankTypes
from GenericPoker.UtilFunc import UtilFunc
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.BaseSubBattleHand import BattleHandEnum, EightCardsBattleHandRank
from GenericPoker.EightCard.EightCardHands import EightCardHands
from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure
from GenericPoker.AcePokerCard import AcePokerCard


class PokerHandCalculator:
    MaxPokerNumber = 20

    EightCardsCompComboToBattleRankDict: Dict[Tuple[BaseCompType, BaseCompType], EightCardsBattleHandRank] = {
        (BaseCompType.Pair, BaseCompType.Pair): EightCardsBattleHandRank.TwoPairs,
        (BaseCompType.ThreeCardsPairInFlush, BaseCompType.Pair): EightCardsBattleHandRank.TownHouse,
        (BaseCompType.ThreeOfKind, BaseCompType.Pair): EightCardsBattleHandRank.FullHouse,
        (BaseCompType.ThreeCardsFlushStraight, BaseCompType.Pair): EightCardsBattleHandRank.Mansion,
    }

    BaseCardsCompTypeDict: Dict[str, BaseCompType] = {
        "2_Kind": BaseCompType.Pair,
        "3_Kind": BaseCompType.ThreeOfKind,
        "4_Kind": BaseCompType.FourOfKind,
        "5_Kind": BaseCompType.FiveOfKind,
        "6_Kind": BaseCompType.SixOfKind,
        "7_Kind": BaseCompType.SevenOfKind,
        "8_Kind": BaseCompType.EightOfKind,
        "3_FlushStraight": BaseCompType.ThreeCardsFlushStraight,
        "4_FlushStraight": BaseCompType.FourCardsFlushStraight,
        "5_Flush": BaseCompType.FiveCardsFlush,
        "5_Straight": BaseCompType.FiveCardsStraight,
        "5_FlushStraight": BaseCompType.FiveCardsFlushStraight,
        "6_Flush": BaseCompType.SixCardsFlush,
        "6_PairInFlush": BaseCompType.SixCardsPairInFlush,
        "6_TwoPairsInFlush": BaseCompType.SixCardsTwoPairsInFlush,
        "6_ThreePairsInFlush": BaseCompType.SixCardsThreePairsInFlush,
        "7_Flush": BaseCompType.SevenCardsFlush,
        "7_PairInFlush": BaseCompType.SevenCardsPairInFlush,
        "7_TwoPairsInFlush": BaseCompType.SevenCardsTwoPairsInFlush,
        "7_ThreePairsInFlush": BaseCompType.SevenCardsThreePairsInFlush,
        "8_Flush": BaseCompType.EightCardsFlush,
        "9_Flush": BaseCompType.NineCardsFlush,
        "8_PairInFlush": BaseCompType.EightCardsPairInFlush,
        "8_TwoPairsInFlush": BaseCompType.EightCardsTwoPairsInFlush,
        "8_ThreePairsInFlush": BaseCompType.EightCardsThreePairsInFlush,
        "8_FourPairsInFlush": BaseCompType.EightCardsFourPairsInFlush,
        "6_Straight": BaseCompType.SixCardsStraight,
        "7_Straight": BaseCompType.SevenCardsStraight,
        "8_Straight": BaseCompType.EightCardsStraight,
        "9_Straight": BaseCompType.NineCardsStraight,
        "6_FlushStraight": BaseCompType.SixCardsFlushStraight,
        "7_FlushStraight": BaseCompType.SevenCardsFlushStraight,
        "8_FlushStraight": BaseCompType.EightCardsFlushStraight,
        "9_FlushStraight": BaseCompType.NineCardsFlushStraight
    }

    def __init__(self):
        self._allPokerCards: List[EightCardPokerCard] = []
        self._minFlushStraightCards = 3
        self._minStraightCards = 5
        self._minFlushCards = 5

    @property
    def min_flush_straight_cards(self) -> int:
        return self._minFlushStraightCards

    @min_flush_straight_cards.setter
    def min_flush_straight_cards(self, value: int):
        self._minFlushStraightCards = value

    @classmethod
    def create_instance(cls, input_card_str: str) -> 'PokerHandCalculator':
        cards = [EightCardPokerCard.create_instance(s.strip()) for s in input_card_str.split(',') if s.strip()]
        calculator = cls()
        calculator.setup_cards(cards)
        return calculator

    def setup_cards(self, input_poker_card_list: List[EightCardPokerCard]) -> None:
        self._allPokerCards = list(input_poker_card_list)

    def test_8_cards(self) -> List[PokerHandStructure]:
        all_candidate_comps: List[PokerHandStructure] = []
        self._recursive_arrange_hands(self._allPokerCards, PokerHandStructure(), all_candidate_comps)

        if len(all_candidate_comps) == 0:
            nothing_hand = PokerHandStructure()
            nothing_hand.set_remaining_cards(self._allPokerCards)
            all_candidate_comps.append(nothing_hand)

        for res in all_candidate_comps:
            res.sort_comps_and_classify()

        all_candidate_comps.sort(key=functools.cmp_to_key(lambda c1, c2: c2.compare_to(c1)))

        seen = set()
        unique_candidates = []
        for c in all_candidate_comps:
            if c.final_comps_str not in seen:
                seen.add(c.final_comps_str)
                unique_candidates.append(c)

        return unique_candidates

    def test_8_cards_two_hands_deploy(self, strategy: Any = None) -> Optional[EightCardHands]:
        from GenericPoker.EightCard.BattleHandArrangeStrategy import RuleTableStrategy
        strat = strategy if strategy is not None else RuleTableStrategy()
        structures = self.test_8_cards()
        if len(structures) > 0:
            return structures[0].arrange_hands(strat)
        return None

    def _determine_comp_type(self, num_cards: int, comp_type: CompType) -> BaseCompType:
        key_str = f"{num_cards}_{comp_type.value}"
        return self.BaseCardsCompTypeDict.get(key_str, BaseCompType.None_)

    def _get_number_groups(self, min_card_count_in_group: int, none_joker_cards: List[EightCardPokerCard]) -> List[List[EightCardPokerCard]]:
        rank_groups_dict: Dict[int, List[EightCardPokerCard]] = {}
        for card in none_joker_cards:
            if card.number not in rank_groups_dict:
                rank_groups_dict[card.number] = []
            rank_groups_dict[card.number].append(card)

        for group in rank_groups_dict.values():
            group.sort(key=functools.cmp_to_key(lambda x, y: y.compare_to(x)))

        result = [
            group for number, group in sorted(rank_groups_dict.items(), key=lambda kv: kv[0], reverse=True)
            if len(group) >= min_card_count_in_group
        ]
        return result

    def _recursive_arrange_hands(self, remaining_cards: List[EightCardPokerCard],
                                 current_hand_candidates: PokerHandStructure,
                                 results: List[PokerHandStructure]) -> None:
        b_found_structure = False

        # 1. Flush or flush straight
        accum_hand_structures = PokerHandStructure(current_hand_candidates)
        flush_groups = self._evaluate_flush_groups(self._minFlushStraightCards, remaining_cards)
        b_found_structure = self._arrange_flush_or_flush_straight(
            flush_groups, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        # 2. Straight
        accum_hand_structures = PokerHandStructure(current_hand_candidates)
        number_group_list = self._get_number_groups(1, remaining_cards)
        all_straight_clusters = self._get_all_straight_cluster(self._minStraightCards, number_group_list)
        b_found_structure = self._arrange_straight_comps(
            all_straight_clusters, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        # 3. Kinds
        accum_hand_structures = PokerHandStructure(current_hand_candidates)
        all_kind_groups = self._get_kind_groups(2, remaining_cards)
        b_found_structure = self._arrange_kind_comps(
            all_kind_groups, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        if not b_found_structure and len(accum_hand_structures.components) > 0:
            new_candidate_comps = PokerHandStructure(accum_hand_structures)
            new_candidate_comps.set_remaining_cards(remaining_cards)
            results.append(new_candidate_comps)

    def _arrange_kind_comps(self, all_kind_groups: List[List[EightCardPokerCard]], remaining_cards: List[EightCardPokerCard],
                            accum_hand_structures: PokerHandStructure, results: List[PokerHandStructure],
                            has_rank: bool) -> bool:
        all_qualified_groups = [x for x in all_kind_groups if len(x) >= 2]
        if len(all_qualified_groups) > 0:
            for kind_group in all_qualified_groups:
                hand_type = self._determine_comp_type(len(kind_group), CompType.Kind)
                new_hand_candidate_data = PokerCardComponent(hand_type, list(kind_group))
                accum_hand_structures.add_comp(new_hand_candidate_data)

            all_cards_to_remove = [card for group in all_qualified_groups for card in group]
            new_remain_cards = UtilFunc.get_exclude_list(remaining_cards, all_cards_to_remove)
            self._recursive_arrange_hands(new_remain_cards, accum_hand_structures, results)
            return True
        return has_rank

    def _arrange_flush_or_flush_straight(self, flush_groups: List[List[EightCardPokerCard]], remaining_cards: List[EightCardPokerCard],
                                         accum_hand_structures: PokerHandStructure, results: List[PokerHandStructure],
                                         has_rank: bool) -> bool:
        for flush_group in flush_groups:
            flush_group_for_straight_finding = [[card] for card in flush_group]
            all_straight_clusters = self._get_all_straight_cluster(self._minFlushStraightCards, flush_group_for_straight_finding)

            if len(all_straight_clusters) > 0:
                for straight_cluster in all_straight_clusters:
                    straight = [sub_list[0] for sub_list in straight_cluster]
                    hand_type = self._determine_comp_type(len(straight), CompType.FlushStraight)
                    new_hand_candidate_data = PokerCardComponent(hand_type, list(straight))
                    accum_hand_structures.add_comp(new_hand_candidate_data)
                    new_remain_cards = UtilFunc.get_exclude_list(remaining_cards, straight)

                    self._recursive_arrange_hands(new_remain_cards, accum_hand_structures, results)
                    accum_hand_structures.remove_last()

                if len(all_straight_clusters[0]) != len(flush_group) and len(flush_group) >= self._minFlushCards:
                    hand_type = self._determine_comp_type(len(flush_group), CompType.Flush)
                    new_hand_candidate_data = PokerCardComponent(hand_type, list(flush_group))
                    accum_hand_structures.add_comp(new_hand_candidate_data)
                    new_remain_cards = UtilFunc.get_exclude_list(remaining_cards, flush_group)
                    self._recursive_arrange_hands(new_remain_cards, accum_hand_structures, results)
                    accum_hand_structures.remove_last()

                has_rank = True
            elif len(flush_group) >= self._minFlushCards:
                hand_type = self._determine_comp_type(len(flush_group), CompType.Flush)
                new_hand_candidate_data = PokerCardComponent(hand_type, list(flush_group))
                accum_hand_structures.add_comp(new_hand_candidate_data)
                new_remain_cards = UtilFunc.get_exclude_list(remaining_cards, flush_group)
                self._recursive_arrange_hands(new_remain_cards, accum_hand_structures, results)
                accum_hand_structures.remove_last()
                has_rank = True

        return has_rank

    def _arrange_straight_comps(self, all_straight_clusters: List[List[List[EightCardPokerCard]]], remaining_cards: List[EightCardPokerCard],
                                accum_hand_structures: PokerHandStructure, results: List[PokerHandStructure],
                                has_rank: bool) -> bool:
        if len(all_straight_clusters) == 0:
            return has_rank

        accum_all_represented_straight_cards: List[EightCardPokerCard] = []
        for straight_cluster in all_straight_clusters:
            straight = [sub_list[0] for sub_list in straight_cluster]
            accum_all_represented_straight_cards.extend(straight)
            is_all_same_suit = all(card.suit == straight[0].suit for card in straight)
            comp_type = CompType.FlushStraight if is_all_same_suit else CompType.Straight
            hand_type = self._determine_comp_type(len(straight), comp_type)
            new_hand_candidate_data = PokerCardComponent(hand_type, list(straight))
            accum_hand_structures.add_comp(new_hand_candidate_data)
            new_remain_cards = UtilFunc.get_exclude_list(remaining_cards, accum_all_represented_straight_cards)
            self._recursive_arrange_hands(new_remain_cards, accum_hand_structures, results)
            accum_hand_structures.remove_last()

        return True

    def _get_kind_groups(self, min_card_count_in_group: int, none_joker_cards: List[EightCardPokerCard]) -> List[List[EightCardPokerCard]]:
        number_groups = self._get_number_groups(min_card_count_in_group, none_joker_cards)
        number_groups.sort(key=lambda x: len(x), reverse=True)
        return number_groups

    def _evaluate_flush_groups(self, min_card_count_in_group: int, all_poker_cards: List[EightCardPokerCard]) -> List[List[EightCardPokerCard]]:
        sorted_list = sorted(all_poker_cards, key=lambda item: item.poker_card_power, reverse=True)
        suit_groups: List[List[EightCardPokerCard]] = []

        sorted_enum_values = [
            s for s in sorted([PokerSuit.Club, PokerSuit.Diamond, PokerSuit.Heart, PokerSuit.Spade], key=lambda e: int(e), reverse=True)
        ]

        for poker_suit in sorted_enum_values:
            same_suit_cards = [e for e in sorted_list if e.suit == poker_suit]
            if len(same_suit_cards) > 0:
                suit_groups.append(same_suit_cards)

        sorted_sub_lists = sorted(suit_groups, key=lambda sublist: len(sublist), reverse=True)
        return [sublist for sublist in sorted_sub_lists if len(sublist) >= min_card_count_in_group]

    @staticmethod
    def _get_all_straight_cluster(straight_count: int, kind_group_list: List[List[EightCardPokerCard]]) -> List[List[List[EightCardPokerCard]]]:
        if len(kind_group_list) == 0:
            return []

        kind_group_list = [list(g) for g in kind_group_list]

        if isinstance(kind_group_list[0][0], AcePokerCard):
            new_kind_group = []
            for ace in kind_group_list[0]:
                new_ace = EightCardPokerCard.create_instance(ace)
                new_ace.set_straight_sub(1)
                new_kind_group.append(new_ace)
            kind_group_list.append(new_kind_group)

        number_clusters: List[List[List[EightCardPokerCard]]] = []
        for num_group in kind_group_list:
            if len(number_clusters) == 0 or (number_clusters[-1][-1][0].number - num_group[0].number != 1):
                number_clusters.append([num_group])
            else:
                number_clusters[-1].append(num_group)

        qualified_clusters = [
            cluster for cluster in number_clusters if len(cluster) >= straight_count
        ]
        qualified_clusters.sort(key=lambda x: len(x), reverse=True)
        return qualified_clusters
