using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker
{
    public class ConsoleCardDealer
    {
        private readonly List<BasePokerCard> _pokerCards;
        private readonly List<BasePokerCard> _discardCards;
        private readonly int _cardDecks;

        public int TotalCards => _pokerCards.Count;
        public List<BasePokerCard> RemainingCards => _pokerCards;
        public List<BasePokerCard> DiscardCards => _discardCards;
        public int CardDecks => _cardDecks;
        public ICardRule Rule { get; set; }

        public ConsoleCardDealer(int cardDecks = 1, bool addJokers = false, ICardRule? rule = null, Func<int, int, PokerSuit, int, int, BasePokerCard>? cardFactory = null)
        {
            _cardDecks = cardDecks;
            Rule = rule ?? EightCardRule.Default;
            _pokerCards = new List<BasePokerCard>();
            _discardCards = new List<BasePokerCard>();

            for (int i = 0; i < _cardDecks; i++)
            {
                for (var suitId = BasePokerCard.RegularSuitClubIndex; suitId <= BasePokerCard.RegularSuitSpadeIndex; suitId++)
                {
                    var pokerSuit = (PokerSuit)Enum.GetValues(typeof(PokerSuit)).GetValue(suitId);
                    for (int number = 2; number <= PokerConst.AceBigNumber; number++)
                    {
                        int id = (suitId - 1) * PokerConst.AceBigNumber + number;
                        BasePokerCard newPokerCard;
                        if (cardFactory != null)
                        {
                            newPokerCard = cardFactory(id, number, pokerSuit, 0, i + 1);
                        }
                        else
                        {
                            newPokerCard = EightCardPokerCard.CreateInstance(id, number, pokerSuit, 0, deckID: i + 1);
                        }
                        _pokerCards.Add(newPokerCard);
                    }
                }
            }

            if (addJokers)
            {
                // Reserved for joker setup
            }

            XRandom.Instance.Shuffle(_pokerCards);
        }

        public void RecycleDiscardAndReShuffle()
        {
            _pokerCards.AddRange(_discardCards);
            _discardCards.Clear();
            ShuffleCards();
        }

        public List<BasePokerCard> DealCards(int numberOfCards)
        {
            if (numberOfCards > _pokerCards.Count)
            {
                RecycleDiscardAndReShuffle();
            }

            var popCards = _pokerCards.GetRange(0, numberOfCards);
            _pokerCards.RemoveRange(0, numberOfCards);
            _discardCards.AddRange(popCards);
            return popCards;
        }

        public void DealCards(ConsolePlayer player, int numberOfCards)
        {
            if (numberOfCards > _pokerCards.Count)
            {
                RecycleDiscardAndReShuffle();
            }

            var popCards = _pokerCards.GetRange(0, numberOfCards);
            _pokerCards.RemoveRange(0, numberOfCards);
            player.SetCards(popCards);
        }

        public void CollectCards(ConsolePlayer player)
        {
            _discardCards.AddRange(player.Cards);
            player.ClearCards();
        }

        public void ShuffleCards()
        {
            XRandom.Instance.Shuffle(_pokerCards);
        }
    }
}
