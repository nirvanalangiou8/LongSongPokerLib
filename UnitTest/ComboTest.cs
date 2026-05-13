using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using NUnit.Framework;

namespace EightCardsProbTest
{
    [TestFixture]
    public class ComboTest
    {

        public List<string> ConvertCombosToStr(List<PokerCardComponent<PokerCardCompRank>> combos)
        {
            var retList = new List<string>();
            
            return new List<string>();
        }
        
        // Test cases defined as a static list
        private static readonly object[] TestCasesThreeCardsStraight =
        {
            new object[]
            {
                "MajorJoker,SuitJoker,StraightJoker,A♠️,2♣️,3♠️,9❤️,Q❤️,10❤️",
                new List<string> {
                    "A♠️#MajorJoker_K#Q❤️", "A♠️#MajorJoker_K#StraightJoker_Q", "MajorJoker_A#StraightJoker_K#Q❤️",
                    "Q❤️#MajorJoker_J#10❤️", "MajorJoker_Q#StraightJoker_J#10❤️", 
                    "MajorJoker_J#10❤️#9❤️",
                    "MajorJoker_J#StraightJoker_10#9❤️", 
                    "MajorJoker_5#StraightJoker_4#3♠️", "MajorJoker_4#3♠️#2♣️", 
                    "MajorJoker_4#StraightJoker_3#2♣️", "3♠️#2♣️#A♠️",
                    // Double check below.
                    "3♠️#MajorJoker_2#A♠️", "MajorJoker_3#2♣️#A♠️", "MajorJoker_3#StraightJoker_2#A♠️"
                },
            },
            new object[]
            {
                
                "2♣️,4♠️,3❤️,4❤️",
                new List<string> {
                    "4♠️#3❤️#2♣️", "4❤️#3❤️#2♣️", 
                },
            },
            new object[]
            {
                "2♣️,3♠️,3❤️,4❤️",
                new List<string> {
                    "4❤️#3♠️#2♣️", "4❤️#3❤️#2♣️",
                },              
            },
        };

        private static readonly object[] TestCases13CardsStraight =
        {
            
            new object[]
            {
                
                    "MajorJoker,A♠️,2♣️,3♠️,9❤️,Q❤️,10❤️," +
                    "5♠️,4♣️,6♠️,7❤️,8❤️,J❤️,K❤️",
                // Normally, we can allow only maximum 13 cards in one pokerhand. This is the extreme case, that if We have A , K　to 2, the whole
                // 13 cards, and one joker what would be the combo permute be.
                // The answer are there ar 27 combos.
                // Actually it's 14*2-1, and there is one unique case that K, Q, ....2, (12 cards) and missing A, in this case, the joker will be sub as Ace-14 rather than
                // Ace-1 
                new List<string> {
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#MajorJoker_2",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#MajorJoker_3#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#MajorJoker_4#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#MajorJoker_5#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#MajorJoker_6#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#MajorJoker_7#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#9❤️#MajorJoker_8#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#10❤️#MajorJoker_9#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#J❤️#MajorJoker_10#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#Q❤️#MajorJoker_J#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#K❤️#MajorJoker_Q#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "A♠️#MajorJoker_K#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "MajorJoker_A#K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#MajorJoker_2#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#MajorJoker_3#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#MajorJoker_4#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#MajorJoker_5#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#MajorJoker_6#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#8❤️#MajorJoker_7#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#9❤️#MajorJoker_8#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#10❤️#MajorJoker_9#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#J❤️#MajorJoker_10#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#Q❤️#MajorJoker_J#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "K❤️#MajorJoker_Q#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️",
                    "MajorJoker_K#Q❤️#J❤️#10❤️#9❤️#8❤️#7❤️#6♠️#5♠️#4♣️#3♠️#2♣️#A♠️"
                },
            },
            
        };
        
        private static readonly object[] TestCasesFourCardsStraight =
        {
            
            new object[]
            {
                "MajorJoker,SuitJoker,MinorJoker,A♠️,2♣️,3♠️,9❤️,Q❤️,10❤️",
                new List<string> {
                    "A♠️#MajorJoker_K#Q❤️#MinorJoker_J", "MajorJoker_K#Q❤️#MinorJoker_J#10❤️", 
                    "Q❤️#MajorJoker_J#10❤️#9❤️",
                    "Q❤️#MajorJoker_J#MinorJoker_10#9❤️",
                    "MajorJoker_Q#MinorJoker_J#10❤️#9❤️",
                    "MajorJoker_5#MinorJoker_4#3♠️#2♣️", "MajorJoker_4#3♠️#2♣️#A♠️",
                    "MajorJoker_4#3♠️#MinorJoker_2#A♠️", "MajorJoker_4#MinorJoker_3#2♣️#A♠️"
                },
            },
        };
        
        // Test cases defined as a static list
        private static readonly object[] TestCasesThreeCardsFlush =
        {
            new object[]
            {
                "A♠️,5♠️,3♠️,3❤️,10❤️,10♣️,10♠️,4♣️,Q♣️,Q❤️",
                new List<string> {
                    "A♠️#10♠️#5♠️", "A♠️#10♠️#3♠️", "A♠️#5♠️#3♠️", 
                    "Q♣️#10♣️#4♣️", "Q❤️#10❤️#3❤️","10♠️#5♠️#3♠️" 
                },
            },
            new object[]
            {
                "MajorJoker,MinorJoker,A♠️,10♠️,9♠️",
                new List<string> {
                    "A♠️#MajorJoker_K♠️#MinorJoker_Q♠️","A♠️#MajorJoker_K♠️#10♠️","MajorJoker_A♠️#MinorJoker_K♠️#10♠️",
                    "A♠️#MajorJoker_K♠️#9♠️", "MajorJoker_A♠️#MinorJoker_K♠️#9♠️", "A♠️#10♠️#9♠️",
                    "MajorJoker_A♠️#10♠️#9♠️"
                },
            },
        };
        
        
        // Test cases defined as a static list
        private static readonly object[] TestCasesThreeCardsFlushStraight =
        {
            new object[]
            {
                "10🔶,J🔶,Q🔶,8🔶,A♠️,2♠️,3♠️,K♠️,Q♠️,J♠️,K❤️,J❤️,Q❤️", 
                new List<string> {
                    "A♠️#K♠️#Q♠️", "K♠️#Q♠️#J♠️","K❤️#Q❤️#J❤️",
                    "Q🔶#J🔶#10🔶", "3♠️#2♠️#A♠️", 
                },
            },
            new object[]
            {
                "MajorJoker,MinorJoker,SuitJoker,StraightJoker,10🔶,J🔶,A♠️,2♠️,3♠️",  
                new List<string> {
                    "A♠️#MajorJoker_K♠️#MinorJoker_Q♠️", "MajorJoker_K🔶#MinorJoker_Q🔶#J🔶",
                    "MajorJoker_Q🔶#J🔶#10🔶", "MajorJoker_Q🔶#MinorJoker_J🔶#10🔶", 
                    "MajorJoker_5♠️#MinorJoker_4♠️#3♠️",
                    "MajorJoker_4♠️#3♠️#2♠️","MajorJoker_4♠️#MinorJoker_3♠️#2♠️", 
                    "3♠️#2♠️#A♠️",
                    // Double check below extra
                    "3♠️#MajorJoker_2♠️#A♠️", "MajorJoker_3♠️#2♠️#A♠️", "MajorJoker_3♠️#MinorJoker_2♠️#A♠️"
                },
            },
        };
        
        private static readonly object[] TestCases8CardsPoker =
        {
            new object[]
            {
                "6♣️,7♣️,4♣️,6♠️,9♣️,5♠️,3♠️,J♠️",
                new List<string> {
                    "A♠️#K♠️#Q♠️", "K♠️#Q♠️#J♠️","K❤️#Q❤️#J❤️",
                    "Q🔶#J🔶#10🔶", "3♠️#2♠️#A♠️", 
                },
            },
          
        };
        
        
        
        
        [Test, TestCaseSource(nameof(TestCasesThreeCardsStraight))]
        public void TestThreeCardsStraight(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            var allThreeCardsStraightCombo = pokerHand.GetAllStraightComps(3);
            List<string> objectStrs = allThreeCardsStraightCombo.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }
        
        [Test, TestCaseSource(nameof(TestCases13CardsStraight))]
        public void Test13CardsStraight(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            var allThreeCardsStraightComp = pokerHand.GetAllStraightComps(13);
            List<string> objectStrs = allThreeCardsStraightComp.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }
        
        [Test, TestCaseSource(nameof(TestCasesFourCardsStraight))]
        public void TestFourCardsStraight(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            var allThreeCardsStraightComp = pokerHand.GetAllStraightComps(4);
            List<string> objectStrs = allThreeCardsStraightComp.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }
        
        [Test, TestCaseSource(nameof(TestCasesThreeCardsFlush))]
        public void TestThreeCardsFlush(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            var allComps = pokerHand.GetAllFlushComps(3);
            List<string> objectStrs = allComps.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }
        
        [Test, TestCaseSource(nameof(TestCasesThreeCardsFlushStraight))]
        public void TestThreeCardsFlushStraight(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            var allComps = pokerHand.GetAllFlushStraightComps(3);
            List<string> objectStrs = allComps.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }
        
        /*
        [Test, TestCaseSource(nameof(TestCases8CardsPoker))]
        public void Test8CardsPoker(string inputCardStr, List<string>  expected)
        {
            var pokerHand = PokerHand.CreateInstance(inputCardStr);
            pokerHand.Test8Cards();
            var allComps = pokerHand.GetAllFlushStraightComps(3);
            List<string> objectStrs = allComps.Select(o => o.CompString).ToList();
            Assert.That(objectStrs, Is.EqualTo(expected));
        }*/
        
    }
}