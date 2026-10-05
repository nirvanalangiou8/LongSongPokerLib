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
            CardCount = 8;
        }

        public override PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? compTypes)
        {
            return base.AssembleHandRank(compTypes);
        }

        public override PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
        {
            return base.AssembleHandRank(components);
        }
    }
}
