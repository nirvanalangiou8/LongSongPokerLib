from enum import Enum, IntEnum
from typing import List, Dict, Tuple, Optional, Any
from GenericPoker.PokerEnumAndDicts import EightCardsCompType
from GenericPoker.PokerCardComponent import PokerCardComponent
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard


class BattleHandEnum(IntEnum):
    FirstHand = 3
    SecondHand = 5


class EightCardsBattleHandRank(Enum):
    Nothing = "Nothing"
    Pair = "Pair"
    TwoPairs = "TwoPairs"
    FourCardStraight = "FourCardStraight"
    FourCardsFlush = "FourCardsFlush"
    ThreeCardsPairInFlush = "ThreeCardsPairInFlush"
    ThreeOfKind = "ThreeOfKind"
    TownHouse = "TownHouse"
    FourCardsPairInFlush = "FourCardsPairInFlush"
    FiveCardsStraight = "FiveCardsStraight"
    FullHouse = "FullHouse"
    ThreeCardsFlushStraight = "ThreeCardsFlushStraight"
    FiveCardsFlush = "FiveCardsFlush"
    Mansion = "Mansion"
    FiveCardsPairInFlush = "FiveCardsPairInFlush"
    SixCardsStraight = "SixCardsStraight"
    FourOfKind = "FourOfKind"
    FourCardsFlushStraight = "FourCardsFlushStraight"
    SixCardsFlush = "SixCardsFlush"
    SixCardsPairInFlush = "SixCardsPairInFlush"
    SevenCardsStraight = "SevenCardsStraight"
    FourCardsTwoPairsInFlush = "FourCardsTwoPairsInFlush"
    FiveCardsTwoPairsInFlush = "FiveCardsTwoPairsInFlush"
    SixCardsTwoPairsInFlush = "SixCardsTwoPairsInFlush"
    FiveCardsFlushStraight = "FiveCardsFlushStraight"
    SevenCardsPairInFlush = "SevenCardsPairInFlush"
    EightCardsStraight = "EightCardsStraight"
    FiveOfKind = "FiveOfKind"
    SevenCardsFlush = "SevenCardsFlush"
    SevenCardsTwoPairsInFlush = "SevenCardsTwoPairsInFlush"
    SixCardsFlushStraight = "SixCardsFlushStraight"
    SixCardsThreePairsInFlush = "SixCardsThreePairsInFlush"
    EightCardsPairInFlush = "EightCardsPairInFlush"
    SevenCardsThreePairsInFlush = "SevenCardsThreePairsInFlush"
    EightCardsTwoPairsInFlush = "EightCardsTwoPairsInFlush"
    SixOfKind = "SixOfKind"
    EightCardsFlush = "EightCardsFlush"
    SevenCardsFlushStraight = "SevenCardsFlushStraight"
    EightCardsThreePairsInFlush = "EightCardsThreePairsInFlush"
    SevenOfKind = "SevenOfKind"
    EightCardsFlushStraight = "EightCardsFlushStraight"
    EightCardsFourPairsInFlush = "EightCardsFourPairsInFlush"
    EightOfKind = "EightOfKind"


EightCardsBattleHandPowerDict: Dict[Tuple[BattleHandEnum, EightCardsBattleHandRank], int] = {
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing): 0,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.Pair): 1,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.TwoPairs): 2,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardStraight): 4,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsFlush): 6,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeCardsPairInFlush): 9,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeOfKind): 15,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsPairInFlush): 20,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeCardsFlushStraight): 24,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourOfKind): 32,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsFlushStraight): 40,
    (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsTwoPairsInFlush): 50,

    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Nothing): 0,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Pair): 1,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.TwoPairs): 2,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardStraight): 4,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsFlush): 6,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeCardsPairInFlush): 8,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeOfKind): 10,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.TownHouse): 16,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsPairInFlush): 20,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsStraight): 24,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FullHouse): 28,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeCardsFlushStraight): 32,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsFlush): 40,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Mansion): 48,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsPairInFlush): 56,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsStraight): 62,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourOfKind): 80,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsFlushStraight): 100,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsFlush): 120,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsPairInFlush): 140,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsStraight): 200,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsTwoPairsInFlush): 220,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsTwoPairsInFlush): 240,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsTwoPairsInFlush): 300,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsFlushStraight): 360,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsPairInFlush): 400,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsStraight): 500,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveOfKind): 700,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsFlush): 800,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsTwoPairsInFlush): 900,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsFlushStraight): 1000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsThreePairsInFlush): 1200,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsPairInFlush): 2000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsThreePairsInFlush): 3000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsTwoPairsInFlush): 5000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixOfKind): 10000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFlush): 20000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsFlushStraight): 40000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsThreePairsInFlush): 80000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenOfKind): 200000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFlushStraight): 400000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFourPairsInFlush): 2000000,
    (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightOfKind): 10000000
}


class EightCardSubBattleHand:
    EightCardsBattleHandPowerDict = EightCardsBattleHandPowerDict

    def __init__(self, battle_hand_enum: BattleHandEnum, input_rank: EightCardsBattleHandRank,
                 *input_combos: PokerCardComponent[EightCardsCompType, EightCardPokerCard]):
        self._cards: List[EightCardPokerCard] = []
        self._components: List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]] = []
        for comp in input_combos:
            self._components.append(comp)
            self._cards.extend(comp.cards)
        self._battle_hand_rank: EightCardsBattleHandRank = input_rank
        self._battle_hand_enum: BattleHandEnum = battle_hand_enum
        self._hand_power: int = EightCardsBattleHandPowerDict.get((self._battle_hand_enum, self._battle_hand_rank), 0)
        self._hand_name: str = self._battle_hand_rank.value

    @property
    def cards(self) -> List[EightCardPokerCard]:
        return self._cards

    @property
    def components(self) -> List[PokerCardComponent[EightCardsCompType, EightCardPokerCard]]:
        return self._components

    @property
    def battle_hand_rank(self) -> EightCardsBattleHandRank:
        return self._battle_hand_rank

    @property
    def hand_power(self) -> int:
        return self._hand_power

    @property
    def hand_name(self) -> str:
        return self._hand_name

    def get_hand_string(self, separator: str = "_") -> str:
        return separator.join(card.card_str for card in self._cards)

    def add_minor_cards(self, remaining_cards: List[EightCardPokerCard]) -> List[EightCardPokerCard]:
        ret_cards = list(remaining_cards)
        for card in list(remaining_cards):
            if len(self._cards) >= int(self._battle_hand_enum):
                break
            self._cards.append(card)
            ret_cards.pop(0)
        return ret_cards

    def add_one_minor_card(self, remaining_cards: List[EightCardPokerCard]) -> List[EightCardPokerCard]:
        ret_cards = list(remaining_cards)
        if len(remaining_cards) == 0 or len(self._cards) >= int(self._battle_hand_enum):
            return ret_cards
        self._cards.append(ret_cards[0])
        ret_cards.pop(0)
        return ret_cards

    def compare_to(self, other: Optional['EightCardSubBattleHand']) -> int:
        if other is None:
            return 1

        if self.battle_hand_rank != other.battle_hand_rank:
            my_power = EightCardsBattleHandPowerDict.get((self._battle_hand_enum, self.battle_hand_rank), 0)
            other_power = EightCardsBattleHandPowerDict.get((other._battle_hand_enum, other.battle_hand_rank), 0)
            if my_power != other_power:
                return 1 if my_power > other_power else -1
            members = list(EightCardsBattleHandRank)
            idx1 = members.index(self.battle_hand_rank)
            idx2 = members.index(other.battle_hand_rank)
            return 1 if idx1 > idx2 else (-1 if idx1 < idx2 else 0)

        component_count = min(len(self._components), len(other.components))
        for i in range(component_count):
            cmp = self._components[i].compare_to(other.components[i])
            if cmp != 0:
                return cmp

        if len(self._components) != len(other.components):
            return 1 if len(self._components) > len(other.components) else -1

        my_sorted_cards = sorted(
            self.cards,
            key=lambda c: (14 if c.number == 1 else c.number, int(c.suit)),
            reverse=True
        )
        other_sorted_cards = sorted(
            other.cards,
            key=lambda c: (14 if c.number == 1 else c.number, int(c.suit)),
            reverse=True
        )

        for i in range(min(len(my_sorted_cards), len(other_sorted_cards))):
            my_num = 14 if my_sorted_cards[i].number == 1 else my_sorted_cards[i].number
            other_num = 14 if other_sorted_cards[i].number == 1 else other_sorted_cards[i].number
            if my_num > other_num:
                return 1
            if my_num < other_num:
                return -1

        if len(my_sorted_cards) > len(other_sorted_cards):
            return 1
        elif len(my_sorted_cards) < len(other_sorted_cards):
            return -1
        return 0

    def __lt__(self, other: 'EightCardSubBattleHand') -> bool:
        return self.compare_to(other) < 0

    def __gt__(self, other: 'EightCardSubBattleHand') -> bool:
        return self.compare_to(other) > 0

    def __le__(self, other: 'EightCardSubBattleHand') -> bool:
        return self.compare_to(other) <= 0

    def __ge__(self, other: 'EightCardSubBattleHand') -> bool:
        return self.compare_to(other) >= 0

    def __eq__(self, other: Any) -> bool:
        if not isinstance(other, EightCardSubBattleHand):
            return False
        return self.compare_to(other) == 0

    def __ne__(self, other: Any) -> bool:
        return not (self == other)

    def __repr__(self) -> str:
        return f"{self.battle_hand_rank.value}: {self.get_hand_string()}"
