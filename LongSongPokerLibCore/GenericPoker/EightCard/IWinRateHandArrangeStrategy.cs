
using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

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
		private static int RepRank(PokerCardComponent comp)
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

		
		public static void DistributeKickers(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand, List<BasePokerCard> sortedKickers)
		{
			if (firstHand == null || secondHand == null) return;
			DistributeKickers(new[] { firstHand, secondHand }, sortedKickers);
		}
		

		public static void DistributeKickers(IList<BaseSubBattleHand> subHands, List<BasePokerCard> sortedKickers)
		{
			if (subHands == null || subHands.Count == 0) return;
			if (sortedKickers == null || sortedKickers.Count == 0) return;

			var kickers = new List<BasePokerCard>(sortedKickers);

			int currentFindIndex = 0;
			int lastIndex = subHands.Count - 1;
			while (currentFindIndex < lastIndex)
			{
				var current = subHands[currentFindIndex];
				var next = subHands[currentFindIndex + 1];
				

				// 2. 條件 A：前後兩手皆為 Nothing（散牌狀況，大 kicker 必須往後給）
				bool bothNothing = current.BattleHandRank == PokerOverAllHandRank.Nothing 
				                   && next.BattleHandRank == PokerOverAllHandRank.Nothing;

				// 3. 條件 B：前後兩手強度完全平手（使用 class comparator 比對）
				// 若使用 CompareTo: current.CompareTo(next) == 0
				bool areEqual = current == next; 

				// 若滿足「皆為 Nothing」或「強度完全平手」，代表當前手無法承受 kicker，往下一手推進
				if (bothNothing || areEqual)
				{
					currentFindIndex++;
				}
				else
				{
					// 後手已嚴格大於前手 (next > current)，當前手有保護傘，可安全作為起始墊牌點
					break;
				}
			}

			int startIndex = currentFindIndex;
		
			
			
			/*// 1. Search the starting point to distribute:
			// Plan an array to store the weighting. The first hand adds 0 weight, the second hand adds 1 weight, the third hand adds 2 weight, etc.
			// Then if the sub hand has rank >= pair, add one extra weight.
			// Find the largest weight point slot (subhand) to start with.*/
			
			/*
			int startIndex = Enumerable.Range(0, subHands.Count)
				.MaxBy(i =>
				{
					
					// 牌型 > Pair 歸零 (0)；其餘牌型賦予 1-based 位置權重 (i + 1)
					int weight = subHands[i].BattleHandRank >= PokerOverAllHandRank.Pair ? -i : i ;

					// 回傳 (Weight, Index)：確保相同權重時結果唯一（由較大的 index 勝出）
					return (Weight: weight, Index: i);
					
				});*/
			

			int moveStep = -1;
			bool firstTimeDistribute = true;
			bool hitFirstHandDeadEnd = false;
			int currentIndex = startIndex;
			while (kickers.Count > 0)
			{
				if (hitFirstHandDeadEnd || currentIndex == 0)
				{
					int numAdd = subHands[currentIndex].AddMinorCards(kickers);
					
					// distributed as many as possible
					if (currentIndex == 0)
					{
						hitFirstHandDeadEnd = true;
						moveStep = 1;
					}
				}
				else
				{
					int numAdd = subHands[currentIndex].AddMinorCards(kickers, 1);
				}

				currentIndex += moveStep;
			}
			
			
		}

		/*public static void DistributeKickers_old(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand, List<BasePokerCard> sortedKickers)
		 
		 int GetTargetCapacity(int idx)
			{
				var h = subHands[idx];
				if (h == null) return 0;
				if ((int)h.BattleSubHandOrder > 0) return (int)h.BattleSubHandOrder;
				return idx == 0 ? 3 : 5;
			}

			void PlaceOneCard(int idx)
			{
				if (kickers.Count == 0) return;
				var h = subHands[idx];
				if (h == null) return;
				int cap = GetTargetCapacity(idx);
				if (h.Cards.Count < cap)
				{
					h.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}
			}

			void FillHandUntilFull(int idx)
			{
				var h = subHands[idx];
				if (h == null) return;
				int cap = GetTargetCapacity(idx);
				while (kickers.Count > 0 && h.Cards.Count < cap)
				{
					h.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}
			}

			// 2. When choose start point, always place one card from distributed pool to the subhand,
			// but if the subhand is the first hand, always fill the card until that subhand is full for designated hand count.
			// 3. Then move to first hand direction, and if it hit the dead end of first hand, then bounce back and make the moving direction in opposite way.
			// 4. If ever hit the deadend when travel to distributed, after that always fill/distributed hand until its full.
			if (startIndex == 0)
			{
				FillHandUntilFull(0);
				for (int i = 1; i < subHands.Count && kickers.Count > 0; i++)
				{
					FillHandUntilFull(i);
				}
			}
			else
			{
				PlaceOneCard(startIndex);

				for (int i = startIndex - 1; i > 0 && kickers.Count > 0; i--)
				{
					PlaceOneCard(i);
				}

				if (kickers.Count > 0)
				{
					FillHandUntilFull(0);
				}

				for (int i = 1; i < subHands.Count && kickers.Count > 0; i++)
				{
					FillHandUntilFull(i);
				}
			}
		 
		{
			if (firstHand == null || secondHand == null) return;
			if (sortedKickers == null || sortedKickers.Count == 0) return;

			var kickers = new List<BasePokerCard>(sortedKickers);

			if (secondHand > firstHand)
			{
				while (kickers.Count > 0 && firstHand.Cards.Count < 3)
				{
					firstHand.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}

				while (kickers.Count > 0 && secondHand.Cards.Count < 5)
				{
					secondHand.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}
			}
			else
			{
				if (secondHand.Cards.Count < 5 && kickers.Count > 0)
				{
					secondHand.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}

				while (kickers.Count > 0 && firstHand.Cards.Count < 3)
				{
					firstHand.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}

				while (kickers.Count > 0 && secondHand.Cards.Count < 5)
				{
					secondHand.Cards.Add(kickers[0]);
					kickers.RemoveAt(0);
				}
			}
		}*/
		
		
		public (BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent> comps,
			List<BasePokerCard>? remainingCards = null,
			ICardRule? rule = null)
		{
			var effectiveRule = rule ?? EightCardRule.Default;
			var sortedRemainingCards = remainingCards != null
				? remainingCards.OrderByDescending(c => c.PokerCardPower).ToList()
				: new List<BasePokerCard>();

			BaseSubBattleHand bestFirst = null;
			BaseSubBattleHand bestSecond = null;
			double bestScore = double.NegativeInfinity;

			if (comps == null || comps.Count == 0)
			{
				var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
				var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing);
				DistributeKickers(firstHand, secondHand, sortedRemainingCards);
				return (firstHand, secondHand);
			}

			int maxFrontCount = comps.Count;
			for (int selectCount = 0; selectCount <= maxFrontCount; selectCount++)
			{
				var possibleGroups = UtilFunc.GetPermutationAllowedDuplicated(comps, selectCount);
				foreach (var group in possibleGroups)
				{
					var frontGroup = group.Selected;
					var backGroup = group.Remaining;

					var frontRank = effectiveRule.AssembleHandRank(frontGroup);
					var backRank = effectiveRule.AssembleHandRank(backGroup);

					// If either rank is None, it is an invalid split; skip it.
					if (frontRank == PokerOverAllHandRank.None || backRank == PokerOverAllHandRank.None)
						continue;

					// Check if ranks are supported in power dictionary
					if (!PokerConst.EightCardsBattleHandPowerDict.ContainsKey((BattleSubHandOrder.FirstHand, frontRank)) ||
					    !PokerConst.EightCardsBattleHandPowerDict.ContainsKey((BattleSubHandOrder.SecondHand, backRank)))
						continue;

					var handA = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, frontRank, frontGroup.ToArray());
					var handB = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, backRank, backGroup.ToArray());

					DistributeKickers(handA, handB, sortedRemainingCards);

					// Front hand cannot be stronger than back hand
					if (handA > handB)
						continue;

					double score = GetSubHandWinRate(handA, BattleSubHandOrder.FirstHand) +
					               GetSubHandWinRate(handB, BattleSubHandOrder.SecondHand);

					if (score > bestScore)
					{
						bestScore = score;
						bestFirst = handA;
						bestSecond = handB;
					}
					else if (Math.Abs(score - bestScore) < 1e-9 && bestFirst != null && bestSecond != null)
					{
						var currentBattle = new BaseBattleHands(handA, handB);
						var bestBattle = new BaseBattleHands(bestFirst, bestSecond);
						if (currentBattle > bestBattle)
						{
							bestScore = score;
							bestFirst = handA;
							bestSecond = handB;
						}
					}
				}
			}

			if (bestFirst == null || bestSecond == null)
			{
				bestFirst = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
				bestSecond = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing);
				DistributeKickers(bestFirst, bestSecond, sortedRemainingCards);
			}

			return (bestFirst, bestSecond);
		}
	}
}