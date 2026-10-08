using System;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public class BaseBattleHands : IComparable<BaseBattleHands>
    {
        private BaseSubBattleHand _firstHand;
        private BaseSubBattleHand _secondHand;
        
        public int TotalPower => _firstHand.HandPower + _secondHand.HandPower;
        
        public BaseSubBattleHand FrontHand => _firstHand;
        public BaseSubBattleHand BackHand => _secondHand;

        public BaseBattleHands(BaseSubBattleHand firstHand, BaseSubBattleHand secondHand)
        {
            _firstHand = firstHand;
            _secondHand = secondHand;
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