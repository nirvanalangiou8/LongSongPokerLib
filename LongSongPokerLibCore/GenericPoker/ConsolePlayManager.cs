using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker
{
    public class ConsolePlayManager
    {
        public ConsoleCardDealer Dealer { get; protected set; }
        public List<ConsolePlayer> Players { get; protected set; }
        public ICardRule Rule { get; set; }
        public int CardsPerPlayer { get; set; }
        public int CurrentRound { get; protected set; }
        public Dictionary<string, int> StatDict { get; protected set; }

        public ConsolePlayManager(ICardRule? rule = null, int cardsPerPlayer = 8, int cardDecks = 1)
        {
            Rule = rule ?? EightCardRule.Default;
            CardsPerPlayer = cardsPerPlayer;
            Dealer = new ConsoleCardDealer(cardDecks, rule: Rule);
            Players = new List<ConsolePlayer>();
            StatDict = new Dictionary<string, int>();
        }

        public ConsolePlayManager(IPlayerFactory<ConsolePlayer> playerFactory, int cardsPerPlayer = 8, int cardDecks = 1, ICardRule? rule = null)
            : this(rule, cardsPerPlayer, cardDecks)
        {
            int totalPlayers = Dealer.TotalCards / cardsPerPlayer;
            for (var i = 1; i <= totalPlayers; i++)
            {
                string playerName = $"Player#{i}";
                var player = playerFactory.Create(playerName);
                Players.Add(player);
            }
        }

        public void AddPlayer(ConsolePlayer player)
        {
            Players.Add(player);
        }

        public bool CheckAndReshuffleIfNeeded(int requiredCards)
        {
            if (Dealer.RemainingCards.Count < requiredCards)
            {
                Console.WriteLine($"牌堆不足 {requiredCards} 張，將棄牌重新洗牌補回牌堆... (reshuffle)\n");
                Dealer.RecycleDiscardAndReShuffle();
                return true;
            }
            return false;
        }

        public void StartNewRound()
        {
            CurrentRound++;
            CollectPlayersCards();
            int totalCardsNeeded = Players.Count * CardsPerPlayer;
            if (totalCardsNeeded == 0) totalCardsNeeded = CardsPerPlayer;
            CheckAndReshuffleIfNeeded(totalCardsNeeded);
            DealCardsToPlayers();
        }

        public void DealCardsToPlayers()
        {
            DealCardsToPlayers(CardsPerPlayer);
        }

        public void DealCardsToPlayers(int cardsPerPlayer)
        {
            foreach (var player in Players)
            {
                Dealer.DealCards(player, cardsPerPlayer);
            }
        }

        public void CollectPlayersCards()
        {
            foreach (var player in Players)
            {
                Dealer.CollectCards(player);
            }
        }

        public void CollectPlayersCardsAndShuffle()
        {
            CollectPlayersCards();
            Dealer.ShuffleCards();
        }

        public virtual void ProcessPlayersHands()
        {
            foreach (var player in Players)
            {
                var ret = player.ProcessHands();
                if (ret == null || ret.Count == 0)
                {
                    if (StatDict.ContainsKey("Nothing"))
                    {
                        StatDict["Nothing"] += 1;
                    }
                    else
                    {
                        StatDict["Nothing"] = 1;
                    }
                }
            }
        }
    }
}
