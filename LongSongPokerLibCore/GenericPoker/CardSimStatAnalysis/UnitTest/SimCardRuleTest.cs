using System.Collections.Generic;
using GenericPoker;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
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
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair }), Is.EqualTo(SimCardOverAllHandRank.Pair));
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.ThreeOfKind }), Is.EqualTo(SimCardOverAllHandRank.ThreeOfKind));
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.FiveCardsStraight }), Is.EqualTo(SimCardOverAllHandRank.FiveCardsStraight));

            // Test Composite Ranks
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.ThreeOfKind, GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair }), Is.EqualTo(SimCardOverAllHandRank.FullHouse));
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.ThreeCardsFlushStraight, GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair }), Is.EqualTo(SimCardOverAllHandRank.Mansion));
            Assert.That(rule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair, GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair }), Is.EqualTo(SimCardOverAllHandRank.TwoPairs));
        }

        [Test]
        public void TestCustomRuleOverride()
        {
            var customRule = new CustomTestRule();

            // In our custom rule, ThreeCardsFlush + Pair is assembled into FullHouse
            var rank = customRule.AssembleHandRank(new[] { GenericPoker.CardSimStatAnalysis.SimCardsCompType.ThreeCardsFlush, GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair });
            Assert.That(rank, Is.EqualTo(SimCardOverAllHandRank.FullHouse));
        }

        [Test]
        public void TestPokerComponentsWithRule()
        {
            var rule = new EightCardRule { MinFlushStraightCount = 4 };
            var comp = new PokerComponents(GenericPoker.CardSimStatAnalysis.SimCardsCompType.FourCardsFlushStraight, rule);

            Assert.That(comp.Rule, Is.SameAs(rule));
            var breakdown = comp.BreakDown(rule);
            Assert.That(breakdown.Count, Is.GreaterThan(0));
        }

        private class CustomTestRule : BaseCardRule
        {
            public override SimCardOverAllHandRank AssembleHandRank(IEnumerable<GenericPoker.CardSimStatAnalysis.SimCardsCompType>? compTypes)
            {
                var list = new List<GenericPoker.CardSimStatAnalysis.SimCardsCompType>(compTypes ?? new List<GenericPoker.CardSimStatAnalysis.SimCardsCompType>());
                if (list.Contains(GenericPoker.CardSimStatAnalysis.SimCardsCompType.ThreeCardsFlush) && list.Contains(GenericPoker.CardSimStatAnalysis.SimCardsCompType.Pair))
                {
                    return SimCardOverAllHandRank.FullHouse;
                }
                return base.AssembleHandRank(compTypes);
            }
        }
    }
}
