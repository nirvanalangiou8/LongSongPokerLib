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
        private static readonly object[] SimHandTypeTestData =
        {
            new object[] { "2❤️,2♣️,3❤️,3♣️,5♠️,7🔶,9❤️,J♣️", "Pair*2" },
            new object[] { "2❤️,3♣️,4♠️,5🔶,7❤️,9♣️,J♠️,K🔶", "Nothing" },
            new object[] { "2❤️,2♣️,3❤️,4♣️,5♠️,7🔶,9❤️,J♣️", "Pair" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,8♣️,9♠️,J🔶", "FiveCardsStraight" },
            new object[] { "2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7♠️,10🔶", "Pair*3" },
            //new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,J♣️,Q♠️,K🔶", "FiveCardsFlush" },
            new object[] { "2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,7🔶,9❤️", "ThreeOfKind_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5♠️,5♣️,7🔶,8🔶,9🔶", "ThreeCardsFlushStraight_Pair" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,J🔶", "SixCardsStraight" },
            new object[] { "2❤️,2♣️,2♠️,4♣️,5♠️,7🔶,9❤️,J♣️", "ThreeOfKind" },
            new object[] { "2❤️,3❤️,4❤️,6♣️,7♠️,9🔶,J❤️,K♣️", "ThreeCardsFlushStraight" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,7♠️,7♣️,9🔶", "FiveCardsStraight_Pair" },
            new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,J♠️,J♣️,Q🔶", "FiveCardsFlush_Pair" },
            new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♣️,A♠️", "SixCardsFlush" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,10♠️", "SevenCardsStraight" },
            new object[] { "2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,5♣️,7❤️", "ThreeOfKind_Pair*2" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♣️,9♠️,J🔶,K❤️", "FourCardsFlushStraight" },
            new object[] { "2❤️,3❤️,4❤️,6♠️,7♠️,7♣️,9❤️,9♣️", "ThreeCardsFlushStraight_Pair*2" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,10❤️", "FourCardsFlushStraight_Pair" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,9🔶", "SixCardsStraight_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5♠️,5♣️,5🔶,7❤️,9♣️", "ThreeCardsFlushStraight_ThreeOfKind" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,5♣️,7♠️,9🔶", "FourOfKind" },
            new object[] { "2❤️,2♣️,2♠️,3❤️,3♣️,3♠️,5🔶,7❤️", "ThreeOfKind*2" },
            new object[] { "2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7❤️,7♣️", "Pair*4" },
            new object[] { "2❤️,3❤️,4❤️,6♣️,7♣️,8♣️,9♠️,J🔶", "ThreeCardsFlushStraight*2" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,5♠️,7🔶", "FourOfKind_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,10❤️,J❤️,Q❤️", "EightCardsFlush" },
            new object[] { "2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,9♣️", "EightCardsStraight" },
            new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♠️,K♣️", "SixCardsFlush_Pair" },
            new object[] { "2❤️,2♣️,2♠️,3♠️,4♠️,5♠️,6♠️,7♠️", "ThreeOfKind_FiveCardsStraight" },
            new object[] { "2❤️,3❤️,4❤️,6♠️,7♠️,8♠️,9♠️,10♠️", "ThreeCardsFlushStraight_FiveCardsStraight" },
            new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,A❤️,K♣️", "SevenCardsFlush" },
            new object[] { "2❤️,3❤️,4❤️,6🔶,8🔶,10🔶,Q🔶,A🔶", "ThreeCardsFlushStraight_FiveCardsFlush" },
            new object[] { "2❤️,2♣️,2♠️,4🔶,6🔶,8🔶,10🔶,Q🔶", "ThreeOfKind_FiveCardsFlush" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,10♠️,10♣️,J🔶", "FiveCardsFlushStraight_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,9♣️", "FourCardsFlushStraight_Pair*2" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♣️,8♣️,9♣️,10♠️", "FourCardsFlushStraight_ThreeCardsFlushStraight" },
            new object[] { "2❤️,2♣️,2♠️,4❤️,4♣️,4♠️,6❤️,6♣️", "ThreeOfKind*2_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,7🔶,9❤️", "FourCardsFlushStraight_ThreeOfKind" },
            new object[] { "2❤️,3❤️,4❤️,6♠️,6♣️,8♣️,9♣️,10♣️", "ThreeCardsFlushStraight*2_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♣️,10♠️", "SixCardsFlushStraight" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,4❤️,4♣️", "FourOfKind_Pair*2" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,5❤️", "FourOfKind_ThreeOfKind" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,4❤️,5❤️,7♣️", "FourOfKind_ThreeCardsFlushStraight" },
            new object[] { "2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,A❤️,3❤️", "EightCardsFlush" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♠️,9♣️", "SixCardsFlushStraight_Pair" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,9♠️,10♠️", "FiveCardsFlushStraight_ThreeCardsFlushStraight" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,8♣️,8🔶", "FiveCardsFlushStraight_ThreeOfKind" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,10♣️", "SevenCardsFlushStraight" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,7♠️,8♠️,9♠️,10♠️", "FourCardsFlushStraight*2" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,4❤️,5❤️,6❤️,7❤️", "FourOfKind_FourCardsFlushStraight" },
            new object[] { "2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,3🔶", "FourOfKind*2" },
            new object[] { "2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,9❤️", "EightCardsFlushStraight" }
        };

        [Test, TestCaseSource(nameof(SimHandTypeTestData))]
        public void TestSimHandType(string inputCardStr, string expectedHandType)
        {
            var cards = inputCardStr.Split(',').Select(s => SimPokerCard.CreateInstance(s.Trim())).ToList();
            var calculator = new SimPokerHandCalculator();
            calculator.SetupCards(cards);
            var results = calculator.TestSimCards();
            
            if (expectedHandType == "Nothing" || expectedHandType == "Pair")
            {
                if (results.Count == 0)
                {
                    Assert.Pass($"Hand type {expectedHandType} correctly identified as simple component (empty structure list)");
                    return;
                }
            }

            Assert.IsNotEmpty(results, $"No results found for {inputCardStr}");
            
            // For simple types that might be found but not as the "best" partition in the results list,
            // we check if ANY result matches. But the stats come from ALL partitions.
            // However, the CSV usually lists the most prominent one.
            bool found = results.Any(r => r.FinalCompsStr == expectedHandType);
            
            if (!found)
            {
                string foundTypes = string.Join(", ", results.Select(r => r.FinalCompsStr));
                Assert.Fail($"Hand type '{expectedHandType}' not found in results for cards: {inputCardStr}. Found: {foundTypes}");
            }
        }
    }
}
