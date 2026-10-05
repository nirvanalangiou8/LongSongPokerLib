from enum import Enum, IntEnum
from typing import Dict


class PokerSuit(IntEnum):
    NoSuit = 0
    Club = 1
    Diamond = 2
    Heart = 4
    Spade = 8
    Star = 15
    Wild = 31


class CompType(Enum):
    Kind = "Kind"
    Flush = "Flush"
    Straight = "Straight"
    FlushStraight = "FlushStraight"
    FullHouse = "FullHouse"


class EightCardsPokerRank(Enum):
    OnePair = "OnePair"
    TwoPairs = "TwoPairs"
    ThreeCardsFlush = "ThreeCardsFlush"
    ThreeCardsStraight = "ThreeCardsStraight"
    ThreeCardsFlushStraight = "ThreeCardsFlushStraight"
    ThreeOfKind = "ThreeOfKind"
    PairThreeCardsFlush = "PairThreeCardsFlush"
    PairThreeCardsStraight = "PairThreeCardsStraight"
    PairThreeCardsFlushStraight = "PairThreeCardsFlushStraight"
    FourCardsFlush = "FourCardsFlush"
    FourCardsStraight = "FourCardsStraight"
    FourCardsFlushStraight = "FourCardsFlushStraight"
    FiveCardsStraight = "FiveCardsStraight"
    FiveCardsFlush = "FiveCardsFlush"
    FullHouse = "FullHouse"
    FiveOfKind = "FiveOfKind"
    SixCardsStraight = "SixCardsStraight"
    SixCardsFlush = "SixCardsFlush"
    SixCardsFlushStraight = "SixCardsFlushStraight"
    SevenCardsStraight = "SevenCardsStraight"
    SevenCardsFlush = "SevenCardsFlush"
    SevenCardsFlushStraight = "SevenCardsFlushStraight"
    EightCardsStraight = "EightCardsStraight"
    EightCardsFlush = "EightCardsFlush"
    EightCardsFlushStraight = "EightCardsFlushStraight"


class BaseCompType(Enum):
    Nothing = "Nothing"
    Pair = "Pair"
    ThreeCardsFlush = "ThreeCardsFlush"
    ThreeCardsPairInFlush = "ThreeCardsPairInFlush"
    ThreeCardsStraight = "ThreeCardsStraight"
    FourCardsFlush = "FourCardsFlush"
    FourCardStraight = "FourCardStraight"
    FiveCardsStraight = "FiveCardsStraight"
    FiveCardsFlush = "FiveCardsFlush"
    FourCardsPairInFlush = "FourCardsPairInFlush"
    FourCardsTwoPairsInFlush = "FourCardsTwoPairsInFlush"
    ThreeOfKind = "ThreeOfKind"
    FiveCardsPairInFlush = "FiveCardsPairInFlush"
    FiveCardsTwoPairsInFlush = "FiveCardsTwoPairsInFlush"
    ThreeCardsFlushStraight = "ThreeCardsFlushStraight"
    FourCardsFlushStraight = "FourCardsFlushStraight"
    FourOfKind = "FourOfKind"
    FiveCardsFlushStraight = "FiveCardsFlushStraight"
    SixCardsFlush = "SixCardsFlush"
    SixCardsStraight = "SixCardsStraight"
    SixCardsPairInFlush = "SixCardsPairInFlush"
    SixCardsTwoPairsInFlush = "SixCardsTwoPairsInFlush"
    SixCardsFlushStraight = "SixCardsFlushStraight"
    SixCardsThreePairsInFlush = "SixCardsThreePairsInFlush"
    SevenCardsFlush = "SevenCardsFlush"
    SevenCardsStraight = "SevenCardsStraight"
    SevenCardsPairInFlush = "SevenCardsPairInFlush"
    SevenCardsTwoPairsInFlush = "SevenCardsTwoPairsInFlush"
    SevenCardsThreePairsInFlush = "SevenCardsThreePairsInFlush"
    SevenCardsFlushStraight = "SevenCardsFlushStraight"
    EightCardsFlush = "EightCardsFlush"
    EightCardsPairInFlush = "EightCardsPairInFlush"
    EightCardsTwoPairsInFlush = "EightCardsTwoPairsInFlush"
    EightCardsStraight = "EightCardsStraight"
    FiveOfKind = "FiveOfKind"
    EightCardsFlushStraight = "EightCardsFlushStraight"
    EightCardsThreePairsInFlush = "EightCardsThreePairsInFlush"
    EightCardsFourPairsInFlush = "EightCardsFourPairsInFlush"
    SixOfKind = "SixOfKind"
    SevenOfKind = "SevenOfKind"
    EightOfKind = "EightOfKind"
    NineCardsFlush = "NineCardsFlush"
    NineCardsStraight = "NineCardsStraight"
    NineCardsFlushStraight = "NineCardsFlushStraight"
    NineOfKind = "NineOfKind"
    TenCardsFlush = "TenCardsFlush"
    TenCardsStraight = "TenCardsStraight"
    TenCardsFlushStraight = "TenCardsFlushStraight"
    TenOfKind = "TenOfKind"
    None_ = "None"

    def __str__(self):
        if self == BaseCompType.None_:
            return "None"
        return self.value


class PokerRankTypes(Enum):
    Nothing = "Nothing"
    Pair = "Pair"
    TwoPairs = "TwoPairs"
    ThreeCardsFlush = "ThreeCardsFlush"
    ThreeCardsStraight = "ThreeCardsStraight"
    ThreeCardsFlushStraight = "ThreeCardsFlushStraight"
    ThreeOfKind = "ThreeOfKind"
    FourCardsFlush = "FourCardsFlush"
    FourCardsStraight = "FourCardsStraight"
    FourCardsFlushStraight = "FourCardsFlushStraight"
    FiveCardsStraight = "FiveCardsStraight"
    FiveCardsFlush = "FiveCardsFlush"
    FullHouse = "FullHouse"
    FourOfKind = "FourOfKind"
    FiveCardsFlushStraight = "FiveCardsFlushStraight"
    FiveOfKind = "FiveOfKind"
    SixCardsStraight = "SixCardsStraight"
    SixCardsFlush = "SixCardsFlush"
    SixCardsFlushStraight = "SixCardsFlushStraight"
    SixOfKind = "SixOfKind"
    SevenCardsStraight = "SevenCardsStraight"
    SevenCardsFlush = "SevenCardsFlush"
    SevenCardsFlushStraight = "SevenCardsFlushStraight"
    SevenOfKind = "SevenOfKind"
    EightCardsStraight = "EightCardsStraight"
    EightCardsFlush = "EightCardsFlush"
    EightCardsFlushStraight = "EightCardsFlushStraight"
    EightOfKind = "EightOfKind"


class PokerCardCompRank(Enum):
    Pair = "Pair"
    ThreeCardFlush = "ThreeCardFlush"
    ThreeCardStraight = "ThreeCardStraight"
    ThreeCardFlushStraight = "ThreeCardFlushStraight"
    ThreeOfKind = "ThreeOfKind"
    FourCardFlush = "FourCardFlush"
    FourCardStraight = "FourCardStraight"
    FourCardFlushStraight = "FourCardFlushStraight"
    FourOfKind = "FourOfKind"
    FiveCardStraight = "FiveCardStraight"
    FiveCardFlush = "FiveCardFlush"
    FullHouse = "FullHouse"
    FiveCardFlushStraight = "FiveCardFlushStraight"
    FiveOfKind = "FiveOfKind"
    SixCardStraight = "SixCardStraight"
    SixCardFlush = "SixCardFlush"
    SixCardFlushStraight = "SixCardFlushStraight"
    SixOfKind = "SixOfKind"
    SevenCardStraight = "SevenCardStraight"
    SevenCardFlush = "SevenCardFlush"
    SevenCardFlushStraight = "SevenCardFlushStraight"
    SevenOfKind = "SevenOfKind"
    EightCardStraight = "EightCardStraight"
    EightCardFlush = "EightCardFlush"
    EightCardFlushStraight = "EightCardFlushStraight"
    EightOfKind = "EightOfKind"
    ThirteenCardStraight = "ThirteenCardStraight"
    ThirteenCardFlushStraight = "ThirteenCardFlushStraight"
    FourteenCardStraight = "FourteenCardStraight"


class PokerConst:
    MaxTotalCountInSameSuit = 13
    TotalRegularSuitCount = 4
    TotalRegularPokerCardsWithoutJokers = MaxTotalCountInSameSuit * TotalRegularSuitCount
    AceBigNumber = MaxTotalCountInSameSuit + 1

    PokerNumberNameDict: Dict[int, str] = {
        1: "A", 2: "2", 3: "3", 4: "4", 5: "5", 6: "6",
        7: "7", 8: "8", 9: "9", 10: "10", 11: "J", 12: "Q", 13: "K",
        14: "A", 15: "Joker", 16: "Joker", 17: "Joker", 18: "Joker",
    }

    PokerStringToNumberDict: Dict[str, int] = {
        "A": 14, "2": 2, "3": 3, "4": 4, "5": 5, "6": 6, "7": 7,
        "8": 8, "9": 9, "10": 10, "J": 11, "Q": 12, "K": 13, "Joker": 15
    }

    PokerSuitToSymbol: Dict[PokerSuit, str] = {
        PokerSuit.NoSuit: "✖️",
        PokerSuit.Club: "♣️",
        PokerSuit.Diamond: "🔶",
        PokerSuit.Heart: "❤️",
        PokerSuit.Spade: "♠️",
        PokerSuit.Star: "⭐",
        PokerSuit.Wild: "🃏",
    }

    SymbolToPokerSuit: Dict[str, PokerSuit] = {
        "✖️": PokerSuit.NoSuit,
        "♣️": PokerSuit.Club,
        "♣": PokerSuit.Club,
        "🔶": PokerSuit.Diamond,
        "❤️": PokerSuit.Heart,
        "♠️": PokerSuit.Spade,
        "♠": PokerSuit.Spade,
        "⭐": PokerSuit.Star,
        "🃏": PokerSuit.Wild,
    }
