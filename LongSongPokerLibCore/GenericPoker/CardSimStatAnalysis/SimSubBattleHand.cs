using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimSubBattleHand : BaseSubBattleHand<BasePokerCard>
    {
        private SimCardsBattleHandRank _simCardsBattleHandRank;
        public new SimCardsBattleHandRank BattleHandRank => _simCardsBattleHandRank;

        public List<PokerCardComponent<SimCardsCompType, BasePokerCard>> Components { get; private set; }

        public SimSubBattleHand(SimCardsBattleHandRank inputRank, params PokerCardComponent<SimCardsCompType, BasePokerCard>[] inputCombos)
        {
            Components = new List<PokerCardComponent<SimCardsCompType, BasePokerCard>>();
            _cards = new List<BasePokerCard>();
            foreach (var comp in inputCombos)
            {
                Components.Add(comp);
                _cards.AddRange(comp.Cards);
            }
            _simCardsBattleHandRank = inputRank;
            // HandPower calculation could be added here if needed for comparison, 
            // for now mimicking basic structure
        }

        public void AddMinorCards(List<BasePokerCard> remainingCards)
        {
            _cards.AddRange(remainingCards);
        }

        public override int CompareTo(BaseSubBattleHand<BasePokerCard> other)
        {
            if (other is SimSubBattleHand otherSim)
            {
                int rankCompare = BattleHandRank.CompareTo(otherSim.BattleHandRank);
                if (rankCompare != 0) return rankCompare;
                
                // Tie-breaker logic would go here
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is SimSubBattleHand other)
            {
                return BattleHandRank == other.BattleHandRank && 
                       Cards.SequenceEqual(other.Cards);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return BattleHandRank.GetHashCode();
        }
    }
}
