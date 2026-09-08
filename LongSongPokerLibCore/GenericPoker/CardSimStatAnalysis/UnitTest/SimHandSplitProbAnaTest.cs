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


        public static readonly string Expected8CardsSplitStatsResult = 
            @"Hand Position,Rank,Count,Probablities,Win/NoLose probablity
Front,FourOfKind,377.00,0.00001049%,100.00000000%
Front,FourCardsFlushStraight,4817.00,0.00013404%,99.99998951%
Front,ThreeCardsFlushStraight,6193374.00,0.17234498%,99.99985546%
Front,ThreeOfKind,14210714.00,0.39544604%,99.82751048%
Front,TwoPairs,21900282.00,0.60942608%,99.43206444%
Front,Pair,927132160.50,25.79960033%,98.82263836%
Front,Nothing,2624149450.50,73.02303803%,73.02303803%
Back,EightCardsFlushStraight,62.00,0.00000173%,100.00000000%
Back,SevenCardsFlushStraight,5513.00,0.00015341%,99.99999827%
Back,EightCardsFlush,20725.00,0.00057672%,99.99984486%
Back,SixCardsFlushStraight,139715.00,0.00388789%,99.99926814%
Back,SevenCardsFlush,1062546.00,0.02956780%,99.99538025%
Back,EightCardsStraight,1830960.00,0.05095070%,99.96581244%
Back,FiveCardsFlushStraight,2312658.00,0.06435507%,99.91486174%
Back,FourOfKind,10085935.00,0.28066451%,99.85050667%
Back,FourCardsFlushStraight,29658673.00,0.82532129%,99.56984216%
Back,SevenCardsStraight,14354277.00,0.39944101%,98.74452088%
Back,SixCardsFlush,20753672.00,0.57751901%,98.34507986%
Back,SixCardsStraight,65679045.00,1.82767159%,97.76756086%
Back,Mansion,73514556.50,2.04571285%,95.93988927%
Back,FullHouse,89066563.00,2.47848346%,93.89417642%
Back,FiveCardsFlush,196836536.00,5.47743264%,91.41569295%
Back,FiveCardsStraight,224750900.00,6.25421449%,85.93826031%
Back,ThreeCardsFlushStraight,229655839.50,6.39070580%,79.68404582%
Back,ThreeOfKind,255317012.00,7.10478737%,73.29334002%
Back,TwoPairs,648950476.00,18.05855047%,66.18855265%
Back,Pair,1532693166.00,42.65073825%,48.13000218%
Back,Nothing,196902345.00,5.47926393%,5.47926393%";

        public static readonly string Expected9CardsSplitStatsResult = 
            @"Hand Position,Rank,Count,Probablities,Win/NoLose probablity
Front,FourOfKind,2720.00,0.00007920%,100.00000000%
Front,FourCardsFlushStraight,34067.00,0.00099198%,99.99992080%
Front,Mansion,89934.00,0.00261873%,99.99892882%
Front,FullHouse,97890.00,0.00285040%,99.99631009%
Front,FiveCardsFlush,143020.00,0.00416451%,99.99345969%
Front,FiveCardsStraight,230418.00,0.00670940%,99.98929517%
Front,ThreeCardsFlushStraight,22532280.00,0.65610395%,99.98258577%
Front,ThreeOfKind,38203358.00,1.11242067%,99.32648182%
Front,TwoPairs,95800950.00,2.78957042%,98.21406114%
Front,Pair,1295507897.00,37.72311765%,95.42449073%
Front,Nothing,1981541402.00,57.69931592%,57.70137308%
Front,None,70648.00,0.00205716%,0.00205716%
Back,NineCardsFlushStraight,10.00,0.00000029%,100.00000000%
Back,NineCardsFlush,1928.00,0.00005614%,99.99999971%
Back,EightCardsFlushStraight,802.00,0.00002335%,99.99994357%
Back,SevenCardsFlushStraight,19944.00,0.00058074%,99.99992022%
Back,EightCardsFlush,135037.00,0.00393206%,99.99933948%
Back,SixCardsFlushStraight,333359.00,0.00970688%,99.99540742%
Back,NineCardsStraight,1074138.00,0.03127718%,99.98570054%
Back,SevenCardsFlush,3514281.00,0.10233024%,99.95442336%
Back,EightCardsStraight,7834539.00,0.22812924%,99.85209311%
Back,FiveCardsFlushStraight,4188741.00,0.12196944%,99.62396387%
Back,FourOfKind,15268977.00,0.44460819%,99.50199443%
Back,FourCardsFlushStraight,43069105.00,1.25410344%,99.05738625%
Back,SevenCardsStraight,33906556.00,0.98730467%,97.80328280%
Back,SixCardsFlush,44243101.00,1.28828833%,96.81597813%
Back,SixCardsStraight,110197038.00,3.20876147%,95.52768980%
Back,Mansion,115671984.00,3.36818314%,92.31892833%
Back,FullHouse,142182404.50,4.14012418%,88.95074518%
Back,FiveCardsFlush,306257661.00,8.91773319%,84.81062100%
Back,FiveCardsStraight,300313882.00,8.74465986%,75.89288781%
Back,ThreeCardsFlushStraight,242857575.00,7.07162411%,67.14822795%
Back,ThreeOfKind,266501654.50,7.76010188%,60.07660385%
Back,TwoPairs,780161345.50,22.71705042%,52.31650197%
Back,Pair,973750034.50,28.35404338%,29.59945154%
Back,Nothing,42699839.00,1.24335101%,1.24540817%
Back,None,70648.00,0.00205716%,0.00205716%";
    }
}
