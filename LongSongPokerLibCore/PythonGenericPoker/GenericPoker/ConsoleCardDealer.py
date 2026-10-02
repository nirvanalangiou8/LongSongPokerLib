from typing import List, Optional, Callable
from GenericPoker.BasePokerCard import BasePokerCard
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.PokerEnumAndDicts import PokerSuit, PokerConst
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.XRandom import XRandom
from GenericPoker.ConsolePlayer import ConsolePlayer


class ConsoleCardDealer:
    def __init__(self, card_decks: int = 1, add_jokers: bool = False, rule: Optional[ICardRule] = None,
                 card_factory: Optional[Callable[[int, int, PokerSuit, int, int], BasePokerCard]] = None):
        self._cardDecks: int = card_decks
        self.rule: ICardRule = rule if rule is not None else EightCardRule.default()
        self._pokerCards: List[BasePokerCard] = []
        self._discardCards: List[BasePokerCard] = []

        for i in range(self._cardDecks):
            for suit_id in range(BasePokerCard.RegularSuitClubIndex, BasePokerCard.RegularSuitSpadeIndex + 1):
                poker_suit = list(PokerSuit)[suit_id]
                for number in range(2, PokerConst.AceBigNumber + 1):
                    card_id = (suit_id - 1) * PokerConst.AceBigNumber + number
                    if card_factory is not None:
                        new_poker_card = card_factory(card_id, number, poker_suit, 0, i + 1)
                    else:
                        new_poker_card = EightCardPokerCard.create_instance(card_id, number, poker_suit, 0, i + 1)
                    self._pokerCards.append(new_poker_card)

        XRandom.get_instance().shuffle(self._pokerCards)

    @property
    def total_cards(self) -> int:
        return len(self._pokerCards)

    @property
    def remaining_cards(self) -> List[BasePokerCard]:
        return self._pokerCards

    @property
    def discard_cards(self) -> List[BasePokerCard]:
        return self._discardCards

    @property
    def card_decks(self) -> int:
        return self._cardDecks

    def recycle_discard_and_reshuffle(self) -> None:
        self._pokerCards.extend(self._discardCards)
        self._discardCards.clear()
        self.shuffle_cards()

    def deal_cards(self, player_or_count, number_of_cards: Optional[int] = None) -> Optional[List[BasePokerCard]]:
        if isinstance(player_or_count, int):
            num_cards = player_or_count
            if num_cards > len(self._pokerCards):
                self.recycle_discard_and_reshuffle()
            pop_cards = self._pokerCards[:num_cards]
            self._pokerCards = self._pokerCards[num_cards:]
            self._discardCards.extend(pop_cards)
            return pop_cards
        else:
            player: ConsolePlayer = player_or_count
            num_cards = number_of_cards if number_of_cards is not None else 8
            if num_cards > len(self._pokerCards):
                self.recycle_discard_and_reshuffle()
            pop_cards = self._pokerCards[:num_cards]
            self._pokerCards = self._pokerCards[num_cards:]
            player.set_cards(pop_cards)
            return None

    def collect_cards(self, player: ConsolePlayer) -> None:
        self._discardCards.extend(player.cards)
        player.clear_cards()

    def shuffle_cards(self) -> None:
        XRandom.get_instance().shuffle(self._pokerCards)
