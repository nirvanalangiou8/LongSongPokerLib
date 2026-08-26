using System;
using System.IO;
using LongSongPokerLibCore.GenericPoker;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SimHandSplitProbAnaTest
    {
        private const string SourceDataPath = "../Data/stats_result_8cards_for_unittest.csv";

        [Test]
        public void TestEightCardHandSplitProbAna()
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempOutputPath = Path.Combine(projectDirectory, "temp_front_back_stats.csv");

            try
            {
                InitEightCardHandSplitProbAna.Run(SourceDataPath, tempOutputPath);

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");

                string csvResult = File.ReadAllText(tempOutputPath);

                // Normalize newlines for cross-platform comparison
                string normalizedActual = csvResult.Replace("\r\n", "\n").TrimEnd();
                string normalizedExpected = Expected8CardsSplitStatsResult.Replace("\r\n", "\n").TrimEnd();

                Assert.That(normalizedActual, Is.EqualTo(normalizedExpected));
            }
            finally
            {
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
            }
        }

        public static readonly string Expected8CardsSplitStatsResult =
@"Hand Position,Rank,Count,Probablities,Win/NoLose probablity
Front,FourOfKind,62.00,0.00000626%,100.00000000%
Front,FourCardsFlushStraight,1027.00,0.00010371%,99.99999374%
Front,ThreeCardsFlushStraight,1507972.00,0.15227890%,99.99989003%
Front,ThreeOfKind,3223865.00,0.32555420%,99.84761113%
Front,TwoPairs,5206605.00,0.52577640%,99.52205693%
Front,Pair,259081481.50,26.16271622%,98.99628052%
Front,Nothing,721248802.50,72.83356430%,72.83356430%
Back,EightCardsFlushStraight,22.00,0.00000222%,100.00000000%
Back,SevenCardsFlushStraight,1116.00,0.00011270%,99.99999778%
Back,EightCardsFlush,4079.00,0.00041191%,99.99988508%
Back,SixCardsFlushStraight,29846.00,0.00301393%,99.99947317%
Back,SevenCardsFlush,244643.00,0.02470468%,99.99645925%
Back,EightCardsStraight,366348.00,0.03699477%,99.97175457%
Back,FiveCardsFlushStraight,503562.00,0.05085099%,99.93475980%
Back,FourOfKind,1901579.00,0.19202635%,99.88390881%
Back,FourCardsFlushStraight,6369052.00,0.64316330%,99.69188246%
Back,SevenCardsStraight,4704344.00,0.47505679%,99.04871916%
Back,SixCardsFlush,5587466.00,0.56423673%,98.57366237%
Back,SixCardsStraight,30426053.00,3.07250131%,98.00942564%
Back,Mansion,18986821.50,1.91733821%,94.93692434%
Back,FullHouse,21404957.00,2.16152777%,93.01958613%
Back,FiveCardsFlush,63181810.00,6.38026213%,90.85805837%
Back,FiveCardsStraight,128882520.00,13.01488928%,84.47779624%
Back,ThreeCardsFlushStraight,45539334.50,4.59867945%,71.46290696%
Back,ThreeOfKind,47746497.00,4.82156441%,66.86422750%
Back,TwoPairs,180006044.00,18.17747459%,62.04266309%
Back,Pair,264035911.00,26.66302729%,43.86518850%
Back,Nothing,170347810.00,17.20216121%,17.20216121%";
    }
}
