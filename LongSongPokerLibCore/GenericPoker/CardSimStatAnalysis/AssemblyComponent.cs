using System;
using System.Collections.Generic;
using System.Linq;

using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public static class AssemblyComponent
    {
        public static ICardRule DefaultRule { get; set; } = EightCardRule.Default;

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(components);
        }

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<SimCardsCompType>? compTypes, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(params SimCardsCompType[] compTypes)
        {
            return DefaultRule.AssembleHandRank((IEnumerable<SimCardsCompType>)compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(ICardRule rule, params SimCardsCompType[] compTypes)
        {
            return rule.AssembleHandRank((IEnumerable<SimCardsCompType>)compTypes);
        }
    }
}
