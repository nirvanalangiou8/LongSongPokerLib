from typing import List, Dict, Optional, Tuple, Any
from GenericPoker.PokerEnumAndDicts import PokerSuit, CompType
from GenericPoker.UtilFunc import UtilFunc
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardsCompType
from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard
from GenericPoker.CardSimStatAnalysis.SimPokerHandStructure import SimPokerHandStructure
from GenericPoker.AcePokerCard import AcePokerCard


class SimStatEstimator:
    MaxPokerNumber = 20

    SimCardsCompTypeDict: Dict[str, SimCardsCompType] = {
        "2_Kind": SimCardsCompType.Pair,
        "3_Kind": SimCardsCompType.ThreeOfKind,
        "4_Kind": SimCardsCompType.FourOfKind,
        "5_Kind": SimCardsCompType.FiveOfKind,
        "6_Kind": SimCardsCompType.SixOfKind,
        "7_Kind": SimCardsCompType.SevenOfKind,
        "8_Kind": SimCardsCompType.EightOfKind,
        "3_FlushStraight": SimCardsCompType.ThreeCardsFlushStraight,
        "4_FlushStraight": SimCardsCompType.FourCardsFlushStraight,
        "5_Flush": SimCardsCompType.FiveCardsFlush,
        "5_Straight": SimCardsCompType.FiveCardsStraight,
        "5_FlushStraight": SimCardsCompType.FiveCardsFlushStraight,
        "6_Flush": SimCardsCompType.SixCardsFlush,
        "6_PairInFlush": SimCardsCompType.SixCardsPairInFlush,
        "6_TwoPairsInFlush": SimCardsCompType.SixCardsTwoPairsInFlush,
        "6_ThreePairsInFlush": SimCardsCompType.SixCardsThreePairsInFlush,
        "7_Flush": SimCardsCompType.SevenCardsFlush,
        "7_PairInFlush": SimCardsCompType.SevenCardsPairInFlush,
        "7_TwoPairsInFlush": SimCardsCompType.SevenCardsTwoPairsInFlush,
        "7_ThreePairsInFlush": SimCardsCompType.SevenCardsThreePairsInFlush,
        "8_Flush": SimCardsCompType.EightCardsFlush,
        "9_Flush": SimCardsCompType.NineCardsFlush,
        "10_Flush": SimCardsCompType.TenCardsFlush,
        "8_PairInFlush": SimCardsCompType.EightCardsPairInFlush,
        "8_TwoPairsInFlush": SimCardsCompType.EightCardsTwoPairsInFlush,
        "8_ThreePairsInFlush": SimCardsCompType.EightCardsThreePairsInFlush,
        "8_FourPairsInFlush": SimCardsCompType.EightCardsFourPairsInFlush,
        "6_Straight": SimCardsCompType.SixCardsStraight,
        "7_Straight": SimCardsCompType.SevenCardsStraight,
        "8_Straight": SimCardsCompType.EightCardsStraight,
        "9_Straight": SimCardsCompType.NineCardsStraight,
        "10_Straight": SimCardsCompType.TenCardsStraight,
        "6_FlushStraight": SimCardsCompType.SixCardsFlushStraight,
        "7_FlushStraight": SimCardsCompType.SevenCardsFlushStraight,
        "8_FlushStraight": SimCardsCompType.EightCardsFlushStraight,
        "9_FlushStraight": SimCardsCompType.NineCardsFlushStraight,
        "10_FlushStraight": SimCardsCompType.TenCardsFlushStraight
    }

    def __init__(self):
        self._allPokerCards: List[SimPokerCard] = []
        self._minFlushStraightCards = 3
        self._minStraightCards = 5
        self._minFlushCards = 5

    def setup_cards(self, input_poker_card_list: List[SimPokerCard]) -> None:
        self._allPokerCards = list(input_poker_card_list)

    def get_hand_string(self) -> str:
        return ",".join(card.card_str for card in self._allPokerCards)

    def _check_comps_card_count(self, all_comps: List[SimPokerHandStructure]) -> bool:
        for structure in all_comps:
            total_cards = 0
            if structure.final_comps_str == "None":
                continue

            comp_parts = structure.final_comps_str.split('_')
            for comp_part in comp_parts:
                parts = comp_part.split('*')
                comp_type_name = parts[0]
                multiplier = int(parts[1]) if len(parts) > 1 else 1

                card_count = 0
                for k, v in self.SimCardsCompTypeDict.items():
                    if v.name == comp_type_name or v.value == comp_type_name:
                        key_parts = k.split('_')
                        card_count = int(key_parts[0])
                        break

                total_cards += card_count * multiplier

            if total_cards > len(self._allPokerCards):
                print(f"Structure final string is {structure.final_comps_str} where the hand string is {self.get_hand_string()}")
                return False

        return True

    def test_sim_cards(self) -> List[SimPokerHandStructure]:
        all_candidate_comps: List[SimPokerHandStructure] = []
        self._recursive_arrange_hands(self._allPokerCards, SimPokerHandStructure(), all_candidate_comps)

        if len(all_candidate_comps) == 0:
            nothing_hand = SimPokerHandStructure()
            nothing_hand.set_remaining_cards(self._allPokerCards)
            all_candidate_comps.append(nothing_hand)

        for res in all_candidate_comps:
            res.sort_comps_and_classify()

        if not self._check_comps_card_count(all_candidate_comps):
            import sys
            sys.exit(1)

        import functools
        all_candidate_comps.sort(key=functools.cmp_to_key(lambda c1, c2: c2.compare_to(c1)))

        # Remove duplicate set of arrangement by their final_comps_str
        seen = set()
        unique_candidates = []
        for c in all_candidate_comps:
            if c.final_comps_str not in seen:
                seen.add(c.final_comps_str)
                unique_candidates.append(c)

        return unique_candidates

    def _get_number_groups(self, min_card_count_in_group: int, none_joker_cards: List[SimPokerCard]) -> List[List[SimPokerCard]]:
        rank_groups_dict: Dict[int, List[SimPokerCard]] = {}
        for card in none_joker_cards:
            if card.number not in rank_groups_dict:
                rank_groups_dict[card.number] = []
            rank_groups_dict[card.number].append(card)

        for group in rank_groups_dict.values():
            import functools
            group.sort(key=functools.cmp_to_key(lambda x, y: y.compare_to(x)))

        result = [
            group for number, group in sorted(rank_groups_dict.items(), key=lambda kv: kv[0], reverse=True)
            if len(group) >= min_card_count_in_group
        ]
        return result

    def _recursive_arrange_hands(self, remaining_cards: List[SimPokerCard],
                                 current_hand_candidates: SimPokerHandStructure,
                                 results: List[SimPokerHandStructure]) -> None:
        b_found_structure = False

        # 1. Flush or flush straight
        accum_hand_structures = SimPokerHandStructure(current_hand_candidates)
        flush_groups = self._evaluate_flush_groups(self._minFlushStraightCards, remaining_cards)
        b_found_structure = self._arrange_flush_or_flush_straight(
            flush_groups, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        # 2. Straight
        accum_hand_structures = SimPokerHandStructure(current_hand_candidates)
        number_group_list = self._get_number_groups(1, remaining_cards)
        all_straight_clusters = self._get_all_straight_cluster(self._minStraightCards, number_group_list)
        b_found_structure = self._arrange_straight_comps(
            all_straight_clusters, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        # 3. Kinds
        accum_hand_structures = SimPokerHandStructure(current_hand_candidates)
        all_kind_groups = self._get_kind_groups(2, remaining_cards)
        b_found_structure = self._arrange_kind_comps(
            all_kind_groups, remaining_cards, accum_hand_structures, results, b_found_structure
        )

        if not b_found_structure and len(accum_hand_structures.components) > 0:
            new_candidate_comps = SimPokerHandStructure(accum_hand_structures)
            new_candidate_comps.set_remaining_cards(remaining_cards)
            results.append(new_candidate_comps)

        if not b_found_structure and len(remaining_cards) == len(self._allPokerCards):
            new_candidate_comps = SimPokerHandStructure(accum_hand_structures)
            new_hand_candidate_data = PokerCardComponent(SimCardsCompType.Nothing, list(remaining_cards))
            new_candidate_comps.add_comp(new_hand_candidate_data)
            results.append(new_candidate_comps)

    def _determine_comp_type(self, num_cards: int, comp_type: CompType) -> SimCardsCompType:
        key_str = f"{num_cards}_{comp_type.value}"
        return self.SimCardsCompTypeDict.get(key_str, SimCardsCompType.None_)

    def _arrange_kind_comps(self, all_kind_groups: List[List[SimPokerCard]], remaining_cards: List[SimPokerCard],
                            accum_hand_structures: SimPokerHandStructure, results: List[SimPokerHandStructure],
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
        else:
            return has_rank

    def _arrange_flush_or_flush_straight(self, flush_groups: List[List[SimPokerCard]], remaining_cards: List[SimPokerCard],
                                         accum_hand_structures: SimPokerHandStructure, results: List[SimPokerHandStructure],
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

    def _arrange_straight_comps(self, all_straight_clusters: List[List[List[SimPokerCard]]], remaining_cards: List[SimPokerCard],
                                accum_hand_structures: SimPokerHandStructure, results: List[SimPokerHandStructure],
                                has_rank: bool) -> bool:
        if len(all_straight_clusters) == 0:
            return has_rank

        accum_all_represented_straight_cards: List[SimPokerCard] = []
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

    def _get_kind_groups(self, min_card_count_in_group: int, none_joker_cards: List[SimPokerCard]) -> List[List[SimPokerCard]]:
        number_groups = self._get_number_groups(min_card_count_in_group, none_joker_cards)
        number_groups.sort(key=lambda x: len(x), reverse=True)
        return number_groups

    def _evaluate_flush_groups(self, min_card_count_in_group: int, all_poker_cards: List[SimPokerCard]) -> List[List[SimPokerCard]]:
        sorted_list = sorted(all_poker_cards, key=lambda item: item.poker_card_power, reverse=True)
        suit_groups: List[List[SimPokerCard]] = []

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
    def _get_all_straight_cluster(straight_count: int, kind_group_list: List[List[SimPokerCard]]) -> List[List[List[SimPokerCard]]]:
        if len(kind_group_list) == 0:
            return []

        # Make copy of kind_group_list to avoid modifying the input
        kind_group_list = [list(g) for g in kind_group_list]

        # Ace-low handling
        if isinstance(kind_group_list[0][0], AcePokerCard):
            new_kind_group = []
            for ace in kind_group_list[0]:
                new_ace = SimPokerCard.create_instance(ace)
                new_ace.set_straight_sub(1)
                new_kind_group.append(new_ace)
            kind_group_list.append(new_kind_group)

        number_clusters: List[List[List[SimPokerCard]]] = []
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
