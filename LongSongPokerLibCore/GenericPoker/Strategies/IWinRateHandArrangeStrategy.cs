
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
		/// <summary>
		/// 取得某牌型在對應手牌 CDF 上的機率區間 (min, max)。查不到時退回對應的 Nothing 區間。
		/// 支援依據傳入的 cardrule (1st, 2nd, 3rd 等墩位) 查表。
		/// </summary>
		private static (double min, double max) GetBand(ICardRule? rule, PokerOverAllHandRank rank, BattleSubHandOrder order = BattleSubHandOrder.SecondHand)
		{
			var effectiveRule = rule ?? EightCardRule.Default;
			var bands = effectiveRule.WinRateCdfBands;
			int orderIdx = (int)order;

			Dictionary<PokerOverAllHandRank, (double min, double max)>? targetDict = null;
			if (bands != null && orderIdx >= 0 && orderIdx < bands.Count)
			{
				targetDict = bands[orderIdx];
			}

			if (targetDict != null && targetDict.TryGetValue(rank, out var band))
			{
				return band;
			}


			return (0.0, (targetDict != null && targetDict.TryGetValue(PokerOverAllHandRank.Nothing, out var n)) ? n.max : 0.1);
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
		public static double GetSubHandWinRate(BaseSubBattleHand hand, BattleSubHandOrder? order = null, ICardRule? rule = null)
		{
			if (hand == null) return 0.0;

			var effectiveRule = rule ?? EightCardRule.Default;
			var actualOrder = order ?? hand.BattleSubHandOrder;
			var (min, max) = GetBand(effectiveRule, hand.BattleHandRank, actualOrder);
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
			
			/*// 1. Search the starting point to distribute:
			// Plan an array to store the weighting. The first hand adds 0 weight, the second hand adds 1 weight, the third hand adds 2 weight, etc.
			// Then if the sub hand has rank >= pair, add one extra weight.
			// Find the largest weight point slot (subhand) to start with.*/
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

		
		public IReadOnlyList<BaseSubBattleHand> ArrangeComps(
			List<PokerCardComponent> comps,
			List<BasePokerCard>? remainingCards = null,
			ICardRule? rule = null)
		{
			var effectiveRule = rule ?? EightCardRule.Default;
			var slots = effectiveRule.SubHandSlots;

			var sortedRemainingCards = remainingCards != null
				? remainingCards.OrderByDescending(c => c.PokerCardPower).ToList()
				: new List<BasePokerCard>();

			if (comps == null || comps.Count == 0)
			{
				var fallbackHands = slots.Select(s => new BaseSubBattleHand(s.Order, PokerOverAllHandRank.Nothing, s.CardCount)).ToList();
				DistributeKickers(fallbackHands, sortedRemainingCards);
				for (int i = 0; i < fallbackHands.Count; i++)
				{
					fallbackHands[i].winScore = (float)GetSubHandWinRate(fallbackHands[i], fallbackHands[i].BattleSubHandOrder, effectiveRule);
				}
				return fallbackHands;
			}

			List<BaseSubBattleHand>? bestSubHands = null;
			double bestScore = double.NegativeInfinity;

			var allPartitions = UtilFunc.GenerateSlotPartitions(comps, slots.Count);

			allPartitions = allPartitions.Where(partition =>
			{
				bool hasSeenNonZero = false;
				for (int i = 0; i < partition.Count; i++)
				{
					if (partition[i].Count > 0)
					{
						hasSeenNonZero = true;
					}
					// when come to here, it implied this partition count is zero, and if previous see none-zero,
					// got the invalid case so filter it out by set it false
					else if (hasSeenNonZero)
					{
						return false;
					}
				}
				return true;
			}).ToList();
			
			

			foreach (var partition in allPartitions)
			{
				bool validRanks = true;
				var subHandsCandidate = new List<BaseSubBattleHand>();

				for (int i = 0; i < slots.Count; i++)
				{
					var slot = slots[i];
					var slotComps = partition[i];

					var rank = effectiveRule.AssembleHandRank(slotComps, slot.Order);
					if (rank == PokerOverAllHandRank.None)
					{
						validRanks = false;
						break;
					}

					var hand = new BaseSubBattleHand(slot.Order, rank, slot.CardCount, slotComps.ToArray());
					subHandsCandidate.Add(hand);
				}

				if (!validRanks) continue;

				DistributeKickers(subHandsCandidate, sortedRemainingCards);

				// Deployment ordering check: subHands[0] <= subHands[1] <= ... <= subHands[N-1]
				bool validDeployment = true;
				for (int i = 0; i < subHandsCandidate.Count - 1; i++)
				{
					if (subHandsCandidate[i] > subHandsCandidate[i + 1])
					{
						validDeployment = false;
						break;
					}
				}

				if (!validDeployment) continue;

				double score = 0;
				for (int i = 0; i < subHandsCandidate.Count; i++)
				{
					double subScore = GetSubHandWinRate(subHandsCandidate[i], subHandsCandidate[i].BattleSubHandOrder, effectiveRule);
					subHandsCandidate[i].winScore = (float)subScore;
					score += subScore;
				}

				if (score > bestScore)
				{
					bestScore = score;
					bestSubHands = subHandsCandidate;
				}
				else if (Math.Abs(score - bestScore) < 1e-9 && bestSubHands != null)
				{
					var currentBattle = new BaseBattleHands(subHandsCandidate);
					var bestBattle = new BaseBattleHands(bestSubHands);
					if (currentBattle > bestBattle)
					{
						bestScore = score;
						bestSubHands = subHandsCandidate;
					}
				}
			}

			if (bestSubHands == null)
			{
				bestSubHands = slots.Select(s => new BaseSubBattleHand(s.Order, PokerOverAllHandRank.Nothing, s.CardCount)).ToList();
				DistributeKickers(bestSubHands, sortedRemainingCards);
				for (int i = 0; i < bestSubHands.Count; i++)
				{
					bestSubHands[i].winScore = (float)GetSubHandWinRate(bestSubHands[i], bestSubHands[i].BattleSubHandOrder, effectiveRule);
				}
			}

			return bestSubHands;
		}
	}
}


