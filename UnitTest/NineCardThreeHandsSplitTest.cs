using System.Linq;
using GenericPoker;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class NineCardThreeHandsSplitTest
    {
        public static readonly object[] NineCardThreeHandTestCases =
        {
            // Case 1: Straight flush split across 3 tiers (1, 3, 5)
            new object[]
            {
                "8❤️,7❤️,6❤️,5❤️,4❤️,3❤️,2❤️,A❤️,9❤️",
                "{A❤️}#{[9❤️,8❤️,7❤️]}#{[6❤️,5❤️,4❤️,3❤️,2❤️]}"
            },
            // Case 2: One pair of 6s in middle hand, one pair of Ks in back hand, Ace high card in front hand
            new object[]
            {
                "8❤️,7❤️,6♣️,6🔶,J❤️,K♣️,K❤️,2♣️,A♣️",
                "{A♣️}#{[6🔶,6♣️],J❤️}#{[K❤️,K♣️],8❤️,7❤️,2♣️}"
            },
            // Case 3 (From issue_hands): Pair of Js in middle hand, Two pairs (3s and 2s) in back hand, Q high in front hand
            new object[]
            {
                "J♠️,2♣️,2❤️,Q♠️,3♣️,10🔶,3❤️,J♣️,7♣️",
                "{Q♠️}#{[J♠️,J♣️],10🔶}#{[3❤️,3♣️,2❤️,2♣️],7♣️}"
            },
            // Case 4 (From issue_hands): Three 6s in middle hand, 3-card straight flush in back hand, Ace high in front hand
            new object[]
            {
                "10❤️,4❤️,6♠️,6❤️,6♣️,9❤️,A♣️,8♣️,J❤️",
                "{A♣️}#{[6♠️,6❤️,6♣️]}#{[J❤️,10❤️,9❤️],8♣️,4❤️}"
            },
            // Case 5 (From issue_hands): High cards in front and middle, Two pairs (Ks and 3s) in back hand
            new object[]
            {
                "Q♣️,J🔶,3♠️,3🔶,4♠️,9♣️,7❤️,K❤️,K🔶",
                "{Q♣️}#{[3♠️,3🔶],J🔶}#{[K❤️,K🔶],9♣️,7❤️,4♠️}"
            },
            // Case 6 (From issue_hands): High cards in front and middle, Two pairs (8s and 5s) in back hand
            new object[]
            {
                "A❤️,10♣️,8❤️,2♠️,5♠️,4🔶,K♠️,5🔶,8🔶",
                "{K♠️}#{A❤️,10♣️,4🔶}#{[8❤️,8🔶,5♠️,5🔶],2♠️}"
            },
            // Case 7 (From issue_hands): Pair of Qs in middle hand, 3-card straight flush (9-8-7) in back hand, Ace high in front
            new object[]
            {
                "8♠️,9♠️,A🔶,K♣️,Q❤️,7♠️,5♣️,2🔶,Q🔶",
                "{A🔶}#{[Q❤️,Q🔶],K♣️}#{[9♠️,8♠️,7♠️],5♣️,2🔶}"
            },
            // Case 8 (From issue_hands): High cards in front and middle, 5-card straight in back hand
            new object[]
            {
                "J♠️,5🔶,5♣️,3♠️,6♠️,2🔶,4♠️,4❤️,7❤️",
                "{5♣️}#{J♠️,4❤️}#{[7❤️,6♠️,5🔶,4♠️,3♠️,2🔶]}"
            },
            // Case 9 (From issue_hands): Three 6s in middle hand, 3-card straight flush (J-10-9) in back hand, King high in front
            new object[]
            {
                "6♣️,J❤️,9♣️,6🔶,K♠️,8🔶,6❤️,9❤️,10❤️",
                "{K♠️}#{[6❤️,6🔶,6♣️]}#{[J❤️,10❤️,9❤️],9♣️,8🔶}"
            },
            // Case 10 (From issue_hands): High cards in front and middle, Pair of Aces in back hand
            new object[]
            {
                "A🔶,4♣️,J♣️,5❤️,9🔶,K❤️,Q♣️,2♣️,A❤️",
                "{Q♣️}#{K❤️,J♣️,9🔶}#{[A❤️,A🔶],5❤️,4♣️,2♣️}"
            },
            // Case 11 (From issue_hands): 3 pairs distributed across middle (10s and 8s) and back (Qs and 2s)
            new object[]
            {
                "10♣️,2❤️,10♠️,8♠️,7♠️,8♣️,2♠️,Q❤️,Q♠️",
                "{[8♠️,8♣️]}#{[Q♠️,Q❤️],7♠️}#{[10♠️,10♣️,2♠️,2❤️]}"
            },
            // Case 12 (From issue_hands): Pair of 4s in middle hand, 5-card straight (9-8-7-6-5) in back hand, Queen high in front
            new object[]
            {
                "4♠️,9♠️,7♠️,J❤️,4♣️,6♣️,Q🔶,5❤️,8♠️",
                "{Q🔶}#{[4♠️,4♣️],J❤️}#{[9♠️,8♠️,7♠️,6♣️,5❤️]}"
            },
            // Case 13 (From issue_hands): High cards only distributed across all three hands
            new object[]
            {
                "5♠️,J♣️,8❤️,9♣️,4🔶,A♣️,K♠️,10🔶,3🔶",
                "{J♣️}#{K♠️,10🔶,9♣️}#{A♣️,8❤️,5♠️,4🔶,3🔶}"
            },
            // Case 14 (From issue_hands): Pair of 8s in middle hand, Three 7s in back hand, Ace high in front
            new object[]
            {
                "K🔶,7♠️,7🔶,7♣️,8♣️,8❤️,A🔶,5♣️,3🔶",
                "{A🔶}#{[8❤️,8♣️],K🔶}#{[7♠️,7🔶,7♣️],5♣️,3🔶}"
            },
            // Case 15: Three pairs + one 3 of kind. Front: 7 high; Middle: Ks and 8s Two Pairs; Back: Aces and 7s Two Pairs
            new object[]
            {
                "K🔶,7♠️,7🔶,7♣️,8♣️,8❤️,A🔶,A♣️,K♣️",
                "{[K🔶,K♣️]}#{[A🔶,A♣️]}#{[7♠️,7🔶,7♣️,8❤️,8♣️]}"
            }
        };

        [Test, TestCaseSource(nameof(NineCardThreeHandTestCases))]
        public void TestNineCardThreeHandCases(string inputCardStr, string expectedResult)
        {
            var rule = NineCardThreeHandsRule.Default;
            var battleHands = new BaseBattleHands(inputCardStr, rule);
            var result = battleHands.GetArrangeHands();

            string actualResult = string.Join("#", result.SubHands.Select(h => $"{{{h.GetHandString(",")}}}"));
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(result.IsValidDeployment(), Is.True);
        }
    }
}
