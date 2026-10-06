using System;
using System.Collections.Generic;
using System.Linq;

using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    /*
    public static class AssemblyComponent
    {
        public static ICardRule DefaultRule { get; set; } = EightCardRule.Default;

        public static PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(components);
        }

        public static PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? compTypes, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? DefaultRule;
            return effectiveRule.AssembleHandRank(compTypes);
        }

        public static PokerOverAllHandRank AssembleHandRank(params BaseCompType[] compTypes)
        {
            return DefaultRule.AssembleHandRank((IEnumerable<BaseCompType>)compTypes);
        }

        public static PokerOverAllHandRank AssembleHandRank(ICardRule rule, params BaseCompType[] compTypes)
        {
            return rule.AssembleHandRank((IEnumerable<BaseCompType>)compTypes);
        }
    }*/
}
