using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using Unity.VisualScripting.FullSerializer;

namespace GenericPoker.EightCard
{
	
	public interface IBattleHandArrangeStrategy
	{
		(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeThreeComps(PokerCardComponent<EightCardsCompType> comp1, 
			PokerCardComponent<EightCardsCompType> comp2, PokerCardComponent<EightCardsCompType> comp3);
		(EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeTwoComps(PokerCardComponent<EightCardsCompType> comp1, 
			PokerCardComponent<EightCardsCompType> comp2);
	}
	
	public class BalancedStrategy : IBattleHandArrangeStrategy
	{
		public (EightCardSubBattleHand , EightCardSubBattleHand ) ArrangeThreeComps(
			PokerCardComponent<EightCardsCompType> comp1,
			PokerCardComponent<EightCardsCompType> comp2, PokerCardComponent<EightCardsCompType> comp3)
		{

			EightCardSubBattleHand firstEightCardSubBattleHand = null;
			EightCardSubBattleHand secondEightCardSubBattleHand = null;
			
			if (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue((comp2.CompRank, comp3.CompRank), out EightCardsBattleHandRank newBattleRank))
			{
				// Q-Pair, 3-Pair, 2-pair 
				// 3-three of kind, Q-pair, 2-Pair (firsthand three of kind larger than second, so need to reverse.
				//var mergedCards = comp2.Cards.Concat(comp3.Cards).ToList();
				// return (newBattleHand, (BattleHand)newBattleHand);
				firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1); 
				secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, comp2, comp3);
				if (firstEightCardSubBattleHand > secondEightCardSubBattleHand)
				{
					(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = (secondEightCardSubBattleHand, firstEightCardSubBattleHand);	
				}
			}
			else
			{
				Console.WriteLine("Fatal error in Battle Hand arrange of strategy.");
			}
			return (firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}
		
		public (EightCardSubBattleHand firstBattleHand, EightCardSubBattleHand secondBattleHand) ArrangeTwoComps(
			PokerCardComponent<EightCardsCompType> comp1,
			PokerCardComponent<EightCardsCompType> comp2)
		{
			//BattleHandEnum.SecondHand, ConvertCompRankToBattleRank(Components[0].CompRank));
			var firstBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, PokerHandStructure.ConvertCompRankToBattleRank(comp2.CompRank), comp2);
			var secondBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, PokerHandStructure.ConvertCompRankToBattleRank(comp1.CompRank), comp1);	
			return (firstBattleHand, secondBattleHand);
		}
	}
	
	// This class is for analyzed poker structure, to breaking the hand down into small components, such as pair, three of kind, flush, straight, etc..
	public class PokerHandStructure : IComparable<PokerHandStructure>
	{
		public readonly List<PokerCardComponent<EightCardsCompType>> Components;
		public List<EightCardPokerCard> remainingCards;
		
		public string FinalCompsStr = "";

		private List<EightCardSubBattleHand> _battleHands;

		private IBattleHandArrangeStrategy _strategy;
		
		public void SortCompsAndClassify()
		{
			Components.Sort((c1, c2) => c2.CompareTo(c1));
			var compTypeCountsList = Components
				.GroupBy(comp => comp.CompRank) // Group by enum value
				.Select(group => group.Count() <= 1
					? $"{group.Key}"
					: $"{group.Key}*{group.Count()}"); // Convert to string (enum_value_count)

			// Generate the final string by joining the tuple elements with a comma
			FinalCompsStr = string.Join("_", compTypeCountsList);
		}

		public static EightCardsBattleHandRank ConvertCompRankToBattleRank(EightCardsCompType compType)
		{
			var enumName = compType.ToString(); // Get the name of the enum item as a string
			
			// Try to parse the string into Enum2
			if (Enum.TryParse<EightCardsBattleHandRank>(enumName, out EightCardsBattleHandRank result))
			{
				return result; // Return the matching Enum2 value
			}
			return EightCardsBattleHandRank.Nothing;
		}
			
		public EightCardHands ArrangeHands(IBattleHandArrangeStrategy strategy)
		{
			EightCardSubBattleHand firstEightCardSubBattleHand;
			EightCardSubBattleHand secondEightCardSubBattleHand;
			
			var sortedRemainingCards = remainingCards.OrderByDescending(item => item.PokerCardPower).ToList();
			
			switch (Components.Count)
			{
				case 4: // four pairs
					// Always put first and last on second hand, and second and third on the first hand.
					// Process FrontHand two pairs
					//var mergedCards = Components[1].Cards.Concat(Components[2].Cards).ToList();
					var newBattleRank =
						PokerHandCalculator.EightCardsCompComboToBattleRankDict[(EightCardsCompType.Pair, EightCardsCompType.Pair)];
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, newBattleRank, Components[1], Components[2]);
					
					
					// process BackHand two pairs
					//mergedCards = Components[0].Cards.Concat(Components[3].Cards).ToList();
					secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, newBattleRank, Components[0], Components[3]);

					break;
				case 3:
					(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = strategy.ArrangeThreeComps(Components[0], Components[1], Components[2]);
					_battleHands.Add(firstEightCardSubBattleHand);
					_battleHands.Add(secondEightCardSubBattleHand);
					break;
				case 2: 
					(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = strategy.ArrangeTwoComps(Components[0], Components[1]);
					_battleHands.Add(firstEightCardSubBattleHand);
					_battleHands.Add(secondEightCardSubBattleHand);
					break;
				case 1:
					// Add first hand
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing);
					
					// Add second Hand
					secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, ConvertCompRankToBattleRank(Components[0].CompRank), Components[0]);
					break;
				case 0: // Nothing for whole 8 cards
					// Add first hand
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing);
					// Add second Hand
					secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, EightCardsBattleHandRank.Nothing);
					break;
				default:
					firstEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing);
					// Add second Hand
					secondEightCardSubBattleHand = new EightCardSubBattleHand(BattleHandEnum.SecondHand, EightCardsBattleHandRank.Nothing);
					break;
			}

			//var firstCardList = remainingCards.Take(1).ToList();
			// The minor cards assigned is to place the largest card in secondHand first to ensure second is larger than first
			// The fill the firsthand with minor cards until it reach the maximum card for first hand (regularly it's 3)
			// If there are remaining, assign back to second hand.
			var newRemainingCards = secondEightCardSubBattleHand.AddOneMinorCard(sortedRemainingCards);
			newRemainingCards = firstEightCardSubBattleHand.AddMinorCards(newRemainingCards);
			newRemainingCards = secondEightCardSubBattleHand.AddMinorCards(newRemainingCards);

			if (newRemainingCards.Count > 0)
			{
				Console.WriteLine("Fatal errors");
			}	
			
			return new EightCardHands(firstEightCardSubBattleHand, secondEightCardSubBattleHand);
		}
		
		private void Init()
		{
			remainingCards = new List<EightCardPokerCard>();
			_battleHands = new List<EightCardSubBattleHand>();
		}
		public PokerHandStructure()
		{
			Components = new List<PokerCardComponent<EightCardsCompType>>();
			Init();
		}

		public PokerHandStructure(PokerHandStructure other)
		{
			Components = new List<PokerCardComponent<EightCardsCompType>>();
			Components.AddRange(other.Components);
			Init();
		}

		public PokerHandStructure(List<PokerCardComponent<EightCardsCompType>> components)
		{
			Components = components;
			Init();
		}

		public void AddComp(PokerCardComponent<EightCardsCompType> newComponent)
		{
			Components.Add(newComponent);
		}

		public void SetRemainingCards(List<EightCardPokerCard> inputRemaining)
		{
			remainingCards.AddRange(inputRemaining);
		}

		public void RemoveLastComp()
		{
			Components.RemoveAt(Components.Count - 1);
		}

		public void ClearComps()
		{
			Components.Clear();
		}


		[SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH")]
		public int CompareTo(PokerHandStructure other)
		{
			foreach (var (comp1, comp2) in Components.Zip(other.Components, (a, b) => (a, b)))
			{
				var compareRes = comp1.CompareTo(comp2);
				if (compareRes != 0) return compareRes;
			}

			// When comes here, they are all euqal, so compare their Comp counts
			if (Components.Count > other.Components.Count)
			{
				return 1;
			} else {
				if (Components.Count < other.Components.Count)
				{
					return -1;
				}

				return 0;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is not PokerHandStructure other)
				return false;
			if (Components.Count != other.Components.Count)
			{
				return false;
			}

			foreach (var (comp1, comp2) in Components.Zip(other.Components, (a, b) => (a, b)))
			{
				bool equRes = comp1.Equals(comp2);
				if (equRes == false)
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return 1;
		}
		
	}
}




/*
for (var i = 0; i < Components.Count; i++)
{
	var component = Components[i];
	if (component.CardCount >= 4) { // directly set as second hand.
		var newBattleHand = new BattleHand(component.Cards, BattleHandEnum.SecondHand,
			ConvertCompRankToBattleRank(component.CompRank));
		_battleHands.Add(newBattleHand);
	} else if (component.CardCount <= 3) {
		// try to find sub set to achieve combo, like Pair + Pair or ThreeCardsFlushStraight + Pair
		for (var j = i + 1; j < component.CardCount; j++) {
			var nextComp = Components[j];
			if (component.CardCount + nextComp.CardCount <= 5) {

			}
		}
	} else {

	}
}*/


/*
		private static readonly Dictionary<(EightCardsCompType, EightCardsCompType), EightCardsBattleHandRank> EightCardsCompComboToBattleRankDict =
			new()
			{
				{ (EightCardsCompType.Pair, EightCardsCompType.Pair ), EightCardsBattleHandRank.TwoPairs},
				{ (EightCardsCompType.ThreeCardsPairInFlush, EightCardsCompType.Pair ), EightCardsBattleHandRank.TownHouse},
				{ (EightCardsCompType.ThreeOfKind, EightCardsCompType.Pair ), EightCardsBattleHandRank.FullHouse},
				{ (EightCardsCompType.ThreeCardsFlushStraight, EightCardsCompType.Pair ), EightCardsBattleHandRank.Mansion},
			};*/
