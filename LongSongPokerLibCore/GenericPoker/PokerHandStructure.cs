using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GenericPoker.EightCard;
//using Unity.VisualScripting.FullSerializer;

namespace GenericPoker
{
	
	// This class is for analyzed poker structure, to breaking the hand down into small components, such as pair, three of kind, flush, straight, etc..
	// That is this class constitutied the PokerCardCompoenents, and the remaining nothing cards. The atomic/smallest component is a pair.
	public class PokerHandStructure : IComparable<PokerHandStructure>
	{
		public readonly List<PokerCardComponent> Components;
		public List<BasePokerCard> remainingCards;
		
		public string FinalCompsStr = "";


		private IBaseHandArrangeStrategy _strategy;
		
		
		private void Init()
		{
			remainingCards = new List<BasePokerCard>();
		}
		
		
		public PokerHandStructure()
		{
			Components = new List<PokerCardComponent>();
			Init();
		}

		
		public PokerHandStructure(PokerHandStructure other)
		{
			//Components = new List<PokerCardComponent>();
			//Components.AddRange(other.Components);
			Components = new List<PokerCardComponent>(other.Components);
			Init();
		}
		
		public PokerHandStructure(List<PokerCardComponent> components)
		{
			Components = components;
			Init();
		}
		
		
		public void Clear()
		{
			Components.Clear();
			remainingCards.Clear();
		}
		
		public void AddComp(PokerCardComponent newComponent)
		{
			Components.Add(newComponent);
		}
		
		
		public void RemoveLast(int count = 1)
		{
			Components.RemoveRange(Components.Count - count, count);
		}

		
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
		
		
		public void SetRemainingCards(List<BasePokerCard> inputRemaining)
		{
			remainingCards.AddRange(inputRemaining);
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
			return FinalCompsStr?.GetHashCode() ?? 0;
		}
		
		public static PokerOverAllHandRank ConvertCompRankToBattleRank(BaseCompType compType)
		{
			var enumName = compType.ToString(); // Get the name of the enum item as a string
			
			// Try to parse the string into Enum2
			if (Enum.TryParse<PokerOverAllHandRank>(enumName, out PokerOverAllHandRank result))
			{
				return result; // Return the matching Enum2 value
			}
			return PokerOverAllHandRank.Nothing;
		}
		
	}
}