from typing import List, Dict, Optional
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.ConsoleCardDealer import ConsoleCardDealer
from GenericPoker.ConsolePlayer import ConsolePlayer, IPlayerFactory


class ConsolePlayManager:
    def __init__(self, rule: Optional[ICardRule] = None, cards_per_player: int = 8, card_decks: int = 1,
                 player_factory: Optional[IPlayerFactory] = None):
        self.rule: ICardRule = rule if rule is not None else EightCardRule.default()
        self.cards_per_player: int = cards_per_player
        self.dealer: ConsoleCardDealer = ConsoleCardDealer(card_decks, rule=self.rule)
        self.players: List[ConsolePlayer] = []
        self.stat_dict: Dict[str, int] = {}
        self.current_round: int = 0

        if player_factory is not None:
            total_players = self.dealer.total_cards // cards_per_player
            for i in range(1, total_players + 1):
                player_name = f"Player#{i}"
                player = player_factory.create(player_name)
                self.players.append(player)

    def add_player(self, player: ConsolePlayer) -> None:
        self.players.append(player)

    def check_and_reshuffle_if_needed(self, required_cards: int) -> bool:
        if len(self.dealer.remaining_cards) < required_cards:
            print(f"牌堆不足 {required_cards} 張，將棄牌重新洗牌補回牌堆... (reshuffle)\n")
            self.dealer.recycle_discard_and_reshuffle()
            return True
        return False

    def start_new_round(self) -> None:
        self.current_round += 1
        self.collect_players_cards()
        total_cards_needed = len(self.players) * self.cards_per_player
        if total_cards_needed == 0:
            total_cards_needed = self.cards_per_player
        self.check_and_reshuffle_if_needed(total_cards_needed)
        self.deal_cards_to_players()

    def deal_cards_to_players(self, cards_per_player: Optional[int] = None) -> None:
        count = cards_per_player if cards_per_player is not None else self.cards_per_player
        for player in self.players:
            self.dealer.deal_cards(player, count)

    def collect_players_cards(self) -> None:
        for player in self.players:
            self.dealer.collect_cards(player)

    def collect_players_cards_and_shuffle(self) -> None:
        self.collect_players_cards()
        self.dealer.shuffle_cards()

    def process_players_hands(self) -> None:
        for player in self.players:
            ret = player.process_hands()
            if ret is None or len(ret) == 0:
                self.stat_dict["Nothing"] = self.stat_dict.get("Nothing", 0) + 1
