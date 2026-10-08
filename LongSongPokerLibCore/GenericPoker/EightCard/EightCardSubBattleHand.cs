using System;
using System.Collections.Generic;

namespace GenericPoker.EightCard
{
    [Obsolete("EightCardSubBattleHand has been merged into BaseSubBattleHand. Use BaseSubBattleHand instead.")]
    public class EightCardSubBattleHand : BaseSubBattleHand
    {
        public EightCardSubBattleHand() : base()
        {
        }

        public EightCardSubBattleHand(List<BasePokerCard> cards) : base(cards)
        {
        }

        public EightCardSubBattleHand(BattleHandEnum battleHandEnum, PokerOverAllHandRank inputRank, 
            params PokerCardComponent<BaseCompType, BasePokerCard>[] inputCombos) 
            : base(battleHandEnum, inputRank, inputCombos)
        {
        }
    }
}
