
using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;
 

namespace EightCardsProbTest
{
    [TestFixture]
    public class EightCardArrangeHandTest
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

        
        private static readonly object[] Test_Deck2 =
        {
            
            new object[]
            {
                "J♣️,J♣️@2,3♣️,5❤️,6🔶,A♠️,9❤️,K♠️",  
                "FiveCardsTwoPairsInFlush:1,FourCardsTwoPairsInFlush:1,FourCardsPairInFlush:2," +
                "ThreeCardsPairInFlush:2,ThreeCardsPairInFlush_Pair:2,ThreeCardsFlush:1,Pair*2:1"
            },
        };
        
        [Test, TestCaseSource(nameof(Test_Deck2))]
        public void Test1_Deck2(string inputCardStr, string  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            pokerHand.MinFlushStraightCards = 3;
            var resHand = pokerHand.Test8CardsTwoHandsDeploy();
            //var actualStr = ConvertFinalString(handRes);
            //Assert.That(actualStr, Is.EqualTo(expected));
        }
    }
}
