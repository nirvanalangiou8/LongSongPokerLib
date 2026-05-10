using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using GenericPoker.FourCard;
using NUnit.Framework;
 

namespace EightCardsProbTest
{
    [TestFixture]
    public class EightCardTest
    {
        public string ConvertFinalString(List<PokerHandStructure> inputComps)
        {
            Dictionary<string, int> statDict = new Dictionary<string, int>();
            
            foreach (var Comp in inputComps)
            {
                if (statDict.ContainsKey(Comp.FinalCompsStr))
                {
                    statDict[Comp.FinalCompsStr] += 1; // Increment by 1 if the key exists
                }
                else
                {
                    statDict[Comp.FinalCompsStr] = 1; // Set to 1 if the key doesn't exist
                }
            }
            List<string> CompStrs = new List<string>();
            foreach(var kvp in statDict)
            {
                CompStrs.Add($"{kvp.Key}:{kvp.Value}");
            }
            return string.Join(",", CompStrs); 
        }    

        
        private static readonly object[] TestMinFlushStraight3 =
        {
            new object[]
            {
                "J♣️,Q♠️,3🔶,5♣️,4❤️,A♣️,2❤️,K❤️",  
                "FiveCardsStraight_ThreeCardsStraight:1,FourCardStraight*2:1,FourCardStraight_ThreeCardsStraight:5," +
                "ThreeCardsStraight*2:5,ThreeCardsStraight_ThreeCardsFlush:1,ThreeCardsFlush*2:1"
            },
            new object[]
            {
                "8♠️,8♣️,8❤️,8🔶,9❤️,9🔶,5♣️,5🔶", 
                "FourOfKind_Pair*2:1,ThreeOfKind_ThreeCardsFlush:1,ThreeOfKind_Pair*2:4,ThreeCardsFlush_Pair:3,Pair*4:6"
            },
            new object[]
            {
                "10♠️,6♣️,10♣️,A🔶,5❤️,6🔶,A♣️,7🔶", 
                "ThreeCardsStraight_ThreeCardsFlush:1,ThreeCardsStraight_Pair*2:2,ThreeCardsFlush*2:1,ThreeCardsFlush_Pair:1,Pair*3:1"
            },
            new object[]
            {
                "10♠️,6♣️,10♣️,A🔶,5❤️,6🔶,A♠️,7🔶", 
                "ThreeCardsStraight_Pair*2:2,ThreeCardsFlush_Pair:1,Pair*3:1"
            },
            new object[]
            {
                "10♠️,9♣️,8♣️,10🔶,9❤️,6❤️,5♠️,4🔶", 
                "ThreeCardsStraight*2:4,ThreeCardsStraight_Pair*2:1"
            },
        };
        
        private static readonly object[] TestMinFlushStraight3_Deck2 =
        {
            
            new object[]
            {
                "J♣️,J♣️@2,3♣️,5♣️,5♣️@2,A♠️,8❤️,K🔶",  
                "FiveCardsTwoPairsInFlush:1,FourCardsTwoPairsInFlush:1,FourCardsPairInFlush:2," +
                "ThreeCardsPairInFlush:2,ThreeCardsPairInFlush_Pair:2,ThreeCardsFlush:1,Pair*2:1"
            },
            new object[]
            {
                "J♣️,J♣️@2,3♣️,5♣️,5♣️@2,3♣️@2,8♣️,8♣️@2",  
                "EightCardsFourPairsInFlush:1,SevenCardsThreePairsInFlush:4,SixCardsThreePairsInFlush_Pair:4,SixCardsTwoPairsInFlush:6," +
                "FiveCardsTwoPairsInFlush_ThreeCardsPairInFlush:12,FiveCardsTwoPairsInFlush_Pair:12,FiveCardsPairInFlush_ThreeCardsFlush:4," +
                "FourCardsTwoPairsInFlush*2:3,FourCardsTwoPairsInFlush_ThreeCardsPairInFlush:12,FourCardsTwoPairsInFlush_Pair*2:6," +
                "FourCardsPairInFlush*2:6,FourCardsPairInFlush_ThreeCardsPairInFlush:24,FourCardsPairInFlush_ThreeCardsFlush:12," +
                "FourCardsPairInFlush_Pair:12,FourCardsFlush*2:1,FourCardsFlush_ThreeCardsFlush:4,ThreeCardsPairInFlush*2_Pair:12," +
                "ThreeCardsPairInFlush*2:12,ThreeCardsPairInFlush_ThreeCardsFlush:12,ThreeCardsPairInFlush_Pair*2:12," +
                "ThreeCardsFlush*2_Pair:4,ThreeCardsFlush*2:6,Pair*4:1"
            },
        };

        
        
        [Test, TestCaseSource(nameof(TestMinFlushStraight3))]
        public void Test1_Deck1(string inputCardStr, string  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            pokerHand.MinFlushStraightCards = 3;
            var handRes = pokerHand.Test8Cards();
            var actualStr = ConvertFinalString(handRes);
            Assert.That(actualStr, Is.EqualTo(expected));
        }
        
        
        [Test, TestCaseSource(nameof(TestMinFlushStraight3_Deck2))]
        public void Test1_Deck2(string inputCardStr, string  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            pokerHand.MinFlushStraightCards = 3;
            var handRes = pokerHand.Test8Cards();
            var actualStr = ConvertFinalString(handRes);
            Assert.That(actualStr, Is.EqualTo(expected));
        }
    }
}
