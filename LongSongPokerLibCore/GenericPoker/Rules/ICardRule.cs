using System.Collections.Generic;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public interface ICardRule
    {
        int MinStraightCount { get; set; }
        int MinFlushCount { get; set;}
        int MinFlushStraightCount { get; set;}
        int MinKindCount { get; set;}
        
        int CardCount { get;}
        int TotalCardCount => CardCount;
        
        IReadOnlyList<SubHandSlotConfig> SubHandSlots { get; set; }
        Dictionary<BattleSubHandOrder, int> SubHandsCardCount { get; set; }

        PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? components, BattleSubHandOrder? subHandOrder = null);
        PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components, BattleSubHandOrder? subHandOrder = null) => AssembleHandRank(components?.Select(c => c.CompType), subHandOrder);
        PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerCardComponent>? components, BattleSubHandOrder? subHandOrder = null) => AssembleHandRank(components?.Select(c => c.CompRank), subHandOrder);
    }
}
