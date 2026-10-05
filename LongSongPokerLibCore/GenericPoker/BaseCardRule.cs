using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public abstract class BaseCardRule : ICardRule
    {
        public static ICardRule Default { get; } = EightCardRule.Default;

        public virtual int MinStraightCount { get; set; } = 5;
        public virtual int MinFlushCount { get; set; } = 5;
        public virtual int MinFlushStraightCount { get; set; } = 3;
        public virtual int MinKindCount { get; set; } = 2;
        public virtual int CardCount { get; set; } = 8;

        public virtual SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
        {
            if (components == null) return SimCardOverAllHandRank.Nothing;
            var compList = components.Select(c => c.CompType).ToList();
            return AssembleHandRank(compList);
        }

        public virtual SimCardOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? compTypes)
        {
            if (compTypes == null) return SimCardOverAllHandRank.Nothing;
            var list = compTypes.Where(c => c != BaseCompType.Nothing && c != BaseCompType.None).ToList();
            if (list.Count == 0) return SimCardOverAllHandRank.Nothing;

            // Sort high power to low power
            list.Sort((a, b) => ((int)b).CompareTo((int)a));

            if (list.Count == 1)
            {
                if (Enum.TryParse<SimCardOverAllHandRank>(list[0].ToString(), out var directRank))
                {
                    return directRank;
                }
                return SimCardOverAllHandRank.None;
            }

            if (list.Count == 2)
            {
                var c1 = list[0];
                var c2 = list[1];

                if (c1 == BaseCompType.ThreeOfKind && c2 == BaseCompType.Pair) return SimCardOverAllHandRank.FullHouse;
                if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.Pair) return SimCardOverAllHandRank.Mansion;
                if (c1 == BaseCompType.Pair && c2 == BaseCompType.Pair) return SimCardOverAllHandRank.TwoPairs;
                if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.ThreeCardsFlushStraight) return SimCardOverAllHandRank.ThreeCardsFlushStraight;

                return SimCardOverAllHandRank.None;
            }

            return SimCardOverAllHandRank.None;
        }

        public virtual SimCardOverAllHandRank AssembleHandRank(params BaseCompType[] compTypes)
        {
            return AssembleHandRank((IEnumerable<BaseCompType>)compTypes);
        }
    }
}
