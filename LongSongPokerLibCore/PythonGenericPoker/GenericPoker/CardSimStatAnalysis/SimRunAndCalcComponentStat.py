import os
import time
from typing import Dict, List, Optional
from GenericPoker.ConsolePlayManager import ConsolePlayManager
from GenericPoker.CardSimStatAnalysis.SimConsolePlayer import SimConsolePlayer, SimCardPlayerFactory


class SimCardGameManager(ConsolePlayManager):
    def __init__(self, factory: SimCardPlayerFactory, cards_per_hand: int = 8):
        from GenericPoker.EightCardRule import EightCardRule
        super().__init__(rule=EightCardRule.default(), cards_per_player=cards_per_hand)
        total_players = self.dealer.total_cards // cards_per_hand
        for i in range(1, total_players + 1):
            player_name = f"Player#{i}"
            player = factory.create(player_name)
            self.players.append(player)

    @property
    def stat_dict_prop(self) -> Dict[str, int]:
        return self.stat_dict

    def process_players_hands(self) -> None:
        for player in self.players:
            if isinstance(player, SimConsolePlayer):
                ret = player.process_sim_hands()
                if len(ret) == 0:
                    self.update_stat("Nothing")
                for combo in ret:
                    self.update_stat(combo.final_comps_str)

    def update_stat(self, key: str) -> None:
        self.stat_dict[key] = self.stat_dict.get(key, 0) + 1


class SimRunAndCalcComponentStat:
    @staticmethod
    def sim_card_run_stat(output_path: str, total_iterations: int = 10000, cards_per_hand: int = 8, use_parallel: bool = False) -> None:
        if not output_path or not output_path.strip():
            raise ValueError("Output path must not be null or whitespace.")

        dir_name = os.path.dirname(output_path)
        if dir_name and not os.path.exists(dir_name):
            raise FileNotFoundError(f"Directory '{dir_name}' does not exist for output path '{output_path}'.")

        start_time = time.time()
        print(f"Running {total_iterations} iterations for {cards_per_hand} cards (Parallel: {use_parallel})...")

        final_stats: Dict[str, int] = {}
        report_threshold = max(1, total_iterations // 1000)
        next_report = report_threshold

        def update_stats(worker_stats: Dict[str, int]):
            for k, v in worker_stats.items():
                final_stats[k] = final_stats.get(k, 0) + v

        def print_progress(current: int):
            nonlocal next_report
            if current >= next_report or current >= total_iterations:
                percent = current / total_iterations * 100
                print(f"Progress: {percent:.0f}% ({current}/{total_iterations})")
                current_sorted = sorted(final_stats.items(), key=lambda x: x[1], reverse=True)[:10]
                for k, v in current_sorted:
                    print(f"  {k}: {v}")
                while next_report <= current:
                    next_report += report_threshold

        factory = SimCardPlayerFactory()
        game_manager = SimCardGameManager(factory, cards_per_hand)

        for i in range(total_iterations):
            game_manager.collect_players_cards_and_shuffle()
            game_manager.deal_cards_to_players(cards_per_hand)
            game_manager.process_players_hands()

            if (i + 1) % 10000 == 0:
                print_progress(i + 1)

        update_stats(game_manager.stat_dict)

        elapsed_ms = int((time.time() - start_time) * 1000)
        print(f"{cards_per_hand}Card Game Test completed in {elapsed_ms} ms.")

        sorted_stats = sorted(final_stats.items(), key=lambda x: x[1], reverse=True)
        total_hands = sum(v for _, v in sorted_stats)

        with open(output_path, "w", encoding="utf-8", newline="\n") as writer:
            writer.write(f"# Total Iterations: {total_iterations}\n")
            writer.write(f"# Cards per Hand: {cards_per_hand}\n")
            writer.write("Hand Type,Count,Probability\n")

            for key, count in sorted_stats:
                probability = count / total_hands if total_hands > 0 else 0.0
                writer.write(f"{key},{count},{probability:.6f}\n")

        print(f"Results saved to {output_path}")
