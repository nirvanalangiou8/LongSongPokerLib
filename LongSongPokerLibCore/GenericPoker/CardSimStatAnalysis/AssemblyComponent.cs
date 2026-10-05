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

        public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? compTypes, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(params BaseCompType[] compTypes)
        {
            return DefaultRule.AssembleHandRank((IEnumerable<BaseCompType>)compTypes);
        }

        public static SimCardOverAllHandRank AssembleHandRank(ICardRule rule, params BaseCompType[] compTypes)
        {
            return rule.AssembleHandRank((IEnumerable<BaseCompType>)compTypes);
        }
    }
}
