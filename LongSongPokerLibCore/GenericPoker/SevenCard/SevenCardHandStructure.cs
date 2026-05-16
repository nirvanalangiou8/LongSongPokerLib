using System;
using System.Collections.Generic;
using System.Linq;

using GenericPoker.EightCard;

namespace GenericPoker.SevenCard
{
    public class SevenCardSubBattleHand
    {
        public BattleHandEnum BattleHandEnum;
        public List<SevenCardPokerCard> Cards = new List<SevenCardPokerCard>();
        public PokerCardComponent<PokerCardCompRank, SevenCardPokerCard> Component;

        public int HandPower
        {
            get
            {
                // Simplified power calculation for now
                return (int)Component.CompRank * 1000 + (int)Cards.Sum(c => c.Number);
            }
        }

        public SevenCardSubBattleHand(BattleHandEnum battleHandEnum, PokerCardComponent<PokerCardCompRank, SevenCardPokerCard> component)
        {
            BattleHandEnum = battleHandEnum;
            Component = component;
            Cards.AddRange(component.Cards);
        }
    }

    public class SevenCardHands : IComparable<SevenCardHands>
    {
        private SevenCardSubBattleHand _frontHand;
        private SevenCardSubBattleHand _backHand;

        public int TotalPower => _frontHand.HandPower + _backHand.HandPower;

        public SevenCardHands(SevenCardSubBattleHand frontHand, SevenCardSubBattleHand backHand)
        {
            _frontHand = frontHand;
            _backHand = backHand;
        }

        public int CompareTo(SevenCardHands other)
        {
            if (other == null) return 1;
            return TotalPower.CompareTo(other.TotalPower);
        }
    }
}
