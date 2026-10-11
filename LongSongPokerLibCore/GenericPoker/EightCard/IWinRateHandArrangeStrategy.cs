
namespace GenericPoker.EightCard
{
    
	
	/// <summary>
	/// 勝率加權排牌策略 (Win-Rate Weighted Strategy)
	///
	/// 核心思想 (對應 Program.cs 的 PokerEvaluator 範例)：
	/// 1. 每一個牌型 (PokerOverAllHandRank) 在 stats_result.csv 的累積分布 (CDF) 上佔有一段
	///    機率區間 (min, max)。越強的牌型，其區間越靠近 1。
	/// 2. 在這段區間之內，再依據手牌實際的點位結構，利用 PokerMath.GetUnifiedWinRate 做相對排名內插，
	///    算出這手牌「精準」的勝率落點。
	/// 3. 排牌時，列舉所有合法的 (前墩, 後墩) 拆法，以 前墩勝率 + 後墩勝率 作為加權分數，
	///    取總分最高者作為最佳排列。
	/// </summary>
	public class IWinRateHandArrangeStrategy : IBaseHandArrangeStrategy
	{
	
		// twohands_prob_8cards.csv Front Hand count table (weakest to strongest)
		private static readonly (PokerOverAllHandRank rank, long count)[] FrontStatCountLadder =
		{
			(PokerOverAllHandRank.Nothing,                2626401484L),
			(PokerOverAllHandRank.Pair,                   930370320L),
			(PokerOverAllHandRank.TwoPairs,               21900282L),
			(PokerOverAllHandRank.ThreeOfKind,            8647866L),
			(PokerOverAllHandRank.ThreeCardsFlushStraight,6266009L),
			(PokerOverAllHandRank.FourCardsFlushStraight, 4838L),
			(PokerOverAllHandRank.FourOfKind,             377L),
		};

		// twohands_prob_8cards.csv Back Hand count table (weakest to strongest)
		private static readonly (PokerOverAllHandRank rank, long count)[] BackStatCountLadder =
		{
			(PokerOverAllHandRank.Nothing,                196902345L),
			(PokerOverAllHandRank.Pair,                   1535935713L),
			(PokerOverAllHandRank.TwoPairs,               648950476L),
			(PokerOverAllHandRank.ThreeOfKind,            253128801L),
			(PokerOverAllHandRank.ThreeCardsFlushStraight,226346673L),
			(PokerOverAllHandRank.FiveCardsStraight,      224750900L),
			(PokerOverAllHandRank.FiveCardsFlush,         196836536L),
			(PokerOverAllHandRank.FullHouse,              91254774L),
			(PokerOverAllHandRank.Mansion,                76893581L),
			(PokerOverAllHandRank.SixCardsStraight,       65679045L),
			(PokerOverAllHandRank.SixCardsFlush,          20753672L),
			(PokerOverAllHandRank.SevenCardsStraight,     14354277L),
			(PokerOverAllHandRank.FourCardsFlushStraight, 29661450L),
			(PokerOverAllHandRank.FourOfKind,             6843389L),
			(PokerOverAllHandRank.FiveCardsFlushStraight, 2312679L),
			(PokerOverAllHandRank.EightCardsStraight,     1830960L),
			(PokerOverAllHandRank.SevenCardsFlush,        1062546L),
			(PokerOverAllHandRank.SixCardsFlushStraight,  69858L),
			(PokerOverAllHandRank.EightCardsFlush,        20725L),
			(PokerOverAllHandRank.SevenCardsFlushStraight,2757L),
			(PokerOverAllHandRank.EightCardsFlushStraight,21L),
		};

		// 由上面的次數階梯建立每個牌型在 Front / Back 手牌的 CDF 機率區間 (min, max)。
		private static readonly Dictionary<PokerOverAllHandRank, (double min, double max)> FrontCdfBandDict =
			BuildCdfBands(FrontStatCountLadder);

		private static readonly Dictionary<PokerOverAllHandRank, (double min, double max)> BackCdfBandDict =
			BuildCdfBands(BackStatCountLadder);

		private static Dictionary<PokerOverAllHandRank, (double min, double max)> BuildCdfBands(
			(PokerOverAllHandRank rank, long count)[] ladder)
		{
			double total = 0;
			foreach (var entry in ladder) total += entry.count;
			if (total <= 0) total = 1;

			var dict = new Dictionary<PokerOverAllHandRank, (double, double)>();
			double cum = 0;
			foreach (var entry in ladder)
			{
				double min = cum / total;
				cum += entry.count;
				double max = cum / total;
				if (dict.TryGetValue(entry.rank, out var old))
					dict[entry.rank] = (System.Math.Min(old.Item1, min), System.Math.Max(old.Item2, max));
				else
					dict[entry.rank] = (min, max);
			}
			return dict;
		}

		/// <summary>
		/// 取得某牌型在 Front 或 Back 手牌 CDF 上的機率區間 (min, max)。查不到時退回對應的 Nothing 區間。
		/// </summary>
		private static (double min, double max) GetBand(PokerOverAllHandRank rank, BattleSubHandOrder order = BattleSubHandOrder.SecondHand)
		{
			var dict = order == BattleSubHandOrder.FirstHand ? FrontCdfBandDict : BackCdfBandDict;
			if (dict.TryGetValue(rank, out var band)) return band;
			var otherDict = order == BattleSubHandOrder.FirstHand ? BackCdfBandDict : FrontCdfBandDict;
			if (otherDict.TryGetValue(rank, out var otherBand)) return otherBand;
			return (0.0, dict.TryGetValue(PokerOverAllHandRank.Nothing, out var n) ? n.max : 0.1);
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
		private static int RepRank(PokerCardComponent<BaseCompType, BasePokerCard> comp)
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
		public static double GetSubHandWinRate(BaseSubBattleHand hand, BattleSubHandOrder? order = null)
		{
			if (hand == null) return 0.0;

			var actualOrder = order ?? hand.BattleSubHandOrder;
			var (min, max) = GetBand(hand.BattleHandRank, actualOrder);
			var comps = hand.Components;

			switch (hand.BattleHandRank)
			{
				case PokerOverAllHandRank.Nothing:
				{
					// 純散牌：取最大的 (最多 3 張) 點位做組合內插。排牌階段往往尚無散牌，直接退回區間下限。
					var ranks = hand.Cards.Select(c => c.Number).OrderByDescending(n => n).Take(3).ToList();
					if (ranks.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Combination, 13, ranks.Count) };
					var offsets = ranks.Select(r => r - 2).ToArray();
					return SafeUnifiedWinRate(offsets, schema, min, max);
				}

				case PokerOverAllHandRank.Pair:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case PokerOverAllHandRank.TwoPairs:
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

				case PokerOverAllHandRank.ThreeOfKind:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case PokerOverAllHandRank.FullHouse:   // ThreeOfKind + Pair
				{
					if (comps.Count < 2) return min;
					var schema = new[]
					{
						new SpaceDef(SpaceType.Cartesian, 13, 1),
						new SpaceDef(SpaceType.Cartesian, 13, 1)
					};
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2, RepRank(comps[1]) - 2 }, schema, min, max);
				}

				case PokerOverAllHandRank.ThreeCardsFlushStraight:
				case PokerOverAllHandRank.FourCardsFlushStraight:
				case PokerOverAllHandRank.FiveCardsFlushStraight:
				{
					if (comps.Count == 0) return min;
					// 同花順以最大張當基準，4-high 為最小。
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 11, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 4 }, schema, min, max);
				}

				case PokerOverAllHandRank.Mansion:     // ThreeCardsFlushStraight + Pair
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

				case PokerOverAllHandRank.FiveCardsStraight:
				case PokerOverAllHandRank.SixCardsStraight:
				case PokerOverAllHandRank.SevenCardsStraight:
				case PokerOverAllHandRank.EightCardsStraight:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 11, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 4 }, schema, min, max);
				}

				case PokerOverAllHandRank.FiveCardsFlush:
				case PokerOverAllHandRank.SixCardsFlush:
				case PokerOverAllHandRank.SevenCardsFlush:
				case PokerOverAllHandRank.EightCardsFlush:
				{
					if (comps.Count == 0) return min;
					var schema = new[] { new SpaceDef(SpaceType.Cartesian, 13, 1) };
					return SafeUnifiedWinRate(new[] { RepRank(comps[0]) - 2 }, schema, min, max);
				}

				case PokerOverAllHandRank.FourOfKind:
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
		public float CalcHandWinRate(BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand)
		{
			return (float)(GetSubHandWinRate(firstBattleHand, BattleSubHandOrder.FirstHand) + 
			               GetSubHandWinRate(secondBattleHand, BattleSubHandOrder.SecondHand));
		}

		public (BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<BaseCompType, BasePokerCard>> comps)
		{
			BaseSubBattleHand bestFirst = null;
			BaseSubBattleHand bestSecond = null;
			double bestScore = double.NegativeInfinity;

			void Consider(BaseSubBattleHand a, BaseSubBattleHand b)
			{
				if (a == null || b == null) return;
				// 確保後墩 (SecondHand) 不弱於前墩 (FirstHand)。
				if (a > b) (a, b) = (b, a);
				double score = GetSubHandWinRate(a, BattleSubHandOrder.FirstHand) + 
				               GetSubHandWinRate(b, BattleSubHandOrder.SecondHand);
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

				if (!PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (selectedComps[0].CompRank, selectedComps[1].CompRank), out var pairedRank))
					continue;

				var handA = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, pairedRank, selectedComps[0], selectedComps[1]);

				if (remainingComps.Count == 2)
				{
					if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var otherRank))
					{
						var handB = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, otherRank, remainingComps[0], remainingComps[1]);
						Consider(handA, handB);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var otherRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var handB = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, otherRank, remainingComps[0]);
					Consider(handA, handB);
				}
				else if (remainingComps.Count == 0)
				{
					var handB = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
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
				var handA = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, aRank, selectedComp);

				if (remainingComps.Count == 2)
				{
					if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var otherRank))
					{
						var handB = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, otherRank, remainingComps[0], remainingComps[1]);
						Consider(handA, handB);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var otherRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var handB = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, otherRank, remainingComps[0]);
					Consider(handA, handB);
				}
			}

			if (bestFirst == null || bestSecond == null)
				return (bestFirst, bestSecond);

			// bestFirst 比較時是以 SecondHand 建立的，這裡重建為 FirstHand，
			// 以便後續配發散牌時前墩容量正確 (前墩最多 3 張)。
			// 僅在該牌型對前墩合法 (存在於 FirstHand 牌力表) 時才轉換。
			if (PokerConst.EightCardsBattleHandPowerDict.ContainsKey(
				    (BattleSubHandOrder.FirstHand, bestFirst.BattleHandRank)))
			{
				bestFirst = new BaseSubBattleHand(
					BattleSubHandOrder.FirstHand, bestFirst.BattleHandRank, bestFirst.Components.ToArray());
			}

			return (bestFirst, bestSecond);
		}
	}
}