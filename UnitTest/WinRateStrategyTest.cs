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

            var hands = strategy.ArrangeComps(new List<PokerCardComponent> { comp1, comp2 });

            Assert.That(hands, Is.Not.Null);
            Assert.That(hands.Count, Is.EqualTo(2));
            var first = hands[0];
            var second = hands[1];
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.BattleSubHandOrder, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(second.BattleSubHandOrder, Is.EqualTo(BattleSubHandOrder.SecondHand));
            Assert.That(first.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(second.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
        }

        [Test]
        public void TestWinRateStrategy_RuleRuntimeTableLoaded()
        {
            var eightRule = new EightCardRule();
            Assert.That(eightRule.WinRateCdfBands, Is.Not.Null);
            Assert.That(eightRule.WinRateCdfBands.Count, Is.EqualTo(2));
            Assert.That(eightRule.WinRateCdfBands[0].ContainsKey(PokerOverAllHandRank.Nothing), Is.True);
            Assert.That(eightRule.WinRateCdfBands[1].ContainsKey(PokerOverAllHandRank.Nothing), Is.True);

            var nineTwoRule = new NineCardTwoHandsRule();
            Assert.That(nineTwoRule.WinRateCdfBands, Is.Not.Null);
            Assert.That(nineTwoRule.WinRateCdfBands.Count, Is.EqualTo(2));

            var nineThreeRule = new NineCardThreeHandsRule();
            Assert.That(nineThreeRule.WinRateCdfBands, Is.Not.Null);
            Assert.That(nineThreeRule.WinRateCdfBands.Count, Is.EqualTo(3));
            Assert.That(nineThreeRule.WinRateCdfBands[0].ContainsKey(PokerOverAllHandRank.Pair), Is.True);
            Assert.That(nineThreeRule.WinRateCdfBands[1].ContainsKey(PokerOverAllHandRank.Pair), Is.True);
            Assert.That(nineThreeRule.WinRateCdfBands[2].ContainsKey(PokerOverAllHandRank.Pair), Is.True);
        }

        [Test]
        public void TestWinRateStrategy_NineCardThreeHandsDistinctTables()
        {
            var nineThreeRule = NineCardThreeHandsRule.Default;

            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var firstPair = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);
            var secondPair = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Pair, comp);
            var thirdPair = new BaseSubBattleHand(BattleSubHandOrder.ThirdHand, PokerOverAllHandRank.Pair, comp);

            double rate1 = IWinRateHandArrangeStrategy.GetSubHandWinRate(firstPair, BattleSubHandOrder.FirstHand, nineThreeRule);
            double rate2 = IWinRateHandArrangeStrategy.GetSubHandWinRate(secondPair, BattleSubHandOrder.SecondHand, nineThreeRule);
            double rate3 = IWinRateHandArrangeStrategy.GetSubHandWinRate(thirdPair, BattleSubHandOrder.ThirdHand, nineThreeRule);

            // In 9 cards 3 hands:
            // 1st hand (1 card) pair rate is extremely high (~0.934 - 0.9999)
            // 2nd hand (3 cards) pair rate is high (~0.579 - 0.968)
            // 3rd hand (5 cards) pair rate is lower (~0.012 - 0.344)
            Assert.That(rate1, Is.GreaterThan(rate2));
            Assert.That(rate2, Is.GreaterThan(rate3));
            Assert.That(rate1, Is.GreaterThan(0.95));
            Assert.That(rate2, Is.GreaterThan(0.50));
            Assert.That(rate3, Is.LessThan(0.40));
        }

        [Test]
        public void TestWinRateStrategy_GetSubHandWinRateWithRuleParameter()
        {
            var card1 = BasePokerCard.CreateInstance("A♠️");
            var card2 = BasePokerCard.CreateInstance("A❤️");
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };
            var hand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);

            double eightRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(hand, BattleSubHandOrder.FirstHand, EightCardRule.Default);
            double nineTwoRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(hand, BattleSubHandOrder.FirstHand, NineCardTwoHandsRule.Default);
            double nineThreeRate = IWinRateHandArrangeStrategy.GetSubHandWinRate(hand, BattleSubHandOrder.FirstHand, NineCardThreeHandsRule.Default);

            Assert.That(eightRate, Is.GreaterThan(0.70));
            Assert.That(nineTwoRate, Is.GreaterThan(0.50));
            Assert.That(nineThreeRate, Is.GreaterThan(0.90));
        }
    }
}
