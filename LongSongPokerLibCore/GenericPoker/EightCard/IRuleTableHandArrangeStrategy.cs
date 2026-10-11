
namespace GenericPoker.EightCard
{
    public class RuleTableStrategy : IBaseHandArrangeStrategy
	{

		public (BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent<BaseCompType, BasePokerCard>> comps)
		{
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
					var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, firstRank, selectedComps[0], selectedComps[1]);
					
					// Process second hand
					if (remainingComps.Count == 2)
					{
						if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
							    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var secondRank))
						{
							var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, remainingComps[0], remainingComps[1]);
							(firstBattleHand, secondBattleHand) = FinalizeHands(firstHand, secondHand);
							return (firstBattleHand, secondBattleHand);
						}
					}
					else if (remainingComps.Count == 1)
					{
						var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
						var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, remainingComps[0]);
						(firstBattleHand, secondBattleHand) = FinalizeHands(firstHand, secondHand);
						return (firstBattleHand, secondBattleHand);
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
				var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, firstRank, selectedComp);

				// Process second hand
				if (remainingComps.Count == 2)
				{
					if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
						    (remainingComps[0].CompRank, remainingComps[1].CompRank), out var secondRank))
					{
						var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, remainingComps[0], remainingComps[1]);
						(firstBattleHand, secondBattleHand) = FinalizeHands(firstHand, secondHand);
						return (firstBattleHand, secondBattleHand);
					}
				}
				else if (remainingComps.Count == 1)
				{
					var secondRank = PokerHandStructure.ConvertCompRankToBattleRank(remainingComps[0].CompRank);
					var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, secondRank, remainingComps[0]);
					(firstBattleHand, secondBattleHand) = FinalizeHands(firstHand, secondHand);
					return (firstBattleHand, secondBattleHand);
				}
			}

			return (firstBattleHand, secondBattleHand);
		}

		private (BaseSubBattleHand, BaseSubBattleHand) FinalizeHands(BaseSubBattleHand first, BaseSubBattleHand second)
		{
			if (first > second)
			{
				return (second, first);
			}
			return (first, second);
		}
	}
}