using System.Collections.Generic;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class BaseSubBattleHandTest
    {
        [Test]
        public void TestBaseSubBattleHand_InitializationAndComponents()
        {
            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var hand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);

            Assert.That(hand.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(hand.BattleSubHandOrder, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(hand.Components.Count, Is.EqualTo(1));
            Assert.That(hand.Cards.Count, Is.EqualTo(2));
            Assert.That(hand.Cards[0], Is.EqualTo(card1));
            Assert.That(hand.Cards[1], Is.EqualTo(card2));
            Assert.That(hand.HandPower, Is.EqualTo(PokerConst.EightCardsBattleHandPowerDict[(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair)]));
        }

        [Test]
        public void TestBaseSubBattleHand_AddMinorCards()
        {
            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var hand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);

            var kicker1 = BasePokerCard.CreateInstance("5🔶");
            var kicker2 = BasePokerCard.CreateInstance("3♣️");
            var remaining = hand.AddMinorCards(new List<BasePokerCard> { kicker1, kicker2 });

            // FirstHand has max 3 cards
            Assert.That(hand.Cards.Count, Is.EqualTo(3));
            Assert.That(hand.Cards[2], Is.EqualTo(kicker1));
            Assert.That(remaining, Is.EqualTo(1));
            //Assert.That(remaining[0], Is.EqualTo(kicker2));
        }

        [Test]
        public void TestBaseSubBattleHand_ComparisonAndEquality()
        {
            var card1 = BasePokerCard.CreateInstance("10♠️");
            var card2 = BasePokerCard.CreateInstance("10❤️");
            var comp1 = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };

            var pairHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Pair, comp1);
            var twoPairHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.TwoPairs, comp1);

            Assert.That(twoPairHand > pairHand, Is.True);
            Assert.That(pairHand < twoPairHand, Is.True);
            Assert.That(pairHand.CompareTo(twoPairHand), Is.LessThan(0));

            var equalPairHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Pair, comp1);
            Assert.That(pairHand.Equals(equalPairHand), Is.True);
            Assert.That(pairHand == equalPairHand, Is.True);
        }

        [Test]
        public void TestBaseSubBattleHand_GetHandString()
        {
            var card1 = BasePokerCard.CreateInstance("A♠️");
            var card2 = BasePokerCard.CreateInstance("K♠️");
            var hand = new BaseSubBattleHand(new List<BasePokerCard> { card1, card2 });

            Assert.That(hand.GetHandString(","), Is.EqualTo("A♠️,K♠️"));

            // Test with components and kickers
            var comp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { card1, card2 }
            };
            var kicker = BasePokerCard.CreateInstance("5🔶");
            var handWithComp = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Pair, comp);
            handWithComp.AddMinorCards(new List<BasePokerCard> { kicker });

            Assert.That(handWithComp.GetHandString(","), Is.EqualTo("[A♠️,K♠️],5🔶"));
            Assert.That(handWithComp.GetHandString(",", "(", ")"), Is.EqualTo("(A♠️,K♠️),5🔶"));
        }

        [Test]
        public void TestBaseSubBattleHand_DesignatedCardCount_DefaultRule()
        {
            var frontHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing);
            var backHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing);

            Assert.That(frontHand.DesignatedCardCount, Is.EqualTo(3));
            Assert.That(backHand.DesignatedCardCount, Is.EqualTo(5));
        }

        /*
        [Test]
        public void TestBaseSubBattleHand_DesignatedCardCount_NineCardRule()
        {
            var rule = NineCardThreeHandRule.Default;
            int frontCount = rule.SubHandsCardCount[BattleSubHandOrder.FirstHand];
            int backCount = rule.SubHandsCardCount[BattleSubHandOrder.SecondHand];
            var frontHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing, frontCount);
            var backHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Nothing, backCount);

            Assert.That(frontHand.DesignatedCardCount, Is.EqualTo(4));
            Assert.That(backHand.DesignatedCardCount, Is.EqualTo(5));
        }*/

        [Test]
        public void TestBaseSubBattleHand_DesignatedCardCount_CustomRule()
        {
            var customRule = new EightCardRule
            {
                SubHandsCardCount = new Dictionary<BattleSubHandOrder, int>
                {
                    { BattleSubHandOrder.FirstHand, 2 },
                    { BattleSubHandOrder.SecondHand, 6 },
                    { BattleSubHandOrder.ThirdHand, 4 }
                }
            };

            var firstHand = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, customRule.SubHandsCardCount[BattleSubHandOrder.FirstHand]);
            var secondHand = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, customRule.SubHandsCardCount[BattleSubHandOrder.SecondHand]);
            var thirdHand = new BaseSubBattleHand(BattleSubHandOrder.ThirdHand, customRule.SubHandsCardCount[BattleSubHandOrder.ThirdHand]);

            Assert.That(firstHand.DesignatedCardCount, Is.EqualTo(2));
            Assert.That(secondHand.DesignatedCardCount, Is.EqualTo(6));
            Assert.That(thirdHand.DesignatedCardCount, Is.EqualTo(4));

            var kickers = new List<BasePokerCard>
            {
                BasePokerCard.CreateInstance("A♠️"),
                BasePokerCard.CreateInstance("K♠️"),
                BasePokerCard.CreateInstance("Q♠️")
            };

            int added = firstHand.AddMinorCards(kickers);
            Assert.That(added, Is.EqualTo(2));
            Assert.That(firstHand.Cards.Count, Is.EqualTo(2));
            Assert.That(kickers.Count, Is.EqualTo(1));
        }
    }
}
