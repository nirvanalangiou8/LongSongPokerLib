using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericPoker;
using GenericPoker.EightCard;
using GenericPoker.CardSimStatAnalysis;

namespace LongSongPokerLibCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Available options: "analyze", "hand", "game", "split", "run_stat", "debug"
            var runOption = "game"; 

            if (args.Length > 0 && args[0] != "hand")
            {
                runOption = args[0];
            }

            switch (runOption)
            {
                case "analyze":
                    // Usage: Program.exe analyze [sourceDataPath] [outputPath] [cardCount: 8 or 9]
                    ICardRule selectedRule = EightCardRule.Default;
                    if (args.Length >= 4 && int.TryParse(args[3], out int cardCount))
                    {
                        selectedRule = cardCount == 9 ? NineCardRule.Default : EightCardRule.Default;
                    }
                    else if (args.Length >= 2 && (args[1].Contains("9cards") || args[1].Contains("9_cards") || args[1].Contains("9card")))
                    {
                        selectedRule = NineCardRule.Default;
                    }

                    if (args.Length >= 3)
                    {
                        InitEightCardHandSplitProbAna.Run(args[1], args[2], selectedRule);
                    }
                    else if (args.Length >= 2)
                    {
                        InitEightCardHandSplitProbAna.Run(args[1], rule: selectedRule);
                    }
                    else
                    {
                        // Explicitly run with 8-card or 9-card rule
                        InitEightCardHandSplitProbAna.Run("G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\LongSongPokerLibCore\\GenericPoker\\CardSimStatAnalysis\\Data\\debug.csv", "debug_out.csv", new EightCardRule());
                        //InitEightCardHandSplitProbAna.Run("G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\LongSongPokerLibCore\\GenericPoker\\CardSimStatAnalysis\\Data\\stats_result_8cards.csv", "test_out_8cards.csv", EightCardRule.Default);
                        //InitEightCardHandSplitProbAna.Run("G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\LongSongPokerLibCore\\GenericPoker\\CardSimStatAnalysis\\Data\\stats_result_9cards.csv", "test_out_9cards.csv", NineCardRule.Default);
                        //InitEightCardHandSplitProbAna.Run("G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\LongSongPokerLibCore\\GenericPoker\\CardSimStatAnalysis\\Data\\stats_result_8cards_for_unittest.csv", rule: EightCardRule.Default);
                        
                    }
                    break;

                case "hand":
                    // Usage: Program.exe hand "A♠️,K♠️,Q♠️,J♠️,10♠️,9♠️,8♠️,7♠️"
                    if (args.Length >= 2)
                    {
                        PlayOneHand(args[1]);
                    }
                    else
                    {
                        // Default hand for demonstration if no argument provided
                        PlayOneHand("A♠️,K♠️,Q♠️,J♠️,10♠️,9♠️,8♠️,7♠️");
                    }
                    break;

                case "game":
                    XRandom.Init(12345678uL);
                    RunSimpleEightCardGame();
                    break;

                case "split":
                    TestHandSplit();
                    break;

                case "run_stat":
                    XRandom.Init(12345678uL);
                    
                    //SimRunAndCalcComponentStat.SimCardRunStat(@"G:\My Drive\GameDev\RiderProjects\LongSongPokerLib\LongSongPokerLibCore\GenericPoker\CardSimStatAnalysis\Data\stats_result_8cards.csv", 500000000, 8, useParallel: true);
                    //SimRunAndCalcComponentStat.SimCardRunStat(@"G:\My Drive\GameDev\RiderProjects\LongSongPokerLib\LongSongPokerLibCore\GenericPoker\CardSimStatAnalysis\Data\stats_result_8cards.csv", 100000, 8, useParallel: true);
                    //SimRunAndCalcComponentStat.SimCardRunStat(@"G:\My Drive\GameDev\RiderProjects\LongSongPokerLib\LongSongPokerLibCore\GenericPoker\CardSimStatAnalysis\Data\stats_result_9cards.csv", 500000000, 9, useParallel: true);
                    SimRunAndCalcComponentStat.SimCardRunStat(@"G:\My Drive\GameDev\RiderProjects\LongSongPokerLib\LongSongPokerLibCore\GenericPoker\CardSimStatAnalysis\Data\stats_result_9cards.csv", 200000, 9, useParallel: true);
                    //SimRunAndCalcComponentStat.SimCardRunStat(10000, 10);
                    break;

                case "debug":
                    DebugSimHandType();
                    break;

                default:
                    Console.WriteLine("Unknown runOption. Available options: analyze, hand, game, split, test, debug");
                    break;
            }
        }

        /// <summary>
        /// 連續的 8 張牌遊戲：
        /// 1. 從一副 52 張的標準牌中持續發出 8 張牌。
        /// 2. 每局印出原始的 8 張牌，並以 WinRateStrategy (勝率加權策略)
        ///    排出前墩 / 後墩，其準則為「(前墩勝率 + 後墩勝率) 總和最大」。
        /// 3. 已用過的牌會放入棄牌堆；當牌堆剩餘不足 8 張時，
        ///    將所有棄牌重新洗牌補回牌堆後繼續發牌。
        /// 4. 每局結束後等待使用者按任意鍵 (hit any key) 再繼續下一局。
        /// </summary>
        static void RunSimpleEightCardGame()
        {
            Console.WriteLine("=== Continuous 8-Card Game ===\n");
            Console.WriteLine("(每局結束後按任意鍵繼續，按 Q 或 Esc 離開)\n");

            var gameManager = new ConsolePlayManager(EightCardRule.Default, cardsPerPlayer: 8);
            var player = new ConsolePlayer("Player1", EightCardRule.Default);
            gameManager.AddPlayer(player);

            while (true)
            {
                gameManager.StartNewRound();

                Console.WriteLine($"------ 第 {gameManager.CurrentRound} 局 (Round {gameManager.CurrentRound}) ------");
                PlayOneHand(string.Join(",", player.Cards.Select(c => c.CardStr)));
                Console.WriteLine($"(牌堆剩餘 {gameManager.Dealer.RemainingCards.Count} 張，棄牌堆 {gameManager.Dealer.DiscardCards.Count} 張)");
                Console.WriteLine("\n按任意鍵繼續下一局 (Hit any key to continue, Q/Esc to quit)...");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n遊戲結束 (Game over)。");
                    break;
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 處理單一局 8 張牌：印出原始牌，並以「窮舉所有排列」的方式排出最佳前/後墩，
        /// 再印出各墩勝率。
        ///
        /// 與舊版 (僅窮舉牌型「component」分組、散牌 (kicker) 配置固定) 不同，
        /// 這裡直接窮舉「哪 3 張當前墩、其餘 5 張當後墩」的全部 C(8,3)=56 種拆法，
        /// 因此散牌 (例如可移到前墩的 K) 也會被納入搜尋，
        /// 最終取 (前墩勝率 + 後墩勝率) 總和最大、且符合「後墩 ≥ 前墩」規則的排列。
        /// </summary>
        static void PlayOneHand(string inputCardStr)
        {
            Console.WriteLine($"隨機發出的 8 張牌 (Dealt 8 cards):\n  {inputCardStr}\n");

            try
            {
                var result = ConsolePlayer.EvaluateBestSplitHand(inputCardStr);
                if (result == null)
                {
                    Console.WriteLine("無法排出有效的前/後墩。");
                    return;
                }

                // 3. 印出最佳排列與各墩勝率。
                Console.WriteLine("最佳排列 (窮舉所有拆法，取 前墩+後墩勝率總和最大)：\n");

                Console.WriteLine("--- 前墩 (Front Hand) ---");
                Console.WriteLine($"  牌型 (Rank): {result.FrontHand.BattleHandRank}");
                Console.WriteLine($"  牌組 (Cards): {result.FrontHand.GetHandString()}");
                Console.WriteLine($"  勝率 (Win Rate): {result.FrontWinRate:P4}\n");

                Console.WriteLine("--- 後墩 (Back Hand) ---");
                Console.WriteLine($"  牌型 (Rank): {result.BackHand.BattleHandRank}");
                Console.WriteLine($"  牌組 (Cards): {result.BackHand.GetHandString()}");
                Console.WriteLine($"  勝率 (Win Rate): {result.BackWinRate:P4}\n");

                Console.WriteLine($"加權總分 (前墩勝率 + 後墩勝率): {result.TotalScore:F4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple 8-Card Game error: {ex.Message}");
            }

            Console.WriteLine("\n==========================");
        }

        public static class PokerEvaluator
        {
            // 假設這是在你統計圖表上查到的 CDF 邊界值
            private const double PROB_NOTHING_MIN = 0.00;
            private const double PROB_NOTHING_MAX = 0.5656;
            private const double PROB_PAIR_MIN = 0.5656;
            private const double PROB_PAIR_MAX = 0.9962;
            private const double PROB_MANSION_MIN = 0.8817;
            private const double PROB_MANSION_MAX = 0.9452;

            public static void RunTests()
            {
                Console.WriteLine("\n=== Poker Evaluator Tests ===");

                double rate1 = EvaluateThreeNothing();
                Console.WriteLine($"範例一 (3 Cards Nothing - A,5,3): {rate1:P4}");

                double rate2 = EvaluatePairWithKicker(8, 11);
                Console.WriteLine($"範例二 (Pair 8 + Kicker J): {rate2:P4}");

                double rate3 = EvaluateMansion(11, 4);
                Console.WriteLine($"範例三 (Mansion - J-10-9 Straight + Pair 4): {rate3:P4}");

                double rate4 = EvaluateFlushStraightWithTwoKickers(12, 8, 2);
                Console.WriteLine($"範例四 (Q-J-10 Straight + 8,2 Nothing): {rate4:P4}");
                Console.WriteLine("=============================\n");

                DemoWinRateStrategy();
            }

            /// <summary>
            /// 範例五：以 WinRateStrategy (勝率加權策略) 排牌，並印出前墩/後墩勝率。
            /// 展示「以 (前墩勝率 + 後墩勝率) 作為加權指引」來最佳化拆牌。
            /// </summary>
            public static void DemoWinRateStrategy()
            {
                Console.WriteLine("=== WinRate Strategy Demo ===");

                // 一手範例 8 張牌：三條 A + 一對 8 (理應拆成 後墩 葫蘆，前墩 散牌)。
                var inputCardStr = "A♣️,A❤️,A♠️,8❤️,8🔶,5♣️,3♣️,2🔶";
                try
                {
                    var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
                    pokerHand.MinFlushStraightCards = 3;

                    var structures = pokerHand.Test8Cards();
                    if (structures.Count > 0)
                    {
                        var strategy = new WinRateStrategy();
                        var hands = structures[0].ArrangeHands(strategy);

                        double frontRate = WinRateStrategy.GetSubHandWinRate(hands.FrontHand);
                        double backRate = WinRateStrategy.GetSubHandWinRate(hands.BackHand);

                        Console.WriteLine($"輸入: {inputCardStr}");
                        Console.WriteLine($"前墩 (FrontHand): {hands.FrontHand.BattleHandRank} -> 勝率 {frontRate:P4}");
                        Console.WriteLine($"後墩 (BackHand):  {hands.BackHand.BattleHandRank} -> 勝率 {backRate:P4}");
                        Console.WriteLine($"加權總分 (前墩+後墩勝率): {(frontRate + backRate):F4}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WinRate Strategy Demo error: {ex.Message}");
                }

                Console.WriteLine("=============================\n");
            }

            /// <summary>
            /// 範例一：純散牌 (3 Cards Nothing)
            /// 測試案例：A, 5, 3 (點位：14, 5, 3)
            /// </summary>
            public static double EvaluateThreeNothing()
            {
                // 1. 定義這副牌的形狀 (1 個組合空間，選 3 張牌)
                var schema = new[] {
                    new SpaceDef(SpaceType.Combination, poolSize: 13, dimensions: 3)
                };

                // 2. 將實際點位轉換為 0 起始的 Offset (撲克牌最小是 2，所以減 2)
                var offsets = new[] {
                    14 - 2, // A
                    5 - 2,  // 5
                    3 - 2   // 3
                };

                // 3. 呼叫大一統引擎
                return PokerMath.GetUnifiedWinRate(offsets, schema, PROB_NOTHING_MIN, PROB_NOTHING_MAX);
            }

            /// <summary>
            /// 範例二：一對 + 單張烏龍 (1 Pair + 1 Kicker)
            /// 測試案例：一對 8，帶一張 J (點位：Pair 8, Kicker 11)
            /// </summary>
            public static double EvaluatePairWithKicker(int pairRank, int kickerRank)
            {
                // 1. 定義形狀 (2 個笛卡兒空間。對子有 13 種可能，Kicker 因為要避開對子點位，剩 12 種)
                var schema = new[] {
                    new SpaceDef(SpaceType.Cartesian, poolSize: 13, dimensions: 1),
                    new SpaceDef(SpaceType.Cartesian, poolSize: 12, dimensions: 1)
                };

                // 2. 計算 Offset
                int pairOffset = pairRank - 2;

                // Kicker 要做降維處理 (ANY_BUT_NOT_SAME_AS_PREVIOUS)
                int kickerOffset = kickerRank < pairRank ? (kickerRank - 2) : (kickerRank - 3);

                var offsets = new[] { pairOffset, kickerOffset };

                return PokerMath.GetUnifiedWinRate(offsets, schema, PROB_PAIR_MIN, PROB_PAIR_MAX);
            }

            /// <summary>
            /// 範例三：Mansion (3張同花順 + 1對)
            /// 測試案例：J-10-9 同花順 + 一對 4 (Straight High: 11, Pair: 4)
            /// </summary>
            public static double EvaluateMansion(int straightHigh, int pairRank)
            {
                // 1. 定義形狀 
                // 同花順最小是 4-3-2，最大是 A-K-Q，所以 poolSize 是 11 (14 - 4 + 1)
                // 對子可以跟同花順重複，所以 poolSize 保持 13
                var schema = new[] {
                    new SpaceDef(SpaceType.Cartesian, poolSize: 11, dimensions: 1),
                    new SpaceDef(SpaceType.Cartesian, poolSize: 13, dimensions: 1)
                };

                // 2. 計算 Offset
                int straightOffset = straightHigh - 4; // 同花順的基底是 4
                int pairOffset = pairRank - 2;

                var offsets = new[] { straightOffset, pairOffset };

                return PokerMath.GetUnifiedWinRate(offsets, schema, PROB_MANSION_MIN, PROB_MANSION_MAX);
            }

            /// <summary>
            /// 範例四：3張同花順 + 2張散牌 (3 Flush Straight + 2 Nothing)
            /// 測試案例：Q-J-10 同花順 + 散牌 8, 2 (Straight High: 12, Kickers: 8, 2)
            /// </summary>
            public static double EvaluateFlushStraightWithTwoKickers(int straightHigh, int k1, int k2)
            {
                // 1. 定義形狀 (前面是 11 種可能的笛卡兒空間，後面接著從 13 張選 2 張的組合空間)
                // 這就是完美的 Mixed-Radix 混合基底！
                var schema = new[] {
                    new SpaceDef(SpaceType.Cartesian, poolSize: 11, dimensions: 1),
                    new SpaceDef(SpaceType.Combination, poolSize: 13, dimensions: 2)
                };

                // 2. 計算 Offset
                // 注意：Offsets 陣列的長度是 3，因為有 1 個 Straight 和 2 個 Kicker
                // 引擎內部會自動根據 schema 的 dimensions 去截取對應數量的 offset
                var offsets = new[] {
                    straightHigh - 4, // 對應第一個 Cartesian 空間 (dimensions = 1)
                    k1 - 2,           // 對應第二個 Combination 空間的第一張牌
                    k2 - 2            // 對應第二個 Combination 空間的第二張牌
                };

                // 假設這個牌型的勝率區間落在 0.64 到 0.81 之間
                return PokerMath.GetUnifiedWinRate(offsets, schema, 0.6450, 0.8154);
            }
        }

        static void DebugSimHandType()
        {
            var inputCardStr = "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,4❤️,4♣️";
            var cards = inputCardStr.Split(',').Select(s => SimPokerCard.CreateInstance(s.Trim())).ToList();
            var calculator = new SimStatEstimator();
            calculator.SetupCards(cards);
            var results = calculator.TestSimCards();

            string foundTypes = string.Join(", ", results.Select(r => r.FinalCompsStr));
            Console.WriteLine($"Input Cards: {inputCardStr}");
            Console.WriteLine($"Found types: {foundTypes}");
            foreach (var r in results)
            {
                Console.WriteLine($"Result Hand Type: {r.FinalCompsStr}");
            }
        }

        static void TestHandSplit()
        {
            Console.WriteLine("Hello World!");
            
            //var inputCardStr = "J♣️,J🔶,3♣️,5��️,6♣️,A♣️,A❤️,A♠️";
            //var inputCardStr = "J♣️,J🔶,3♣️,6♣️,6❤️,A♣️,A❤️,A♠️";
            //var inputCardStr = "8❤️,7❤️,6❤️,5❤️,4❤️,3♣️,2♣️,A♣️";
            var inputCardStr = "8❤️,8🔶,6❤️,6🔶,4❤️,4♣️,2♣️,2♣️"; // test for four pairs.
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            
            var handRes = pokerHand.Test8Cards();
            
            pokerHand.MinFlushStraightCards = 3;
            var resHand = pokerHand.Test8CardsTwoHandsDeploy();

            // Display front hand result
            Console.WriteLine("\n=== Front Hand ===");
            Console.WriteLine($"Rank: {resHand.FrontHand.BattleHandRank}");
            Console.Write("Cards: ");
            Console.WriteLine(resHand.FrontHand.GetHandString());

            // Display back hand result
            Console.WriteLine("\n=== Back Hand ===");
            Console.WriteLine($"Rank: {resHand.BackHand.BattleHandRank}");
            Console.Write("Cards: ");
            Console.WriteLine(resHand.BackHand.GetHandString());

            Console.WriteLine("\nSuccessfully created poker hand and deployed.");
        }
    }
}
