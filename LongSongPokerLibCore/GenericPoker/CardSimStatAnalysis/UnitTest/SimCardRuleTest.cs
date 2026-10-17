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

            var nineCardRule = new NineCardTwoHandRule();
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
        public void TestBaseRuleAssembleHandRank_PokerCardComponent()
        {
            var rule = new EightCardRule();

            var compPair1 = new PokerCardComponent { CompRank = BaseCompType.Pair };
            var compPair2 = new PokerCardComponent { CompRank = BaseCompType.Pair };
            var compTrips = new PokerCardComponent { CompRank = BaseCompType.ThreeOfKind };
            var compFlushStraight = new PokerCardComponent { CompRank = BaseCompType.ThreeCardsFlushStraight };

            // Test Single Component Direct Ranks
            Assert.That(rule.AssembleHandRank(new[] { compPair1 }), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(rule.AssembleHandRank(new[] { compTrips }), Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));

            // Test Composite Ranks
            Assert.That(rule.AssembleHandRank(new[] { compTrips, compPair1 }), Is.EqualTo(PokerOverAllHandRank.FullHouse));
            Assert.That(rule.AssembleHandRank(new[] { compFlushStraight, compPair1 }), Is.EqualTo(PokerOverAllHandRank.Mansion));
            Assert.That(rule.AssembleHandRank(new[] { compPair1, compPair2 }), Is.EqualTo(PokerOverAllHandRank.TwoPairs));
            Assert.That(rule.AssembleHandRank(System.Array.Empty<PokerCardComponent>()), Is.EqualTo(PokerOverAllHandRank.Nothing));
        }

        [Test]
        public void TestBaseRuleAssembleHandRank_BaseCompType()
        {
            var rule = new EightCardRule();

            // Test Single Component Direct Ranks
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.Pair }), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.ThreeOfKind }), Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.FiveCardsStraight }), Is.EqualTo(PokerOverAllHandRank.FiveCardsStraight));

            // Test Composite Ranks
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.ThreeOfKind, BaseCompType.Pair }), Is.EqualTo(PokerOverAllHandRank.FullHouse));
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.ThreeCardsFlushStraight, BaseCompType.Pair }), Is.EqualTo(PokerOverAllHandRank.Mansion));
            Assert.That(rule.AssembleHandRank(new[] { BaseCompType.Pair, BaseCompType.Pair }), Is.EqualTo(PokerOverAllHandRank.TwoPairs));
            Assert.That(rule.AssembleHandRank(System.Array.Empty<BaseCompType>()), Is.EqualTo(PokerOverAllHandRank.Nothing));
        }

        [Test]
        public void TestAssembleHandRankWithSubHandOrder()
        {
            var nineCardRule = new NineCardTwoHandRule();
            // First hand in NineCardRule cannot have > 1 component
            Assert.That(nineCardRule.AssembleHandRank(new[] { BaseCompType.Pair }, BattleSubHandOrder.FirstHand), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(nineCardRule.AssembleHandRank(new[] { BaseCompType.Pair, BaseCompType.Pair }, BattleSubHandOrder.FirstHand), Is.EqualTo(PokerOverAllHandRank.TwoPairs));
            // Second hand in NineCardRule allows 2 components (TwoPairs)
            Assert.That(nineCardRule.AssembleHandRank(new[] { BaseCompType.Pair, BaseCompType.Pair }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.TwoPairs));

            var threeHandRule = new NineCardThreeHandRule();
            // 1-card Front hand cannot have any multi-card components
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.Pair }, BattleSubHandOrder.FirstHand), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(threeHandRule.AssembleHandRank(System.Array.Empty<BaseCompType>(), BattleSubHandOrder.FirstHand), Is.EqualTo(PokerOverAllHandRank.Nothing));

            // 3-card Middle hand allows Pair, Trips, TwoPairs, but not 4+ card components or FullHouse
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.Pair }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.ThreeOfKind }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.Pair, BaseCompType.Pair }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.TwoPairs));
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.FiveCardsStraight }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.FiveCardsStraight));
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.ThreeOfKind, BaseCompType.Pair }, BattleSubHandOrder.SecondHand), Is.EqualTo(PokerOverAllHandRank.FullHouse));

            // 5-card Back hand allows FullHouse, TwoPairs, Straight, etc.
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.ThreeOfKind, BaseCompType.Pair }, BattleSubHandOrder.ThirdHand), Is.EqualTo(PokerOverAllHandRank.FullHouse));
            Assert.That(threeHandRule.AssembleHandRank(new[] { BaseCompType.FiveCardsStraight }, BattleSubHandOrder.ThirdHand), Is.EqualTo(PokerOverAllHandRank.FiveCardsStraight));
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
