using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public abstract class BaseCardRule : ICardRule
    {
        public static ICardRule Default { get; } = EightCardRule.Default;

        public IReadOnlyList<int> SubHandsCardCount { get; protected set; } = new[] { 3, 5 };
        
        public int CardCount
        {
            get => SubHandsCardCount.Sum();
        }
        
        public int MinStraightCount { get;  set; } = 5;
        public int MinFlushCount { get;  set; } = 5;
        public int MinFlushStraightCount { get;  set; } = 3;
        public int MinKindCount { get;  set; } = 2;
         

        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? components)
        {
            if (components == null) return PokerOverAllHandRank.Nothing;
            var list = components
                .Where(c => c != BaseCompType.Nothing && c != BaseCompType.None)
                .ToList();
            if (list.Count == 0) return PokerOverAllHandRank.Nothing;

            // Sort high power to low power
            list.Sort((a, b) => ((int)b).CompareTo((int)a));

            if (list.Count == 1)
            {
                if (Enum.TryParse<PokerOverAllHandRank>(list[0].ToString(), out var directRank))
                {
                    return directRank;
                }
                return PokerOverAllHandRank.None;
            }

            if (list.Count == 2)
            {
                var c1 = list[0];
                var c2 = list[1];

                if (c1 == BaseCompType.ThreeOfKind && c2 == BaseCompType.Pair) return PokerOverAllHandRank.FullHouse;
                if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.Pair) return PokerOverAllHandRank.Mansion;
                if (c1 == BaseCompType.Pair && c2 == BaseCompType.Pair) return PokerOverAllHandRank.TwoPairs;
                if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.ThreeCardsFlushStraight) return PokerOverAllHandRank.ThreeCardsFlushStraight;

                return PokerOverAllHandRank.None;
            }

            return PokerOverAllHandRank.None;
        }

        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components) => AssembleHandRank(components?.Select(c => c.CompType));
        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerCardComponent>? components) => AssembleHandRank(components?.Select(c => c.CompRank));


    }
}
