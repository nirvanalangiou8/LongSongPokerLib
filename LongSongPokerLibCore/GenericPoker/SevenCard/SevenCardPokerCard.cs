using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GenericPoker.SevenCard
{
    public class SevenCardPokerCard : BasePokerCard
    {
        public virtual bool IsNumberable => true;

        private float PokerCardPokerSuitModulationRatio = 1.0f / (float)(Enum.GetValues(typeof(PokerSuit)).Cast<int>().Last() + 1);

        public virtual float PokerCardPower => Number + (float)_suit * PokerCardPokerSuitModulationRatio;

        public static SevenCardPokerCard CreateInstance(int id, int number, PokerSuit pokerSuit, int objectID = 0, int deckID = 1)
        {
            var data = new SevenCardPokerCard();
            data.Init(id, number, pokerSuit, objectID, deckID);
            return data;
        }

        private static (string value, string suit) SplitCard(string input)
        {
            var si = new StringInfo(input);
            int totalElements = si.LengthInTextElements;
            string suit = si.SubstringByTextElements(totalElements - 1);
            string value = si.SubstringByTextElements(0, totalElements - 1);
            return (value, suit);
        }

        public static SevenCardPokerCard CreateInstance(string pokerCardStr, int objectID = 0, int deckID = 1)
        {
            var (numStr, suitSymbol) = SplitCard(pokerCardStr);
            var data = new SevenCardPokerCard();
            var number = PokerConst.PokerStringToNumberDict[numStr];
            var suit = PokerConst.SymbolToPokerSuit[suitSymbol];
            var id = ((int)suit - 1) * PokerConst.MaxTotalCountInSameSuit + number;
            data.Init(id, number, suit, objectID, deckID);
            return data;
        }

/*
        public static SevenCardPokerCard CreateInstance(SevenCardPokerCard another)
        {
            return CreateInstance(another._cardID, another._number, another._suit, another.ObjectID, deckID: another.DeckID);
        }
*/
    }
}
