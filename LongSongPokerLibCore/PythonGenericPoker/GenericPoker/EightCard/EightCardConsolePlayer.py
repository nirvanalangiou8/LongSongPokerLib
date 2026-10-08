from typing import List, Optional
from GenericPoker.ConsolePlayer import ConsolePlayer
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.EightCard.EightCardPokerCard import EightCardPokerCard
from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
from GenericPoker.EightCard.PokerHandStructure import PokerHandStructure


class EightCardConsolePlayer(ConsolePlayer):
    def __init__(self, player_name: str, rule: Optional[ICardRule] = None):
        super().__init__(player_name, rule if rule is not None else EightCardRule.default())
        self._pokerHandCalculator = PokerHandCalculator()

    def process_hands(self) -> List[PokerHandStructure]:
        cards = [
            c if isinstance(c, EightCardPokerCard) else EightCardPokerCard.create_instance(c.card_str)
            for c in self._pokerCards
        ]
        self._pokerHandCalculator.setup_cards(cards)
        return self._pokerHandCalculator.test_8_cards()
