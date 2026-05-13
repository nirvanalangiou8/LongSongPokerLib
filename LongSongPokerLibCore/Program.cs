using System;
using GenericPoker.EightCard;

namespace LongSongPokerLibCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var inputCardStr = "J♣️,J♣️@2,3♣️,5❤️,6🔶,A♠️,A❤️,K♠️";  
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            pokerHand.MinFlushStraightCards = 3;
            var resHand = pokerHand.Test8CardsTwoHandsDeploy();
            Console.WriteLine("Successfully created poker hand and deployed.");
        }
    }
}
