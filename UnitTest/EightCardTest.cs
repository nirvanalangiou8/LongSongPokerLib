using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
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
                "FiveCardsStraight:1"
            },
            new object[]
            {
                "8♠️,8♣️,8❤️,8🔶,9❤️,9🔶,5♣️,5🔶", 
                "FourOfKind_Pair*2:1,ThreeOfKind_Pair*2:4,Pair*4:6"
            },
            new object[]
            {
                "10♠️,6♣️,10♣️,A🔶,5❤️,6🔶,A♣️,7🔶", 
                "Pair*3:1"
            },
            new object[]
            {
                "10♠️,6♣️,10♣️,A🔶,5❤️,6🔶,A♠️,7🔶", 
                "Pair*3:1"
            },
            new object[]
            {
                "10♠️,9♣️,8♣️,10🔶,9❤️,6❤️,5♠️,4🔶", 
                "Pair*2:1"
            },
        };
        
        private static readonly object[] TestMinFlushStraight3_Deck2 =
        {
            
            new object[]
            {
                "J♣️,J♣️@2,3♣️,5♣️,5♣️@2,A♠️,8❤️,K🔶",  
                "FiveCardsFlush:1,Pair*2:1"
            },
            new object[]
            {
                "J♣️,J♣️@2,3♣️,5♣️,5♣️@2,3♣️@2,8♣️,8♣️@2",  
                "EightCardsFlush:1,SevenCardsFlush:4,SixCardsFlush_Pair:4,SixCardsFlush:6,FiveCardsFlush_Pair:12,FiveCardsFlush:4,Pair*4:1"
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
