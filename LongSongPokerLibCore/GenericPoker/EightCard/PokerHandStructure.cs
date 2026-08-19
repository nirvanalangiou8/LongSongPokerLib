using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using Unity.VisualScripting.FullSerializer;

namespace GenericPoker.EightCard
{
	
	// This class is for analyzed poker structure, to breaking the hand down into small components, such as pair, three of kind, flush, straight, etc..
	// That is this class constitutied the PokerCardCompoenents, and the remaining nothing cards. The atomic/smallest component is a pair.
	public class PokerHandStructure : IComparable<PokerHandStructure>
	{
		public readonly List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> Components;
		public List<EightCardPokerCard> remainingCards;
		
		public string FinalCompsStr = "";

		//private List<EightCardSubBattleHand> _battleHands;

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
				case 2:
					(firstEightCardSubBattleHand, secondEightCardSubBattleHand) = strategy.ArrangeComps(Components);

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
			//_battleHands = new List<EightCardSubBattleHand>();
		}
		public PokerHandStructure()
		{
			Components = new List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>();
			Init();
		}

		public PokerHandStructure(PokerHandStructure other)
		{
			Components = new List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>();
			Components.AddRange(other.Components);
			Init();
		}

		public PokerHandStructure(List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> components)
		{
			Components = components;
			Init();
		}

		public void AddComp(PokerCardComponent<EightCardsCompType, EightCardPokerCard> newComponent)
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

/*
		public void ClearComps()
		{
			Components.Clear();
		}
*/


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
			return FinalCompsStr?.GetHashCode() ?? 0;
		}
		
	}
}