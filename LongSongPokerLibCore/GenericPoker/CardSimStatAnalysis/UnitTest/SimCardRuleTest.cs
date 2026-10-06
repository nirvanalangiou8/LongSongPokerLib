using System.Collections.Generic;
using GenericPoker;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class SimCardRuleTest
    {
        [Test]
        public void TestDefaultRuleProperties()
        {
            var eightCardRule = new EightCardRule();
            Assert.That(eightCardRule.MinStraightCount, Is.EqualTo(5));
            Assert.That(eightCardRule.MinFlushCount, Is.EqualTo(5));
            Assert.That(eightCardRule.MinFlushStraightCount, Is.EqualTo(3));
            Assert.That(eightCardRule.MinKindCount, Is.EqualTo(2));
            Assert.That(eightCardRule.CardCount, Is.EqualTo(8));

            var nineCardRule = new NineCardRule();
            Assert.That(nineCardRule.MinStraightCount, Is.EqualTo(5));
            Assert.That(nineCardRule.MinFlushCount, Is.EqualTo(5));
            Assert.That(nineCardRule.MinFlushStraightCount, Is.EqualTo(3));
            Assert.That(nineCardRule.MinKindCount, Is.EqualTo(2));
            Assert.That(nineCardRule.CardCount, Is.EqualTo(9));
        }

        [Test]
        public void TestBaseRuleAssembleHandRank()
        {
            var rule = new EightCardRule();

            // Test Single Component Direct Ranks
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.Pair) }), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.ThreeOfKind) }), Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.FiveCardsStraight) }), Is.EqualTo(PokerOverAllHandRank.FiveCardsStraight));

            // Test Composite Ranks
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.ThreeOfKind), new PokerComponents(BaseCompType.Pair) }), Is.EqualTo(PokerOverAllHandRank.FullHouse));
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.ThreeCardsFlushStraight), new PokerComponents(BaseCompType.Pair) }), Is.EqualTo(PokerOverAllHandRank.Mansion));
            Assert.That(rule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.Pair), new PokerComponents(BaseCompType.Pair) }), Is.EqualTo(PokerOverAllHandRank.TwoPairs));
        }
        
        
        [Test]
        public void TestPokerComponentsWithRule()
        {
            var rule = new EightCardRule { MinFlushStraightCount = 4 };
            var comp = new PokerComponents(BaseCompType.FourCardsFlushStraight, rule);

            Assert.That(comp.Rule, Is.SameAs(rule));
            var breakdown = comp.BreakDown(rule);
            Assert.That(breakdown.Count, Is.GreaterThan(0));
        }
        
        /*        
        [Test]
        public void TestCustomRuleOverride()
        {
            var customRule = new CustomTestRule();

            // In our custom rule, ThreeCardsFlush + Pair is assembled into FullHouse
            var rank = customRule.AssembleHandRank(new[] { new PokerComponents(BaseCompType.ThreeCardsFlush), new PokerComponents(BaseCompType.Pair) });
            Assert.That(rank, Is.EqualTo(PokerOverAllHandRank.FullHouse));
        }

        
        private class CustomTestRule : BaseCardRule
        {
            public override PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
            {
                var list = components?.Select(c => c.CompType).ToList() ?? new List<BaseCompType>();
                if (list.Contains(BaseCompType.ThreeCardsFlush) && list.Contains(BaseCompType.Pair))
                {
                    return PokerOverAllHandRank.FullHouse;
                }
                return base.AssembleHandRank(components);
            }
        }*/
    }
}
