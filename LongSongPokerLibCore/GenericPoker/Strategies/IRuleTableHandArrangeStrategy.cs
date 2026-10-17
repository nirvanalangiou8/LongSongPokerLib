
using System;
using System.Collections.Generic;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public class RuleTableStrategy : IBaseHandArrangeStrategy
	{

		public IReadOnlyList<BaseSubBattleHand> ArrangeComps(
			List<PokerCardComponent> comps,
			List<BasePokerCard>? remainingCards = null,
			ICardRule? rule = null)
		{
			var effectiveRule = rule ?? EightCardRule.Default;
			int firstCount = effectiveRule?.SubHandsCardCount != null &&
			                 effectiveRule.SubHandsCardCount.TryGetValue(BattleSubHandOrder.FirstHand, out var fc) ? fc : 3;
			int secondCount = effectiveRule?.SubHandsCardCount != null &&
			                  effectiveRule.SubHandsCardCount.TryGetValue(BattleSubHandOrder.SecondHand, out var sc) ? sc : 5;

			BaseSubBattleHand firstBattleHand = null;
			BaseSubBattleHand secondBattleHand = null;

			// 1. permutate the current comps starting to select two in comps
			var twoCompPermutations = UtilFunc.GetPermutation(comps, 2);
			foreach (var selectedComps in twoCompPermutations)
			{
				var remainingComps = UtilFunc.GetExcludeList(comps, selectedComps);

				// Tempoarailoiy if any selected combs has more than 2 combs, skip the loop.
				if (remainingComps.Count > 2) continue;

				if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
					    (selectedComps[0].CompRank, selectedComps[1].CompRank), out var firstRank))
				{
					var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, firstRank, firstCount, selectedComps[0], selectedComps[1]);
					
					// Process second hand
					if (remainingComps.Count == 2)
					{
						if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
							    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var secondRank))
						{
							var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, secondCount, remainingComps[0], remainingComps[1]);
							return FinalizeHands(firstHand, secondHand);
						}
					}
					else if (remainingComps.Count == 1)
					{
						var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
						var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, secondCount, remainingComps[0]);
						return FinalizeHands(firstHand, secondHand);
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
				var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, firstRank, firstCount, selectedComp);

				// Process second hand
				if (remainingComps.Count == 2)
				{
					if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var secondRank))
					{
						var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, secondCount, remainingComps[0], remainingComps[1]);
						return FinalizeHands(firstHand, secondHand);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, secondCount, remainingComps[0]);
					return FinalizeHands(firstHand, secondHand);
				}
			}

			return FinalizeHands(firstBattleHand, secondBattleHand);
		}

		private IReadOnlyList<BaseSubBattleHand> FinalizeHands(BaseSubBattleHand? first, BaseSubBattleHand? second)
		{
			var list = new List<BaseSubBattleHand>();
			if (first == null && second == null) return list;
			if (first == null) { list.Add(second!); return list; }
			if (second == null) { list.Add(first); return list; }

			if (first > second)
			{
				list.Add(second);
				list.Add(first);
			}
			else
			{
				list.Add(first);
				list.Add(second);
			}
			return list;
		}
	}
}