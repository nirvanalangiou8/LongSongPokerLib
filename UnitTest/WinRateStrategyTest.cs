using System.Collections.Generic;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class WinRateStrategyTest
    {
        [Test]
        public void TestWinRateStrategy_FrontVsBackDistinctProbabilities()
        {
            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var frontPair = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);
            var backPair = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Pair, comp);

            double frontRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(frontPair);
            double backRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(backPair);

            // In twohands_prob_8cards.csv:
            // Front Pair min count is 2626401484 / 3593591176 (~0.730857)
            // Back Pair min count is 196902345 / 3593591178 (~0.0547926)
            // Therefore, frontRate must be significantly higher than backRate for a Pair.
            Assert.That(frontRate, Is.GreaterThan(0.70));
            Assert.That(backRate, Is.LessThan(0.50));
            Assert.That(frontRate, Is.Not.EqualTo(backRate));
        }

        [Test]
        public void TestWinRateStrategy_NothingHandProbabilities()
        {
            var card1 = BasePokerCard.CreateInstance("A♠️");
            var card2 = BasePokerCard.CreateInstance("K❤️");
            var card3 = BasePokerCard.CreateInstance("Q♣️");

            var frontNothing = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
            frontNothing.AddMinorCards(new List<BasePokerCard> { card1, card2, card3 });

            var backNothing = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing);
            backNothing.AddMinorCards(new List<BasePokerCard> { card1, card2, card3 });

            double frontRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(frontNothing);
            double backRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(backNothing);

            // Front Nothing max is 2626401484 / 3593591176 (~0.730857)
            // Back Nothing max is 196902345 / 3593591178 (~0.0547926)
            Assert.That(frontRate, Is.InRange(0.0, 2626401484.0 / 3593591176.0));
            Assert.That(backRate, Is.InRange(0.0, 196902345.0 / 3593591178.0));
        }

        [Test]
        public void TestWinRateStrategy_CalcHandWinRate()
        {
            var strategy = new IWinRateHandArrangeStrategy();

            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var card3 = BasePokerCard.CreateInstance("A♠️");
            var card4 = BasePokerCard.CreateInstance("A❤️");
            var card5 = BasePokerCard.CreateInstance("A♣️");
            var comp2 = new PokerCardComponent
            {
                CompRank = BaseCompType.ThreeOfKind,
                Cards = new List<BasePokerCard> { card3, card4, card5 }
            };

            var frontHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp1);
            var backHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.ThreeOfKind, comp2);

            float totalWinRate = strategy.CalcHandWinRate(frontHand, backHand);
            double frontRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(frontHand);
            double backRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(backHand);

            Assert.That(totalWinRate, Is.EqualTo((float)(frontRate + backRate)).Within(1e-5));
        }

        [Test]
        public void TestWinRateStrategy_ArrangeComps()
        {
            var strategy = new IWinRateHandArrangeStrategy();

            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var card3 = BasePokerCard.CreateInstance("A♠️");
            var card4 = BasePokerCard.CreateInstance("A❤️");
            var card5 = BasePokerCard.CreateInstance("A♣️");
            var comp2 = new PokerCardComponent
            {
                CompRank = BaseCompType.ThreeOfKind,
                Cards = new List<BasePokerCard> { card3, card4, card5 }
            };

            var (first, second) = strategy.ArrangeComps(new List<PokerCardComponent> { comp1, comp2 });

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.BattleSubHandOrder, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(second.BattleSubHandOrder, Is.EqualTo(BattleSubHandOrder.SecondHand));
            Assert.That(first.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(second.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
        }
    }
}
