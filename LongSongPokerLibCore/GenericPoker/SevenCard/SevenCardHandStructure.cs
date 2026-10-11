using System;
using System.Collections.Generic;
using System.Linq;

using GenericPoker.EightCard;

namespace GenericPoker.SevenCard
{
    public class SevenCardSubBattleHand
    {
        public BattleSubHandOrder BattleSubHandOrder;
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

        public SevenCardSubBattleHand(BattleSubHandOrder battleSubHandOrder, PokerCardComponent<PokerCardCompRank, SevenCardPokerCard> component)
        {
            BattleSubHandOrder = battleSubHandOrder;
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
