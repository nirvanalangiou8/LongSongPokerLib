using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SplitHandBreakAndAssemblyTest
    {
        private static string SplitHandToString(string handName)
        {
            List<(SimCardOverAllHandRank Front, SimCardOverAllHandRank Back)> solutions;
            if (handName == "Nothing")
            {
                solutions = new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>
                {
                    (SimCardOverAllHandRank.Nothing, SimCardOverAllHandRank.Nothing)
                };
            }
            else
            {
                var components = InitEightCardHandSplitProbAna.ParseHandName(handName);
                solutions = InitEightCardHandSplitProbAna.SplitHand(components);
            }

            return string.Join(",", solutions.Select(s => $"[{s.Front},{s.Back}]"));
        }

        #region 8 Cards Test Cases

        [TestCase("Pair", "[Nothing,Pair]")]
        [TestCase("Pair*2", "[Nothing,TwoPairs],[Pair,Pair]")]
        [TestCase("Nothing", "[Nothing,Nothing]")]
        [TestCase("Pair*3", "[Pair,TwoPairs]")]
        [TestCase("ThreeOfKind", "[Nothing,ThreeOfKind]")]
        [TestCase("FiveCardsStraight", "[Nothing,FiveCardsStraight]")]
        [TestCase("ThreeOfKind_Pair", "[Nothing,FullHouse],[Pair,ThreeOfKind]")]
        [TestCase("FiveCardsFlush", "[Nothing,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight", "[Nothing,ThreeCardsFlushStraight]")]
        [TestCase("ThreeCardsFlushStraight_Pair", "[Nothing,Mansion],[Pair,ThreeCardsFlushStraight]")]
        [TestCase("FiveCardsStraight_Pair", "[Pair,FiveCardsStraight]")]
        [TestCase("SixCardsStraight", "[Nothing,SixCardsStraight]")]
        [TestCase("FiveCardsFlush_Pair", "[Pair,FiveCardsFlush]")]
        [TestCase("ThreeOfKind_Pair*2", "[TwoPairs,ThreeOfKind],[Pair,FullHouse]")]
        [TestCase("FourCardsFlushStraight", "[Nothing,FourCardsFlushStraight]")]
        [TestCase("SixCardsFlush", "[Nothing,SixCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_Pair*2", "[TwoPairs,ThreeCardsFlushStraight],[Pair,Mansion]")]
        [TestCase("SevenCardsStraight", "[Nothing,SevenCardsStraight]")]
        [TestCase("FourCardsFlushStraight_Pair", "[Pair,FourCardsFlushStraight]")]
        [TestCase("SixCardsStraight_Pair", "[Pair,SixCardsStraight]")]
        [TestCase("ThreeCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,ThreeCardsFlushStraight],[Nothing,Mansion]")]
        [TestCase("FourOfKind", "[Nothing,FourOfKind],[Pair,Pair]")]
        [TestCase("ThreeOfKind*2", "[ThreeOfKind,ThreeOfKind],[Nothing,FullHouse]")]
        [TestCase("Pair*4", "[TwoPairs,TwoPairs]")]
        [TestCase("ThreeCardsFlushStraight*2", "[ThreeCardsFlushStraight,ThreeCardsFlushStraight]")]
        [TestCase("FourOfKind_Pair", "[Pair,FourOfKind]")]
        [TestCase("FiveCardsFlushStraight", "[Nothing,FiveCardsFlushStraight]")]
        [TestCase("EightCardsStraight", "[Nothing,EightCardsStraight]")]
        [TestCase("SixCardsFlush_Pair", "[Pair,SixCardsFlush]")]
        [TestCase("ThreeOfKind_FiveCardsStraight", "[ThreeOfKind,FiveCardsStraight]")]
        [TestCase("ThreeCardsFlushStraight_FiveCardsStraight", "[ThreeCardsFlushStraight,FiveCardsStraight]")]
        [TestCase("SevenCardsFlush", "[Nothing,SevenCardsFlush]")]
        [TestCase("ThreeOfKind_FiveCardsFlush", "[ThreeOfKind,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_FiveCardsFlush", "[ThreeCardsFlushStraight,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_ThreeOfKind_Pair", "[ThreeCardsFlushStraight,FullHouse],[ThreeOfKind,Mansion]")]
        [TestCase("FiveCardsFlushStraight_Pair", "[Pair,FiveCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_Pair*2", "[TwoPairs,FourCardsFlushStraight]")]
        [TestCase("ThreeOfKind*2_Pair", "[ThreeOfKind,FullHouse]")]
        [TestCase("FourCardsFlushStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,FourCardsFlushStraight]")]
        [TestCase("ThreeCardsFlushStraight*2_Pair", "[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("SixCardsFlushStraight", "[Nothing,SixCardsFlushStraight],[ThreeCardsFlushStraight,ThreeCardsFlushStraight]")]
        [TestCase("FourOfKind_Pair*2", "[TwoPairs,FourOfKind]")]
        [TestCase("FourOfKind_ThreeOfKind", "[ThreeOfKind,FourOfKind]")]
        [TestCase("FourOfKind_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FourOfKind]")]
        [TestCase("EightCardsFlush", "[Nothing,EightCardsFlush]")]
        [TestCase("SixCardsFlushStraight_Pair", "[Pair,SixCardsFlushStraight],[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("FiveCardsFlushStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FiveCardsFlushStraight]")]
        [TestCase("FiveCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,FiveCardsFlushStraight]")]
        [TestCase("SevenCardsFlushStraight", "[Nothing,SevenCardsFlushStraight],[ThreeCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight*2", "[FourCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FourOfKind_FourCardsFlushStraight", "[FourCardsFlushStraight,FourOfKind]")]
        [TestCase("FourOfKind*2", "[FourOfKind,FourOfKind]")]
        [TestCase("EightCardsFlushStraight", "[Nothing,EightCardsFlushStraight],[ThreeCardsFlushStraight,FiveCardsFlushStraight],[FourCardsFlushStraight,FourCardsFlushStraight]")]
        public void Test8CardsSplitHandBreakAndAssembly(string handName, string expectedSolutions)
        {
            string actualSolutions = SplitHandToString(handName);
            Assert.That(actualSolutions, Is.EqualTo(expectedSolutions), $"Mismatch for 8-card hand: {handName}");
        }

        #endregion

        #region 9 Cards Test Cases

        [TestCase("Pair*2", "[Nothing,TwoPairs],[Pair,Pair]")]
        [TestCase("Pair", "[Nothing,Pair]")]
        [TestCase("Pair*3", "[Pair,TwoPairs]")]
        [TestCase("ThreeOfKind_Pair", "[Nothing,FullHouse],[Pair,ThreeOfKind]")]
        [TestCase("FiveCardsFlush", "[Nothing,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_Pair", "[Nothing,Mansion],[Pair,ThreeCardsFlushStraight]")]
        [TestCase("FiveCardsStraight", "[Nothing,FiveCardsStraight]")]
        [TestCase("FiveCardsStraight_Pair", "[Pair,FiveCardsStraight]")]
        [TestCase("ThreeOfKind", "[Nothing,ThreeOfKind]")]
        [TestCase("ThreeCardsFlushStraight", "[Nothing,ThreeCardsFlushStraight]")]
        [TestCase("FiveCardsFlush_Pair", "[Pair,FiveCardsFlush]")]
        [TestCase("SixCardsStraight", "[Nothing,SixCardsStraight]")]
        [TestCase("ThreeOfKind_Pair*2", "[TwoPairs,ThreeOfKind],[Pair,FullHouse]")]
        [TestCase("ThreeCardsFlushStraight_Pair*2", "[TwoPairs,ThreeCardsFlushStraight],[Pair,Mansion]")]
        [TestCase("Nothing", "[Nothing,Nothing]")]
        [TestCase("SixCardsFlush", "[Nothing,SixCardsFlush]")]
        [TestCase("SixCardsStraight_Pair", "[Pair,SixCardsStraight]")]
        [TestCase("SevenCardsStraight", "[Nothing,SevenCardsStraight]")]
        [TestCase("Pair*4", "[TwoPairs,TwoPairs]")]
        [TestCase("FourCardsFlushStraight", "[Nothing,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_Pair", "[Pair,FourCardsFlushStraight]")]
        [TestCase("ThreeCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,ThreeCardsFlushStraight],[Nothing,Mansion]")]
        [TestCase("ThreeOfKind*2", "[ThreeOfKind,ThreeOfKind],[Nothing,FullHouse]")]
        [TestCase("FiveCardsStraight_Pair*2", "[TwoPairs,FiveCardsStraight]")]
        [TestCase("SixCardsFlush_Pair", "[Pair,SixCardsFlush]")]
        [TestCase("EightCardsStraight", "[Nothing,EightCardsStraight]")]
        [TestCase("FourOfKind", "[Nothing,FourOfKind],[Pair,Pair]")]
        [TestCase("FourOfKind_Pair", "[Pair,FourOfKind]")]
        [TestCase("ThreeCardsFlushStraight*2", "[ThreeCardsFlushStraight,ThreeCardsFlushStraight]")]
        [TestCase("ThreeOfKind_FiveCardsStraight", "[ThreeOfKind,FiveCardsStraight]")]
        [TestCase("ThreeCardsFlushStraight_FiveCardsStraight", "[ThreeCardsFlushStraight,FiveCardsStraight]")]
        [TestCase("FiveCardsFlush_Pair*2", "[TwoPairs,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_FiveCardsFlush", "[ThreeCardsFlushStraight,FiveCardsFlush]")]
        [TestCase("SevenCardsStraight_Pair", "[Pair,SevenCardsStraight]")]
        [TestCase("ThreeOfKind_FiveCardsFlush", "[ThreeOfKind,FiveCardsFlush]")]
        [TestCase("ThreeCardsFlushStraight_ThreeOfKind_Pair", "[ThreeCardsFlushStraight,FullHouse],[ThreeOfKind,Mansion]")]
        [TestCase("SevenCardsFlush", "[Nothing,SevenCardsFlush]")]
        [TestCase("FiveCardsFlushStraight", "[Nothing,FiveCardsFlushStraight]")]
        [TestCase("ThreeOfKind*2_Pair", "[ThreeOfKind,FullHouse]")]
        [TestCase("FourCardsFlushStraight_Pair*2", "[TwoPairs,FourCardsFlushStraight]")]
        [TestCase("ThreeOfKind_Pair*3", "[TwoPairs,FullHouse]")]
        [TestCase("ThreeCardsFlushStraight*2_Pair", "[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("ThreeCardsFlushStraight_Pair*3", "[TwoPairs,Mansion]")]
        [TestCase("FiveCardsFlushStraight_Pair", "[Pair,FiveCardsFlushStraight]")]
        [TestCase("NineCardsStraight", "[Nothing,NineCardsStraight]")]
        [TestCase("FourCardsFlushStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,FourCardsFlushStraight]")]
        [TestCase("FourOfKind_Pair*2", "[TwoPairs,FourOfKind]")]
        [TestCase("SixCardsStraight_ThreeOfKind", "[ThreeOfKind,SixCardsStraight]")]
        [TestCase("SixCardsStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,SixCardsStraight]")]
        [TestCase("FourOfKind_ThreeOfKind", "[ThreeOfKind,FourOfKind]")]
        [TestCase("SixCardsFlushStraight", "[Nothing,SixCardsFlushStraight],[ThreeCardsFlushStraight,ThreeCardsFlushStraight]")]
        [TestCase("SevenCardsFlush_Pair", "[Pair,SevenCardsFlush]")]
        [TestCase("FourOfKind_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FourOfKind]")]
        [TestCase("FourCardsFlushStraight_FiveCardsStraight", "[FiveCardsStraight,FourCardsFlushStraight]")]
        [TestCase("SixCardsFlush_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,SixCardsFlush]")]
        [TestCase("SixCardsFlush_ThreeOfKind", "[ThreeOfKind,SixCardsFlush]")]
        [TestCase("EightCardsFlush", "[Nothing,EightCardsFlush]")]
        [TestCase("FourCardsFlushStraight_FiveCardsFlush", "[FiveCardsFlush,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_ThreeCardsFlushStraight_Pair", "[Mansion,FourCardsFlushStraight]")]
        [TestCase("FourCardsFlushStraight_ThreeOfKind_Pair", "[FullHouse,FourCardsFlushStraight]")]
        [TestCase("SixCardsFlushStraight_Pair", "[Pair,SixCardsFlushStraight],[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("FourOfKind_FiveCardsStraight", "[FiveCardsStraight,FourOfKind]")]
        [TestCase("FiveCardsFlushStraight_Pair*2", "[TwoPairs,FiveCardsFlushStraight]")]
        [TestCase("FiveCardsFlushStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,FiveCardsFlushStraight]")]
        [TestCase("FiveCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,FiveCardsFlushStraight]")]
        [TestCase("FourOfKind_ThreeOfKind_Pair", "[FullHouse,FourOfKind]")]
        [TestCase("FourOfKind_FiveCardsFlush", "[FiveCardsFlush,FourOfKind]")]
        [TestCase("ThreeCardsFlushStraight_ThreeOfKind*2", "[ThreeCardsFlushStraight,FullHouse],[ThreeOfKind,Mansion]")]
        [TestCase("ThreeCardsFlushStraight*2_ThreeOfKind", "[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("FourCardsFlushStraight*2", "[FourCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FourOfKind_ThreeCardsFlushStraight_Pair", "[Mansion,FourOfKind]")]
        [TestCase("SevenCardsFlushStraight", "[Nothing,SevenCardsFlushStraight],[ThreeCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("ThreeOfKind*3", "[ThreeOfKind,FullHouse]")]
        [TestCase("FourOfKind_FourCardsFlushStraight", "[FourCardsFlushStraight,FourOfKind]")]
        [TestCase("ThreeCardsFlushStraight*3", "[ThreeCardsFlushStraight,ThreeCardsFlushStraight]")]
        [TestCase("FourOfKind*2", "[FourOfKind,FourOfKind]")]
        [TestCase("NineCardsFlush", "[Nothing,NineCardsFlush]")]
        [TestCase("SevenCardsFlushStraight_Pair", "[Pair,SevenCardsFlushStraight],[Mansion,FourCardsFlushStraight]")]
        [TestCase("FiveCardsFlushStraight_FourCardsFlushStraight", "[FourCardsFlushStraight,FiveCardsFlushStraight]")]
        [TestCase("SixCardsFlushStraight_ThreeOfKind", "[ThreeOfKind,SixCardsFlushStraight],[ThreeCardsFlushStraight,Mansion]")]
        [TestCase("SixCardsFlushStraight_ThreeCardsFlushStraight", "[ThreeCardsFlushStraight,SixCardsFlushStraight]")]
        [TestCase("EightCardsFlushStraight", "[Nothing,EightCardsFlushStraight],[ThreeCardsFlushStraight,FiveCardsFlushStraight],[FourCardsFlushStraight,FourCardsFlushStraight]")]
        [TestCase("FiveCardsFlushStraight_FourOfKind", "[FourOfKind,FiveCardsFlushStraight]")]
        [TestCase("NineCardsFlushStraight", "[Nothing,NineCardsFlushStraight],[ThreeCardsFlushStraight,SixCardsFlushStraight],[FourCardsFlushStraight,FiveCardsFlushStraight]")]
        public void Test9CardsSplitHandBreakAndAssembly(string handName, string expectedSolutions)
        {
            string actualSolutions = SplitHandToString(handName);
            Assert.That(actualSolutions, Is.EqualTo(expectedSolutions), $"Mismatch for 9-card hand: {handName}");
        }

        #endregion
    }
}