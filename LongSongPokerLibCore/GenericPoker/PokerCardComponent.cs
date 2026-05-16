using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GenericPoker.EightCard;

namespace GenericPoker
{
	
    public struct PokerCardComponent<TEnum, TCard> : IComparable<PokerCardComponent<TEnum, TCard>> 
        where TEnum : Enum
        where TCard : BasePokerCard
	{
		public TEnum CompRank;
		private List<TCard> _cards;

		public int CardCount => _cards.Count;
		public string CompString => string.Join("#", _cards.Select(o => o.CardUnitTestStr));
		
		public List<TCard> Cards
		{
			get => _cards;
			set
			{
				_cards = value;
			}
		}
		
		public override bool Equals(object obj)
		{
			if (obj is not PokerCardComponent<TEnum, TCard> other)
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

/*
		public int GetHashCode(PokerCardComponent<TEnum, TCard> obj)
		{
			return 1;
		}
*/
		
		
		public int CompareTo(PokerCardComponent<TEnum, TCard> other)
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
				bool isAJoker = cardA is IJoker;
				bool isBJoker = cardB is IJoker;
				
				if (isAJoker && isBJoker)
				{
					compareRes = ((IJoker)cardA).JokerPower.CompareTo(((IJoker)cardB).JokerPower);
					if (compareRes != 0) 
						return compareRes;
				}
				else if (isAJoker) // A is joker, B is not, so B win (A is smaller, nature way win)
				{
					return -1;
				}
				else if (isBJoker) // B is joker, A is not, so A win (A is larger)
				{
					return 1;
				}
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

