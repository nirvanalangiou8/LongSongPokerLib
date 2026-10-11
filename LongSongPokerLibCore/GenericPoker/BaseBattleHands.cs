using System;
using GenericPoker;
using GenericPoker.EightCard;

namespace GenericPoker
{
    public class BaseBattleHands : IComparable<BaseBattleHands>
    {
        private BaseSubBattleHand _firstHand;
        private BaseSubBattleHand _secondHand;
        
        public int TotalPower => _firstHand.HandPower + _secondHand.HandPower;
        
        public BaseSubBattleHand FrontHand => _firstHand;
        public BaseSubBattleHand BackHand => _secondHand;
        
        private readonly List<BasePokerCard> _allPokerCards;
        
        private PokerHandCalculator _pokerHandCalculator;

        public BaseBattleHands(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand)
        {
            _firstHand = firstHand;
            _secondHand = secondHand;
        }
        
        public BaseBattleHands(string wholeCardStr)
        {
            _pokerHandCalculator = PokerHandCalculator.CreateInstance(wholeCardStr);
            _allPokerCards = _pokerHandCalculator.AllPokerCards;
        }
        
        
        public BaseBattleHands GetArrangeHands()
        {
            var allCandidateComps = new List<PokerHandStructure>();
            _pokerHandCalculator.RecursiveExploreHandStructures(_pokerHandCalculator.AllPokerCards, new PokerHandStructure(), allCandidateComps);
            
            foreach (var res in allCandidateComps) res.SortCompsAndClassify();
            allCandidateComps.Sort((c1, c2) => c2.CompareTo(c1));
            var uniqueCandidates = allCandidateComps.Distinct().ToList();

            // If no components found (pure high card / Nothing), add empty structure with all cards as kickers
            if (uniqueCandidates.Count == 0)
            {
                var nothingStruct = new PokerHandStructure();
                nothingStruct.SetRemainingCards(_allPokerCards);
                uniqueCandidates.Add(nothingStruct);
            }

            var arrangeStrategy = new IWinRateHandArrangeStrategy();
            var allBattleHands = new List<BaseBattleHands>();

            foreach (var pokerStructure in uniqueCandidates)
            {
                var battleHand = ArrangeSingleStructure(pokerStructure, arrangeStrategy);
                if (battleHand != null)
                {
                    allBattleHands.Add(battleHand);
                }
            }

            if (allBattleHands.Count == 0)
            {
                // Fallback
                var fallbackStruct = new PokerHandStructure();
                fallbackStruct.SetRemainingCards(_allPokerCards);
                return ArrangeSingleStructure(fallbackStruct, arrangeStrategy);
            }

            // Select the best battle hand based on win rate / total power
            allBattleHands.Sort((h1, h2) =>
            {
                double score1 = IWinRateHandArrangeStrategy.GetSubHandWinRate(h1.FrontHand, BattleSubHandOrder.FirstHand) +
                                IWinRateHandArrangeStrategy.GetSubHandWinRate(h1.BackHand, BattleSubHandOrder.SecondHand);
                double score2 = IWinRateHandArrangeStrategy.GetSubHandWinRate(h2.FrontHand, BattleSubHandOrder.FirstHand) +
                                IWinRateHandArrangeStrategy.GetSubHandWinRate(h2.BackHand, BattleSubHandOrder.SecondHand);
                int cmp = score2.CompareTo(score1);
                if (cmp != 0) return cmp;
                return h2.CompareTo(h1);
            });

            return allBattleHands[0];
        }

        private BaseBattleHands ArrangeSingleStructure(PokerHandStructure structure, IWinRateHandArrangeStrategy strategy)
        {
            var comps = structure.Components;
            var remainingCards = new List<BasePokerCard>(structure.remainingCards);

            // Sort remaining kicker cards descending by power/rank
            var sortedRemainingCards = remainingCards
                .OrderByDescending(c => c.PokerCardPower)
                .ToList();

            BaseSubBattleHand firstBattleHand;
            BaseSubBattleHand secondBattleHand;

            switch (comps.Count)
            {
                case 4: // 4 pairs: put (1st, 4th) in back hand, (2nd, 3rd) in front hand
                {
                    var newBattleRank = PokerConst.EightCardsCompComboToBattleRankDict[(BaseCompType.Pair, BaseCompType.Pair)];
                    firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, newBattleRank, comps[1], comps[2]);
                    secondBattleHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, newBattleRank, comps[0], comps[3]);
                    break;
                }
                case 3:
                case 2:
                {
                    (firstBattleHand, secondBattleHand) = strategy.ArrangeComps(comps);
                    break;
                }
                case 1:
                {
                    firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
                    secondBattleHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
                    break;
                }
                case 0:
                default:
                {
                    firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
                    secondBattleHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing);
                    break;
                }
            }

            // Distribute remaining / kicker cards with sub-leading strategy
            DistributeKickers(firstBattleHand, secondBattleHand, sortedRemainingCards);

            return new BaseBattleHands(firstBattleHand, secondBattleHand);
        }

        private void DistributeKickers(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand, List<BasePokerCard> sortedKickers)
        {
            if (sortedKickers == null || sortedKickers.Count == 0) return;

            var kickers = new List<BasePokerCard>(sortedKickers);

            // Check if back hand already has a stronger component rank than front hand
            if (secondHand.BattleHandRank > firstHand.BattleHandRank)
            {
                // Back hand already strictly stronger by rank (e.g., Back has Pair/Trips/Flush/Straight, Front is Nothing).
                // Place sub-leading/top kickers into front hand up to its capacity (3 cards max),
                // and the rest into back hand (5 cards max).
                while (kickers.Count > 0 && firstHand.Cards.Count < 3)
                {
                    firstHand.Cards.Add(kickers[0]);
                    kickers.RemoveAt(0);
                }

                while (kickers.Count > 0 && secondHand.Cards.Count < 5)
                {
                    secondHand.Cards.Add(kickers[0]);
                    kickers.RemoveAt(0);
                }
            }
            else
            {
                // Both hands have the same rank (e.g. Nothing vs Nothing, or same rank).
                // Leading kicker (highest) must stay in back hand so back hand > front hand.
                if (secondHand.Cards.Count < 5 && kickers.Count > 0)
                {
                    secondHand.Cards.Add(kickers[0]);
                    kickers.RemoveAt(0);
                }

                // Next highest kickers (sub-leading) go to front hand up to 3 cards
                while (kickers.Count > 0 && firstHand.Cards.Count < 3)
                {
                    firstHand.Cards.Add(kickers[0]);
                    kickers.RemoveAt(0);
                }

                // Remaining kickers go to back hand up to 5 cards
                while (kickers.Count > 0 && secondHand.Cards.Count < 5)
                {
                    secondHand.Cards.Add(kickers[0]);
                    kickers.RemoveAt(0);
                }
            }
        }

        /*public BaseBattleHands GetArrangedHands()
        {
            return GetArrangeHands();
        }*/
        
        
        public int CompareTo(BaseBattleHands other)
        {
            if (other == null) return 1;
            if (_firstHand == other._firstHand && _secondHand == other._secondHand) return 0;
            if (TotalPower == other.TotalPower)
            {
                return 0;
            } else {
                return other.TotalPower.CompareTo(other.TotalPower);
            }
            return 0;
        }
        
        public static bool operator >(BaseBattleHands left, BaseBattleHands right) => left.CompareTo(right) > 0;
        public static bool operator <(BaseBattleHands left, BaseBattleHands right) => left.CompareTo(right) < 0;
        public static bool operator >=(BaseBattleHands left, BaseBattleHands right) => left.CompareTo(right) >= 0;
        public static bool operator <=(BaseBattleHands left, BaseBattleHands right) => left.CompareTo(right) <= 0;
        public static bool operator ==(BaseBattleHands left, BaseBattleHands right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(BaseBattleHands left, BaseBattleHands right) => !(left == right);
        
        
        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object obj)
        {
            if (obj is BaseBattleHands other)
            {
                if (TotalPower != other.TotalPower) return false;
                else return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }

    }
}