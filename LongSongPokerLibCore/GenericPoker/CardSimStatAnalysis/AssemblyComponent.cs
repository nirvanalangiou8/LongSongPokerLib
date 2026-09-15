using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    public static class AssemblyComponent
    {
        public static ISimCardRule DefaultRule { get; set; } = EightCardSimRule.Default;

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components, ISimCardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(components);
        }

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<SimCardsCompType>? compTypes, ISimCardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(params SimCardsCompType[] compTypes)
        {
            return DefaultRule.AssembleHandRank((IEnumerable<SimCardsCompType>)compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(ISimCardRule rule, params SimCardsCompType[] compTypes)
        {
            return rule.AssembleHandRank((IEnumerable<SimCardsCompType>)compTypes);
        }
    }
}
