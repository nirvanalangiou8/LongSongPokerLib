import itertools
from typing import List, Optional, Iterable, TypeVar, Generic, Any
from GenericPoker.BasePokerCard import BasePokerCard
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.EightCard.EightCardSubBattleHand import EightCardSubBattleHand, BattleHandEnum, EightCardsBattleHandRank
from GenericPoker.EightCard.BattleHandArrangeStrategy import WinRateStrategy
from GenericPoker.HandSplitResult import HandSplitResult
from GenericPoker.PokerEnumAndDicts import EightCardsCompType
from GenericPoker.PokerCardComponent import PokerCardComponent

TPlayer = TypeVar('TPlayer', bound='ConsolePlayer')


class IPlayerFactory(Generic[TPlayer]):
    def create(self, name: str) -> TPlayer:
        raise NotImplementedError


class ConsolePlayer:
    def __init__(self, player_name: str, rule: Optional[ICardRule] = None):
        self.player_name: str = player_name
        self.rule: ICardRule = rule if rule is not None else EightCardRule.default()
        self._pokerCards: List[BasePokerCard] = []

    @property
    def cards(self) -> List[BasePokerCard]:
        return self._pokerCards

    def set_cards(self, input_cards: Iterable[BasePokerCard]) -> None:
        self._pokerCards.extend(input_cards)

    def clear_cards(self) -> None:
        self._pokerCards.clear()

    def process_hands(self) -> Any:
        return None

    def evaluate_best_split_hand_instance(self) -> Optional[HandSplitResult]:
        return self.evaluate_best_split_hand(self._pokerCards, self.rule)

    @classmethod
    def evaluate_best_split_hand(cls, input_cards_or_str: Any, rule: Optional[ICardRule] = None) -> Optional[HandSplitResult]:
        if isinstance(input_cards_or_str, str):
            cards = [
                EightCardPokerCard.create_instance(s.strip())
                for s in input_cards_or_str.split(',') if s.strip()
            ]
        else:
            cards = [
                c if isinstance(c, EightCardPokerCard) else EightCardPokerCard.create_instance(c.card_str)
                for c in input_cards_or_str
            ]

        if len(cards) != 8:
            return None

        best_front_hand: Optional[EightCardSubBattleHand] = None
        best_back_hand: Optional[EightCardSubBattleHand] = None
        best_front: float = 0.0
        best_back: float = 0.0
        best_total: float = float('-inf')

        for front_idx in itertools.combinations(range(len(cards)), 3):
            front_set = set(front_idx)
            front_cards = [cards[i] for i in front_idx]
            back_cards = [cards[i] for i in range(len(cards)) if i not in front_set]

            front_hand = cls.evaluate_best_single_hand(front_cards, BattleHandEnum.FirstHand)
            back_hand = cls.evaluate_best_single_hand(back_cards, BattleHandEnum.SecondHand)

            if back_hand.compare_to(front_hand) < 0:
                continue

            front = WinRateStrategy.get_sub_hand_win_rate(front_hand)
            back = WinRateStrategy.get_sub_hand_win_rate(back_hand)
            total = front + back

            if total > best_total:
                best_total = total
                best_front = front
                best_back = back
                best_front_hand = front_hand
                best_back_hand = back_hand

        if best_front_hand is None or best_back_hand is None:
            return None

        return HandSplitResult(best_front_hand, best_back_hand, best_front, best_back, best_total)

    @classmethod
    def evaluate_best_single_hand(cls, cards: List[EightCardPokerCard], which: BattleHandEnum) -> EightCardSubBattleHand:
        from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
        from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure

        best = cls.build_single_hand(which, EightCardsBattleHandRank.Nothing, [], cards)

        calc = PokerHandCalculator()
        calc.setup_cards(cards)
        calc.min_flush_straight_cards = 3

        structures = calc.test_8_cards()
        for st in structures:
            comps = st.components
            if len(comps) == 0:
                continue

            used_comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]] = []
            if len(comps) >= 2 and (
                (comps[0].comp_rank, comps[1].comp_rank) in PokerHandCalculator.EightCardsCompComboToBattleRankDict
            ):
                rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[(comps[0].comp_rank, comps[1].comp_rank)]
                used_comps.append(comps[0])
                used_comps.append(comps[1])
            elif len(comps) >= 2 and (
                (comps[1].comp_rank, comps[0].comp_rank) in PokerHandCalculator.EightCardsCompComboToBattleRankDict
            ):
                rank = PokerHandCalculator.EightCardsCompComboToBattleRankDict[(comps[1].comp_rank, comps[0].comp_rank)]
                used_comps.append(comps[0])
                used_comps.append(comps[1])
            else:
                rank = PokerHandStructure.convert_comp_rank_to_battle_rank(comps[0].comp_rank)
                used_comps.append(comps[0])

            if (which, rank) not in EightCardSubBattleHand.EightCardsBattleHandPowerDict:
                continue

            used_set = set()
            for comp in used_comps:
                for card in comp.cards:
                    used_set.add(card)

            leftovers = [c for c in cards if c not in used_set]
            cand = cls.build_single_hand(which, rank, used_comps, leftovers)
            if cand.compare_to(best) > 0:
                best = cand

        return best

    @classmethod
    def build_single_hand(cls, which: BattleHandEnum, rank: EightCardsBattleHandRank,
                           comps: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]],
                           kickers: List[EightCardPokerCard]) -> EightCardSubBattleHand:
        hand = EightCardSubBattleHand(which, rank, *comps)
        sorted_kickers = sorted(kickers, key=lambda c: c.poker_card_power, reverse=True)
        hand.add_minor_cards(sorted_kickers)
        return hand
