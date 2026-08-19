using GenericPoker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    
    public class PokerCardComparer : IEqualityComparer<SimPokerCard>
    {
        public bool Equals(SimPokerCard x, SimPokerCard y)
        {
            bool retBool = x.Equals(y);
            if (!retBool) return false;
            return x.DeckID == y.DeckID; 
        }

        public int GetHashCode(SimPokerCard obj)
        {
            return 1;
        }
    }
    
    public class SimPokerCard :  BasePokerCard  // IEquatable<PokerCard> //
	{
		/*
		public static readonly Dictionary<PokerCardRangeGroup, (int, int)> MatchCardRangeNumberGroupDict = new Dictionary<PokerCardRangeGroup, (int, int)>
		{
			{ PokerCardRangeGroup.Royal, (10,14) },
			{ PokerCardRangeGroup.MiddleClass, (6,9)},
			{ PokerCardRangeGroup.LowerClass, (1,5) },
		};*/
    
		
		// Consider how many Joker's to count the maximum number of ModulatorScale.
		// This is majorly used for count the hand power whenever need to compare card to card by using decimal concepts.
		public static readonly int PokerPowerModulatorScale = (PokerConst.AceBigNumber + 1)*SimPokerHandCalculator.MaxPokerNumber;
		
		
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
		
		public static SimPokerCard CreateInstance(int id, int number, PokerSuit pokerSuit, int objectID = 0, int deckID = 1)
		{
			var data = number == PokerConst.AceBigNumber ? 
				new AcePokerCard() : new SimPokerCard();
		
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
		public static SimPokerCard CreateInstance(string pokerCardStr, int objectID = 0, int deckID = 1)
		{
			var (numStr, suitSymbol) = SplitCard(pokerCardStr);
			
			var data = numStr == "A" ? new AcePokerCard() : new SimPokerCard();
			
			var number = PokerConst.PokerStringToNumberDict[numStr];
			
			var suit = PokerConst.SymbolToPokerSuit[suitSymbol];
			var id = ((int)suit - 1) * PokerConst.MaxTotalCountInSameSuit + number;
			data.Init(id, number, suit, objectID, deckID);
			data._computePokerRangeGroup();
			return data;
		}
		
		public static SimPokerCard CreateInstance(SimPokerCard another)
		{
			return CreateInstance(another._cardID, another._number, another._suit, another.ObjectID, deckID: another.DeckID);
		}
		
		
		

		private void _computePokerRangeGroup()
		{/*
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
			}*/
		}
		
	}
    /*
    public class SimCardPokerCard : BasePokerCard
    {
        public bool IsNumberable => true;
        public SimCardPokerCard() : base() { }

        public SimCardPokerCard(string cardStr)
        {
            var match = System.Text.RegularExpressions.Regex.Match(cardStr, @"^([2-9]|10|[JQKA]|Joker)(.*)$");
            if (match.Success)
            {
                string numPart = match.Groups[1].Value;
                string suitPart = match.Groups[2].Value;
                int number = numPart == "Joker" ? 0 : PokerConst.PokerStringToNumberDict[numPart];
                PokerSuit suit = PokerConst.SymbolToPokerSuit.TryGetValue(suitPart, out var s) ? s : PokerSuit.NoSuit;
                Init(0, number, suit, 0, 0);
            }
        }

        public static SimCardPokerCard CreateInstance(int id, int number, PokerSuit suit, int objectID, int deckID)
        {
            var card = new SimCardPokerCard();
            card.Init(id, number, suit, objectID, deckID);
            return card;
        }

        public static SimCardPokerCard CreateInstance(BasePokerCard other)
        {
            if (other is IJoker)
            {
                return new SimCardJokerCard(other.CardStr);
            }
            return new SimCardPokerCard(other.CardStr);
        }

        public override bool Equals(object obj)
        {
            if (obj is SimCardPokerCard other)
            {
                return CardStr == other.CardStr;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CardStr.GetHashCode();
        }
    }

    public class SimCardJokerCard : SimCardPokerCard, IJoker
    {
        public new bool IsNumberable => true;
        public int JokerPower => 100;
        public SimCardJokerCard(string cardStr) : base(cardStr) { }
    }

    public class SimCardPokerCardComparer : IEqualityComparer<SimCardPokerCard>
    {
        public bool Equals(SimCardPokerCard x, SimCardPokerCard y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.CardStr == y.CardStr;
        }

        public int GetHashCode(SimCardPokerCard obj)
        {
            return obj.CardStr.GetHashCode();
        }
    }*/
}
