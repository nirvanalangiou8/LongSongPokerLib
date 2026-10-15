using System.Collections.Generic;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public sealed class NineCardRule : BaseCardRule
    {
        public static new NineCardRule Default { get; } = new();

        public NineCardRule()
        {
            MinStraightCount = 5;
            MinFlushCount = 5;
            MinFlushStraightCount = 3;
            MinKindCount = 2;
            SubHandsCardCount = new[] { 4, 5 };
        }
        /*
        public override PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
        {
            return base.AssembleHandRank(components);
        }*/
    }
}
