using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.FourCard;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System;


namespace EightCardsProbTest
{
    [TestFixture]
    public class FourCardConsolePlayTest
    {
        [Test]
        public void Test_Deck_No_Jokers()
        {
            var initChips = 500000000;
            XRandom.Init(1234567);
            var rulePlayers = new List<FourCardConsolePlayer>
            {
                new FourCardConsolePlayer("Player1_Conservative", initChips,
                    bettingStrategy: FourCardConsolePlayer.RuleStrategyConservative),
                new FourCardConsolePlayer("Player2_ModerateConservative", initChips,
                    bettingStrategy: FourCardConsolePlayer.RuleStrategyModerateConservative),
                new FourCardConsolePlayer("Player3_Moderate", initChips,
                    bettingStrategy: FourCardConsolePlayer.RuleStrategyModerate),
                new FourCardConsolePlayer("Player4_Aggressive", initChips,
                    bettingStrategy: FourCardConsolePlayer.RuleStrategyAggressive)
            };

            var pokerManager = new ConsoleGameManager(rulePlayers, 10000, initChips);
            var SortedGameWinnerPlayers = pokerManager.RunGames();
            foreach (var player in SortedGameWinnerPlayers)
            {
                Console.WriteLine($"{player.PlayerName} has {player.Chips}");
            }

            /*
            var expectedResult = new List<(string playerName, int chips)>
            {
                ("Player4_Aggressive", 5001930),
                ("Player1_Conservative", 5000630),
                ("Player2_ModerateConservative", 4999670),
                ("Player3_Moderate", 4997770),
            };*/
            
            
            var expectedResult = new List<(string playerName, int chips)>
            {
                ("Player3_Moderate", 500048836),
                ("Player2_ModerateConservative", 500034098),
                ("Player1_Conservative", 499985188),
                ("Player4_Aggressive", 499931878),
            };
            var actualResult = SortedGameWinnerPlayers
                .Select(p => (p.PlayerName, p.Chips))
                .ToList();
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}