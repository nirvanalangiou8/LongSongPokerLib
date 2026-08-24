using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GenericPoker;
using GenericPoker.EightCard;
using LongSongPokerLibCore.GenericPoker;
using GenericPoker.CardSimStatAnalysis;

namespace LongSongPokerLibCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Available options: "analyze", "hand", "game", "split", "run_stat", "debug"
            var runOption = "run_stat"; 

            if (args.Length > 0 && args[0] != "hand")
            {
                runOption = args[0];
            }

            switch (runOption)
            {
                case "analyze":
                    InitEightCardHandSplitProbAna.Run();
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
                    
                    SimRunAndCalcComponentStat.SimCardRunStat(500000000, 8, useParallel: true);
                    //SimRunAndCalcComponentStat.SimCardRunStat(500000000, 9, useParallel: true);
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

            // 1. 建立一副 52 張的標準牌 (4 花色 x 13 點數) 的字串表示。
            string[] numberStrs = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            string[] suitSymbols = { "♣️", "🔶", "❤️", "♠️" };

            var fullDeck = new System.Collections.Generic.List<string>();
            foreach (var suit in suitSymbols)
                foreach (var num in numberStrs)
                    fullDeck.Add(num + suit);

            // 牌堆 (尚未發出的牌) 與棄牌堆 (已用過的牌)。
            var deck = new System.Collections.Generic.List<string>(fullDeck);
            var discard = new System.Collections.Generic.List<string>();
            XRandom.Instance.Shuffle(deck);

            int round = 0;
            while (true)
            {
                round++;

                // 若牌堆剩餘不足 8 張，將棄牌全部洗回牌堆。
                if (deck.Count < 8)
                {
                    Console.WriteLine("牌堆不足 8 張，將棄牌重新洗牌補回牌堆... (reshuffle)\n");
                    deck.AddRange(discard);
                    discard.Clear();
                    XRandom.Instance.Shuffle(deck);
                }

                // 從牌堆頂端發出 8 張，其餘留在牌堆。
                var dealtCards = deck.GetRange(0, 8);
                deck.RemoveRange(0, 8);
                discard.AddRange(dealtCards);

                Console.WriteLine($"------ 第 {round} 局 (Round {round}) ------");
                PlayOneHand(string.Join(",", dealtCards));
                Console.WriteLine($"(牌堆剩餘 {deck.Count} 張，棄牌堆 {discard.Count} 張)");
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
                // 1. 解析 8 張牌。
                var cards = inputCardStr.Split(',')
                    .Select(s => EightCardPokerCard.CreateInstance(s.Trim()))
                    .ToList();

                if (cards.Count != 8)
                {
                    Console.WriteLine("輸入牌張數不是 8，略過。");
                    return;
                }

                EightCardSubBattleHand bestFrontHand = null, bestBackHand = null;
                double bestFront = 0, bestBack = 0, bestTotal = double.NegativeInfinity;

                // 2. 窮舉所有 C(8,3) 的拆法：選 3 張當前墩，其餘 5 張當後墩。
                foreach (var frontIdx in Combinations(cards.Count, 3))
                {
                    var frontCards = frontIdx.Select(i => cards[i]).ToList();
                    var backCards = Enumerable.Range(0, cards.Count)
                        .Where(i => !frontIdx.Contains(i))
                        .Select(i => cards[i])
                        .ToList();

                    // 各墩各自評估出最強牌型 (含散牌)。
                    var frontHand = EvaluateBestSingleHand(frontCards, BattleHandEnum.FirstHand);
                    var backHand = EvaluateBestSingleHand(backCards, BattleHandEnum.SecondHand);

                    // 規則限制：後墩不得弱於前墩 (否則為相公/犯規)。
                    if (backHand.CompareTo(frontHand) < 0) continue;

                    double front = WinRateStrategy.GetSubHandWinRate(frontHand);
                    double back = WinRateStrategy.GetSubHandWinRate(backHand);
                    double total = front + back;

                    if (total > bestTotal)
                    {
                        bestTotal = total;
                        bestFront = front;
                        bestBack = back;
                        bestFrontHand = frontHand;
                        bestBackHand = backHand;
                    }
                }

                if (bestFrontHand == null || bestBackHand == null)
                {
                    Console.WriteLine("無法排出有效的前/後墩。");
                    return;
                }

                // 3. 印出最佳排列與各墩勝率。
                Console.WriteLine("最佳排列 (窮舉所有拆法，取 前墩+後墩勝率總和最大)：\n");

                Console.WriteLine("--- 前墩 (Front Hand) ---");
                Console.WriteLine($"  牌型 (Rank): {bestFrontHand.BattleHandRank}");
                Console.WriteLine($"  牌組 (Cards): {bestFrontHand.GetHandString()}");
                Console.WriteLine($"  勝率 (Win Rate): {bestFront:P4}\n");

                Console.WriteLine("--- 後墩 (Back Hand) ---");
                Console.WriteLine($"  牌型 (Rank): {bestBackHand.BattleHandRank}");
                Console.WriteLine($"  牌組 (Cards): {bestBackHand.GetHandString()}");
                Console.WriteLine($"  勝率 (Win Rate): {bestBack:P4}\n");

                Console.WriteLine($"加權總分 (前墩勝率 + 後墩勝率): {bestTotal:F4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple 8-Card Game error: {ex.Message}");
            }

            Console.WriteLine("\n==========================");
        }

        /// <summary>
        /// 將一組牌 (3 張或 5 張) 評估成「單一墩」的最強牌型 (EightCardSubBattleHand)，
        /// 散牌 (kicker) 會依點數由大到小補入。利用 PokerHandCalculator 的拆解結果取得正確牌型，
        /// 並在所有拆解中挑出最強者；若都不成牌，退回純散牌 (Nothing)。
        /// </summary>
        static EightCardSubBattleHand EvaluateBestSingleHand(
            System.Collections.Generic.List<EightCardPokerCard> cards, BattleHandEnum which)
        {
            // 基準：純散牌 (Nothing)，全部當作散牌。
            var best = BuildSingleHand(which, EightCardsBattleHandRank.Nothing,
                new System.Collections.Generic.List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>(),
                cards);

            var calc = new PokerHandCalculator();
            calc.SetupCards(cards);
            calc.MinFlushStraightCards = 3;

            var structures = calc.Test8Cards();

            foreach (var st in structures)
            {
                var comps = st.Components;
                if (comps.Count == 0) continue;

                EightCardsBattleHandRank rank;
                var usedComps = new System.Collections.Generic.List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>();

                // 嘗試以前兩個 component 組成複合牌型 (例如 葫蘆 = 三條 + 一對，兩對 = 一對 + 一對)。
                if (comps.Count >= 2 &&
                    (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
                         (comps[0].CompRank, comps[1].CompRank), out rank) ||
                     PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
                         (comps[1].CompRank, comps[0].CompRank), out rank)))
                {
                    usedComps.Add(comps[0]);
                    usedComps.Add(comps[1]);
                }
                else
                {
                    rank = PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank);
                    usedComps.Add(comps[0]);
                }

                // 該牌型必須對此墩 (前墩/後墩) 合法，否則略過。
                if (!EightCardSubBattleHand.EightCardsBattleHandPowerDict.ContainsKey((which, rank)))
                    continue;

                var usedSet = new System.Collections.Generic.HashSet<EightCardPokerCard>(
                    usedComps.SelectMany(c => c.Cards));
                var leftovers = cards.Where(c => !usedSet.Contains(c)).ToList();

                var cand = BuildSingleHand(which, rank, usedComps, leftovers);
                if (cand.CompareTo(best) > 0) best = cand;
            }

            return best;
        }

        /// <summary>
        /// 以指定牌型與 component 建立一個墩，並把散牌依點數由大到小補入 (受該墩容量限制：前墩 3、後墩 5)。
        /// </summary>
        static EightCardSubBattleHand BuildSingleHand(BattleHandEnum which, EightCardsBattleHandRank rank,
            System.Collections.Generic.List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps,
            System.Collections.Generic.List<EightCardPokerCard> kickers)
        {
            var hand = new EightCardSubBattleHand(which, rank, comps.ToArray());
            var sorted = kickers.OrderByDescending(c => c.PokerCardPower).ToList();
            hand.AddMinorCards(sorted);
            return hand;
        }

        /// <summary>
        /// 產生「從 n 個元素中取 k 個」的所有索引組合 (字典序)。
        /// </summary>
        static System.Collections.Generic.IEnumerable<int[]> Combinations(int n, int k)
        {
            var idx = new int[k];
            for (int i = 0; i < k; i++) idx[i] = i;

            while (true)
            {
                yield return (int[])idx.Clone();

                int pos = k - 1;
                while (pos >= 0 && idx[pos] == n - k + pos) pos--;
                if (pos < 0) break;

                idx[pos]++;
                for (int i = pos + 1; i < k; i++) idx[i] = idx[i - 1] + 1;
            }
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
            
            //var inputCardStr = "J♣️,J🔶,3♣️,5♣️,6♣️,A♣️,A❤️,A♠️";
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
