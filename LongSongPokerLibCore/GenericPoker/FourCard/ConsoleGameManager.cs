using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.FourCard;
using GenericPoker;

namespace GenericPoker.FourCard
{
    public class ConsoleGameManager
    {
        private List<FourCardConsolePlayer> _players;
        private List<FourCardConsolePlayer> _inGamePlayers;
        private ConsoleCardDealer<FourCardPokerCard> _dealer;
        private int _games;
        private int _playerInitMoney;
        private int _maxBet;
        private int _baseBet;
        private int _maxPlayRounds;
        private int _mainPot;
        public Dictionary<string, int> statDict;
        
        
        public ConsoleGameManager(List<FourCardConsolePlayer> players,  int games, int playerInitMoney,  int cardDecks = 1)
        {
            _games = games;
            _playerInitMoney = playerInitMoney;
            _maxBet = 100;
            _baseBet = 5;
            _maxPlayRounds = 10;
            _mainPot = 0;
            
            _dealer = new ConsoleCardDealer<FourCardPokerCard>(cardDecks);
            _players = new List<FourCardConsolePlayer>();
            _players.AddRange(players);
        }

        private void PlayersPlaceBaseBet()
        {
            List<FourCardConsolePlayer> updatedPlayers = new List<FourCardConsolePlayer>();

            foreach (var player in _inGamePlayers)
            {
                // If place betting is successful, meaning player has enough money to bet, and add it to inGamePlayer.
                if (player.PlaceBet(_baseBet))
                {
                    updatedPlayers.Add(player);
                    _mainPot += _baseBet;
                } else {
                    Console.WriteLine($"Player {player.PlayerName}, fold as they can not afford base bet.");
                }
            }
            _inGamePlayers = updatedPlayers;
        }
        
        private void PlayersDealtCards()
        {
            foreach (var player in _inGamePlayers)
            {
                _dealer.DealCards(player, 4);
                player.ArrangeCards();
            }
        }

        // this function get the deck remaining cards and also active players hand cards except the evaluated player/opponents.
        // the AI or other strategy can leverage the existing remaining cards to evaluate if their current hands are good or not?
        public List<FourCardPokerCard> GetRemainingCards(FourCardConsolePlayer currentPlayer)
        {
            // Make a copy of the deck's cards
            var retCards = new List<FourCardPokerCard>(_dealer.RemainingCards);

            foreach (var player in _inGamePlayers.Where(player => player != currentPlayer))
            {
                retCards.AddRange(player.Cards);
            }

            return retCards;
        }    
        
        private void PlayersBetting()
        {
            var roundBetInfo = new FourCardBetInfo
            {
                MainPot = _mainPot,
                RoundAccumPlacedBet = 0,
                PreviousBetPlayer = null,
                MaxBet = _maxBet,
                RemainingCards = _dealer.RemainingCards
            };

            // Randomly select the first player
            var bettingPlayers = new List<FourCardConsolePlayer>(_inGamePlayers);
            
            int startIndex = XRandom.Instance.NextInt(0, bettingPlayers.Count-1);
            
            Console.WriteLine($"Start Player index = {startIndex}, betting players count = {bettingPlayers.Count}");
            
            var rotatedPlayers = new List<FourCardConsolePlayer>();
            rotatedPlayers.AddRange(bettingPlayers.Skip(startIndex));
            rotatedPlayers.AddRange(bettingPlayers.Take(startIndex));

            var startPlayer = _inGamePlayers[startIndex];
            roundBetInfo.RoundStartPlayer = startPlayer;

            while (rotatedPlayers.Count > 0)
            {
                var currentPlayer = rotatedPlayers[0];
                var opponents = new List<FourCardConsolePlayer>(rotatedPlayers);
                opponents.Remove(currentPlayer);
                roundBetInfo.Opponents = opponents;

                var (betAction, betChips, placeBet) = currentPlayer.Betting(roundBetInfo);

                UtilFunc.CustomPrint(true, $"{currentPlayer.PlayerName} does {betAction} and bet_chips = {betChips}, place_bet = {placeBet}");

                if (betAction == BetAction.Fold)
                {
                    if (rotatedPlayers.Count == 1)
                    {
                        Console.WriteLine("DBG pop ### the remaining bettingPlayers = 1");
                    }
                    //rotatedPlayers.RemoveAt(0);
                    rotatedPlayers.Remove(currentPlayer);
                    currentPlayer.ResetRound();
                    _dealer.CollectCards(currentPlayer);
                    continue;
                }

                if (betAction == BetAction.Call)
                {
                    var totalCalls = rotatedPlayers
                        .Count(p => p.LastBetRecord.betAction == BetAction.Call);
                    if (totalCalls == rotatedPlayers.Count)
                    {
                        break;
                    }
                }

                if (betAction == BetAction.Bet || betAction == BetAction.Raise)
                {
                    roundBetInfo.PreviousBetPlayer = currentPlayer;
                }

                roundBetInfo.RoundAccumPlacedBet += betChips;
                roundBetInfo.MainPot += placeBet;

                // Move current player to the end of the list
                rotatedPlayers.Add(currentPlayer);
                rotatedPlayers.RemoveAt(0);
            }

            // Update inGamePlayers with the remaining ones
            _inGamePlayers = new List<FourCardConsolePlayer>(rotatedPlayers);
            _mainPot = roundBetInfo.MainPot;
        }
        
        
        
        public List<FourCardConsolePlayer> RunGames()
        {
            int gameCount = 1;
            
            while (gameCount <= _games)
            {
                _inGamePlayers = new List<FourCardConsolePlayer>(_players); // Assuming Player is a class and players is List<Player>

                // Assuming globalPrintVerbose is a global/static variable
                Console.WriteLine( "\n=====================================");
                Console.WriteLine( $"=========Game #{gameCount} Start....");
                Console.WriteLine( "=====================================");

                for (int round = 0; round < _maxPlayRounds; round++)
                {
                    Console.WriteLine($"\n### Round #{round} Start");

                    PlayersPlaceBaseBet();
                    PlayersDealtCards();
                    PlayersBetting();
                    var winner = PlayersBattle(_inGamePlayers); // Assumes it returns a Player or null

                    if (winner != null) {
                        ResolvePot(winner);
                        ShowChips();
                        ResetRound();
                        break;
                    } else {
                        Console.WriteLine( $"No clear winner, continue next round, accumulated_pot_size = {_mainPot}");
                    }

                    ShowChips();
                    ResetRound();
                }

                //ResetGame();
                gameCount++;
            }
            
            var sortedGameWinnerPlayers = _players
                .OrderByDescending(player => player.Chips)
                .ToList();

            return sortedGameWinnerPlayers;
        }

        private void ResolvePot(FourCardConsolePlayer winner)
        {
            winner.CollectPot(_mainPot);
            _mainPot = 0;
        }
        
        
        public static FourCardConsolePlayer? PlayersBattle(List<FourCardConsolePlayer> gamePlayers)
        {
            var sortedLowHandPlayers = gamePlayers
                .OrderByDescending(player => player.PlayHand.LowHand.HandPower)
                .ToList();

            var sortedHighHandPlayers = gamePlayers
                .OrderByDescending(player => player.PlayHand.HighHand.HandPower)
                .ToList();

            var lowHandWinner = sortedLowHandPlayers.First();
            var highHandWinner = sortedHighHandPlayers.First();

            var lowHandSecondWinner = gamePlayers.Count == 1 ? null : sortedLowHandPlayers.ElementAtOrDefault(1);
            var highHandSecondWinner = gamePlayers.Count == 1 ? null : sortedHighHandPlayers.ElementAtOrDefault(1);

            FourCardConsolePlayer? clearWinner = null;

            foreach (var player in gamePlayers)
            {
                var outputStr = $"{player.PlayerName} -> {player.PlayHand.LowHand.HandStrOnlyNum} ({player.PlayHand.LowHand.HandName})";

                if (player == lowHandWinner)
                    outputStr += " ** winner,";

                outputStr += $" {player.PlayHand.HighHand.HandStrOnlyNum} ({player.PlayHand.HighHand.HandName})";

                if (player == highHandWinner)
                    outputStr += " ** winner,";

                var isClearWinner = player == lowHandWinner && player == highHandWinner &&
                                     lowHandSecondWinner != null && highHandSecondWinner != null &&
                                     lowHandWinner.PlayHand.LowHand.HandPower > lowHandSecondWinner.PlayHand.LowHand.HandPower &&
                                     highHandWinner.PlayHand.HighHand.HandPower > highHandSecondWinner.PlayHand.HighHand.HandPower;

                if (isClearWinner)
                {
                    outputStr += " [!!Clear winner!!]";
                    clearWinner = player;
                }

                Console.WriteLine(outputStr);
            }

            return clearWinner;
        }
        
        private void ResetRound()
        {
            foreach (var player in _inGamePlayers)
            {
                player.ResetRound();
                _dealer.CollectCards(player);
            }
        }

        private void ShowChips()
        {
            foreach (var player in _players)
            {
                Console.WriteLine($"{player.PlayerName} has {player.Chips} chips.");
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
        }
    
    }    
}
