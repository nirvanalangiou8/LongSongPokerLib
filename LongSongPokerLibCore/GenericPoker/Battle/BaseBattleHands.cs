using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker
{
    public class BaseBattleHands : IComparable<BaseBattleHands>, IEnumerable<BaseSubBattleHand>
    {
        private readonly List<BaseSubBattleHand> _subHands;
        
        public IReadOnlyList<BaseSubBattleHand> SubHands => _subHands;
        public int Count => _subHands.Count;

        public BaseSubBattleHand this[int index] => _subHands[index];
        public BaseSubBattleHand this[BattleSubHandOrder order] => _subHands[(int)order];

        public BaseSubBattleHand? FrontHand => _subHands.Count > 0 ? _subHands[0] : null;
        public BaseSubBattleHand? MiddleHand => _subHands.Count >= 3 ? _subHands[1] : null;
        public BaseSubBattleHand? MidHand => MiddleHand;
        public BaseSubBattleHand? BackHand => _subHands.Count > 0 ? _subHands[^1] : null;

        public int TotalPower => _subHands.Sum(h => h.HandPower);
        
        private readonly List<BasePokerCard>? _allPokerCards;
        private readonly PokerHandCalculator? _pokerHandCalculator;

        public BaseBattleHands(IEnumerable<BaseSubBattleHand> subHands)
        {
            _subHands = subHands?.ToList() ?? new List<BaseSubBattleHand>();
        }

        public BaseBattleHands(params BaseSubBattleHand[] subHands)
        {
            _subHands = subHands?.ToList() ?? new List<BaseSubBattleHand>();
        }

        public BaseBattleHands(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand)
        {
            _subHands = new List<BaseSubBattleHand> { firstHand, secondHand };
        }
        
        public BaseBattleHands(string wholeCardStr, ICardRule? rule = null)
        {
            _pokerHandCalculator = PokerHandCalculator.CreateInstance(wholeCardStr, rule);
            _allPokerCards = _pokerHandCalculator.AllPokerCards;
            _subHands = new List<BaseSubBattleHand>();
        }

        /// <summary>
        /// Checks if this deployment satisfies poker hand ordering rules (no foul hand / 倒水).
        /// Typically: SubHand[0] <= SubHand[1] <= ... <= SubHand[N-1]
        /// </summary>
        public virtual bool IsValidDeployment()
        {
            for (int i = 0; i < _subHands.Count - 1; i++)
            {
                if (_subHands[i].CompareTo(_subHands[i + 1]) > 0)
                {
                    return false; // Foul! Earlier sub-hand cannot be stronger than later sub-hands
                }
            }
            return true;
        }
        
        public BaseBattleHands GetArrangeHands()
        {
            if (_pokerHandCalculator == null || _allPokerCards == null)
            {
                return this;
            }

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
                var effectiveRule = _pokerHandCalculator?.Rule ?? EightCardRule.Default;
                double score1 = h1.SubHands.Sum(h => IWinRateHandArrangeStrategy.GetSubHandWinRate(h, h.BattleSubHandOrder, effectiveRule));
                double score2 = h2.SubHands.Sum(h => IWinRateHandArrangeStrategy.GetSubHandWinRate(h, h.BattleSubHandOrder, effectiveRule));
                
                int cmp = score2.CompareTo(score1);
                if (cmp != 0) return cmp;
                return h2.CompareTo(h1);
            });

            return allBattleHands[0];
        }

        private BaseBattleHands? ArrangeSingleStructure(PokerHandStructure structure, IWinRateHandArrangeStrategy strategy, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? _pokerHandCalculator?.Rule ?? EightCardRule.Default;
            var subHands = strategy.ArrangeComps(structure.Components, structure.remainingCards, effectiveRule);
            if (subHands == null || subHands.Count == 0)
            {
                return null;
            }
            return new BaseBattleHands((IEnumerable<BaseSubBattleHand>)subHands);
        }
        
        public int CompareTo(BaseBattleHands? other)
        {
            if (other == null) return 1;
            if (ReferenceEquals(this, other)) return 0;
            return TotalPower.CompareTo(other.TotalPower);
        }
        
        public static bool operator >(BaseBattleHands? left, BaseBattleHands? right) => left?.CompareTo(right) > 0;
        public static bool operator <(BaseBattleHands? left, BaseBattleHands? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >=(BaseBattleHands? left, BaseBattleHands? right) => left is null ? right is null : left.CompareTo(right) >= 0;
        public static bool operator <=(BaseBattleHands? left, BaseBattleHands? right) => left is null || left.CompareTo(right) <= 0;
        public static bool operator ==(BaseBattleHands? left, BaseBattleHands? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(BaseBattleHands? left, BaseBattleHands? right) => !(left == right);
        
        public IEnumerator<BaseSubBattleHand> GetEnumerator() => _subHands.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object? obj)
        {
            if (obj is BaseBattleHands other)
            {
                if (_subHands.Count != other._subHands.Count) return false;
                if (TotalPower != other.TotalPower) return false;
                for (int i = 0; i < _subHands.Count; i++)
                {
                    if (!_subHands[i].Equals(other._subHands[i])) return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            foreach (var hand in _subHands)
            {
                hash.Add(hand);
            }
            return hash.ToHashCode();
        }
    }
}