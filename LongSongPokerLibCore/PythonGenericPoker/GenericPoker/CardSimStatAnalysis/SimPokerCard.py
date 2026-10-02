from typing import Tuple
from GenericPoker.BasePokerCard import BasePokerCard
from GenericPoker.PokerEnumAndDicts import PokerSuit, PokerConst


class SimPokerCard(BasePokerCard):
    def __init__(self, card_id: int = 0, number: int = 0, suit: PokerSuit = PokerSuit.NoSuit, object_id: int = 0, deck_id: int = 1):
        super().__init__(card_id, number, suit, object_id, deck_id)

    @property
    def is_numberable(self) -> bool:
        return True

    @staticmethod
    def _split_card(input_str: str) -> Tuple[str, str]:
        # Split number part and suit symbol part
        # Unicode suits can be 1 or 2 code points (e.g. ♣️ is \u2663\ufe0f or ♣)
        # Check against known suit symbols
        input_str = input_str.strip()
        for symbol in sorted(PokerConst.SymbolToPokerSuit.keys(), key=lambda s: len(s), reverse=True):
            if input_str.endswith(symbol):
                val = input_str[:-len(symbol)]
                return val, symbol
        # Fallback if no matching symbol
        return input_str[:-1], input_str[-1:]

    @classmethod
    def create_instance(cls, *args, **kwargs) -> 'SimPokerCard':
        # Overloads:
        # 1. create_instance(id: int, number: int, suit: PokerSuit, object_id=0, deck_id=1)
        # 2. create_instance(card_str: str, object_id=0, deck_id=1)
        # 3. create_instance(another: SimPokerCard)
        from GenericPoker.AcePokerCard import AcePokerCard

        if len(args) == 1 and isinstance(args[0], SimPokerCard):
            another = args[0]
            return cls.create_instance(another._cardID, another._number, another._suit, another.object_id, another.deck_id)

        if len(args) >= 1 and isinstance(args[0], str):
            poker_card_str = args[0]
            object_id = args[1] if len(args) > 1 else kwargs.get('object_id', 0)
            deck_id = args[2] if len(args) > 2 else kwargs.get('deck_id', 1)

            num_str, suit_symbol = cls._split_card(poker_card_str)
            data = AcePokerCard() if num_str == "A" else SimPokerCard()
            number = PokerConst.PokerStringToNumberDict[num_str]
            suit = PokerConst.SymbolToPokerSuit[suit_symbol]
            card_id = (int(suit) - 1) * PokerConst.MaxTotalCountInSameSuit + number
            data.init(card_id, number, suit, object_id, deck_id)
            return data

        if len(args) >= 3 and isinstance(args[0], int) and isinstance(args[1], int):
            card_id = args[0]
            number = args[1]
            suit = args[2]
            object_id = args[3] if len(args) > 3 else kwargs.get('object_id', 0)
            deck_id = args[4] if len(args) > 4 else kwargs.get('deck_id', 1)

            data = AcePokerCard() if number == PokerConst.AceBigNumber else SimPokerCard()
            data.init(card_id, number, suit, object_id, deck_id)
            return data

        return SimPokerCard()
