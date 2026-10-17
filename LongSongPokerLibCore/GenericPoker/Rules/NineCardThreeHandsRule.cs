using System.Collections.Generic;

namespace GenericPoker
{
    public class NineCardThreeHandsRule : BaseCardRule
    {
        public static new NineCardThreeHandsRule Default { get; } = new();

        public NineCardThreeHandsRule()
        {
            MinStraightCount = 5;
            MinFlushCount = 5;
            MinFlushStraightCount = 3;
            MinKindCount = 2;
            WinRateTableFile = "prob_9cards_threehands";
            SubHandSlots = new List<SubHandSlotConfig>
            {
                new(BattleSubHandOrder.FirstHand, 1, "Front"),
                new(BattleSubHandOrder.SecondHand, 3, "Middle"),
                new(BattleSubHandOrder.ThirdHand, 5, "Back")
            };
        }
        
        public override PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? components, BattleSubHandOrder? subHandOrder = null)
        {
            if (components == null || components.Count() == 0) return PokerOverAllHandRank.Nothing;
            
            var list = components.ToList();

            // temopoarial the only nine card rule different to normal people is first
            // hand can not allow more than one comp, like two pairs.
            if (subHandOrder == BattleSubHandOrder.FirstHand && list.Count > 1)
            {
                return PokerOverAllHandRank.None;
            }

            switch (list.Count)
            {
                case 1 when Enum.TryParse<PokerOverAllHandRank>(list[0].ToString(), out var directRank):
                    return directRank;
                case 1:
                    return PokerOverAllHandRank.None;
                case 2:
                {
                    var c1 = list[0];
                    var c2 = list[1];

                    if (c1 == BaseCompType.ThreeOfKind && c2 == BaseCompType.Pair) return PokerOverAllHandRank.FullHouse;
                    if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.Pair) return PokerOverAllHandRank.Mansion;
                    if (c1 == BaseCompType.Pair && c2 == BaseCompType.Pair) return PokerOverAllHandRank.TwoPairs;
                    if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.ThreeCardsFlushStraight) return PokerOverAllHandRank.ThreeCardsFlushStraight;

                    return PokerOverAllHandRank.None;
                }
                default:
                    return PokerOverAllHandRank.None;
            }
            
            
        }
    }
}
