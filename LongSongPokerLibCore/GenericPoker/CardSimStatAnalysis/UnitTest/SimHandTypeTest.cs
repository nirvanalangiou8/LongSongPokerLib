using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SimHandTypeTest
    {
        private static readonly (string Cards, string Expected)[] EightCardRawTestData =
        {
            ("2❤️,2♣️,3❤️,3♣️,4♠️,7🔶,7❤️,8♣️", "Pair*3"),
            ("2❤️,2♣️,3❤️,3♣️,5♠️,7🔶,9❤️,J♣️", "Pair*2"),
            ("2❤️,3♣️,4♠️,5🔶,7❤️,9♣️,J♠️,K🔶", "Nothing"),
            ("2❤️,2♣️,3❤️,4♣️,5♠️,7🔶,9❤️,J♣️", "Pair"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,8♣️,9♠️,J🔶", "FiveCardsStraight"),
            ("2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7♠️,10🔶", "Pair*3"),
            //("2❤️,4❤️,6❤️,8❤️,10❤️,J♣️,Q♠️,K🔶", "FiveCardsFlush"),
            ("2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,7🔶,9❤️", "ThreeOfKind_Pair"),
            ("2❤️,3❤️,4❤️,5♠️,5♣️,7🔶,8🔶,9🔶", "ThreeCardsFlushStraight*2_Pair"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,J🔶", "SixCardsStraight"),
            ("2❤️,2♣️,2♠️,4♣️,5♠️,7🔶,9❤️,J♣️", "ThreeOfKind"),
            ("2❤️,3❤️,4❤️,6♣️,7♠️,9🔶,J❤️,K♣️", "ThreeCardsFlushStraight"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,7♠️,7♣️,9🔶", "SixCardsStraight,FiveCardsStraight_Pair"),
            ("2❤️,4❤️,6❤️,8❤️,10❤️,J♠️,J♣️,Q🔶", "FiveCardsFlush_Pair"),
            ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♣️,A♠️", "SixCardsFlush"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,10♠️", "SevenCardsStraight"),
            ("2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,5♣️,7❤️", "ThreeOfKind_Pair*2"),
            ("2❤️,3❤️,4❤️,5❤️,7♣️,9♠️,J🔶,K❤️", "FourCardsFlushStraight,FiveCardsFlush"),
            ("2❤️,3❤️,4❤️,6♠️,7♠️,7♣️,9❤️,9♣️", "ThreeCardsFlushStraight_Pair*2"),
            ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,10❤️", "FourCardsFlushStraight_Pair,FiveCardsFlush_Pair"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,9🔶", "SixCardsStraight_Pair"),
            ("2❤️,3❤️,4❤️,5♠️,5♣️,5🔶,7❤️,9♣️", "ThreeCardsFlushStraight_ThreeOfKind"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,5♣️,7♠️,9🔶", "FourOfKind"),
            ("2❤️,2♣️,2♠️,3❤️,3♣️,3♠️,5🔶,7❤️", "ThreeOfKind*2"),
            ("2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7❤️,7♣️", "Pair*4"),
            ("2❤️,3❤️,4❤️,6♣️,7♣️,8♣️,9♠️,J🔶", "ThreeCardsFlushStraight*2"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,5♠️,7🔶", "FourOfKind_Pair"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,10❤️,J❤️,Q❤️", "EightCardsFlush,FiveCardsFlushStraight_ThreeCardsFlushStraight"),
            ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,9♣️", "EightCardsStraight"),
            ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♠️,K♣️", "SixCardsFlush_Pair"),
            ("2❤️,2♣️,2♠️,3♠️,4♠️,5♠️,6♠️,7♠️", "SixCardsFlushStraight_Pair,FiveCardsFlushStraight_ThreeOfKind"),
            ("2❤️,3❤️,4❤️,6♠️,7♠️,8♠️,9♠️,10♠️", "FiveCardsFlushStraight_ThreeCardsFlushStraight"),
            ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,A❤️,K♣️", "SevenCardsFlush"),
            ("2❤️,3❤️,4❤️,6🔶,8🔶,10🔶,Q🔶,A🔶", "ThreeCardsFlushStraight_FiveCardsFlush"),
            ("2❤️,2♣️,2♠️,4🔶,6🔶,8🔶,10🔶,Q🔶", "ThreeOfKind_FiveCardsFlush"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,10♠️,10♣️,J🔶", "FiveCardsFlushStraight_Pair"),
            ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,9♣️", "FourCardsFlushStraight_Pair*2"),
            ("2❤️,3❤️,4❤️,5❤️,7♣️,8♣️,9♣️,10♠️", "FourCardsFlushStraight_ThreeCardsFlushStraight"),
            ("2❤️,2♣️,2♠️,4❤️,4♣️,4♠️,6❤️,6♣️", "ThreeOfKind*2_Pair"),
            ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,7🔶,9❤️", "FourCardsFlushStraight_ThreeOfKind,ThreeOfKind_FiveCardsFlush"),
            ("2❤️,3❤️,4❤️,6♠️,6♣️,8♣️,9♣️,10♣️", "ThreeCardsFlushStraight*2_Pair"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♣️,10♠️", "SixCardsFlushStraight"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,4❤️,4♣️", "FourOfKind_Pair*2,ThreeCardsFlushStraight*2_Pair,ThreeCardsFlushStraight_ThreeOfKind"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,5❤️", "FourOfKind_ThreeOfKind"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,4❤️,5❤️,7♣️", "FourOfKind_ThreeCardsFlushStraight,FourCardsFlushStraight_ThreeOfKind"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♠️,9♣️", "SixCardsFlushStraight_Pair"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,9♠️,10♠️", "FiveCardsFlushStraight_ThreeCardsFlushStraight"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,8♣️,8🔶", "FiveCardsFlushStraight_ThreeOfKind"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,10♣️", "SevenCardsFlushStraight"),
            ("2❤️,3❤️,4❤️,5❤️,7♠️,8♠️,9♠️,10♠️", "FourCardsFlushStraight*2"),
            ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,3🔶", "FourOfKind*2"),
            ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,9❤️", "EightCardsFlushStraight"),
            ("A❤️,2❤️,3❤️,4❤️,5❤️,K❤️,Q❤️,J❤️", "EightCardsFlush,FiveCardsFlushStraight_ThreeCardsFlushStraight,FourCardsFlushStraight*2"),
            ("A❤️,2♣️,3♣️,4❤️,5♠️,J❤️,K♠️,Q❤️", "FiveCardsStraight"),
            // These are complicated and challenging cases and may need to adjust the result later.
            // Temporarily below are intentionally make it test result compare failed due to we dont know if those rank order in 8 cards are still valid for 9 or 10 cards.
            // will come back to later to review this.
            ("8♠️,9♠️,10♠️,10🔶,J🔶,Q♣️,Q🔶,Q♠️", "ThreeCardsFlushStraight*2_Pair,ThreeCardsFlushStraight_ThreeOfKind,FiveCardsStraight_Pair"),
            ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,A❤️,3❤️", "EightCardsFlush"),
            ("2❤️,2♣️,2♠️,2🔶,4❤️,5❤️,6❤️,7❤️", "FourOfKind_FourCardsFlushStraight"),
            
        };
        
        private static readonly (string Cards, string Expected)[] NineCardRawTestData =
        {
            ("7♠️,5❤️,9♠️,4🔶,2♠️,10❤️,8❤️,6❤️,3🔶", "NineCardsStraight"),
        };
            
        

        public static IEnumerable<TestCaseData> EightCardSimHandTypeTestData
        {
            get
            {
                for (int i = 0; i < EightCardRawTestData.Length; i++)
                {
                    var (cards, expected) = EightCardRawTestData[i];
                    yield return new TestCaseData(cards, expected)
                        .SetProperty("Order", i)
                        .SetName($"[{i + 1:D2}] {expected} ({cards})");
                }
            }
        }
        
        public static IEnumerable<TestCaseData> NineCardSimHandTypeTestData
        {
            get
            {
                for (int i = 0; i < NineCardRawTestData.Length; i++)
                {
                    var (cards, expected) = NineCardRawTestData[i];
                    yield return new TestCaseData(cards, expected)
                        .SetProperty("Order", i)
                        .SetName($"[{i + 1:D2}] {expected} ({cards})");
                }
            }
        }

        [Test, TestCaseSource(nameof(EightCardSimHandTypeTestData))]
        public void EightCardTestSimHandType(string inputCardStr, string expectedHandType)
        {
            var cards = inputCardStr.Split(',').Select(s => SimPokerCard.CreateInstance(s.Trim())).ToList();
            //var calculator = new SimPokerHandCalculator();
            var calculator = new SimStatEstimator();
            calculator.SetupCards(cards);
            var results = calculator.TestSimCards();

            //"FourOfKind_Pair*2, ThreeCardsFlushStraight*2_Pair, ThreeCardsFlushStraight*2_Pair, ThreeOfKind, ThreeOfKind";
            string runStr = string.Join(",", results.Select(r => r.FinalCompsStr));

            Assert.That(runStr, Is.EqualTo(expectedHandType));
        }
        
        [Test, TestCaseSource(nameof(NineCardSimHandTypeTestData))]
        public void NineCardTestSimHandType(string inputCardStr, string expectedHandType)
        {
            var cards = inputCardStr.Split(',').Select(s => SimPokerCard.CreateInstance(s.Trim())).ToList();
            //var calculator = new SimPokerHandCalculator();
            var calculator = new SimStatEstimator();
            calculator.SetupCards(cards);
            var results = calculator.TestSimCards();

            //"FourOfKind_Pair*2, ThreeCardsFlushStraight*2_Pair, ThreeCardsFlushStraight*2_Pair, ThreeOfKind, ThreeOfKind";
            string runStr = string.Join(",", results.Select(r => r.FinalCompsStr));

            Assert.That(runStr, Is.EqualTo(expectedHandType));
        }
    }
}
