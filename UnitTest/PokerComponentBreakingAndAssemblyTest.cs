using System.Collections.Generic;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class PokerComponentBreakingAndAssemblyTest
    {
        [Test]
        public void TestNineCardFlushBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.NineCardsFlush);
            var breakdowns = comp.BreakDown();

            Assert.That(breakdowns, Is.Not.Empty);

            // Verify that 9-card flush breaks into 5-card flush + 4-card flush
            bool has5And4Flush = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FiveCardsFlush) &&
                b.Any(c => c.CompType == SimCardsCompType.FourCardsFlush));

            Assert.That(has5And4Flush, Is.True, "9-card flush should break into 5-card flush and 4-card flush");

            // Verify that 9-card flush also breaks into 6-card flush + 3-card flush
            bool has6And3Flush = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.SixCardsFlush) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsFlush));

            Assert.That(has6And3Flush, Is.True, "9-card flush should break into 6-card flush and 3-card flush");
        }

        [Test]
        public void TestEightCardFlushBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.EightCardsFlush);
            var breakdowns = comp.BreakDown();

            // 8-card flush breaks into 5-card flush + 3-card flush or 4-card flush + 4-card flush
            bool has5And3 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FiveCardsFlush) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsFlush));

            bool has4And4 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Count(c => c.CompType == SimCardsCompType.FourCardsFlush) == 2);

            Assert.That(has5And3, Is.True);
            Assert.That(has4And4, Is.True);
        }

        [Test]
        public void TestFourOfKindBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.FourOfKind);
            var breakdowns = comp.BreakDown();

            bool hasTwoPairs = breakdowns.Any(b =>
                b.Count == 2 &&
                b.All(c => c.CompType == SimCardsCompType.Pair));

            Assert.That(hasTwoPairs, Is.True, "FourOfKind should be breakable into Pair + Pair");
        }

        [Test]
        public void TestCartesianProduct()
        {
            // Sequence 1: Flush breakdown options: [ [Flush5, Flush4], [Flush9] ]
            var seq1 = new List<List<PokerComponents>>
            {
                new() { new(SimCardsCompType.FiveCardsFlush), new(SimCardsCompType.FourCardsFlush) },
                new() { new(SimCardsCompType.NineCardsFlush) }
            };

            // Sequence 2: Sets breakdown options: [ [Pair, Pair], [FourOfKind] ]
            var seq2 = new List<List<PokerComponents>>
            {
                new() { new(SimCardsCompType.Pair), new(SimCardsCompType.Pair) },
                new() { new(SimCardsCompType.FourOfKind) }
            };

            var sequences = new List<List<List<PokerComponents>>> { seq1, seq2 };
            var product = PokerComponents.CartesianProduct(sequences);

            Assert.That(product.Count, Is.EqualTo(4), "Cartesian product of 2 options * 2 options should equal 4 combinations");

            // Verify each product item has 2 groups
            foreach (var item in product)
            {
                Assert.That(item.Count, Is.EqualTo(2));
                var flatList = item.SelectMany(x => x).ToList();
                Assert.That(flatList.Count, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void TestAssemblyComponent()
        {
            // Single components
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.Pair), Is.EqualTo(SimCardOverAllHandRank.Pair));
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.FiveCardsStraight), Is.EqualTo(SimCardOverAllHandRank.FiveCardsStraight));
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.FiveCardsFlush), Is.EqualTo(SimCardOverAllHandRank.FiveCardsFlush));

            // Compound combinations
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.Pair, SimCardsCompType.Pair), Is.EqualTo(SimCardOverAllHandRank.TwoPairs));
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.ThreeOfKind, SimCardsCompType.Pair), Is.EqualTo(SimCardOverAllHandRank.FullHouse));
            Assert.That(AssemblyComponent.AssembleHandRank(SimCardsCompType.ThreeCardsFlushStraight, SimCardsCompType.Pair), Is.EqualTo(SimCardOverAllHandRank.Mansion));

            // Null or empty
            Assert.That(AssemblyComponent.AssembleHandRank(new List<PokerComponents>()), Is.EqualTo(SimCardOverAllHandRank.Nothing));
        }
    }
}
