

using System;
using System.Collections.Generic;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public class BalancedStrategy : IBaseHandArrangeStrategy
    {
        public IReadOnlyList<BaseSubBattleHand> ArrangeComps(
            List<PokerCardComponent> comps,
            List<BasePokerCard>? remainingCards = null,
            ICardRule? rule = null)
        {
            var effectiveRule = rule ?? EightCardRule.Default;
            int firstCount = effectiveRule?.SubHandsCardCount != null &&
                             effectiveRule.SubHandsCardCount.TryGetValue(BattleSubHandOrder.FirstHand, out var fc) ? fc : 3;
            int secondCount = effectiveRule?.SubHandsCardCount != null &&
                              effectiveRule.SubHandsCardCount.TryGetValue(BattleSubHandOrder.SecondHand, out var sc) ? sc : 5;

            BaseSubBattleHand firstBattleHand = null;
            BaseSubBattleHand secondBattleHand = null;

            if (comps.Count == 3)
            {
                if (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
                        (comps[1].CompRank, comps[2].CompRank), out PokerOverAllHandRank newBattleRank))
                {
                    firstBattleHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand,
                        PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), firstCount, comps[0]);
                    secondBattleHand =
                        new BaseSubBattleHand(BattleSubHandOrder.SecondHand, newBattleRank, secondCount, comps[1], comps[2]);
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
                    PokerHandStructure.ConvertCompRankToBattleRank(comps[1].CompRank), firstCount, comps[1]);
                secondBattleHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand,
                    PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank), secondCount, comps[0]);
            }

            var result = new List<BaseSubBattleHand>();
            if (firstBattleHand != null) result.Add(firstBattleHand);
            if (secondBattleHand != null) result.Add(secondBattleHand);
            return result;
        }
    }
}