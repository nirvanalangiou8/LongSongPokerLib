using System.Collections.Generic;
using GenericPoker;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class UtilFuncDominatedTest
    {
        [Test]
        public void TestSingleComponentDominance()
        {
            // A >= B -> kill B, keep A
            var compA = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("A♠️"), BasePokerCard.CreateInstance("A❤️") }
            };
            var compB = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("K♠️"), BasePokerCard.CreateInstance("K❤️") }
            };

            var structA = new PokerHandStructure(new List<PokerCardComponent> { compA });
            var structB = new PokerHandStructure(new List<PokerCardComponent> { compB });

            var filtered = UtilFunc.FilterDominated(
                new[] { structA, structB },
                s => s.Components,
                compatibilityCheck: (c1, c2) => c1.CompRank == c2.CompRank);

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered[0], Is.EqualTo(structA));
        }

        [Test]
        public void TestTwoComponentsDominance()
        {
            // C: [Pair of Ks, Pair of 8s]
            // D: [Pair of Qs, Pair of 3s]
            // C1 >= D1 && C2 >= D2 -> kill D, keep C
            var c1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("K♠️"), BasePokerCard.CreateInstance("K❤️") }
            };
            var c2 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("8♠️"), BasePokerCard.CreateInstance("8❤️") }
            };

            var d1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("Q♠️"), BasePokerCard.CreateInstance("Q❤️") }
            };
            var d2 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("3♠️"), BasePokerCard.CreateInstance("3❤️") }
            };

            var structC = new PokerHandStructure(new List<PokerCardComponent> { c1, c2 });
            var structD = new PokerHandStructure(new List<PokerCardComponent> { d1, d2 });

            var filtered = UtilFunc.FilterDominated(
                new[] { structC, structD },
                s => s.Components,
                compatibilityCheck: (x, y) => x.CompRank == y.CompRank);

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered[0], Is.EqualTo(structC));
        }

        [Test]
        public void TestTwoComponentsTradeoff_NotDominated()
        {
            // E: [Pair of Ks, Pair of 3s]
            // F: [Pair of Qs, Pair of 8s]
            // E1 > F1 but E2 < F2 -> keep both
            var e1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("K♠️"), BasePokerCard.CreateInstance("K❤️") }
            };
            var e2 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("3♠️"), BasePokerCard.CreateInstance("3❤️") }
            };

            var f1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("Q♠️"), BasePokerCard.CreateInstance("Q❤️") }
            };
            var f2 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("8♠️"), BasePokerCard.CreateInstance("8❤️") }
            };

            var structE = new PokerHandStructure(new List<PokerCardComponent> { e1, e2 });
            var structF = new PokerHandStructure(new List<PokerCardComponent> { f1, f2 });

            var filtered = UtilFunc.FilterDominated(
                new[] { structE, structF },
                s => s.Components,
                compatibilityCheck: (x, y) => x.CompRank == y.CompRank);

            Assert.That(filtered.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestFilterDominatedSolutions()
        {
            var sol1 = new List<PokerOverAllHandRank> { PokerOverAllHandRank.Pair, PokerOverAllHandRank.TwoPairs };
            var sol2 = new List<PokerOverAllHandRank> { PokerOverAllHandRank.Nothing, PokerOverAllHandRank.Pair };
            var sol3 = new List<PokerOverAllHandRank> { PokerOverAllHandRank.ThreeOfKind, PokerOverAllHandRank.TwoPairs };

            var filtered = UtilFunc.FilterDominatedSolutions(new[] { sol1, sol2, sol3 });

            // sol3 dominates sol1 and sol2; sol1 dominates sol2
            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered[0], Is.EqualTo(sol3));
        }
    }
}
