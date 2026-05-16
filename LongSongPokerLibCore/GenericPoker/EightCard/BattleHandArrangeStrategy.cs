

namespace GenericPoker.EightCard
{
	

	public interface IBattleHandArrangeStrategy
	{
		(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeThreeComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp3);

		(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeTwoComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2);
	}

	public class BalancedStrategy : IBattleHandArrangeStrategy
	{
		public (EightCardSubBattleHand, EightCardSubBattleHand ) ArrangeThreeComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp3)
		{

			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;

			if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
				    (comp2.CompRank, comp3.CompRank), out EightCardsBattleHandRank newBattleRank))
			{
				// Q-Pair, 3-Pair, 2-pair 
				// 3-three of kind, Q-pair, 2-Pair (firsthand three of kind larger than second, so need to reverse.
				//var mergedCards = comp2.Cards.Concat(comp3.Cards).ToList();
				// return (newBattleHand, (BattleHand)newBattleHand);
				firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1);
				secondEightCardSubBattleHand =
					new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, comp2, comp3);
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

			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}

		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeTwoComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2)
		{
			//BattleHandEnum.SecondHand, ConvertCompRankToBattleRank(Components[0].CompRank));
			var firstBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
				PokerHandStructure.ConvertCompRankToBattleRank(comp2.CompRank), comp2);
			var secondBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand,
				PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1);
			return (firstBattleHand, secondBattleHand);
		}
	}
	
	
	public class RuleTableStrategy : IBattleHandArrangeStrategy
	{
		public (EightCardSubBattleHand, EightCardSubBattleHand ) ArrangeThreeComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp3)
		{

			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;

			if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
				    (comp2.CompRank, comp3.CompRank), out EightCardsBattleHandRank newBattleRank))
			{
				// Q-Pair, 3-Pair, 2-pair 
				// 3-three of kind, Q-pair, 2-Pair (firsthand three of kind larger than second, so need to reverse.
				//var mergedCards = comp2.Cards.Concat(comp3.Cards).ToList();
				// return (newBattleHand, (BattleHand)newBattleHand);
				firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
					PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1);
				secondEightCardSubBattleHand =
					new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, comp2, comp3);
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

			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}

		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeTwoComps(
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp1,
			PokerCardComponent<EightCardsCompType, EightCardPokerCard> comp2)
		{
			//BattleHandEnum.SecondHand, ConvertCompRankToBattleRank(Components[0].CompRank));
			var firstBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand,
				PokerHandStructure.ConvertCompRankToBattleRank(comp2.CompRank), comp2);
			var secondBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand,
				PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1);
			return (firstBattleHand, secondBattleHand);
		}
	}
	
	
}