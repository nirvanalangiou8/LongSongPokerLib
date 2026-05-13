using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GenericPoker.EightCard;

namespace GenericPoker
{
	
    public struct PokerCardComponent<TEnum> : IComparable<PokerCardComponent<TEnum>> where TEnum : Enum
	    //, IEqualityComparer<PokerCardCombo<TEnum>> 
	{
		public TEnum CompRank;
		private List<EightCardPokerCard> _cards;

		public int CardCount => _cards.Count;
		public string CompString => string.Join("#", _cards.Select(o => o.CardUnitTestStr));
		
		public List<EightCardPokerCard> Cards
		{
			get => _cards;
			set
			{
				_cards = value;
			}
		}
		
		public override bool Equals(object obj)
		{
			if (obj is not PokerCardComponent<TEnum> other)
				return false;
			
			bool rankEqualRes = ((IComparable)CompRank).Equals(other.CompRank);
			if (!rankEqualRes) return false;
			foreach (var (card1, card2) in Cards.Zip(other.Cards, (card1, card2) => (card1, card2)))
			{
				rankEqualRes = card1.Equals(card2);
				if (!rankEqualRes) return rankEqualRes;
			}
			return true;
		}

		public int GetHashCode(PokerCardComponent<TEnum> obj)
		{
			return 1;
		}
		
		
		public int CompareTo(PokerCardComponent<TEnum> other)
		{
			var compareRes = 0;
			
			compareRes = ((IComparable)CompRank).CompareTo(other.CompRank);
			if (compareRes != 0) 
				return compareRes;
			
			// If ComboRank are sames, check their position card power side by side.
			foreach (var (cardA, cardB) in _cards.Zip(other.Cards, (A, B) => (A, B)))
			{
				compareRes = cardA.CompareTo_DontCareSuit(cardB);
				if (compareRes != 0) 
					return compareRes;
			}
			
			// if runs to here, it means all cards' number are equal, then check who has more nature way to form the combo.
			foreach (var (cardA, cardB) in _cards.Zip(other.Cards, (A, B) => (A, B)))
			{
				if (cardA is EightCardJokerCard)
				{
					if (cardB is EightCardJokerCard)
					{
						compareRes = ((EightCardJokerCard)cardA).JokerPower.CompareTo(((EightCardJokerCard)cardB).JokerPower);
						if (compareRes != 0) 
							return compareRes;
					}
					else // A is joker, B is not, so B win. 
					{
						return -1;
					}
				} else if (cardB is EightCardJokerCard) { // A is not joker, and B is, So A win
					return 1;
				} // No need to check A and B are both no jokers, so continue the loop to find next one. 
			}
			
			// If runs to here, it means all equal even with jokers. so return 0 (equal).
			return 0;
		}

		
		// This key is for identify if two combos are identical, so that we can remove duplicated.
		// Usually used in filter out straight and flush for a flushstraight. If not filter out, we will have some duplicates.
		// Ex: Ace-Spade, K-Spade, Q-Spade, can be a flushStraight, flush and straight
		public string CompUniqueKey()
		{
			return string.Join("_", Cards.Select(card => card.CardStr));
		}
	}
}

