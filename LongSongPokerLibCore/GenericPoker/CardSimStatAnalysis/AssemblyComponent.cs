using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    public static class AssemblyComponent
    {
        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
        {
            if (components == null) return SimCardOverAllHandRank.Nothing;
            var compList = components.Select(c => c.CompType).ToList();
            return AssembleHandRank(compList);
        }

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<SimCardsCompType>? compTypes)
        {
            if (compTypes == null) return SimCardOverAllHandRank.Nothing;
            var list = compTypes.Where(c => c != SimCardsCompType.Nothing && c != SimCardsCompType.None).ToList();
            if (list.Count == 0) return SimCardOverAllHandRank.Nothing;

            // Sort high power to low power
            list.Sort((a, b) => PokerComponents.GetCompPower(b).CompareTo(PokerComponents.GetCompPower(a)));

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

                if (c1 == SimCardsCompType.ThreeOfKind && c2 == SimCardsCompType.Pair) return SimCardOverAllHandRank.FullHouse;
                if (c1 == SimCardsCompType.ThreeCardsFlushStraight && c2 == SimCardsCompType.Pair) return SimCardOverAllHandRank.Mansion;
                if (c1 == SimCardsCompType.Pair && c2 == SimCardsCompType.Pair) return SimCardOverAllHandRank.TwoPairs;
                if (c1 == SimCardsCompType.ThreeCardsFlushStraight && c2 == SimCardsCompType.ThreeCardsFlushStraight) return SimCardOverAllHandRank.ThreeCardsFlushStraight;

                return SimCardOverAllHandRank.None;
            }

            return SimCardOverAllHandRank.None;
        }

        public static SimCardOverAllHandRank AssembleHandRank(params SimCardsCompType[] compTypes)
        {
            return AssembleHandRank((IEnumerable<SimCardsCompType>)compTypes);
        }
    }
}
