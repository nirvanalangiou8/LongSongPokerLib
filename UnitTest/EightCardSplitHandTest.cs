using System.Collections.Generic;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class EightCardSplitHandTest
    {
        public static readonly object[] SevenTestCases =
        {
            // Case 1
            new object[]
            {
                "J♣️,J🔶,3♣️,5🔶,6♣️,A♣️,A❤️,A♠️",
                "[J🔶,J♣️],6♣️#[A♠️,A❤️,A♣️],5🔶,3♣️"
            },
            // Case 2
            new object[]
            {
                "J♣️,J🔶,3♣️,6♣️,6❤️,A♣️,A❤️,A♠️",
                "[J🔶,J♣️],3♣️#[A♠️,A❤️,A♣️,6❤️,6♣️]"
            },
            // Case 3
            new object[]
            {
                "8❤️,7❤️,6❤️,5❤️,4❤️,3♣️,2♣️,A♣️",
                "[3♣️,2♣️,A♣️]#[8❤️,7❤️,6❤️,5❤️,4❤️]"
            },
            // Case 4
            new object[]
            {
                "8❤️,8🔶,6❤️,6🔶,4❤️,4♣️,2♠️,2♣️",
                "[6❤️,6🔶,4❤️,4♣️]#[8❤️,8🔶,2♠️,2♣️]"
            },
            // Case 5
            new object[]
            {
                "8❤️,7❤️,6♣️,6❤️,J❤️,K♣️,2♣️,A♣️",
                "A♣️,K♣️,J❤️#[8❤️,7❤️,6❤️],6♣️,2♣️"
            },
            // Case 6
            new object[]
            {
                "8❤️,7❤️,6♣️,4🔶,J❤️,K♣️,2♣️,A♣️",
                "K♣️,J❤️,8❤️#A♣️,7❤️,6♣️,4🔶,2♣️"
            },
            // Case 7
            new object[]
            {
                "8❤️,7❤️,6♣️,6🔶,J❤️,K♣️,2♣️,A♣️",
                "A♣️,K♣️,J❤️#[6🔶,6♣️],8❤️,7❤️,2♣️"
            },
            new object[]
            {
                "8❤️,7❤️,6❤️,5❤️,4❤️,3❤️,2❤️,A❤️",
                "[8❤️,7❤️,6❤️]#[5❤️,4❤️,3❤️,2❤️,A❤️]"
            },
            // Case 9 - Two pairs (10s and 9s) where Ace kicker should be placed in front
            new object[]
            {
                "8❤️,A🔶,9♣️,10🔶,7🔶,2♠️,9🔶,10♣️",
                "[9🔶,9♣️],A🔶#[10🔶,10♣️],8❤️,7🔶,2♠️"
            },
            new object[]
            {
                "A❤️,A🔶,A♣️,K🔶,J🔶,A♠️,8🔶,10♣️",
                "K🔶,J🔶,10♣️#[A♠️,A❤️,A🔶,A♣️],8🔶"
            },
            new object[]
            {
                "A❤️,A🔶,A♣️,5🔶,3🔶,A♠️,6🔶,2♣️",
                "[A♠️,A❤️],5🔶#[A🔶,A♣️],6🔶,3🔶,2♣️"
            }
            
        };

        [Test, TestCaseSource(nameof(SevenTestCases))]
        public void TestSevenCases(string inputCardStr, string expectedResult)
        {
            var battleHands = new BaseBattleHands(inputCardStr);
            var result = battleHands.GetArrangeHands();

            string actualResult = $"{result.FrontHand.GetHandString(",")}#{result.BackHand.GetHandString(",")}";
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
