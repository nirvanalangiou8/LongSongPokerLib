using System.Collections.Generic;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public class EightCardRule : BaseCardRule
    {
        public static new EightCardRule Default { get; } = new();

        public EightCardRule()
        {
            MinStraightCount = 5;
            MinFlushCount = 5;
            MinFlushStraightCount = 3;
            MinKindCount = 2;
            WinRateTableFile = "prob_8cards_twohands";
            SubHandSlots = new List<SubHandSlotConfig>
            {
                new(BattleSubHandOrder.FirstHand, 3, "Front"),
                new(BattleSubHandOrder.SecondHand, 5, "Back")
            };
            LoadWinRateTable();
        }
        /*
        public override PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
        {
            return base.AssembleHandRank(components);
        }*/
    }
}
