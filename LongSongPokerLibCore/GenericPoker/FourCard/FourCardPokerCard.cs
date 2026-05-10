using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace GenericPoker.FourCard
{
    public class FourCardPokerCard : BasePokerCard
    {
	    
		public virtual bool IsNumberable => true;
		
		public virtual bool IsJoker => false;
		
		// If we have last element of PokerSuit is Wild which has associated 31 value, then below will be 1/32. Using
		// this for PokerCardPower comparison.
		private float PokerCardPokerSuitModulationRatio = 1.0f/(float)(Enum.GetValues(typeof(PokerSuit)).Cast<int>().Last()+1);
		
		// This is for regular suit sort not for pokerHand Comparission.
		// This is for suit sort. Ex A-spade is bigger than K-spade... 2-Spade, then A-heart... the 2-club is smallest
		// In order to tell which is greater between same number but different suit, we use suit as minor weight.
		// Ex: 7-Spade will be the power 7+8*(1/32) = 7+1/4, (Spade suit id is 4), and 7-club is 7+1*0.25 = 7+1/32.
		//public virtual float PokerCardPower => BiggerNumber + (float)_suit * PokerCardPokerSuitModulationRatio;
		public virtual float PokerCardPower => Number + (float)_suit * PokerCardPokerSuitModulationRatio;

		public virtual bool IsPair(FourCardPokerCard anotherCard)
		{
			return Number == anotherCard.Number;
		}
		
		public int DecideBestPts(BasePokerCard anotherCard)
		{
			var largestTotalPts = 0;
			//int [] allowedNumList1 = {};
			//int[] allowedNumList2 = { };
			List<int> allowedNumList1 = new List<int>();
			List<int> allowedNumList2 = new List<int>();
			
			if (this is IFourCardSpecialCard speicalCard1)
			{
				allowedNumList1 = new List<int>(speicalCard1.AllowReplacedNumbers); 
			} else {
				allowedNumList1.Add(this.Number);
			}
			
			if (anotherCard is IFourCardSpecialCard speicalCard2)
			{
				allowedNumList2 = new List<int>(speicalCard2.AllowReplacedNumbers); 
			} else {
				allowedNumList2.Add(anotherCard.Number);
			}
			
			var bestPts = 0;
			foreach (var number in allowedNumList1)
			{
				var newBestPts = allowedNumList2.Max(allowNum => (number + allowNum) % 10);
				if (newBestPts > bestPts)
				{
					bestPts = newBestPts;
				}
			}
			return bestPts;
		}
		
		// The input string would be like 10♣️, or A♣️, since we don't know if first number has one or two chars, so
		// we leverage "♣️" has always return "one" for length even these symbal actually occupy two bytes for uni-code.
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
		
		// Convert 10♣️ to 10_Club
		public static string CardSymbolToStr(string symbolStr)
		{
			var (numStr, suitSymbol) = SplitCard(symbolStr);
			return $"{numStr}_{PokerConst.SymbolToPokerSuit[suitSymbol].ToString()}";
		}
		
		// Convert 10_Club to 10♣️
		public static string CardStrToSymbol(string cardStr)
		{
			string[] parts = cardStr.Split('_');
			var numStr = parts[0];
			var suit = (PokerSuit)Enum.Parse(typeof(PokerSuit), parts[1]);
			
			return $"{numStr}_{PokerConst.PokerSuitToSymbol[suit]}";
		}
		
		// The input pokerCardStr needs to be in the form like 10♣️, or A❤️, etc.
		public static FourCardPokerCard CreateInstance(string pokerCardStr, int objectID = 0, int deckID = 1)
		{
			FourCardPokerCard data = null;
			var number = 0;
			var suit = PokerSuit.NoSuit;
			
			if (pokerCardStr.Contains("SmallJoker"))
			{
				data = new FourCardSmallJoker();
				number = 15;
			} else if  (pokerCardStr.Contains("BigJoker"))
			{
				data = new FourCardBigJoker();
				number = 16;
			} else {
				var (numStr, suitSymbol) = SplitCard(pokerCardStr);
				data = numStr == "A" ? new FourCardAceCard() : new FourCardPokerCard();
				number = PokerConst.PokerStringToNumberDict[numStr];
				suit = PokerConst.SymbolToPokerSuit[suitSymbol];
			}
			
			var id = ((int)suit - 1) * PokerConst.MaxTotalCountInSameSuit + number;
			
			
			data.Init(id, number, suit, objectID, deckID);
			return data;
		}
		
		
		public static FourCardPokerCard CreateInstance(int number, PokerSuit pokerSuit, int objectID = 0, int deckID = 1)
		{
			var pokerCardStr = $"{PokerConst.PokerNumberNameDict[number]}{PokerConst.PokerSuitToSymbol[pokerSuit]}";
			return CreateInstance(pokerCardStr, objectID, deckID); }
		
    }
}