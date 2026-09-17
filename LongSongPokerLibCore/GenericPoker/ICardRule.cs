using System.Collections.Generic;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public interface ICardRule
    {
        int MinStraightCount { get; set; }
        int MinFlushCount { get; set; }
        int MinFlushStraightCount { get; set; }
        int MinKindCount { get; set; }
        int CardCount { get; set; }

        SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components);
        SimCardOverAllHandRank AssembleHandRank(IEnumerable<GenericPoker.CardSimStatAnalysis.SimCardsCompType>? compTypes);
        SimCardOverAllHandRank AssembleHandRank(params GenericPoker.CardSimStatAnalysis.SimCardsCompType[] compTypes);
    }
}
