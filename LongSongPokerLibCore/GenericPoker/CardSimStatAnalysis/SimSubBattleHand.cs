using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimSubBattleHand : BaseSubBattleHand<SimCardsBattleHandRank, SimCardsCompType, BasePokerCard>
    {
        public SimSubBattleHand() : base()
        {
        }

        public SimSubBattleHand(SimCardsBattleHandRank inputRank, params PokerCardComponent<SimCardsCompType, BasePokerCard>[] inputCombos)
            : base(inputRank, inputCombos)
        {
        }
    }
}
