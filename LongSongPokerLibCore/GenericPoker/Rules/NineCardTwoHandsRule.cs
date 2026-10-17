using System.Collections.Generic;

namespace GenericPoker
{
    public class NineCardTwoHandsRule : BaseCardRule
    {
        public static new NineCardTwoHandsRule Default { get; } = new();

        public NineCardTwoHandsRule()
        {
            MinStraightCount = 5;
            MinFlushCount = 5;
            MinFlushStraightCount = 3;
            MinKindCount = 2;
            SubHandSlots = new List<SubHandSlotConfig>
            {
                new(BattleSubHandOrder.FirstHand, 4, "Front"),
                new(BattleSubHandOrder.SecondHand, 5, "Back")
            };
        }
    }
}
