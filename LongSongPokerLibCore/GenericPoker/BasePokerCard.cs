using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.FourCard;
using GenericPoker.EightCard;

namespace GenericPoker
{
	
	public enum PokerSuit
	{
		NoSuit = 0, // Some joker can not be replaced as suit, but straight, so set this extra options.
		Club = 1,
		Diamond = 2,
		Heart = 4,
		Spade = 8, 
		Star = 15,
		Wild = 31,
	}
	//🂿 ♣ ♠️♠️♣️ ❤️, 🃏⭐ ♠️, 🔶 ♣️ ✖️
	
	
	public static class PokerConst
	{
		public const int MaxTotalCountInSameSuit = 13;
		public const int TotalRegularSuitCount = 4;
		public const int TotalRegularPokerCardsWithoutJokers = MaxTotalCountInSameSuit * TotalRegularSuitCount;
		public const int AceBigNumber = PokerConst.MaxTotalCountInSameSuit + 1;
		
		public static readonly Dictionary<int, string> PokerNumberNameDict = new Dictionary<int, string> {
			{ 1, "A" }, { 2, "2" }, { 3, "3" }, { 4, "4" }, { 5, "5" }, { 6, "6" },
			{ 7, "7" }, { 8, "8" }, { 9, "9" }, { 10, "10" }, { 11, "J" }, { 12, "Q" }, { 13, "K" }, {14, "A"}, {15, "Joker"}, {16, "Joker"}, {17, "Joker"}, {18, "Joker"},};
    
		public static readonly Dictionary<string, int> PokerStringToNumberDict = new Dictionary<string, int> {
			{"A", 14 }, {"2", 2}, {"3", 3}, {"4", 4}, {"5", 5}, {"6", 6 }, {"7", 7}, {"8", 8}, {"9", 9}, 
			{"10", 10}, {"J", 11}, {"Q", 12}, {"K", 13}, {"Joker", 15}};
		
		public static readonly Dictionary<PokerSuit, string> PokerSuitToSymbol = new Dictionary<PokerSuit, string> {
			{PokerSuit.NoSuit , "✖️" }, { PokerSuit.Club, "♣️" }, { PokerSuit.Diamond, "🔶" }, { PokerSuit.Heart, "❤️" }, 
			{ PokerSuit.Spade, "♠️" }, { PokerSuit.Star, "⭐" }, { PokerSuit.Wild, "🃏" },
		};
		
		public static readonly Dictionary<string, PokerSuit> SymbolToPokerSuit = new Dictionary<string, PokerSuit> {
			{"✖️", PokerSuit.NoSuit}, {"♣️", PokerSuit.Club}, {"🔶", PokerSuit.Diamond }, {"❤️", PokerSuit.Heart}, 
			{"♠️", PokerSuit.Spade}, {"⭐",  PokerSuit.Star}, {"🃏", PokerSuit.Wild},
		};
	}
	/*
	static class GlobalPokerDeckInfo
	{
		public static int totalPokerNumber = 13;
		public static int totalPokerSuit = 4;
		public static int totalTotalPokerCards = totalPokerNumber * totalPokerSuit;
	}*/
	
	
    public class BasePokerCard :  IComparable<BasePokerCard> // IEquatable<PokerCard> // 
	{
		// This cardID will be unique ID to differentiated between cards?
		protected int _cardID;
		
		// This objectID can be used for sibling ID or any other scenario to borrow this variable, so that keep the mapping
		// Between this pokerCard and associated to Unity Game Object.
		protected int _objectID;
		
		// If blend multi poker card, then this card's _deckID can provide information for which deck belongs to originally.
		protected int _deckID;
		
		protected int _number;
		protected PokerSuit _suit;
		
		public const int RegularSuitClubIndex = 1;
		public const int RegularSuitSpadeIndex = 4;
		
		public PokerSuit Suit => _suit;
		public virtual int Number => _number;
		
		protected virtual string NumberStr => PokerConst.PokerNumberNameDict[_number];
		protected virtual string SuitStr => PokerConst.PokerSuitToSymbol[_suit];
		
		// Will normally return like "6❤️"
		public virtual string CardStr => $"{NumberStr}{SuitStr}";
		
		public virtual string CardStrNumOnly => $"{NumberStr}";
		
		// This string is used for unit test for unit test inspection, Joker will override this.
		// For the regular poker card, it's as same as CardStr.
		public virtual string CardUnitTestStr => CardStr;

		public int ObjectID
		{
			get { return _objectID; } // Getter: retrieves the value of _objectID
			set { _objectID = value; } // Setter: sets the value of _objectID
		}
		
		public int DeckID
		{
			get { return _deckID; } // Getter: retrieves the value of _objectID
			set { _deckID = value; } // Setter: sets the value of _objectID
		}
		
		protected void Init(int id, int number, PokerSuit suit, int objectID, int deckID)
		{
			_cardID = id;
			this._number = number;
			this._suit = suit;
			_objectID = objectID;
			_deckID = deckID;
		}

		
		public virtual bool MatchSuit(PokerSuit inputSuit)
		{
			return ((int)_suit & (int)inputSuit) != 0;
		}

		//public virtual bool IsNumberable => true;
		
		
		// If we have last element of PokerSuit is Wild which has associated 31 value, then below will be 1/32. Using
		// this for PokerCardPower comparison.
		private float PokerCardPokerSuitModulationRatio = 1.0f/(float)(Enum.GetValues(typeof(PokerSuit)).Cast<int>().Last()+1);
		
		// This is for regular suit sort not for pokerHand Comparission.
		// This is for suit sort. Ex A-spade is bigger than K-spade... 2-Spade, then A-heart... the 2-club is smallest
		// In order to tell which is greater between same number but different suit, we use suit as minor weight.
		// Ex: 7-Spade will be the power 7+8*(1/32) = 7+1/4, (Spade suit id is 4), and 7-club is 7+1*0.25 = 7+1/32.
		//public virtual float PokerCardPower => BiggerNumber + (float)_suit * PokerCardPokerSuitModulationRatio;
		public virtual float PokerCardPower => Number + (float)_suit * PokerCardPokerSuitModulationRatio;
		
		
		// This function is for straight evaluation.
		// A-K is valid and also 2-A is valid. So we need to consider two special cases if we encounter A.
		public bool IsNextNeighborNumber(EightCardPokerCard nextCard)
		{
			int leftNumber = (Number == 1) ? PokerConst.AceBigNumber : Number;
			int rightNumber = nextCard.Number; // If rightNumber is Ace, it's just represent as "1"
			return leftNumber - rightNumber == 1;
		}
		
		public static bool operator == (BasePokerCard c1, BasePokerCard c2)
		{
			return ((c1._number == c2._number) && (c1._suit == c2._suit));
		}
		
		public static bool operator != (BasePokerCard c1, BasePokerCard c2)
		{
			return ((c1._number != c2._number) || (c1._suit != c2._suit));
		}
	
		public static bool operator  > (BasePokerCard c1, BasePokerCard c2)
		{
			if (c1._number > c2._number) {
				return true;
			} else if (c1._number < c2._number) {
				return false;
			} else {
				return (c1._suit > c2._suit);
			}
		}
	
		public static bool operator  >= (BasePokerCard c1, BasePokerCard c2)
		{
			return ((c1 > c2) || (c1 == c2));
		}
	
		public static bool operator  < (BasePokerCard c1, BasePokerCard c2)
		{
			if (c1._number < c2._number) {
				return true;
			} else if (c1._number > c2._number) {
				return false;
			} else {
				return (c1._suit < c2._suit);
			}
		}
	
		public static bool operator  <= (BasePokerCard c1, BasePokerCard c2)
		{
			return ((c1 < c2) || (c1 == c2));
		}
	
		public int CompareTo(BasePokerCard b)
		{
			if (this > b) {
				return 1;
			} else if (this == b) {
				return 0;
			} else {
				return -1;
			}
		}  
	
		public int CompareTo_DontCareSuit(BasePokerCard b)
		{
			if (this.Number > b.Number) {
				return 1;
			} else if (this.Number == b.Number) {
				return 0;
			} else {
				return -1;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is not BasePokerCard other)
				return false;
			//bool ret;
			var numberA = Number;
			var numberB = other.Number;
			
			if (this is AceCard) numberA = PokerConst.AceBigNumber;
			if (other is AceCard) numberB = PokerConst.AceBigNumber;
			
			return (numberA == numberB) && (_suit == other._suit);
		}
		
		public override int GetHashCode()
		{

			int retNumber = Number;
			if (this is AceCard)
			{
				retNumber = PokerConst.AceBigNumber;
			}
			return 20*(int)_suit+retNumber;
		}
	}
}
