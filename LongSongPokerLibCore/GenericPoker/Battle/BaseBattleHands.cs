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
            var allCandidateStructures = new List<PokerHandStructure>();
            _pokerHandCalculator.RecursiveExploreHandStructures(_pokerHandCalculator.AllPokerCards, new PokerHandStructure(), allCandidateStructures);
            
            foreach (var res in allCandidateStructures) res.SortCompsAndClassify();
            allCandidateStructures.Sort((c1, c2) => c2.CompareTo(c1));
            var uniqueCandidateStructures = allCandidateStructures.Distinct().ToList();

            // If no components found (pure high card / Nothing), add empty structure with all cards as kickers
            if (uniqueCandidateStructures.Count == 0)
            {
                var nothingStruct = new PokerHandStructure();
                nothingStruct.SetRemainingCards(_allPokerCards);
                uniqueCandidateStructures.Add(nothingStruct);
            }

            var arrangeStrategy = new IWinRateHandArrangeStrategy();
            var allBattleHands = new List<BaseBattleHands>();

            foreach (var pokerStructure in uniqueCandidateStructures)
            {
                var battleHand = ArrangeSingleStructure(pokerStructure, arrangeStrategy, _pokerHandCalculator?.Rule);
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
                return ArrangeSingleStructure(fallbackStruct, arrangeStrategy, _pokerHandCalculator?.Rule);
            }

            // Select the best battle hand based on win rate / total power
            allBattleHands.Sort((h1, h2) =>
            {
                double score1_front = IWinRateHandArrangeStrategy.GetSubHandWinRate(h1.FrontHand, BattleSubHandOrder.FirstHand);
                double score1_back = IWinRateHandArrangeStrategy.GetSubHandWinRate(h1.BackHand, BattleSubHandOrder.SecondHand);
                double score1 = score1_front + score1_back;

                double score2_front = IWinRateHandArrangeStrategy.GetSubHandWinRate(h2.FrontHand, BattleSubHandOrder.FirstHand); 
                double score2_back = IWinRateHandArrangeStrategy.GetSubHandWinRate(h2.BackHand, BattleSubHandOrder.SecondHand);
                double score2 = score2_front + score2_back;
                
                int cmp = score2.CompareTo(score1);
                if (cmp != 0) return cmp;
                return h2.CompareTo(h1);
            });

            return allBattleHands[0];
        }

        private BaseBattleHands ArrangeSingleStructure(PokerHandStructure structure, IWinRateHandArrangeStrategy strategy, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? _pokerHandCalculator?.Rule ?? EightCardRule.Default;
            var (firstBattleHand, secondBattleHand) = strategy.ArrangeComps(structure.Components, structure.remainingCards, effectiveRule);
            if (firstBattleHand == null || secondBattleHand == null)
            {
                return null;
            }
            return new BaseBattleHands(firstBattleHand, secondBattleHand);
        }
        
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