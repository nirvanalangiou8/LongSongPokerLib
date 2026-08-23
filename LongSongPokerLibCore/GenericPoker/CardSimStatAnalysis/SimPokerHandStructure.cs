using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimPokerHandStructure : IComparable<SimPokerHandStructure>
    {
        public readonly List<PokerCardComponent<SimCardsCompType, SimPokerCard>> Components;
        public List<SimPokerCard> remainingCards;
        public string FinalCompsStr;

        public SimPokerHandStructure()
        {
            Components = new List<PokerCardComponent<SimCardsCompType, SimPokerCard>>();
            remainingCards = new List<SimPokerCard>();
        }

        public SimPokerHandStructure(SimPokerHandStructure other)
        {
            Components = new List<PokerCardComponent<SimCardsCompType, SimPokerCard>>(other.Components);
            remainingCards = new List<SimPokerCard>(other.remainingCards);
        }

        public void Clear()
        {
            Components.Clear();
            remainingCards.Clear();
        }
        
        public void AddComp(PokerCardComponent<SimCardsCompType, SimPokerCard> newComponent)
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

        public void SetRemainingCards(List<SimPokerCard> inputRemaining)
        {
            remainingCards.AddRange(inputRemaining);
        }
        
        public int CompareTo(SimPokerHandStructure other)
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
            
            if (obj is not SimPokerHandStructure other)
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
