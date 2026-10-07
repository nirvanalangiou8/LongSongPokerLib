using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class ConsoleGameTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            XRandom.Init(12345678uL);
        }

        [Test]
        public void TestCardRulesUnderGenericPoker()
        {
            ICardRule eightCardRule = EightCardRule.Default;
            Assert.That(eightCardRule.CardCount, Is.EqualTo(8));
            Assert.That(eightCardRule.MinStraightCount, Is.EqualTo(5));
            Assert.That(eightCardRule.MinFlushCount, Is.EqualTo(5));
            Assert.That(eightCardRule.MinFlushStraightCount, Is.EqualTo(3));
            Assert.That(eightCardRule.MinKindCount, Is.EqualTo(2));

            ICardRule nineCardRule = NineCardRule.Default;
            Assert.That(nineCardRule.CardCount, Is.EqualTo(9));
            Assert.That(nineCardRule.MinStraightCount, Is.EqualTo(5));
            Assert.That(nineCardRule.MinFlushCount, Is.EqualTo(5));
            Assert.That(nineCardRule.MinFlushStraightCount, Is.EqualTo(3));
            Assert.That(nineCardRule.MinKindCount, Is.EqualTo(2));
        }

        [Test]
        public void TestConsoleCardDealer()
        {
            var dealer = new ConsoleCardDealer(cardDecks: 1, rule: EightCardRule.Default);
            Assert.That(dealer.TotalCards, Is.EqualTo(52));
            Assert.That(dealer.RemainingCards.Count, Is.EqualTo(52));
            Assert.That(dealer.DiscardCards.Count, Is.EqualTo(0));

            var player = new ConsolePlayer("Player1", EightCardRule.Default);
            dealer.DealCards(player, 8);

            Assert.That(player.Cards.Count, Is.EqualTo(8));
            Assert.That(dealer.RemainingCards.Count, Is.EqualTo(44));

            dealer.CollectCards(player);
            Assert.That(player.Cards.Count, Is.EqualTo(0));
            Assert.That(dealer.DiscardCards.Count, Is.EqualTo(8));
        }

        [Test]
        public void TestConsoleCardDealerReshuffle()
        {
            var dealer = new ConsoleCardDealer(cardDecks: 1, rule: EightCardRule.Default);
            var player = new ConsolePlayer("Player1");

            // Deal 6 rounds of 8 cards = 48 cards dealt, 4 remaining in deck
            for (int i = 0; i < 6; i++)
            {
                dealer.DealCards(player, 8);
                dealer.CollectCards(player);
            }

            Assert.That(dealer.RemainingCards.Count, Is.EqualTo(4));
            Assert.That(dealer.DiscardCards.Count, Is.EqualTo(48));

            // Requesting 8 cards when only 4 remaining triggers recycle & reshuffle
            dealer.DealCards(player, 8);
            Assert.That(player.Cards.Count, Is.EqualTo(8));
            Assert.That(dealer.RemainingCards.Count, Is.EqualTo(52 - 8));
        }

        [Test]
        public void TestConsolePlayerEvaluateBestSplitHand()
        {
            var player = new ConsolePlayer("TestPlayer");
            var handResult = ConsolePlayer.EvaluateBestSplitHand("A♣️,A❤️,A♠️,8❤️,8🔶,5♣️,3♣️,2🔶");

            Assert.That(handResult, Is.Not.Null);
            Assert.That(handResult.BackHand, Is.Not.Null);
            Assert.That(handResult.FrontHand, Is.Not.Null);
            Assert.That(handResult.BackHand.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.ThreeOfKind));
            Assert.That(handResult.FrontHand.BattleHandRank, Is.EqualTo(PokerOverAllHandRank.Pair));
            Assert.That(handResult.TotalScore, Is.GreaterThan(0));
        }

        [Test]
        public void TestConsolePlayManager()
        {
            var gameManager = new ConsolePlayManager(EightCardRule.Default, cardsPerPlayer: 8);
            var player = new ConsolePlayer("Player1", EightCardRule.Default);
            gameManager.AddPlayer(player);

            Assert.That(gameManager.Players.Count, Is.EqualTo(1));
            Assert.That(gameManager.CurrentRound, Is.EqualTo(0));

            gameManager.StartNewRound();
            Assert.That(gameManager.CurrentRound, Is.EqualTo(1));
            Assert.That(player.Cards.Count, Is.EqualTo(8));

            var result = player.EvaluateBestSplitHand();
            Assert.That(result, Is.Not.Null);

            gameManager.StartNewRound();
            Assert.That(gameManager.CurrentRound, Is.EqualTo(2));
            Assert.That(player.Cards.Count, Is.EqualTo(8));
        }
    }
}
