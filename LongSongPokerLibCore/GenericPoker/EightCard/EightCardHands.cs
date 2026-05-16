using System;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public class EightCardHands : IComparable<EightCardHands>
    {
        private EightCardSubBattleHand _firstHand;
        private EightCardSubBattleHand _secondHand;
        
        public int TotalPower => _firstHand.HandPower + _secondHand.HandPower;
        
        public EightCardSubBattleHand FrontHand => _firstHand;
        public EightCardSubBattleHand BackHand => _secondHand;

        public EightCardHands(EightCardSubBattleHand firstHand, EightCardSubBattleHand secondHand)
        {
            _firstHand = firstHand;
            _secondHand = secondHand;
        }
        
        public int CompareTo(EightCardHands other)
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
        
        public static bool operator >(EightCardHands left, EightCardHands right) => left.CompareTo(right) > 0;
        public static bool operator <(EightCardHands left, EightCardHands right) => left.CompareTo(right) < 0;
        public static bool operator >=(EightCardHands left, EightCardHands right) => left.CompareTo(right) >= 0;
        public static bool operator <=(EightCardHands left, EightCardHands right) => left.CompareTo(right) <= 0;
        public static bool operator ==(EightCardHands left, EightCardHands right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(EightCardHands left, EightCardHands right) => !(left == right);
        
        
        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object obj)
        {
            if (obj is EightCardHands other)
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