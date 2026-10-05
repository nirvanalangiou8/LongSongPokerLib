using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimSubBattleHand : BaseSubBattleHand<SimCardsBattleHandRank, BaseCompType, BasePokerCard>
    {
        public SimSubBattleHand() : base()
        {
        }

        public SimSubBattleHand(SimCardsBattleHandRank inputRank, params PokerCardComponent<BaseCompType, BasePokerCard>[] inputCombos)
            : base(inputRank, inputCombos)
        {
        }
    }
}
