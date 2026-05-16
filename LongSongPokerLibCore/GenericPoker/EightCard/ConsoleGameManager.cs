using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker.EightCard
{
    public class ConsoleGameManager<TCard, TPlayer>  
        where TCard : BasePokerCard
        where TPlayer : ConsolePlayer<TCard>
    {
        private List<TPlayer> _players;
        private ConsoleCardDealer<TCard> _dealer;
        public Dictionary<string, int> statDict;
        //private const int TotalPlayers = 13;
        private readonly IPlayerFactory<TPlayer> _playerFactory;
        
        
        public ConsoleGameManager(IPlayerFactory<TPlayer> playerFactory, int cardDecks = 1)
        {
            _playerFactory = playerFactory;
            statDict = new Dictionary<string, int>();
            _dealer = new ConsoleCardDealer<TCard>(cardDecks);
            _players = new List<TPlayer>();
            int totalPlayers = _dealer.TotalCards/8; 
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
        
        public void DealCardsToPlayers()
        {
            foreach (var player in _players)
            {
                _dealer.DealCards(player, 8);
            }
        }
        
        public void ProcessPlayersHands()
        {
            foreach (var player in _players)
            {

                var ret = player.ProcessHands();
                
                if (ret.Count == 0)
                {
                    if (statDict.ContainsKey("Nothing"))
                    {
                        statDict["Nothing"] += 1;  // Increment by 1 if the key exists
                    } else {
                        statDict["Nothing"]  = 1;  // Set to 1 if the key doesn't exist
                    }
                }
                
                foreach (var combo in ret)
                {
                    if (statDict.ContainsKey(combo.FinalCompsStr))
                    {
                        statDict[combo.FinalCompsStr] += 1;  // Increment by 1 if the key exists
                    }
                    else
                    {
                        statDict[combo.FinalCompsStr]  = 1;  // Set to 1 if the key doesn't exist
                    }
                }
            }
        }
    }
}

