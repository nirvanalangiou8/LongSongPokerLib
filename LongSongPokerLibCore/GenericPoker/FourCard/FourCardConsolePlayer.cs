using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace GenericPoker.FourCard
{

    public struct BetRecord
    {
        public BetAction betAction;
        public int betChips;
        public int playerAccumBet;
    }
    
    public struct FourCardBetInfo
    {
        public int HandPower;
        
        // the current main pot size
        public int MainPot;
        
        public List<FourCardConsolePlayer> Opponents;
        
        public int TotalOpponentPrevBet;
        
        // the total bets sum for your opponents for current round
        public int TotalOpponentBetThisRound;

        // Total active bet opponents, meaning each bet or raise opponents, how many of them.
        public int NumBetOpponents;
        
        // get remaining_cards info, so that one can calculate avg hand points probability to facilitate the betting decision.
        public List<FourCardPokerCard> RemainingCards;

        public FourCardConsolePlayer CurrentPlayer;
        
        // the previous bet opponents, bet
        //public FourCardConsolePlayer PreviousBettingOpponent;

        // max bet/raise per turn
        public int MaxBet;

        // current betting round
        public int Round;

        // accumulated placed bet for the current round
        public int RoundAccumPlacedBet;
        
        // previous bet player. It could be yourself if other 3 opponents are called.
        public FourCardConsolePlayer PreviousBetPlayer; 

        // Round start player
        public FourCardConsolePlayer RoundStartPlayer;

    }
    
    public enum BetAction
    {
        Fold = 0,
        Call = 1,
        Bet = 2,
        Raise = 3,
    }
    
    public enum BetStrategyDictType
    {
        Conservative = 0,
        ModerateConservative = 1,
        Moderate = 2,
        Aggressive = 3
    }
    
    public class FourCardConsolePlayer : ConsolePlayer<FourCardPokerCard> 
    {
        //public int Chips { get; private set; }
        
        
        // Event triggered whenever the player's chips change
        public event Action<int> OnChipsChanged;

        // Backing field for Chips
        private int _chips;

        // Property with a private setter
        public int Chips
        {
            get => _chips;
            private set
            {
                if (_chips == value) return; // Only trigger if the value changes
                _chips = value;
                OnChipsChanged?.Invoke(_chips); // Trigger the event
            }
        }

        

        private List<BetRecord> _betHistory = new List<BetRecord>();
        
        public BetRecord LastBetRecord => _betHistory[^1];

        public FourCardHands? PlayHand { get; private set; } 

        private readonly Action _cardArrangeStrategy;
        private readonly Func<FourCardBetInfo, (BetAction, int)> _bettingStrategy;
        //private int chips;

        public void SetChips(int chips)
        {
            Chips = chips;
        }
        
        public FourCardConsolePlayer(string playerName, 
            int initChips,
            Action? cardArrangeStrategy = null,
            Func<FourCardBetInfo, (BetAction, int)>? bettingStrategy = null
            ) : base(playerName)
        {
            Chips = initChips;
            _cardArrangeStrategy = cardArrangeStrategy ?? DefaultCardArrangeStrategy;
            _bettingStrategy = bettingStrategy ?? DefaultBettingStrategy;
            _betHistory.Add(new BetRecord { betAction = BetAction.Fold, betChips = 0, playerAccumBet = 0});
        }

        public void ResetRound()
        {
            _betHistory.Clear();
            _betHistory.Add(new BetRecord { betAction = BetAction.Fold, betChips = 0, playerAccumBet = 0});
        }
        
        public void ArrangeCards()
        {
            _cardArrangeStrategy();
        }

        public static (BetAction betAction, int betChips) RuleStrategyConservative(FourCardBetInfo betTips)
        {
            return CommonRulePlayStrategy(BetStrategyDictType.Conservative, betTips);
        }
        
        public static (BetAction betAction, int betChips) RuleStrategyModerateConservative(FourCardBetInfo betTips)
        {
            return CommonRulePlayStrategy(BetStrategyDictType.ModerateConservative, betTips);
        }
        public static (BetAction betAction, int betChips) RuleStrategyModerate(FourCardBetInfo betTips)
        {
            return CommonRulePlayStrategy(BetStrategyDictType.Moderate, betTips);
        }
        public static (BetAction betAction, int betChips) RuleStrategyAggressive(FourCardBetInfo betTips)
        {
            return CommonRulePlayStrategy(BetStrategyDictType.Aggressive, betTips);
        }
        private void DefaultCardArrangeStrategy()
        {
            PlayHand = new FourCardHands(_pokerCards);
        }

        private (BetAction betAction, int betChips) DefaultBettingStrategy(FourCardBetInfo betTips)
        {
            return FourCardConsolePlayer.CommonRulePlayStrategy(BetStrategyDictType.Aggressive, betTips);
        }
        
        
        private FourCardBetInfo PrepareBetTips(FourCardBetInfo roundBetInfo)
        {
            
            //FourCardBetInfo retBetInfo = new FourCardBetInfo();
            FourCardBetInfo retBetInfo = roundBetInfo;
            
            retBetInfo.CurrentPlayer = this;
            retBetInfo.HandPower = PlayHand.TotalPower;
            //retBetInfo.Opponents = roundBetInfo.Opponents;
            
            
            retBetInfo.NumBetOpponents = retBetInfo.Opponents.Count(player =>
                (player.LastBetRecord.betAction == BetAction.Bet ||
                 player.LastBetRecord.betAction == BetAction.Raise));
            
            
            retBetInfo.TotalOpponentBetThisRound = retBetInfo.Opponents
                .Where(opponent => 
                                   (opponent.LastBetRecord.betAction == BetAction.Bet ||
                                    opponent.LastBetRecord.betAction == BetAction.Raise))
                .Sum(opponent => opponent.LastBetRecord.betChips);
            
            return retBetInfo;
        }   
        
        public void CollectPot(int mainPot)
        {
            Console.WriteLine($"{PlayerName} Claimed the winning Pot {mainPot}");
            Chips += mainPot;
        }

        public bool PlaceBet(int bet)
        {
            if (bet > Chips)
                return false;

            Chips -= bet;
            return true;
        }

        public void AddBetHistory(BetAction betAction, int betChips, int playerAccumBet)
        {
            _betHistory.Add(new BetRecord
            {
                betAction = betAction,
                betChips = betChips,
                playerAccumBet = playerAccumBet
            });
        }
        
        public (BetAction betAction, int betChips, int placeBet) Betting(FourCardBetInfo roundBetInfo)
        {
            // If the betting has looped back to this player
            if (roundBetInfo.PreviousBetPlayer == this)
            {
                AddBetHistory(BetAction.Call, 0, 0);
                return (BetAction.Call, 0, 0);
            }

            int roundAccumPlacedBet = roundBetInfo.RoundAccumPlacedBet;

            var betTips = PrepareBetTips(roundBetInfo);

            (BetAction betAction, int betChips) = _bettingStrategy(betTips);
            
            int placeBet = ProcessBet(betAction, betChips, roundBetInfo);

            return (betAction, betChips, placeBet);
        }


        // Once we know the bet decision and current bet amount, we will call this function to process the bet
        // including the place bet, documented the best history, and calculate how much of chips needs to be place,
        // called (placeBet) which includes your raise/bet and 
        // other opponents bet.
        public int ProcessBet(BetAction betAction, int betChips, FourCardBetInfo roundBetInfo)
        {
            int placeBet;

            if (betAction == BetAction.Fold) {
                placeBet = 0;
            } else {
                int prevPlayerAccumBet = _betHistory.Last().playerAccumBet;
                placeBet = betChips + roundBetInfo.RoundAccumPlacedBet - prevPlayerAccumBet;
                PlaceBet(placeBet);

                int playerAccumBet = prevPlayerAccumBet + placeBet;

                _betHistory.Add(new BetRecord
                {
                    betAction = betAction,
                    betChips = betChips,
                    playerAccumBet = playerAccumBet
                });
            }

            return placeBet;
        }
        
        
        
        public static Dictionary<string, object>? GetStrategyDict(BetStrategyDictType strategyType)
        {
            Dictionary<string, object>? retDict = strategyType switch
            {
                BetStrategyDictType.Conservative => new Dictionary<string, object>
                {
                    ["hand_power"] = new Dictionary<(int, int), int>
                    {
                        {(0, 8), -30}, {(6, 10), 1}, {(10, 15), 4}, {(15, 20), 8},
                        {(20, 25), 15}, {(25, 30), 30}, {(30, 500), 50}
                    },
                    ["main_pot"] = new Dictionary<(int, int), int>
                    {
                        {(0, 50), 0}, {(50, 100), 5}, {(100, 200), 10}, {(200, 500), 20}, {(500, 200000), 50}
                    },
                    ["total_opponent_bet"] = new Dictionary<(int, int), int>
                    {
                        {(0, 5), 0}, {(5, 10), -20}, {(10, 30), -40}, {(30, 100), -60},
                        {(100, 200), -85}, {(200, 100000), -110}
                    },
                    ["num_bet_opponents"] = new Dictionary<int, int>
                    {
                        {0, 0}, {1, -10}, {2, -30}, {3, -40}
                    },
                    ["bluff_range"] = (10, 50)
                },

                BetStrategyDictType.ModerateConservative => new Dictionary<string, object>
                {
                    ["hand_power"] = new Dictionary<(int, int), int>
                    {
                        {(0, 7), -25}, {(6, 10), 1}, {(10, 15), 4}, {(15, 20), 10},
                        {(20, 25), 20}, {(25, 30), 50}, {(30, 500), 100}
                    },
                    ["main_pot"] = new Dictionary<(int, int), int>
                    {
                        {(0, 50), 0}, {(50, 100), 5}, {(100, 200), 10}, {(200, 500), 20}, {(500, 200000), 50}
                    },
                    ["total_opponent_bet"] = new Dictionary<(int, int), int>
                    {
                        {(0, 5), 0}, {(5, 10), -15}, {(10, 30), -35}, {(30, 100), -55},
                        {(100, 200), -80}, {(200, 100000), -100}
                    },
                    ["num_bet_opponents"] = new Dictionary<int, int>
                    {
                        {0, 0}, {1, -8}, {2, -25}, {3, -35}
                    },
                    ["bluff_range"] = (10, 75)
                },

                BetStrategyDictType.Moderate => new Dictionary<string, object>
                {
                    ["hand_power"] = new Dictionary<(int, int), int>
                    {
                        {(0, 7), -20}, {(6, 10), 2}, {(10, 15), 5}, {(15, 20), 10},
                        {(20, 25), 20}, {(25, 30), 50}, {(30, 500), 100}
                    },
                    ["main_pot"] = new Dictionary<(int, int), int>
                    {
                        {(0, 50), 0}, {(50, 100), 5}, {(100, 200), 10}, {(200, 500), 20}, {(500, 200000), 50}
                    },
                    ["total_opponent_bet"] = new Dictionary<(int, int), int>
                    {
                        {(0, 5), 0}, {(5, 10), -10}, {(10, 30), -30}, {(30, 100), -50},
                        {(100, 200), -75}, {(200, 100000), -100}
                    },
                    ["num_bet_opponents"] = new Dictionary<int, int>
                    {
                        {0, 0}, {1, -5}, {2, -20}, {3, -30}
                    },
                    ["bluff_range"] = (10, 100)
                },

                BetStrategyDictType.Aggressive => new Dictionary<string, object>
                {
                    ["hand_power"] = new Dictionary<(int, int), int>
                    {
                        {(0, 6), -20}, {(6, 10), 4}, {(10, 15), 7}, {(15, 20), 15},
                        {(20, 25), 25}, {(25, 30), 60}, {(30, 500), 100}
                    },
                    ["main_pot"] = new Dictionary<(int, int), int>
                    {
                        {(0, 50), 0}, {(50, 100), 5}, {(100, 200), 10}, {(200, 500), 20}, {(500, 200000), 50}
                    },
                    ["total_opponent_bet"] = new Dictionary<(int, int), int>
                    {
                        {(0, 5), 0}, {(5, 10), -5}, {(10, 30), -20}, {(30, 100), -35},
                        {(100, 200), -50}, {(200, 100000), -70}
                    },
                    ["num_bet_opponents"] = new Dictionary<int, int>
                    {
                        {0, 0}, {1, -2}, {2, -10}, {3, -20}
                    },
                    ["bluff_range"] = (30, 130)
                },

                _ => null
            };

            return retDict;
        }
    
        public static (BetAction betAction, int betChips) CommonRulePlayStrategy(BetStrategyDictType strategyType, FourCardBetInfo betTips)
        {
            var betStrategyDict = GetStrategyDict(strategyType);
            
            var totalWeightDict = new Dictionary<string, (int min, int max)>
            {
                ["hand_power"] = (50, 70),
                ["main_pot"] = (0, 20),
                ["total_opponent_bet"] = (5, 25),
                ["num_bet_opponents"] = (0, 20),
                ["bluff_range"] = (0, 10)
            };

            //var weightDictKeys = totalWeightDict.Keys.OrderBy(_ => Guid.NewGuid()).ToList(); 
            
            // Convert keys to a list
            var weightDictKeys = totalWeightDict.Keys.ToList();

            // Use XRandom singleton to shuffle the list in place
            XRandom.Instance.Shuffle(weightDictKeys);
            
            // Shuffle
            int weightRemaining = 100;
            int totalBetPoints = 0;

            foreach (var weightKey in weightDictKeys)
            {
                var (min, max) = totalWeightDict[weightKey];
                //int weight = UtilFunc.GetRandomInt(min, max);
                int weight = XRandom.Instance.NextInt(min, max);
                
                weight = Math.Min(weight, weightRemaining);

                var numOpponents = betTips.NumBetOpponents;
                
                int? selectedValue = weightKey switch
                {
                    "hand_power" => UtilFunc.SelectRange(betTips.HandPower, (Dictionary<(int, int), int>)betStrategyDict[weightKey]),
                    "main_pot" => UtilFunc.SelectRange(betTips.MainPot, (Dictionary<(int, int), int>)betStrategyDict[weightKey]),
                    "total_opponent_bet" => UtilFunc.SelectRange(betTips.TotalOpponentBetThisRound, (Dictionary<(int, int), int>)betStrategyDict[weightKey]),
                    "num_bet_opponents" => ((Dictionary<int, int>)betStrategyDict[weightKey])[numOpponents],
                    "bluff_range" => XRandom.Instance.NextInt(((ValueTuple<int, int>)betStrategyDict[weightKey]).Item1, 
                        ((ValueTuple<int, int>)betStrategyDict[weightKey]).Item2 ),
                    _ => null
                    //"bluff_range" => UtilFunc.GetRandomInt(((ValueTuple<int, int>)betStrategyDict[weightKey]).Item1, 
                    //    ((ValueTuple<int, int>)betStrategyDict[weightKey]).Item2 ),
                    //_ => null
                };

                if (selectedValue == null)
                {
                    Console.WriteLine("DBG");
                }

                totalBetPoints += selectedValue.GetValueOrDefault() * weight;
                weightRemaining -= weight;
                if (weightRemaining <= 0) break;
            }

            totalBetPoints = totalBetPoints / 100;

            int betChips = 0;
            BetAction betAction;

            if (totalBetPoints < -0.1)
            {
                betAction = betTips.NumBetOpponents == 0 ? BetAction.Call : BetAction.Fold;
                betChips = 0;
            } else if (totalBetPoints < 10) {
                betAction = BetAction.Call;
                betChips = 0;
            } else {
                if (betTips.NumBetOpponents == 0)
                {
                    betAction = BetAction.Bet;
                    betChips = totalBetPoints;
                }
                else if (totalBetPoints > betTips.RoundAccumPlacedBet)
                {
                    betAction = BetAction.Raise;
                    betChips = totalBetPoints - betTips.RoundAccumPlacedBet;
                }
                else
                {
                    betAction = BetAction.Call;
                    betChips = 0;
                }
            }

            betChips = Math.Min(betChips, betTips.MaxBet);
            return (betAction, betChips);
        }       
    }
}


/*
         int placeBet;

         if (betAction == BetAction.Fold) {
             placeBet = 0;
         } else {
             int prevPlayerAccumBet = _betHistory.Last().playerAccumBet;
             placeBet = betChips + roundAccumPlacedBet - prevPlayerAccumBet;
             PlaceBet(placeBet);

             int playerAccumBet = prevPlayerAccumBet + placeBet;

             _betHistory.Add(new BetRecord
             {
                 betAction = betAction,
                 betChips = betChips,
                 playerAccumBet = playerAccumBet
             });
         }*/
