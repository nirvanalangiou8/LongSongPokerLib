using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker.SevenCard
{
    public class SevenCardHandCalculator
    {
        private List<SevenCardPokerCard> _allCards;
        private List<SevenCardPokerCard> _noneJokerCards;
        private List<JokerCard> _jokerCards;

        public SevenCardHandCalculator(List<SevenCardPokerCard> cards)
        {
            _allCards = cards;
            _noneJokerCards = cards.Where(c => !(c is JokerCard)).ToList();
            _jokerCards = cards.OfType<JokerCard>().ToList();
        }

        public static SevenCardHandCalculator CreateInstance(string cardStr)
        {
             var inputCardStrs = cardStr.Split(',');
             var newCardList = new List<SevenCardPokerCard>();
             foreach (var str in inputCardStrs)
             {
                 newCardList.Add(SevenCardPokerCard.CreateInstance(str));
             }
             return new SevenCardHandCalculator(newCardList);
        }

        public List<PokerCardComponent<EightCardsCompType, SevenCardPokerCard>> GetAllKindGroups(int minCount)
        {
            var result = new List<PokerCardComponent<EightCardsCompType, SevenCardPokerCard>>();
            var groups = _noneJokerCards.GroupBy(c => c.Number).Where(g => g.Count() >= minCount);
            foreach (var g in groups)
            {
                var rankName = g.Count() == 2 ? "Pair" : (g.Count() == 3 ? "ThreeOfKind" : (g.Count() == 4 ? "FourOfKind" : "None"));
                var rank = (EightCardsCompType)Enum.Parse(typeof(EightCardsCompType), rankName);
                result.Add(new PokerCardComponent<EightCardsCompType, SevenCardPokerCard> { CompRank = rank, Cards = g.ToList() });
            }
            return result;
        }

        // More complex logic can be added here, mirroring EightCard but adapted for 7 cards.
        // For Seven Card game, we split into 2 (front) and 5 (back).
    }
}
