import os
import sys

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8')

from GenericPoker.XRandom import XRandom
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.NineCardRule import NineCardRule
from GenericPoker.EightCard.PokerMath import PokerMath, SpaceDef, SpaceType
from GenericPoker.EightCard.PokerHandCalculator import PokerHandCalculator
from GenericPoker.EightCard.BattleHandArrangeStrategy import WinRateStrategy
from GenericPoker.ConsolePlayer import ConsolePlayer
from GenericPoker.ConsolePlayManager import ConsolePlayManager
from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard
from GenericPoker.CardSimStatAnalysis.SimStatEstimator import SimStatEstimator
from GenericPoker.CardSimStatAnalysis.SimRunAndCalcComponentStat import SimRunAndCalcComponentStat
from GenericPoker.CardSimStatAnalysis.InitEightCardHandSplitProbAna import InitEightCardHandSplitProbAna


class PokerEvaluator:
    PROB_NOTHING_MIN = 0.00
    PROB_NOTHING_MAX = 0.5656
    PROB_PAIR_MIN = 0.5656
    PROB_PAIR_MAX = 0.9962
    PROB_MANSION_MIN = 0.8817
    PROB_MANSION_MAX = 0.9452

    @classmethod
    def run_tests(cls) -> None:
        print("\n=== Poker Evaluator Tests ===")

        rate1 = cls.evaluate_three_nothing()
        print(f"範例一 (3 Cards Nothing - A,5,3): {rate1 * 100:.4f}%")

        rate2 = cls.evaluate_pair_with_kicker(8, 11)
        print(f"範例二 (Pair 8 + Kicker J): {rate2 * 100:.4f}%")

        rate3 = cls.evaluate_mansion(11, 4)
        print(f"範例三 (Mansion - J-10-9 Straight + Pair 4): {rate3 * 100:.4f}%")

        rate4 = cls.evaluate_flush_straight_with_two_kickers(12, 8, 2)
        print(f"範例四 (Q-J-10 Straight + 8,2 Nothing): {rate4 * 100:.4f}%")
        print("=============================\n")

        cls.demo_win_rate_strategy()

    @classmethod
    def demo_win_rate_strategy(cls) -> None:
        print("=== WinRate Strategy Demo ===")
        input_card_str = "A♣️,A❤️,A♠️,8❤️,8🔶,5♣️,3♣️,2🔶"
        try:
            poker_hand = PokerHandCalculator.create_instance(input_card_str)
            poker_hand.min_flush_straight_cards = 3
            structures = poker_hand.test_8_cards()
            if len(structures) > 0:
                strategy = WinRateStrategy()
                hands = structures[0].arrange_hands(strategy)

                front_rate = WinRateStrategy.get_sub_hand_win_rate(hands.front_hand)
                back_rate = WinRateStrategy.get_sub_hand_win_rate(hands.back_hand)

                print(f"輸入: {input_card_str}")
                print(f"前墩 (FrontHand): {hands.front_hand.battle_hand_rank.value} -> 勝率 {front_rate * 100:.4f}%")
                print(f"後墩 (BackHand):  {hands.back_hand.battle_hand_rank.value} -> 勝率 {back_rate * 100:.4f}%")
                print(f"加權總分 (前墩+後墩勝率): {(front_rate + back_rate):.4f}")
        except Exception as ex:
            print(f"WinRate Strategy Demo error: {ex}")
        print("=============================\n")

    @classmethod
    def evaluate_three_nothing(cls) -> float:
        schema = [SpaceDef(SpaceType.Combination, pool_size=13, dimensions=3)]
        offsets = [14 - 2, 5 - 2, 3 - 2]
        return PokerMath.get_unified_win_rate(offsets, schema, cls.PROB_NOTHING_MIN, cls.PROB_NOTHING_MAX)

    @classmethod
    def evaluate_pair_with_kicker(cls, pair_rank: int, kicker_rank: int) -> float:
        schema = [
            SpaceDef(SpaceType.Cartesian, pool_size=13, dimensions=1),
            SpaceDef(SpaceType.Cartesian, pool_size=12, dimensions=1)
        ]
        pair_offset = pair_rank - 2
        kicker_offset = (kicker_rank - 2) if kicker_rank < pair_rank else (kicker_rank - 3)
        offsets = [pair_offset, kicker_offset]
        return PokerMath.get_unified_win_rate(offsets, schema, cls.PROB_PAIR_MIN, cls.PROB_PAIR_MAX)

    @classmethod
    def evaluate_mansion(cls, straight_high: int, pair_rank: int) -> float:
        schema = [
            SpaceDef(SpaceType.Cartesian, pool_size=11, dimensions=1),
            SpaceDef(SpaceType.Cartesian, pool_size=13, dimensions=1)
        ]
        straight_offset = straight_high - 4
        pair_offset = pair_rank - 2
        offsets = [straight_offset, pair_offset]
        return PokerMath.get_unified_win_rate(offsets, schema, cls.PROB_MANSION_MIN, cls.PROB_MANSION_MAX)

    @classmethod
    def evaluate_flush_straight_with_two_kickers(cls, straight_high: int, k1: int, k2: int) -> float:
        schema = [
            SpaceDef(SpaceType.Cartesian, pool_size=11, dimensions=1),
            SpaceDef(SpaceType.Combination, pool_size=13, dimensions=2)
        ]
        offsets = [straight_high - 4, k1 - 2, k2 - 2]
        return PokerMath.get_unified_win_rate(offsets, schema, 0.6450, 0.8154)


def play_one_hand(input_card_str: str) -> None:
    print(f"隨機發出的 8 張牌 (Dealt 8 cards):\n  {input_card_str}\n")
    try:
        result = ConsolePlayer.evaluate_best_split_hand(input_card_str)
        if result is None:
            print("無法排出有效的前/後墩。")
            return

        print("最佳排列 (窮舉所有拆法，取 前墩+後墩勝率總和最大)：\n")
        print("--- 前墩 (Front Hand) ---")
        print(f"  牌型 (Rank): {result.front_hand.battle_hand_rank.value}")
        print(f"  牌組 (Cards): {result.front_hand.get_hand_string()}")
        print(f"  勝率 (Win Rate): {result.front_win_rate * 100:.4f}%\n")

        print("--- 後墩 (Back Hand) ---")
        print(f"  牌型 (Rank): {result.back_hand.battle_hand_rank.value}")
        print(f"  牌組 (Cards): {result.back_hand.get_hand_string()}")
        print(f"  勝率 (Win Rate): {result.back_win_rate * 100:.4f}%\n")

        print(f"加權總分 (前墩勝率 + 後墩勝率): {result.total_score:.4f}")
    except Exception as ex:
        print(f"Simple 8-Card Game error: {ex}")
    print("\n==========================")


def run_simple_eight_card_game() -> None:
    print("=== Continuous 8-Card Game ===\n")
    print("(每局結束後按任意鍵繼續，按 Q 或 Esc 離開)\n")

    game_manager = ConsolePlayManager(EightCardRule.default(), cards_per_player=8)
    player = ConsolePlayer("Player1", EightCardRule.default())
    game_manager.add_player(player)

    while True:
        game_manager.start_new_round()
        print(f"------ 第 {game_manager.current_round} 局 (Round {game_manager.current_round}) ------")
        cards_str = ",".join(c.card_str for c in player.cards)
        play_one_hand(cards_str)
        print(f"(牌堆剩餘 {len(game_manager.dealer.remaining_cards)} 張，棄牌堆 {len(game_manager.dealer.discard_cards)} 張)")
        print("\n按任意鍵繼續下一局 (Hit any key to continue, Q/Esc to quit)...")

        key = input().strip()
        if key.lower() == 'q':
            print("\n遊戲結束 (Game over)。")
            break
        print()


def debug_sim_hand_type() -> None:
    input_card_str = "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,4❤️,4♣️"
    cards = [SimPokerCard.create_instance(s.strip()) for s in input_card_str.split(',') if s.strip()]
    calculator = SimStatEstimator()
    calculator.setup_cards(cards)
    results = calculator.test_sim_cards()

    found_types = ", ".join(r.final_comps_str for r in results)
    print(f"Input Cards: {input_card_str}")
    print(f"Found types: {found_types}")
    for r in results:
        print(f"Result Hand Type: {r.final_comps_str}")


def test_hand_split() -> None:
    print("Hello World!")
    input_card_str = "8❤️,8🔶,6❤️,6🔶,4❤️,4♣️,2♣️,2♣️"
    poker_hand = PokerHandCalculator.create_instance(input_card_str)
    hand_res = poker_hand.test_8_cards()
    poker_hand.min_flush_straight_cards = 3
    res_hand = poker_hand.test_8_cards_two_hands_deploy()

    if res_hand is not None:
        print("\n=== Front Hand ===")
        print(f"Rank: {res_hand.front_hand.battle_hand_rank.value}")
        print(f"Cards: {res_hand.front_hand.get_hand_string()}")

        print("\n=== Back Hand ===")
        print(f"Rank: {res_hand.back_hand.battle_hand_rank.value}")
        print(f"Cards: {res_hand.back_hand.get_hand_string()}")

    print("\nSuccessfully created poker hand and deployed.")


def main(args: list) -> None:
    run_option = "run_stat"
    if len(args) > 1 and args[1] != "hand":
        run_option = args[1]

    if run_option == "analyze":
        selected_rule: ICardRule = EightCardRule.default()
        if len(args) >= 5 and args[4].isdigit():
            card_count = int(args[4])
            selected_rule = NineCardRule.default() if card_count == 9 else EightCardRule.default()
        elif len(args) >= 3 and any(k in args[2] for k in ["9cards", "9_cards", "9card"]):
            selected_rule = NineCardRule.default()

        if len(args) >= 4:
            InitEightCardHandSplitProbAna.run(args[2], args[3], selected_rule)
        elif len(args) >= 3:
            InitEightCardHandSplitProbAna.run(args[2], rule=selected_rule)
        else:
            InitEightCardHandSplitProbAna.run("stats_result_8cards.csv", "twohands_prob_8cards.csv", EightCardRule.default())
            InitEightCardHandSplitProbAna.run("stats_result_9cards.csv", "twohands_prob_9cards.csv", NineCardRule.default())

    elif run_option == "hand":
        if len(args) >= 3:
            play_one_hand(args[2])
        else:
            play_one_hand("A♠️,K♠️,Q♠️,J♠️,10♠️,9♠️,8♠️,7♠️")

    elif run_option == "game":
        XRandom.init(12345678)
        run_simple_eight_card_game()

    elif run_option == "split":
        test_hand_split()

    elif run_option == "run_stat":
        XRandom.init(12345678)
        out_file = args[2] if len(args) > 2 else "stats_result_9cards.csv"
        iters = int(args[3]) if len(args) > 3 else 100000
        cards = int(args[4]) if len(args) > 4 else 9
        SimRunAndCalcComponentStat.sim_card_run_stat(out_file, iters, cards, use_parallel=False)

    elif run_option in ("runtest", "test", "evaluator"):
        PokerEvaluator.run_tests()

    elif run_option == "debug":
        debug_sim_hand_type()

    else:
        print("Unknown runOption. Available options: analyze, hand, game, split, run_stat, runtest, test, debug")


if __name__ == "__main__":
    main(sys.argv)
