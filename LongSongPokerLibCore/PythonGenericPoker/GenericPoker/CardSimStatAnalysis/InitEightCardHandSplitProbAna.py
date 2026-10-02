import os
import math
from typing import Dict, List, Tuple, Optional, Any
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.NineCardRule import NineCardRule
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardOverAllHandRank, SimCardsCompType
from GenericPoker.CardSimStatAnalysis.PokerComponents import PokerComponents
from GenericPoker.UtilFunc import UtilFunc


class InitEightCardHandSplitProbAna:
    @classmethod
    def run(cls, input_path: Optional[str] = None, output_path: Optional[str] = None, rule: Optional[ICardRule] = None) -> Tuple[Dict[SimCardOverAllHandRank, float], Dict[SimCardOverAllHandRank, float]]:
        return cls.analyze(input_path, output_path, rule)

    @staticmethod
    def resolve_input_path(input_path: Optional[str]) -> str:
        if input_path and os.path.exists(input_path):
            return os.path.abspath(input_path)

        curr = os.path.abspath(os.getcwd())
        while curr:
            if input_path:
                cand = os.path.join(curr, input_path)
                if os.path.exists(cand):
                    return cand
                cand = os.path.join(curr, "GenericPoker", "CardSimStatAnalysis", input_path)
                if os.path.exists(cand):
                    return cand
                cand = os.path.join(curr, "GenericPoker", "CardSimStatAnalysis", "Data", os.path.basename(input_path))
                if os.path.exists(cand):
                    return cand
            parent = os.path.dirname(curr)
            if parent == curr:
                break
            curr = parent
        return input_path or ""

    @staticmethod
    def resolve_output_path(output_path: Optional[str], input_path: Optional[str] = None) -> str:
        if output_path is not None and output_path.strip():
            if os.path.isabs(output_path):
                return output_path
            resolved_in = InitEightCardHandSplitProbAna.resolve_input_path(input_path)
            in_dir = os.path.dirname(resolved_in)
            if in_dir:
                return os.path.join(in_dir, output_path)
            return os.path.abspath(output_path)
        return "front_back_stats.csv"

    @classmethod
    def analyze(cls, input_path: Optional[str] = None, output_path: Optional[str] = None, rule: Optional[ICardRule] = None) -> Tuple[Dict[SimCardOverAllHandRank, float], Dict[SimCardOverAllHandRank, float]]:
        resolved_input_path = cls.resolve_input_path(input_path)
        resolved_output_path = cls.resolve_output_path(output_path, resolved_input_path)

        effective_rule = rule or (NineCardRule.default() if ("9cards" in resolved_input_path or "9_cards" in resolved_input_path or "9card" in resolved_input_path) else EightCardRule.default())

        if not os.path.exists(resolved_input_path):
            print(f"Input file not found: {resolved_input_path}")
            return ({}, {})

        front_hand_stats: Dict[SimCardOverAllHandRank, float] = {}
        back_hand_stats: Dict[SimCardOverAllHandRank, float] = {}
        total_input_count = 0

        header_notes: List[str] = []
        with open(resolved_input_path, "r", encoding="utf-8") as f:
            lines = f.readlines()

        for line in lines:
            line_str = line.strip()
            if not line_str:
                continue

            if line_str.startswith("#"):
                header_notes.append(line_str)
                continue

            if line_str.startswith("Hand Type"):
                continue

            parts = line_str.split(",")
            if len(parts) < 2:
                continue

            hand_name = parts[0].strip()
            try:
                count = int(parts[1].strip())
            except ValueError:
                continue

            total_input_count += count

            if hand_name == "Nothing":
                back_hand_stats[SimCardOverAllHandRank.Nothing] = back_hand_stats.get(SimCardOverAllHandRank.Nothing, 0.0) + count
                front_hand_stats[SimCardOverAllHandRank.Nothing] = front_hand_stats.get(SimCardOverAllHandRank.Nothing, 0.0) + count
                continue

            components = cls.parse_hand_name(hand_name, effective_rule)
            solutions = cls.split_hand(components, effective_rule)

            if len(solutions) > 0:
                per_solution_count = float(count) / len(solutions)
                for sol in solutions:
                    front_hand_stats[sol[0]] = front_hand_stats.get(sol[0], 0.0) + per_solution_count
                    back_hand_stats[sol[1]] = back_hand_stats.get(sol[1], 0.0) + per_solution_count
            else:
                front_hand_stats[SimCardOverAllHandRank.None_] = front_hand_stats.get(SimCardOverAllHandRank.None_, 0.0) + count
                back_hand_stats[SimCardOverAllHandRank.None_] = back_hand_stats.get(SimCardOverAllHandRank.None_, 0.0) + count

        if resolved_output_path:
            cls.save_stats(resolved_output_path, front_hand_stats, back_hand_stats, resolved_input_path, header_notes)
            print(f"Analysis completed. Results saved to {resolved_output_path}")

        total_front = sum(front_hand_stats.values())
        total_back = sum(back_hand_stats.values())
        print(f"Total Input Appearance Count: {total_input_count}")
        print(f"Total Front Stat Count: {total_front:.2f}")
        print(f"Total Back Stat Count: {total_back:.2f}")

        return front_hand_stats, back_hand_stats

    @classmethod
    def parse_hand_name(cls, hand_name: str, rule: Optional[ICardRule] = None) -> List[PokerComponents]:
        comps: List[PokerComponents] = []
        parts = hand_name.split("_")
        for part in parts:
            type_str = part
            count = 1
            if "*" in part:
                sub_parts = part.split("*")
                type_str = sub_parts[0]
                count = int(sub_parts[1])

            comp_type = None
            for ct in SimCardsCompType:
                if ct.name == type_str or ct.value == type_str:
                    comp_type = ct
                    break

            if comp_type is not None:
                for _ in range(count):
                    comps.append(PokerComponents(comp_type, rule=rule))

        comps.sort(key=lambda c: c.power, reverse=True)
        return comps

    @classmethod
    def split_hand(cls, comps: List[PokerComponents], rule: Optional[ICardRule] = None,
                   min_flush_straight_cards: int = -1, min_flush_cards: int = -1,
                   min_straight_cards: int = -1, min_kind_cards: int = -1) -> List[Tuple[SimCardOverAllHandRank, SimCardOverAllHandRank]]:
        if not comps:
            return []

        effective_rule = rule or next((c.rule for c in comps if c.rule is not None), EightCardRule.default())

        breakdown_sequences = [
            c.break_down(effective_rule, min_flush_straight_cards, min_flush_cards, min_straight_cards, min_kind_cards)
            for c in comps
        ]

        candidate_combinations = PokerComponents.cartesian_product(breakdown_sequences)
        solutions: List[Tuple[SimCardOverAllHandRank, SimCardOverAllHandRank]] = []

        rank_members = list(SimCardOverAllHandRank)

        for combination in candidate_combinations:
            atomic_comps = [c for group in combination for c in group]
            atomic_comps.sort(key=lambda a: a.power, reverse=True)

            max_front_count = len(atomic_comps) // 2
            for select_count in range(max_front_count + 1):
                possible_groups = UtilFunc.get_permutation_allowed_duplicated(atomic_comps, select_count)
                for group in possible_groups:
                    front_group = group[0]
                    back_group = group[1]

                    front_rank = effective_rule.assemble_hand_rank(front_group)
                    back_rank = effective_rule.assemble_hand_rank(back_group)

                    if front_rank == SimCardOverAllHandRank.None_ or back_rank == SimCardOverAllHandRank.None_:
                        continue

                    f_idx = rank_members.index(front_rank)
                    b_idx = rank_members.index(back_rank)

                    if f_idx > b_idx:
                        swapped_front_rank = back_rank
                        swapped_back_rank = front_rank
                        if rank_members.index(swapped_back_rank) >= rank_members.index(swapped_front_rank):
                            solutions.append((swapped_front_rank, swapped_back_rank))
                    else:
                        solutions.append((front_rank, back_rank))

        # Distinct
        unique_solutions = []
        seen = set()
        for sol in solutions:
            if sol not in seen:
                seen.add(sol)
                unique_solutions.append(sol)

        return cls.filter_dominated_solutions(unique_solutions)

    @classmethod
    def filter_dominated_solutions(cls, solutions: List[Tuple[SimCardOverAllHandRank, SimCardOverAllHandRank]]) -> List[Tuple[SimCardOverAllHandRank, SimCardOverAllHandRank]]:
        if not solutions:
            return []

        rank_members = list(SimCardOverAllHandRank)
        unique_list = list(dict.fromkeys(solutions))
        filtered = []

        for i, s1 in enumerate(unique_list):
            is_dominated = False
            s1_f = rank_members.index(s1[0])
            s1_b = rank_members.index(s1[1])

            for j, s2 in enumerate(unique_list):
                if i == j:
                    continue
                s2_f = rank_members.index(s2[0])
                s2_b = rank_members.index(s2[1])

                if s2_f >= s1_f and s2_b >= s1_b and (s2_f > s1_f or s2_b > s1_b):
                    is_dominated = True
                    break

            if not is_dominated:
                filtered.append(s1)

        return filtered

    @staticmethod
    def _round_away_from_zero(val: float) -> int:
        if val >= 0:
            return math.floor(val + 0.5)
        else:
            return math.ceil(val - 0.5)

    @classmethod
    def save_stats(cls, path: str, front: Dict[SimCardOverAllHandRank, float], back: Dict[SimCardOverAllHandRank, float],
                   input_path: Optional[str] = None, header_notes: Optional[List[str]] = None) -> None:
        dir_name = os.path.dirname(path)
        if dir_name and not os.path.exists(dir_name):
            os.makedirs(dir_name, exist_ok=True)

        rank_members = list(SimCardOverAllHandRank)

        sorted_front = sorted(
            front.items(),
            key=lambda e: rank_members.index(e[0]),
            reverse=True
        )
        total_front_count = sum(cls._round_away_from_zero(v) for _, v in sorted_front)

        sorted_back = sorted(
            back.items(),
            key=lambda e: rank_members.index(e[0]),
            reverse=True
        )
        total_back_count = sum(cls._round_away_from_zero(v) for _, v in sorted_back)

        with open(path, "w", encoding="utf-8", newline="\n") as writer:
            if header_notes:
                for note in header_notes:
                    writer.write(f"{note}\n")
            if input_path:
                writer.write(f"# Source: {input_path}\n")

            writer.write("Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities\n")

            # Front Hand
            cumulative_front_count = 0
            front_lines = []
            for rank, val in reversed(sorted_front):
                count = cls._round_away_from_zero(val)
                if count == 0 and val == 0:
                    continue
                cumulative_front_count += count
                prob = (count / total_front_count) if total_front_count > 0 else 0.0
                win_no_lose_prob = (cumulative_front_count / total_front_count) if total_front_count > 0 else 0.0
                rank_str = rank.value if rank != SimCardOverAllHandRank.None_ else "None"
                front_lines.append(f"Front,{rank_str},{count},{cumulative_front_count},{prob:.16%},{win_no_lose_prob:.16%}\n")

            for line in reversed(front_lines):
                writer.write(line)

            # Back Hand
            cumulative_back_count = 0
            back_lines = []
            for rank, val in reversed(sorted_back):
                count = cls._round_away_from_zero(val)
                if count == 0 and val == 0:
                    continue
                cumulative_back_count += count
                prob = (count / total_back_count) if total_back_count > 0 else 0.0
                win_no_lose_prob = (cumulative_back_count / total_back_count) if total_back_count > 0 else 0.0
                rank_str = rank.value if rank != SimCardOverAllHandRank.None_ else "None"
                back_lines.append(f"Back,{rank_str},{count},{cumulative_back_count},{prob:.16%},{win_no_lose_prob:.16%}\n")

            for line in reversed(back_lines):
                writer.write(line)
