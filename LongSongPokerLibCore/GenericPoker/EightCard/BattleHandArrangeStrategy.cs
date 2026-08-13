using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.EightCard
{
	

	public interface IBattleHandArrangeStrategy
	{
		float CalcHandWinRate(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand)
		{
			return 0.5f;
		}
		
		(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps);
	}

	public class BalancedStrategy : IBattleHandArrangeStrategy
	{
		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps)
		{
			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;

			if (comps.Count == 3)
			{
				if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (comps[1].CompRank, comps[2].CompRank), out EightCardsBattleHandRank newBattleRank))
				{
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
						PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
					secondEightCardSubBattleHand =
						new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, comps[1], comps[2]);
					if (firstEightCardSubBattleHand > secondEightCardSubBattleHand)
					{
						(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = (secondEightCardSubBattleHand,
							firstEightCardSubBattleHand);
					}
				}
				else
				{
					Console.WriteLine("Fatal error in Battle Hand arrange of strategy (3 comps).");
				}
			}
			else if (comps.Count == 2)
			{
				firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comps[1].CompRank), comps[1]);
				secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
			}

			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}
	}
	
	public class RuleTableStrategy : IBattleHandArrangeStrategy
	{
		/*
		private LookupTable()
		{
			
		}*/
		
		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps)
		{
			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;

			// 1. permutate the current comps starting to select two in comps
			var twoCompPermutations = UtilFunc.GetPermutation(comps, 2);
			foreach (var selectedComps in twoCompPermutations)
			{
				var remainingComps = UtilFunc.GetExcludeList(comps, selectedComps);

				// Tempoarailoiy if any selected combs has more than 2 combs, skip the loop.
				if (remainingComps.Count > 2) continue;

				if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (selectedComps[0].CompRank, selectedComps[1].CompRank), out var firstRank))
				{
					var firstHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, firstRank, selectedComps[0], selectedComps[1]);
					
					// Process second hand
					if (remainingComps.Count == 2)
					{
						if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
							    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var secondRank))
						{
							var secondHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, secondRank, remainingComps[0], remainingComps[1]);
							(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = FinalizeHands(firstHand, secondHand);
							return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
						}
					}
					else if (remainingComps.Count == 1)
					{
						var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
						var secondHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, secondRank, remainingComps[0]);
						(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = FinalizeHands(firstHand, secondHand);
						return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
					}
				}
			}

			// 2. then one in comps
			var oneCompPermutations = UtilFunc.GetPermutation(comps, 1);
			foreach (var selectedComps in oneCompPermutations)
			{
				var selectedComp = selectedComps[0];
				var remainingComps = UtilFunc.GetExcludeList(comps, selectedComps);

				// Tempoarailoiy if any selected combos has more than 2 combos, skip the loop.
				if (remainingComps.Count > 2) continue;

				var firstRank = PokerHandStructure.ConvertCompRankToBattleRank(selectedComp.CompRank);
				var firstHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, firstRank, selectedComp);

				// Process second hand
				if (remainingComps.Count == 2)
				{
					if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out EightCardsBattleHandRank secondRank))
					{
						var secondHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, secondRank, remainingComps[0], remainingComps[1]);
						(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = FinalizeHands(firstHand, secondHand);
						return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var secondHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, secondRank, remainingComps[0]);
					(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = FinalizeHands(firstHand, secondHand);
					return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
				}
			}

			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}

		private (EightCardSubBattleHand, EightCardSubBattleHand) FinalizeHands(EightCardSubBattleHand first, EightCardSubBattleHand second)
		{
			if (first > second)
			{
				return (second, first);
			}
			return (first, second);
		}
	}
	
	/// <summary>
	/// 勝率加權排牌策略 (Win-Rate Weighted Strategy)
	///
	/// 核心思想 (對應 Program.cs 的 PokerEvaluator 範例)：
	/// 1. 每一個牌型 (EightCardsBattleHandRank) 在 stats_result.csv 的累積分布 (CDF) 上佔有一段
	///    機率區間 (min, max)。越強的牌型，其區間越靠近 1。
	/// 2. 在這段區間之內，再依據手牌實際的點位結構，利用 PokerMath.GetUnifiedWinRate 做相對排名內插，
	///    算出這手牌「精準」的勝率落點。
	/// 3. 排牌時，列舉所有合法的 (前墩, 後墩) 拆法，以 前墩勝率 + 後墩勝率 作為加權分數，
	///    取總分最高者作為最佳排列。
	/// </summary>
	public class WinRateStrategy : IBattleHandArrangeStrategy
	{
		// stats_result.csv (5000 萬次模擬) 統計到的各牌型出現機率。
		// 依 SecondHand 的牌力由弱到強排序，用來建立累積分布 (CDF) 區間。
		private static readonly (EightCardsBattleHandRank rank, double prob)[] StatProbLadder =
		{
			(EightCardsBattleHandRank.Nothing,                0.171982),
			(EightCardsBattleHandRank.Pair,                   0.138407),
			(EightCardsBattleHandRank.TwoPairs,               0.256321), // Pair*2
			(EightCardsBattleHandRank.ThreeCardsPairInFlush,  0.034262), // ThreeCardsFlushStraight_Pair 近似
			(EightCardsBattleHandRank.ThreeOfKind,            0.025718),
			(EightCardsBattleHandRank.TownHouse,              0.001622), // ThreeCardsFlushStraight_ThreeOfKind 近似
			(EightCardsBattleHandRank.FiveCardsStraight,      0.106110),
			(EightCardsBattleHandRank.FullHouse,              0.038412), // ThreeOfKind_Pair
			(EightCardsBattleHandRank.ThreeCardsFlushStraight,0.024445),
			(EightCardsBattleHandRank.FiveCardsFlush,         0.051904),
			(EightCardsBattleHandRank.Mansion,                0.034262), // ThreeCardsFlushStraight_Pair
			(EightCardsBattleHandRank.SixCardsStraight,       0.028945),
			(EightCardsBattleHandRank.FourOfKind,             0.001126),
			(EightCardsBattleHandRank.FourCardsFlushStraight, 0.003965),
			(EightCardsBattleHandRank.SixCardsFlush,          0.005314),
			(EightCardsBattleHandRank.SevenCardsStraight,     0.004747),
			(EightCardsBattleHandRank.FiveCardsFlushStraight, 0.000421),
			(EightCardsBattleHandRank.EightCardsStraight,     0.000367),
			(EightCardsBattleHandRank.SevenCardsFlush,        0.000247),
			(EightCardsBattleHandRank.SixCardsFlushStraight,  0.000029),
			(EightCardsBattleHandRank.EightCardsFlush,        0.000004),
		};

		// 由上面的階梯機率建立每個牌型的 CDF 機率區間 (min, max)。
		private static readonly Dictionary<EightCardsBattleHandRank, (double min, double max)> CdfBandDict =
			BuildCdfBands();

		private static Dictionary<EightCardsBattleHandRank, (double min, double max)> BuildCdfBands()
		{
			double total = 0;
			foreach (var entry in StatProbLadder) total += entry.prob;
			if (total <= 0) total = 1;

			var dict = new Dictionary<EightCardsBattleHandRank, (double, double)>();
			double cum = 0;
			foreach (var entry in StatProbLadder)
			{
				double min = cum / total;
				cum += entry.prob;
				double max = cum / total;
				// 同名牌型可能在階梯出現兩次 (例如 ThreeCardsPairInFlush)，以較寬區間為準。
				if (dict.TryGetValue(entry.rank, out var old))
					dict[entry.rank] = (System.Math.Min(old.Item1, min), System.Math.Max(old.Item2, max));
				else
					dict[entry.rank] = (min, max);
			}
			return dict;
		}

		/// <summary>
		/// 取得某牌型在 CDF 上的機率區間 (min, max)。查不到時退回最弱 (Nothing) 區間。
		/// </summary>
		private static (double min, double max) GetBand(EightCardsBattleHandRank rank)
		{
			if (CdfBandDict.TryGetValue(rank, out var band)) return band;
			return (0.0, CdfBandDict.TryGetValue(EightCardsBattleHandRank.Nothing, out var n) ? n.max : 0.1);
		}

		/// <summary>
		/// 安全呼叫 PokerMath：將 offsets 夾在合法範圍內，避免索引越界。
		/// </summary>
		private static double SafeUnifiedWinRate(int[] offsets, SpaceDef[] schema, double min, double max)
		{
			// 依 schema 推算每個 offset 的合法上限後夾值。
			int ptr = 0;
			foreach (var space in schema)
			{
				int upper = space.PoolSize - 1;
				for (int d = 0; d < space.Dimensions && ptr < offsets.Length; d++, ptr++)
				{
					if (offsets[ptr] < 0) offsets[ptr] = 0;
					if (offsets[ptr] > upper) offsets[ptr] = upper;
				}
			}
			return PokerMath.GetUnifiedWinRate(offsets, schema, min, max);
		}

		// 取得某 component 的代表點位 (最大張的點數，2~14)。
		private static int RepRank(PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp)
		{
			int best = 2;
			foreach (var card in comp.Cards)
				if (card.Number > best) best = card.Number;
			return best;
		}

		/// <summary>
		/// 大一統勝率計算機 (long switch)：依牌型結構組出 PokerMath 的 schema / offsets，
		/// 並在該牌型的 CDF 機率區間 (min, max) 內做相對排名內插。
		/// </summary>
		public static double GetSubHandWinRate(EightCardSubBattleHand hand)
		{
			if (hand == null) return 0.0;

			var (min, max) = GetBand(hand.BattleHandRank);
			var comps = hand.Components;

			switch (hand.BattleHandRank)
			{
				case EightCardsBattleHandRank.Nothing:
				{
					// 純散牌：取最大的 (最多 3 張) 點位做組合內插。排牌階段往往尚無散牌，直接退回區間下限。
					var ranks = hand.Cards.Select(c => c.Number).OrderByDescending(n => n).Take(3).ToList();
					if (ranks.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Combination, 13, ranks.Count) };
					var offsets = ranks.Select(r => r - 2).ToArray();
					return SafeUnifiedWinRate(offsets, schema, min, max);
				}

				case EightCardsBattleHandRank.Pair:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.TwoPairs:
				{
					if (comps.Count < 2) return min;
					var schema = new[]
					{
						new SpaceDef(SpaceType.Cartesian, 13, 1),
						new SpaceDef(SpaceType.Cartesian, 13, 1)
					};
					int hi = System.Math.Max(RepRank(comps[0]), RepRank(comps[1]));
					int lo = System.Math.Min(RepRank(comps[0]), RepRank(comps[1]));
					return SafeUnifiedWinRate(new[] { hi - 2, lo - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.ThreeOfKind:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.FullHouse:   // ThreeOfKind + Pair
				case EightCardsBattleHandRank.TownHouse:    // ThreeCardsPairInFlush + Pair
				{
					if (comps.Count < 2) return min;
					var schema = new[]
					{
						new SpaceDef(SpaceType.Cartesian, 13, 1),
						new SpaceDef(SpaceType.Cartesian, 13, 1)
					};
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2, RepRank(comps[1]) - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.ThreeCardsFlushStraight:
				case EightCardsBattleHandRank.FourCardsFlushStraight:
				case EightCardsBattleHandRank.FiveCardsFlushStraight:
				{
					if (comps.Count == 0) return min;
					// 同花順以最大張當基準，4-high 為最小。
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 11, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 4 }, schema, min, max);
				}

				case EightCardsBattleHandRank.Mansion:     // ThreeCardsFlushStraight + Pair
				{
					if (comps.Count < 2) return min;
					var schema = new[]
					{
						new SpaceDef(SpaceType.Cartesian, 11, 1),
						new SpaceDef(SpaceType.Cartesian, 13, 1)
					};
					// comps 已由強到弱排序，同花順為主成分。
					int straightHigh = RepRank(comps[0]);
					int pairRank = RepRank(comps[1]);
					return SafeUnifiedWinRate(new[] { straightHigh - 4, pairRank - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.FiveCardsStraight:
				case EightCardsBattleHandRank.SixCardsStraight:
				case EightCardsBattleHandRank.SevenCardsStraight:
				case EightCardsBattleHandRank.EightCardsStraight:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 11, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 4 }, schema, min, max);
				}

				case EightCardsBattleHandRank.FiveCardsFlush:
				case EightCardsBattleHandRank.SixCardsFlush:
				case EightCardsBattleHandRank.SevenCardsFlush:
				case EightCardsBattleHandRank.EightCardsFlush:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case EightCardsBattleHandRank.FourOfKind:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				default:
				{
					// 其它較罕見的牌型：以區間中點作為穩定的勝率估計值。
					return (min + max) * 0.5;
				}
			}
		}

		/// <summary>
		/// 前墩 + 後墩 的綜合勝率 (加權指引用)。
		/// </summary>
		public float CalcHandWinRate(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand)
		{
			return (float)(GetSubHandWinRate(firstBattleHand) + GetSubHandWinRate(secondBattleHand));
		}

		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps)
		{
			EightCardSubBattleHand bestFirst = null;
			EightCardSubBattleHand bestSecond = null;
			double bestScore = double.NegativeInfinity;

			void Consider(EightCardSubBattleHand a, EightCardSubBattleHand b)
			{
				if (a == null || b == null) return;
				// 確保後墩 (SecondHand) 不弱於前墩 (FirstHand)。
				if (a > b) (a, b) = (b, a);
				double score = GetSubHandWinRate(a) + GetSubHandWinRate(b);
				if (score > bestScore)
				{
					bestScore = score;
					bestFirst = a;
					bestSecond = b;
				}
			}

			// 1. 從 comps 中選 2 個組成一墩，其餘組成另一墩。
			var twoCompPermutations = UtilFunc.GetPermutation(comps, 2);
			foreach (var selectedComps in twoCompPermutations)
			{
				var remainingComps = UtilFunc.GetExcludeList(comps, selectedComps);
				if (remainingComps.Count > 2) continue;

				if (!PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (selectedComps[0].CompRank, selectedComps[1].CompRank), out var pairedRank))
					continue;

				var handA = new EightCardSubBattleHand(BattleHandEnum.SecondHand, pairedRank, selectedComps[0], selectedComps[1]);

				if (remainingComps.Count == 2)
				{
					if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var otherRank))
					{
						var handB = new EightCardSubBattleHand(BattleHandEnum.SecondHand, otherRank, remainingComps[0], remainingComps[1]);
						Consider(handA, handB);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var otherRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var handB = new EightCardSubBattleHand(BattleHandEnum.SecondHand, otherRank, remainingComps[0]);
					Consider(handA, handB);
				}
				else if (remainingComps.Count == 0)
				{
					var handB = new EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing);
					Consider(handA, handB);
				}
			}

			// 2. 從 comps 中選 1 個組成一墩，其餘組成另一墩。
			var oneCompPermutations = UtilFunc.GetPermutation(comps, 1);
			foreach (var selectedComps in oneCompPermutations)
			{
				var selectedComp = selectedComps[0];
				var remainingComps = UtilFunc.GetExcludeList(comps, selectedComps);
				if (remainingComps.Count > 2) continue;

				var aRank = PokerHandStructure.ConvertCompRankToBattleRank(selectedComp.CompRank);
				var handA = new EightCardSubBattleHand(BattleHandEnum.SecondHand, aRank, selectedComp);

				if (remainingComps.Count == 2)
				{
					if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var otherRank))
					{
						var handB = new EightCardSubBattleHand(BattleHandEnum.SecondHand, otherRank, remainingComps[0], remainingComps[1]);
						Consider(handA, handB);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var otherRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var handB = new EightCardSubBattleHand(BattleHandEnum.SecondHand, otherRank, remainingComps[0]);
					Consider(handA, handB);
				}
			}

			if (bestFirst == null || bestSecond == null)
				return (bestFirst, bestSecond);

			// bestFirst 比較時是以 SecondHand 建立的，這裡重建為 FirstHand，
			// 以便後續配發散牌時前墩容量正確 (前墩最多 3 張)。
			// 僅在該牌型對前墩合法 (存在於 FirstHand 牌力表) 時才轉換。
			if (EightCardSubBattleHand.EightCardsBattleHandPowerDict.ContainsKey(
				    (BattleHandEnum.FirstHand, bestFirst.BattleHandRank)))
			{
				bestFirst = new EightCardSubBattleHand(
					BattleHandEnum.FirstHand, bestFirst.BattleHandRank, bestFirst.Components.ToArray());
			}

			return (bestFirst, bestSecond);
		}
	}

	/*
	public class RuleTableStrategy : IBattleHandArrangeStrategy
	{
		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps)
		{
			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;

			if (comps.Count == 3)
			{
				if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (comps[1].CompRank, comps[2].CompRank), out EightCardsBattleHandRank newBattleRank))
				{
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
						PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
					secondEightCardSubBattleHand =
						new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, comps[1], comps[2]);
					if (firstEightCardSubBattleHand > secondEightCardSubBattleHand)
					{
						(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = (secondEightCardSubBattleHand,
							firstEightCardSubBattleHand);
					}
				}
				else
				{
					Console.WriteLine("Fatal error in Battle Hand arrange of strategy.");
				}
			}
			else if (comps.Count == 2)
			{
				firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comps[1].CompRank), comps[1]);
				secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
			}

			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}
	}
	*/
	
}