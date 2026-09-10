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
        public void TestEightCardFlushStraightBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.EightCardsFlushStraight);
            var breakdowns = comp.BreakDown();

            Assert.That(breakdowns, Is.Not.Empty);

            // 8 cards flushtraight with min=3 should have (8), (7), (6), (5,3), (5), (4,4), (4,3), (4), (3,3), (3)
            bool has8 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.EightCardsFlushStraight);
            bool has7 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.SevenCardsFlushStraight);
            bool has6 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.SixCardsFlushStraight);
            bool has5 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.FiveCardsFlushStraight);
            bool has4 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.FourCardsFlushStraight);
            bool has3 = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.ThreeCardsFlushStraight);

            bool has5And3 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FiveCardsFlushStraight) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsFlushStraight));

            bool has4And4 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Count(c => c.CompType == SimCardsCompType.FourCardsFlushStraight) == 2);

            bool has4And3 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FourCardsFlushStraight) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsFlushStraight));

            bool has3And3 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Count(c => c.CompType == SimCardsCompType.ThreeCardsFlushStraight) == 2);

            Assert.That(has8, Is.True, "Should contain (8)");
            Assert.That(has7, Is.True, "Should contain (7)");
            Assert.That(has6, Is.True, "Should contain (6)");
            Assert.That(has5And3, Is.True, "Should contain (5, 3)");
            Assert.That(has4And4, Is.True, "Should contain (4, 4)");
            Assert.That(has4And3, Is.True, "Should contain (4, 3)");
            Assert.That(has3And3, Is.True, "Should contain (3, 3)");

            // Verify no subcomponent has card count < 3
            bool hasAnyLessThanMin = breakdowns.SelectMany(b => b).Any(c => c.CardCount < 3);
            Assert.That(hasAnyLessThanMin, Is.False, "No subcomponents should have card count < 3");
        }

        [Test]
        public void TestEightCardFlushBreakDownWithMinConstraints()
        {
            var comp = new PokerComponents(SimCardsCompType.EightCardsFlush);

            // Default minFlushCards = 5
            var breakdownsDefault = comp.BreakDown();
            Assert.That(breakdownsDefault.All(b => b.All(c => c.CardCount >= 5)), Is.True, "All default flush breakdown parts must be >= 5");

            bool has8 = breakdownsDefault.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.EightCardsFlush);
            bool has7 = breakdownsDefault.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.SevenCardsFlush);
            bool has6 = breakdownsDefault.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.SixCardsFlush);
            bool has5 = breakdownsDefault.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.FiveCardsFlush);

            Assert.That(has8, Is.True);
            Assert.That(has7, Is.True);
            Assert.That(has6, Is.True);
            Assert.That(has5, Is.True);

            // With custom minFlushCards = 3
            var breakdownsMin3 = comp.BreakDown(minFlushCards: 3);
            bool has5And3 = breakdownsMin3.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FiveCardsFlush) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsFlush));

            bool has4And4 = breakdownsMin3.Any(b =>
                b.Count == 2 &&
                b.Count(c => c.CompType == SimCardsCompType.FourCardsFlush) == 2);

            Assert.That(has5And3, Is.True);
            Assert.That(has4And4, Is.True);
        }

        [Test]
        public void TestNineCardStraightBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.NineCardsStraight);

            // Default minStraightCards = 5
            var breakdowns = comp.BreakDown();
            Assert.That(breakdowns.All(b => b.All(c => c.CardCount >= 5)), Is.True);

            // With minStraightCards = 3
            var breakdownsMin3 = comp.BreakDown(minStraightCards: 3);
            bool has5And4Straight = breakdownsMin3.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FiveCardsStraight) &&
                b.Any(c => c.CompType == SimCardsCompType.FourCardStraight));

            bool has6And3Straight = breakdownsMin3.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.SixCardsStraight) &&
                b.Any(c => c.CompType == SimCardsCompType.ThreeCardsStraight));

            Assert.That(has5And4Straight, Is.True);
            Assert.That(has6And3Straight, Is.True);
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

            bool hasFour = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.FourOfKind);
            bool hasThree = breakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.ThreeOfKind);

            Assert.That(hasFour, Is.True);
            Assert.That(hasThree, Is.True);
        }

        [Test]
        public void TestThreeOfKindAndPairBreakDown()
        {
            var three = new PokerComponents(SimCardsCompType.ThreeOfKind);
            var threeBreakdowns = three.BreakDown();
            Assert.That(threeBreakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.ThreeOfKind), Is.True);
            Assert.That(threeBreakdowns.Any(b => b.Count == 1 && b[0].CompType == SimCardsCompType.Pair), Is.True);

            var pair = new PokerComponents(SimCardsCompType.Pair);
            var pairBreakdowns = pair.BreakDown();
            Assert.That(pairBreakdowns.Count, Is.EqualTo(1));
            Assert.That(pairBreakdowns[0][0].CompType, Is.EqualTo(SimCardsCompType.Pair));
        }

        [Test]
        public void TestSixOfKindBreakDown()
        {
            var six = new PokerComponents(SimCardsCompType.SixOfKind);
            var breakdowns = six.BreakDown();

            // 6-kind with min=2 should decompose into [3, 3], [4, 2], [2, 2, 2], etc.
            bool has3And3 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Count(c => c.CompType == SimCardsCompType.ThreeOfKind) == 2);

            bool has4And2 = breakdowns.Any(b =>
                b.Count == 2 &&
                b.Any(c => c.CompType == SimCardsCompType.FourOfKind) &&
                b.Any(c => c.CompType == SimCardsCompType.Pair));

            bool hasThreePairs = breakdowns.Any(b =>
                b.Count == 3 &&
                b.All(c => c.CompType == SimCardsCompType.Pair));

            Assert.That(has3And3, Is.True);
            Assert.That(has4And2, Is.True);
            Assert.That(hasThreePairs, Is.True);
        }

        [Test]
        public void TestNonDecomposableComponentBreakDown()
        {
            var comp = new PokerComponents(SimCardsCompType.Nothing);
            var breakdowns = comp.BreakDown();

            Assert.That(breakdowns.Count, Is.EqualTo(1));
            Assert.That(breakdowns[0].Count, Is.EqualTo(1));
            Assert.That(breakdowns[0][0].CompType, Is.EqualTo(SimCardsCompType.Nothing));
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

        [Test]
        public void TestSplitHandWithThreeOfKindAndTwoPairs()
        {
            // Input: [ThreeOfKind, Pair, Pair]
            var comps = new List<PokerComponents>
            {
                new(SimCardsCompType.ThreeOfKind),
                new(SimCardsCompType.Pair),
                new(SimCardsCompType.Pair)
            };

            var splits = InitEightCardHandSplitProbAna.SplitHand(comps);

            Assert.That(splits, Is.Not.Empty);

            // Possible valid splits:
            // 1. Front: Pair, Back: FullHouse (ThreeOfKind + Pair)
            bool hasPairAndFullHouse = splits.Contains((SimCardOverAllHandRank.Pair, SimCardOverAllHandRank.FullHouse));
            // 2. Front: TwoPairs (Pair + Pair), Back: ThreeOfKind (or swapped depending on power)
            bool hasTwoPairsAndThreeOfKind = splits.Contains((SimCardOverAllHandRank.TwoPairs, SimCardOverAllHandRank.ThreeOfKind));

            Assert.That(hasPairAndFullHouse, Is.True, "Should include (Pair, FullHouse)");
            Assert.That(hasTwoPairsAndThreeOfKind, Is.True, "Should include (TwoPairs, ThreeOfKind)");

            // (Pair, TwoPairs) is dominated by (Pair, FullHouse) because FullHouse > TwoPairs with same Front (Pair)
            bool hasPairAndTwoPairs = splits.Contains((SimCardOverAllHandRank.Pair, SimCardOverAllHandRank.TwoPairs));
            Assert.That(hasPairAndTwoPairs, Is.False, "Dominated split (Pair, TwoPairs) should be filtered out by (Pair, FullHouse)");

            // Verify back rank is always >= front rank
            foreach (var (front, back) in splits)
            {
                Assert.That((int)back, Is.GreaterThanOrEqualTo((int)front), $"Back rank {back} must be >= Front rank {front}");
            }
        }

        [Test]
        public void TestSplitHandWithFourOfKind()
        {
            // Input: [FourOfKind]
            // FourOfKind breaks down into: [FourOfKind], [Pair, Pair], [ThreeOfKind], [Pair]
            var comps = new List<PokerComponents>
            {
                new(SimCardsCompType.FourOfKind)
            };

            var splits = InitEightCardHandSplitProbAna.SplitHand(comps);

            Assert.That(splits, Is.Not.Empty);

            // Candidate non-dominated splits:
            // - (Pair, Pair)
            // - (Nothing, FourOfKind)
            // (Nothing, TwoPairs), (Nothing, ThreeOfKind), (Nothing, Pair) are dominated by (Nothing, FourOfKind)
            bool hasPairPair = splits.Contains((SimCardOverAllHandRank.Pair, SimCardOverAllHandRank.Pair));
            bool hasNothingFourOfKind = splits.Contains((SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.FourOfKind));
            bool hasNothingTwoPairs = splits.Contains((SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.TwoPairs));

            Assert.That(hasPairPair, Is.True, "Should include (Pair, Pair) resulting from Pair+Pair decomposition");
            Assert.That(hasNothingFourOfKind, Is.True, "Should include (Nothing, FourOfKind)");
            Assert.That(hasNothingTwoPairs, Is.False, "Dominated split (Nothing, TwoPairs) should be filtered out by (Nothing, FourOfKind)");
        }

        [Test]
        public void TestSplitHandWithEightCardFlushStraight()
        {
            // Input: [EightCardsFlushStraight]
            // Decomposes into (8), (5,3) -> [FiveCardsFlushStraight, ThreeCardsFlushStraight], (4,4) -> [FourCardsFlushStraight, FourCardsFlushStraight], etc.
            var comps = new List<PokerComponents>
            {
                new(SimCardsCompType.EightCardsFlushStraight)
            };

            var splits = InitEightCardHandSplitProbAna.SplitHand(comps);

            Assert.That(splits, Is.Not.Empty);

            // Non-dominated solutions:
            // (Nothing, EightCardsFlushStraight)
            // (ThreeCardsFlushStraight, FiveCardsFlushStraight)
            // (FourCardsFlushStraight, FourCardsFlushStraight)
            bool hasThreeAndFiveFlushStraight = splits.Contains((SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.FiveCardsFlushStraight));
            bool hasFourAndFourFlushStraight = splits.Contains((SimCardOverAllHandRank.FourCardsFlushStraight, SimCardOverAllHandRank.FourCardsFlushStraight));
            bool hasNothingAndEightFlushStraight = splits.Contains((SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.EightCardsFlushStraight));

            Assert.That(hasThreeAndFiveFlushStraight, Is.True, "Should include (ThreeCardsFlushStraight, FiveCardsFlushStraight)");
            Assert.That(hasFourAndFourFlushStraight, Is.True, "Should include (FourCardsFlushStraight, FourCardsFlushStraight)");
            Assert.That(hasNothingAndEightFlushStraight, Is.True, "Should include (Nothing, EightCardsFlushStraight)");

            // Obvious losers filtered out:
            // (Nothing, 7FS), (Nothing, 6FS), (3FS, 4FS), (3FS, 3FS)
            bool hasNothingSevenFS = splits.Contains((SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.SevenCardsFlushStraight));
            bool hasThreeFourFS = splits.Contains((SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.FourCardsFlushStraight));
            bool hasThreeThreeFS = splits.Contains((SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.ThreeCardsFlushStraight));

            Assert.That(hasNothingSevenFS, Is.False, "Dominated split (Nothing, SevenCardsFlushStraight) must be filtered out");
            Assert.That(hasThreeFourFS, Is.False, "Dominated split (3FS, 4FS) must be filtered out by (3FS, 5FS)");
            Assert.That(hasThreeThreeFS, Is.False, "Dominated split (3FS, 3FS) must be filtered out");
        }

        [Test]
        public void TestFilterDominatedSolutions()
        {
            var rawSolutions = new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>
            {
                (SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.EightCardsFlushStraight),
                (SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.SevenCardsFlushStraight),
                (SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.SixCardsFlushStraight),
                (SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.FiveCardsFlushStraight),
                (SimCardOverAllHandRank.FourCardsFlushStraight, SimCardOverAllHandRank.FourCardsFlushStraight),
                (SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.FourCardsFlushStraight),
                (SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.ThreeCardsFlushStraight),
            };

            var filtered = InitEightCardHandSplitProbAna.FilterDominatedSolutions(rawSolutions);

            Assert.That(filtered.Count, Is.EqualTo(3));
            Assert.That(filtered, Contains.Item((SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.EightCardsFlushStraight)));
            Assert.That(filtered, Contains.Item((SimCardOverAllHandRank.ThreeCardsFlushStraight, SimCardOverAllHandRank.FiveCardsFlushStraight)));
            Assert.That(filtered, Contains.Item((SimCardOverAllHandRank.FourCardsFlushStraight, SimCardOverAllHandRank.FourCardsFlushStraight)));
        }
    }
}
