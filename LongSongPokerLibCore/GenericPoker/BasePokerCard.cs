using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;
using System.Globalization;

namespace GenericPoker
{
	public class PokerCardComparer : IEqualityComparer<BasePokerCard>
	{
		public bool Equals(BasePokerCard x, BasePokerCard y)
		{
			bool retBool = x.Equals(y);
			if (!retBool) return false;
			return x.DeckID == y.DeckID; 
		}

		public int GetHashCode(BasePokerCard obj)
		{
			return 1;
		}
	}
	
    public class BasePokerCard :  IComparable<BasePokerCard>  // IEquatable<PokerCard> // 
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
		
		// Consider how many Joker's to count the maximum number of ModulatorScale.
		// This is majorly used for count the hand power whenever need to compare card to card by using decimal concepts.
		public static readonly int PokerPowerModulatorScale = (PokerConst.AceBigNumber + 1)*PokerHandCalculator.MaxPokerNumber;

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
		
		public virtual bool IsNumberable => true;
		
		
		// If we have last element of PokerSuit is Wild which has associated 31 value, then below will be 1/32. Using
		// this for PokerCardPower comparison.
		private float PokerCardPokerSuitModulationRatio = 1.0f/(float)(Enum.GetValues(typeof(PokerSuit)).Cast<int>().Last()+1);
		
		// This is for regular suit sort not for pokerHand Comparission.
		// This is for suit sort. Ex A-spade is bigger than K-spade... 2-Spade, then A-heart... the 2-club is smallest
		// In order to tell which is greater between same number but different suit, we use suit as minor weight.
		// Ex: 7-Spade will be the power 7+8*(1/32) = 7+1/4, (Spade suit id is 4), and 7-club is 7+1*0.25 = 7+1/32.
		//public virtual float PokerCardPower => BiggerNumber + (float)_suit * PokerCardPokerSuitModulationRatio;
		public virtual float PokerCardPower => Number + (float)_suit * PokerCardPokerSuitModulationRatio;
		
		// to store poker Range group bits. Ex: Ace is like mini joker, will have 2 bits, (Royal and Lower class bits),
		// other card will only have 1 bit
		private int _pokerRangeGroupBits;
		
		
		protected void Init(int id, int number, PokerSuit suit, int objectID, int deckID)
		{
			_cardID = id;
			this._number = number;
			this._suit = suit;
			_objectID = objectID;
			_deckID = deckID;
		}

		public static BasePokerCard CreateInstance(int id, int number, PokerSuit pokerSuit, int objectID = 0, int deckID = 1)
		{
			// TODO , will enable below remarks later.
			//return null;
			
			var data = number == PokerConst.AceBigNumber ? 
				new AcePokerCard() : new BasePokerCard();
		
			data.Init(id, number, pokerSuit, objectID, deckID);
			return data;
		}
		
		// The input string would be like 10♣️, or A♣️, since we don't know if first number has one or two chars, so
		// we leverage "♣️" has always return "one" for length even these symbol actually occupy two bytes for uni-code.
		private static (string value, string suit) SplitCard(string input)
		{
			// Use StringInfo to safely iterate over grapheme clusters
			var si = new StringInfo(input);
			int totalElements = si.LengthInTextElements;

			// Assume the suit is always the last grapheme cluster
			string suit = si.SubstringByTextElements(totalElements - 1);
			string value = si.SubstringByTextElements(0, totalElements - 1);

			return (value, suit);
		}
		
		// The input pokerCardStr needs to be in the form like 10♣️, or A❤️, etc.
		public static BasePokerCard CreateInstance(string pokerCardStr, int objectID = 0, int deckID = 1)
		{
			// TODO , will enable below remarks later.
			
			
			var (numStr, suitSymbol) = SplitCard(pokerCardStr);
			
			var data = numStr == "A" ? new AcePokerCard() : new BasePokerCard();
			
			var number = PokerConst.PokerStringToNumberDict[numStr];
			
			var suit = PokerConst.SymbolToPokerSuit[suitSymbol];
			var id = ((int)suit - 1) * PokerConst.MaxTotalCountInSameSuit + number;
			data.Init(id, number, suit, objectID, deckID);
			data._computePokerRangeGroup();
			return data; 
		}
		
		public static BasePokerCard CreateInstance(BasePokerCard another)
		{
			return CreateInstance(another._cardID, another._number, another._suit, another.ObjectID, deckID: another.DeckID);
		}

		
		
		// This function is for straight evaluation.
		// A-K is valid and also 2-A is valid. So we need to consider two special cases if we encounter A.
		public bool IsNextNeighborNumber(BasePokerCard nextCard)
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
			
			if (this is AcePokerCard) numberA = PokerConst.AceBigNumber;
			if (other is AcePokerCard) numberB = PokerConst.AceBigNumber;
			
			return (numberA == numberB) && (_suit == other._suit);
		}
		
		public override int GetHashCode()
		{

			int retNumber = Number;
			if (this is AcePokerCard)
			{
				retNumber = PokerConst.AceBigNumber;
			}
			return 20*(int)_suit+retNumber;
		}
		
		private void _computePokerRangeGroup()
		{
			_pokerRangeGroupBits = 0b0000;
			if (_number == 1) // ace case
			{
				_pokerRangeGroupBits = 0b0101;
				return;
			}
			foreach (PokerConst.PokerCardRangeGroup rangeGroup in Enum.GetValues(typeof(PokerConst.PokerCardRangeGroup)) )
			{
				var lowerRange = PokerConst.MatchCardRangeNumberGroupDict[rangeGroup].Item1;
				var upperRange = PokerConst.MatchCardRangeNumberGroupDict[rangeGroup].Item2;
				if (_number >= lowerRange && _number <= upperRange)
				{
					_pokerRangeGroupBits |= (int)rangeGroup;
					break;
				}
			}
		}
	}
}
