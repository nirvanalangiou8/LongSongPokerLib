using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GenericPoker.EightCard
{
	
	public enum PokerCardRangeGroup
	{
		Royal = 0b0100,
		MiddleClass = 0b010,
		LowerClass = 0b001,
	}
	
	public class PokerCardComparer : IEqualityComparer<EightCardPokerCard>
	{
		public bool Equals(EightCardPokerCard x, EightCardPokerCard y)
		{
			bool retBool = x.Equals(y);
			if (!retBool) return false;
			return x.DeckID == y.DeckID; 
		}

		public int GetHashCode(EightCardPokerCard obj)
		{
			return 1;
		}
	}
	
	public class EightCardPokerCard :  BasePokerCard  // IEquatable<PokerCard> //
	{
		
		public static readonly Dictionary<PokerCardRangeGroup, (int, int)> MatchCardRangeNumberGroupDict = new Dictionary<PokerCardRangeGroup, (int, int)>
		{
			{ PokerCardRangeGroup.Royal, (10,14) },
			{ PokerCardRangeGroup.MiddleClass, (6,9)},
			{ PokerCardRangeGroup.LowerClass, (1,5) },
		};
    
		
		// Consider how many Joker's to count the maximum number of ModulatorScale.
		// This is majorly used for count the hand power whenever need to compare card to card by using decimal concepts.
		public static readonly int PokerPowerModulatorScale = (PokerConst.AceBigNumber + 1)*PokerHandCalculator.MaxPokerNumber;
		
		
		public virtual bool IsNumberable => true;
		
		//public virtual int BiggerNumber =>  _number == 1 ? (MaxTotalCountInSameSuit+1) : _number;
		
		
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
		
		public static EightCardPokerCard CreateInstance(int id, int number, PokerSuit pokerSuit, int objectID = 0, int deckID = 1)
		{
			var data = number == PokerConst.AceBigNumber ? 
				new AceCard() : new EightCardPokerCard();
		
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
		public static EightCardPokerCard CreateInstance(string pokerCardStr, int objectID = 0, int deckID = 1)
		{
			var (numStr, suitSymbol) = SplitCard(pokerCardStr);
			
			var data = numStr == "A" ? new AceCard() : new EightCardPokerCard();
			
			var number = PokerConst.PokerStringToNumberDict[numStr];
			
			var suit = PokerConst.SymbolToPokerSuit[suitSymbol];
			var id = ((int)suit - 1) * PokerConst.MaxTotalCountInSameSuit + number;
			data.Init(id, number, suit, objectID, deckID);
			data._computePokerRangeGroup();
			return data;
		}
		
		public static EightCardPokerCard CreateInstance(EightCardPokerCard another)
		{
			return CreateInstance(another._cardID, another._number, another._suit, another.ObjectID, deckID: another.DeckID);
		}
		
		
		

		private void _computePokerRangeGroup()
		{
			_pokerRangeGroupBits = 0b0000;
			if (_number == 1) // ace case
			{
				_pokerRangeGroupBits = 0b0101;
				return;
			}
			foreach (PokerCardRangeGroup rangeGroup in Enum.GetValues(typeof(PokerCardRangeGroup)) )
			{
				var lowerRange = MatchCardRangeNumberGroupDict[rangeGroup].Item1;
				var upperRange = MatchCardRangeNumberGroupDict[rangeGroup].Item2;
				if (_number >= lowerRange && _number <= upperRange)
				{
					_pokerRangeGroupBits |= (int)rangeGroup;
					break;
				}
			}
		}
		
	}
}

/*
		public bool CheckInRangeGroup(PokerCardRangeGroup pokerCardRangeGroup)
		{
			return (_pokerRangeGroupBits & (int)pokerCardRangeGroup) != 0;
		}
*/


/*
		protected void Init(int id, int number, PokerSuit suit, int objectID, int deckID)
		{
			_cardID = id;
			this._number = number;
			this._suit = suit;
			_objectID = objectID;
			_deckID = deckID;
			_computePokerRangeGroup();
		}*/
		
/*
// Convert 10♣️ to 10_Club, etc.
public static string CardSymbolToStr(string symbolStr)
{
	var (numStr, suitSymbol) = SplitCard(symbolStr);
	return $"{numStr}_{PokerConst.SymbolToPokerSuit[suitSymbol].ToString()}";
}
*/
		
/*
// Convert 10_Club to 10♣️
public static string CardStrToSymbol(string cardStr)
{
	string[] parts = cardStr.Split('_');
	var numStr = parts[0];
	var suit = (PokerSuit)Enum.Parse(typeof(PokerSuit), parts[1]);

	return $"{numStr}_{PokerConst.PokerSuitToSymbol[suit]}";
}*/

/*
		public virtual int DecideBestFourCardAceNumber(EightCardPokerCard anotherCard)
		{
			if (anotherCard is AceCard)
			{
				var totalPts1 = (Number + 14 ) % 10;
				var totalPts2 = (Number + 1) % 10;
				return Math.Max(totalPts1, totalPts2);
			} else {

				return  (Number + anotherCard.Number) % 10;
			}
		}
*/
