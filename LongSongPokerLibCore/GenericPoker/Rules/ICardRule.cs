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
        
        Dictionary<BattleSubHandOrder, int> SubHandsCardCount { get; }

        PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? components);
        PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components) => AssembleHandRank(components?.Select(c => c.CompType));
        PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerCardComponent>? components) => AssembleHandRank(components?.Select(c => c.CompRank));
    }
}
