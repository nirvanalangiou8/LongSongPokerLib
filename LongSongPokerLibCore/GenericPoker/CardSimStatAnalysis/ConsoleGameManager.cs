using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class ConsoleGameManager<TCard, TPlayer, TResult>  
        where TCard : BasePokerCard
        where TPlayer : ConsolePlayer<TCard>
    {
        public List<TPlayer> _players;
        public ConsoleCardDealer<TCard> _dealer;
        public Dictionary<string, int> statDict;
        public readonly EightCard.IPlayerFactory<TPlayer> _playerFactory;
        
        public ConsoleGameManager(EightCard.IPlayerFactory<TPlayer> playerFactory, int cardsPerPlayer = 8, int cardDecks = 1)
        {
            _playerFactory = playerFactory;
            statDict = new Dictionary<string, int>();
            _dealer = new ConsoleCardDealer<TCard>(cardDecks);
            _players = new List<TPlayer>();
            int totalPlayers = _dealer.TotalCards / cardsPerPlayer; 
            for (var i = 1; i <= totalPlayers; i++)
            {
                string playerName = $"Player#{i}";
                var player = _playerFactory.Create(playerName);
                _players.Add(player);
            } 
        }

        public void CollectPlayersCardsAndShuffle()
        {
            foreach (var player in _players)
            {
                _dealer.CollectCards(player);
            }
            _dealer.ShuffleCards();
        }
        
        public void DealCardsToPlayers(int cardsPerPlayer = 8)
        {
            foreach (var player in _players)
            {
                _dealer.DealCards(player, cardsPerPlayer);
            }
        }
        
        public virtual void ProcessPlayersHands()
        {
            foreach (var player in _players)
            {
                var ret = player.ProcessHands();
                
                if (ret.Count == 0)
                {
                    if (statDict.ContainsKey("Nothing"))
                    {
                        statDict["Nothing"] += 1;
                    } else {
                        statDict["Nothing"]  = 1;
                    }
                }
                
                foreach (var combo in ret)
                {
                    // For general case, we assume TResult has FinalCompsStr or we use reflection/dynamic
                    // In EightCard/NineCard it works because TResult is PokerHandStructure
                }
            }
        }
    }
}

