using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.SevenCard;
using NUnit.Framework;

namespace SevenCardsProbTest
{
    [TestFixture]
    public class SevenCardTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            XRandom.Init(123);
        }

        [Test]
        public void TestSevenCardKinds()
        {
            var cardStr = "3♠️,3❤️,3♣️,4❤️,4♠️,5❤️,6❤️";
            var calculator = SevenCardHandCalculator.CreateInstance(cardStr);
            var threeOfKind = calculator.GetAllKindGroups(3);
            var pairs = calculator.GetAllKindGroups(2);

            Assert.That(threeOfKind.Count, Is.EqualTo(1));
            Assert.That(threeOfKind[0].CompRank, Is.EqualTo(EightCardsCompType.ThreeOfKind));
            
            // Should find 3-pair and 4-pair (Total 2)
            Assert.That(pairs.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestSevenCardDealer()
        {
            var dealer = new ConsoleCardDealer<SevenCardPokerCard>(1, false);
            var player = new ConsolePlayer<SevenCardPokerCard>("TestPlayer");
            dealer.DealCards(player, 7);
            
            Assert.That(player.Cards.Count, Is.EqualTo(7));
            Assert.That(dealer.RemainingCards.Count, Is.EqualTo(52 - 7));
        }
    }
}
