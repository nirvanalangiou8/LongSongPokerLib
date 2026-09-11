using System;
using System.IO;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SimHandSplitProbAnaTest
    {
        private const string SourceData8CardsPath = "../Data/stats_result_8cards_for_unittest.csv";
        private const string SourceData9CardsPath = "../Data/stats_result_9cards_for_unittest.csv";

        [Test]
        public void TestEightCardHandSplitProbAna()
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempOutputPath = Path.Combine(projectDirectory, "temp_front_back_stats.csv");

            try
            {
                InitEightCardHandSplitProbAna.Run(SourceData8CardsPath, tempOutputPath);

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");

                string csvResult = File.ReadAllText(tempOutputPath);

                Console.WriteLine("=== ACTUAL 8-CARDS RUN RESULT ===");
                Console.WriteLine(csvResult);
                Console.WriteLine("=================================");

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

        [Test]
        public void TestNineCardHandSplitProbAna()
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempOutputPath = Path.Combine(projectDirectory, "temp_front_back_stats_9cards.csv");

            try
            {
                InitEightCardHandSplitProbAna.Run(SourceData9CardsPath, tempOutputPath);

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");

                string csvResult = File.ReadAllText(tempOutputPath);

                Console.WriteLine("=== ACTUAL 9-CARDS RUN RESULT ===");
                Console.WriteLine(csvResult);
                Console.WriteLine("=================================");

                // Normalize newlines for cross-platform comparison
                string normalizedActual = csvResult.Replace("\r\n", "\n").TrimEnd();
                string normalizedExpected = Expected9CardsSplitStatsResult.Replace("\r\n", "\n").TrimEnd();

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

// TODO : There is NONE type below, need to fix. 
        public static readonly string Expected8CardsSplitStatsResult = 
            @"Hand Position,Rank,Count,Probablities,Win/NoLose probablity
Front,FourOfKind,377.00,0.00001049%,100.00000000%
Front,FourCardsFlushStraight,4837.67,0.00013462%,99.99998951%
Front,ThreeCardsFlushStraight,6266008.67,0.17436621%,99.99985489%
Front,ThreeOfKind,8647866.00,0.24064691%,99.82548868%
Front,TwoPairs,21900282.00,0.60942608%,99.58484178%
Front,Pair,930370320.00,25.88970962%,98.97541569%
Front,Nothing,2626401483.67,73.08570607%,73.08570607%
Back,EightCardsFlushStraight,20.67,0.00000058%,100.00000000%
Back,SevenCardsFlushStraight,2756.50,0.00007671%,99.99999942%
Back,EightCardsFlush,20725.00,0.00057672%,99.99992272%
Back,SixCardsFlushStraight,69857.50,0.00194395%,99.99934600%
Back,SevenCardsFlush,1062546.00,0.02956780%,99.99740205%
Back,EightCardsStraight,1830960.00,0.05095070%,99.96783425%
Back,FiveCardsFlushStraight,2312678.67,0.06435564%,99.91688354%
Back,FourOfKind,6843388.50,0.19043314%,99.85252790%
Back,FourCardsFlushStraight,29661450.17,0.82539857%,99.66209476%
Back,SevenCardsStraight,14354277.00,0.39944101%,98.83669619%
Back,SixCardsFlush,20753672.00,0.57751901%,98.43725518%
Back,SixCardsStraight,65679045.00,1.82767159%,97.85973617%
Back,Mansion,76893580.50,2.13974202%,96.03206458%
Back,FullHouse,91254774.00,2.53937550%,93.89232256%
Back,FiveCardsFlush,196836536.00,5.47743264%,91.35294706%
Back,FiveCardsStraight,224750900.00,6.25421449%,85.87551441%
Back,ThreeCardsFlushStraight,226346673.00,6.29862057%,79.62129992%
Back,ThreeOfKind,253128801.00,7.04389533%,73.32267935%
Back,TwoPairs,648950476.00,18.05855047%,66.27878402%
Back,Pair,1535935712.50,42.74096962%,48.22023355%
Back,Nothing,196902345.00,5.47926393%,5.47926393%";

        public static readonly string Expected9CardsSplitStatsResult = 
            @"Hand Position,Rank,Count,Probablities,Win/NoLose probablity
Front,FourOfKind,2720.00,0.00007920%,100.00000000%
Front,FourCardsFlushStraight,34337.67,0.00099986%,99.99992080%
Front,Mansion,90510.50,0.00263552%,99.99892094%
Front,FullHouse,97890.00,0.00285040%,99.99628542%
Front,FiveCardsFlush,143020.00,0.00416451%,99.99343502%
Front,FiveCardsStraight,230418.00,0.00670940%,99.98927051%
Front,ThreeCardsFlushStraight,22745130.17,0.66230181%,99.98256110%
Front,ThreeOfKind,27050556.50,0.78766893%,99.32025929%
Front,TwoPairs,95800950.00,2.78957042%,98.53259036%
Front,Pair,1298946599.50,37.82324716%,95.74301994%
Front,Nothing,1989104292.67,57.91953520%,57.91977278%
Front,None,8159.00,0.00023758%,0.00023758%
Back,NineCardsFlushStraight,3.33,0.00000010%,100.00000000%
Back,NineCardsFlush,1928.00,0.00005614%,99.99999990%
Back,EightCardsFlushStraight,267.33,0.00000778%,99.99994376%
Back,SevenCardsFlushStraight,9972.00,0.00029037%,99.99993598%
Back,EightCardsFlush,135037.00,0.00393206%,99.99964561%
Back,SixCardsFlushStraight,167120.33,0.00486628%,99.99571355%
Back,NineCardsStraight,1074138.00,0.03127718%,99.99084727%
Back,SevenCardsFlush,3514281.00,0.10233024%,99.95957009%
Back,EightCardsStraight,7834539.00,0.22812924%,99.85723985%
Back,FiveCardsFlushStraight,4189011.67,0.12197732%,99.62911061%
Back,FourOfKind,11801406.00,0.34363807%,99.50713329%
Back,FourCardsFlushStraight,43079344.33,1.25440160%,99.16349522%
Back,SevenCardsStraight,33906556.00,0.98730467%,97.90909363%
Back,SixCardsFlush,44243101.00,1.28828833%,96.92178895%
Back,SixCardsStraight,110197038.00,3.20876147%,95.63350062%
Back,Mansion,122450494.00,3.56556251%,92.42473915%
Back,FullHouse,146673024.00,4.27088384%,88.85917664%
Back,FiveCardsFlush,306257661.00,8.91773319%,84.58829280%
Back,FiveCardsStraight,300313882.00,8.74465986%,75.67055961%
Back,ThreeCardsFlushStraight,236282249.00,6.88016113%,66.92589975%
Back,ThreeOfKind,262036582.00,7.63008611%,60.04573862%
Back,TwoPairs,780161345.50,22.71705042%,52.41565251%
Back,Pair,977217605.50,28.45501350%,29.69860209%
Back,Nothing,42699839.00,1.24335101%,1.24358859%
Back,None,8159.00,0.00023758%,0.00023758%";
    }
}
