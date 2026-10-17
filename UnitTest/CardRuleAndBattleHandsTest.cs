using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class CardRuleAndBattleHandsTest
    {
        [Test]
        public void TestEightCardRuleTopology()
        {
            var rule = EightCardRule.Default;
            Assert.That(rule.CardCount, Is.EqualTo(8));
            Assert.That(rule.TotalCardCount, Is.EqualTo(8));
            Assert.That(rule.SubHandSlots.Count, Is.EqualTo(2));
            
            Assert.That(rule.SubHandSlots[0].Order, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(rule.SubHandSlots[0].CardCount, Is.EqualTo(3));
            Assert.That(rule.SubHandSlots[0].SlotName, Is.EqualTo("Front"));

            Assert.That(rule.SubHandSlots[1].Order, Is.EqualTo(BattleSubHandOrder.SecondHand));
            Assert.That(rule.SubHandSlots[1].CardCount, Is.EqualTo(5));
            Assert.That(rule.SubHandSlots[1].SlotName, Is.EqualTo("Back"));

            Assert.That(rule.SubHandsCardCount[BattleSubHandOrder.FirstHand], Is.EqualTo(3));
            Assert.That(rule.SubHandsCardCount[BattleSubHandOrder.SecondHand], Is.EqualTo(5));
        }

        
        [Test]
        public void TestNineCardTwoHandRuleTopology()
        {
            var rule = NineCardTwoHandsRule.Default;
            Assert.That(rule.CardCount, Is.EqualTo(9));
            Assert.That(rule.TotalCardCount, Is.EqualTo(9));
            Assert.That(rule.SubHandSlots.Count, Is.EqualTo(2));

            Assert.That(rule.SubHandSlots[0].Order, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(rule.SubHandSlots[0].CardCount, Is.EqualTo(4));
            Assert.That(rule.SubHandSlots[0].SlotName, Is.EqualTo("Front"));

            Assert.That(rule.SubHandSlots[1].Order, Is.EqualTo(BattleSubHandOrder.SecondHand));
            Assert.That(rule.SubHandSlots[1].CardCount, Is.EqualTo(5));
            Assert.That(rule.SubHandSlots[1].SlotName, Is.EqualTo("Back"));
        }

        [Test]
        public void TestNineCardThreeHandRuleTopology()
        {
            var rule = NineCardThreeHandsRule.Default;
            Assert.That(rule.CardCount, Is.EqualTo(9));
            Assert.That(rule.TotalCardCount, Is.EqualTo(9));
            Assert.That(rule.SubHandSlots.Count, Is.EqualTo(3));

            Assert.That(rule.SubHandSlots[0].Order, Is.EqualTo(BattleSubHandOrder.FirstHand));
            Assert.That(rule.SubHandSlots[0].CardCount, Is.EqualTo(1));
            Assert.That(rule.SubHandSlots[0].SlotName, Is.EqualTo("Front"));

            Assert.That(rule.SubHandSlots[1].Order, Is.EqualTo(BattleSubHandOrder.SecondHand));
            Assert.That(rule.SubHandSlots[1].CardCount, Is.EqualTo(3));
            Assert.That(rule.SubHandSlots[1].SlotName, Is.EqualTo("Middle"));

            Assert.That(rule.SubHandSlots[2].Order, Is.EqualTo(BattleSubHandOrder.ThirdHand));
            Assert.That(rule.SubHandSlots[2].CardCount, Is.EqualTo(5));
            Assert.That(rule.SubHandSlots[2].SlotName, Is.EqualTo("Back"));
        }

        [Test]
        public void TestBaseBattleHandsContainerAndTopology()
        {
            var frontCards = new List<BasePokerCard>
            {
                BasePokerCard.CreateInstance("A♠️"),
                BasePokerCard.CreateInstance("K♠️"),
                BasePokerCard.CreateInstance("Q♠️")
            };
            var backCards = new List<BasePokerCard>
            {
                BasePokerCard.CreateInstance("10❤️"),
                BasePokerCard.CreateInstance("10♠️"),
                BasePokerCard.CreateInstance("10♣️"),
                BasePokerCard.CreateInstance("5🔶"),
                BasePokerCard.CreateInstance("6🔶")
            };

            var frontSubHand = new BaseSubBattleHand(frontCards, designatedCardCount: 3);
            frontSubHand.BattleSubHandOrder = BattleSubHandOrder.FirstHand;
            var backSubHand = new BaseSubBattleHand(backCards, designatedCardCount: 5);
            backSubHand.BattleSubHandOrder = BattleSubHandOrder.SecondHand;

            var battleHands = new BaseBattleHands(frontSubHand, backSubHand);

            Assert.That(battleHands.Count, Is.EqualTo(2));
            Assert.That(battleHands.FrontHand, Is.EqualTo(frontSubHand));
            Assert.That(battleHands.BackHand, Is.EqualTo(backSubHand));
            Assert.That(battleHands[0], Is.EqualTo(frontSubHand));
            Assert.That(battleHands[1], Is.EqualTo(backSubHand));
            Assert.That(battleHands[BattleSubHandOrder.FirstHand], Is.EqualTo(frontSubHand));
            Assert.That(battleHands[BattleSubHandOrder.SecondHand], Is.EqualTo(backSubHand));
            Assert.That(battleHands.SubHands.Count, Is.EqualTo(2));
            Assert.That(battleHands.ToList().Count, Is.EqualTo(2));
        }

        [Test]
        public void TestBaseBattleHandsThreeHandsValidation()
        {
            // 3-Tier Hand: Hand 0 (Nothing/High Card) <= Hand 1 (Pair) <= Hand 2 (ThreeOfKind)
            var hand2PairComp = new PokerCardComponent
            {
                CompRank = BaseCompType.Pair,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("K♠️"), BasePokerCard.CreateInstance("K❤️") }
            };
            var hand3ThreeComp = new PokerCardComponent
            {
                CompRank = BaseCompType.ThreeOfKind,
                Cards = new List<BasePokerCard> { BasePokerCard.CreateInstance("Q♠️"), BasePokerCard.CreateInstance("Q❤️"), BasePokerCard.CreateInstance("Q♣️") }
            };

            var subHand1 = new BaseSubBattleHand(BattleSubHandOrder.FirstHand, PokerOverAllHandRank.Nothing, 1);
            subHand1.AddMinorCards(new List<BasePokerCard> { BasePokerCard.CreateInstance("A♠️") });

            var subHand2 = new BaseSubBattleHand(BattleSubHandOrder.SecondHand, PokerOverAllHandRank.Pair, 3, hand2PairComp);
            subHand2.AddMinorCards(new List<BasePokerCard> { BasePokerCard.CreateInstance("2♣️") });

            var subHand3 = new BaseSubBattleHand(BattleSubHandOrder.ThirdHand, PokerOverAllHandRank.ThreeOfKind, 5, hand3ThreeComp);
            subHand3.AddMinorCards(new List<BasePokerCard> { BasePokerCard.CreateInstance("3🔶"), BasePokerCard.CreateInstance("4🔶") });

            var validHands = new BaseBattleHands(subHand1, subHand2, subHand3);
            Assert.That(validHands.Count, Is.EqualTo(3));
            Assert.That(validHands.FrontHand, Is.EqualTo(subHand1));
            Assert.That(validHands.MiddleHand, Is.EqualTo(subHand2));
            Assert.That(validHands.MidHand, Is.EqualTo(subHand2));
            Assert.That(validHands.BackHand, Is.EqualTo(subHand3));
            Assert.That(validHands.IsValidDeployment(), Is.True);

            // Foul Deployment: Hand 0 (ThreeOfKind) > Hand 1 (Pair)
            var foulHands = new BaseBattleHands(subHand3, subHand2, subHand1);
            Assert.That(foulHands.IsValidDeployment(), Is.False);
        }
    }
}
