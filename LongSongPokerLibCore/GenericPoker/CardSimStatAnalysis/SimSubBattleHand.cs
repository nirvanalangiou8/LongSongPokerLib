using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimSubBattleHand : BaseSubBattleHand<PokerBattleHandRank, BaseCompType, BasePokerCard>
    {
        public SimSubBattleHand() : base()
        {
        }

        public SimSubBattleHand(PokerBattleHandRank inputRank, params PokerCardComponent<BaseCompType, BasePokerCard>[] inputCombos)
            : base(inputRank, inputCombos)
        {
        }
    }
}
