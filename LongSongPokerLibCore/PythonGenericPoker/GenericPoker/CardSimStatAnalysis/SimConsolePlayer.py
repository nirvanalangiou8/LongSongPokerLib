from typing import List, Any
from GenericPoker.ConsolePlayer import ConsolePlayer, IPlayerFactory
from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard
from GenericPoker.CardSimStatAnalysis.SimStatEstimator import SimStatEstimator
from GenericPoker.CardSimStatAnalysis.SimPokerHandStructure import SimPokerHandStructure


class SimConsolePlayer(ConsolePlayer):
    def __init__(self, player_name: str):
        super().__init__(player_name)
        self._pokerHandCalculator = SimStatEstimator()

    def process_sim_hands(self) -> List[SimPokerHandStructure]:
        sim_cards = [
            c if isinstance(c, SimPokerCard) else SimPokerCard.create_instance(c.card_str)
            for c in self._pokerCards
        ]
        self._pokerHandCalculator.setup_cards(sim_cards)
        return self._pokerHandCalculator.test_sim_cards()

    def process_hands(self) -> List[Any]:
        return []


class SimCardPlayerFactory(IPlayerFactory[SimConsolePlayer]):
    def create(self, name: str) -> SimConsolePlayer:
        return SimConsolePlayer(name)
