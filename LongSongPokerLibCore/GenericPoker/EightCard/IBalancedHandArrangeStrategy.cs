

using System;
using System.Collections.Generic;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public class BalancedStrategy : IBaseHandArrangeStrategy
    {
        public (BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand) ArrangeComps(
            List<PokerCardComponent> comps,
            List<BasePokerCard>? remainingCards = null,
            ICardRule? rule = null)
        {
            BaseSubBattleHand firstBattleHand = null;
            BaseSubBattleHand secondBattleHand = null;

            if (comps.Count == 3)
            {
                if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
                        (comps[1].CompRank, comps[2].CompRank), out PokerOverAllHandRank newBattleRank))
                {
                    firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand,
                        PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
                    secondBattleHand =
                        new BaseSubBattleHand(BattleSubHandOrder.SecondHand, newBattleRank, comps[1], comps[2]);
                    if (firstBattleHand > secondBattleHand)
                    {
                        (firstBattleHand, secondBattleHand) = (secondBattleHand,
                            firstBattleHand);
                    }
                }
                else
                {
                    Console.WriteLine("Fatal error in Battle Hand arrange of strategy (3 comps).");
                }
            }
            else if (comps.Count == 2)
            {
                firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand,
                    PokerHandStructure.ConvertCompRankToBattleRank(comps[1].CompRank), comps[1]);
                secondBattleHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand,
                    PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), comps[0]);
            }

            return (firstBattleHand, secondBattleHand);
        }
    }
}