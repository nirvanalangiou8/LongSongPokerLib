using System;
using System.Collections.Generic;
using System.IO;
using GenericPoker;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SimHandIterationTest
    {
        [SetUp]
        public void Setup()
        {
            // Seed the deterministic RNG for reproducible simulation results
            XRandom.Init(12345678uL);
        }

        public static IEnumerable<TestCaseData> SimHandIterationTestData
        {
            get
            {
                yield return new TestCaseData(8, 100000, Expected8Cards100kResult)
                    .SetName("SimCardRunStat_8Cards_100k");
                yield return new TestCaseData(9, 100000, Expected9Cards100kResult)
                    .SetName("SimCardRunStat_9Cards_100k");
            }
        }

        [Test, TestCaseSource(nameof(SimHandIterationTestData))]
        public void TestSimCardRunStat(int cardsPerHand, int totalIterations, string expectedResult)
        {
            XRandom.Init(12345678uL);
            SimRunAndCalcComponentStat.SimCardRunStat(totalIterations, cardsPerHand, useParallel: false);

            // Check that the file was written to disk
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string defaultBase = Path.Combine(projectDirectory, "..", "..", "..");
            string expectedPath = Directory.Exists(Path.Combine(defaultBase, "LongSongPokerLibCore"))
                ? Path.Combine(defaultBase, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", $"stats_result_{cardsPerHand}cards.csv")
                : Path.Combine(defaultBase, "GenericPoker", "CardSimStatAnalysis", "Data", $"stats_result_{cardsPerHand}cards.csv");

            Assert.That(File.Exists(expectedPath), Is.True, $"File {expectedPath} does not exist");

            string csvResult = File.ReadAllText(expectedPath);

            // Normalize newlines for cross-platform comparison
            string normalizedActual = csvResult.Replace("\r\n", "\n").TrimEnd();
            string normalizedExpected = expectedResult.Replace("\r\n", "\n").TrimEnd();

            Assert.That(normalizedActual, Is.EqualTo(normalizedExpected));
        }

        public static readonly string Expected8Cards100kResult =
@"# Total Iterations: 100000
# Cards per Hand: 8
Hand Type,Count,Probability
Pair,213079,0.296384
Pair*2,187307,0.260536
Nothing,39461,0.054889
Pair*3,35473,0.049341
FiveCardsStraight,32447,0.045132
ThreeOfKind,32407,0.045077
ThreeOfKind_Pair,31416,0.043698
FiveCardsFlush,31257,0.043477
ThreeCardsFlushStraight,29227,0.040653
ThreeCardsFlushStraight_Pair,26087,0.036286
FiveCardsStraight_Pair,12051,0.016762
SixCardsStraight,11757,0.016353
FiveCardsFlush_Pair,7799,0.010848
ThreeOfKind_Pair*2,3978,0.005533
FourCardsFlushStraight,3916,0.005447
SixCardsFlush,3833,0.005332
ThreeCardsFlushStraight_Pair*2,3057,0.004252
SevenCardsStraight,2790,0.003881
FourCardsFlushStraight_Pair,1841,0.002561
SixCardsStraight_Pair,1397,0.001943
ThreeCardsFlushStraight_ThreeOfKind,1371,0.001907
FourOfKind,1347,0.001874
ThreeOfKind*2,892,0.001241
Pair*4,728,0.001013
ThreeCardsFlushStraight*2,692,0.000963
FourOfKind_Pair,657,0.000914
FiveCardsFlushStraight,387,0.000538
EightCardsStraight,354,0.000492
SixCardsFlush_Pair,309,0.000430
ThreeOfKind_FiveCardsStraight,279,0.000388
ThreeCardsFlushStraight_FiveCardsStraight,253,0.000352
SevenCardsFlush,206,0.000287
ThreeOfKind_FiveCardsFlush,154,0.000214
ThreeCardsFlushStraight_FiveCardsFlush,147,0.000204
ThreeCardsFlushStraight_ThreeOfKind_Pair,98,0.000136
FiveCardsFlushStraight_Pair,79,0.000110
FourCardsFlushStraight_Pair*2,67,0.000093
FourCardsFlushStraight_ThreeCardsFlushStraight,61,0.000085
ThreeOfKind*2_Pair,58,0.000081
ThreeCardsFlushStraight*2_Pair,52,0.000072
FourCardsFlushStraight_ThreeOfKind,52,0.000072
FourOfKind_Pair*2,39,0.000054
FourOfKind_ThreeCardsFlushStraight,20,0.000028
FourOfKind_ThreeOfKind,19,0.000026
SixCardsFlushStraight,19,0.000026
EightCardsFlush,3,0.000004
FourCardsFlushStraight*2,2,0.000003
FiveCardsFlushStraight_ThreeCardsFlushStraight,2,0.000003
FourOfKind_FourCardsFlushStraight,1,0.000001
SevenCardsFlushStraight,1,0.000001
SixCardsFlushStraight_Pair,1,0.000001";

        public static readonly string Expected9Cards100kResult =
@"# Total Iterations: 100000
# Cards per Hand: 9
Hand Type,Count,Probability
Pair*2,174223,0.253541
Pair,107715,0.156754
Pair*3,64222,0.093460
ThreeOfKind_Pair,42504,0.061855
FiveCardsFlush,39128,0.056942
ThreeCardsFlushStraight_Pair,34970,0.050891
FiveCardsStraight,31182,0.045378
FiveCardsStraight_Pair,25259,0.036759
ThreeOfKind,24299,0.035362
ThreeCardsFlushStraight,22529,0.032786
FiveCardsFlush_Pair,19130,0.027839
SixCardsStraight,15793,0.022983
ThreeOfKind_Pair*2,12224,0.017789
ThreeCardsFlushStraight_Pair*2,9399,0.013678
Nothing,8511,0.012386
SixCardsFlush,7181,0.010450
SevenCardsStraight,6133,0.008925
SixCardsStraight_Pair,6044,0.008796
Pair*4,4443,0.006466
FourCardsFlushStraight,4002,0.005824
FourCardsFlushStraight_Pair,3741,0.005444
ThreeCardsFlushStraight_ThreeOfKind,2684,0.003906
ThreeOfKind*2,1743,0.002537
FiveCardsStraight_Pair*2,1644,0.002392
SixCardsFlush_Pair,1638,0.002384
EightCardsStraight,1566,0.002279
FourOfKind,1353,0.001969
FourOfKind_Pair,1349,0.001963
ThreeCardsFlushStraight*2,1331,0.001937
ThreeOfKind_FiveCardsStraight,1277,0.001858
ThreeCardsFlushStraight_FiveCardsStraight,1214,0.001767
FiveCardsFlush_Pair*2,1024,0.001490
ThreeCardsFlushStraight_FiveCardsFlush,944,0.001374
SevenCardsStraight_Pair,725,0.001055
ThreeOfKind_FiveCardsFlush,662,0.000963
SevenCardsFlush,652,0.000949
ThreeCardsFlushStraight_ThreeOfKind_Pair,606,0.000882
FiveCardsFlushStraight,573,0.000834
FourCardsFlushStraight_Pair*2,428,0.000623
ThreeOfKind*2_Pair,410,0.000597
ThreeCardsFlushStraight*2_Pair,327,0.000476
ThreeOfKind_Pair*3,317,0.000461
FiveCardsFlushStraight_Pair,250,0.000364
ThreeCardsFlushStraight_Pair*3,248,0.000361
FourCardsFlushStraight_ThreeCardsFlushStraight,201,0.000293
NineCardsStraight,189,0.000275
FourCardsFlushStraight_ThreeOfKind,176,0.000256
FourOfKind_Pair*2,173,0.000252
SixCardsStraight_ThreeOfKind,168,0.000244
SixCardsStraight_ThreeCardsFlushStraight,119,0.000173
FourOfKind_ThreeOfKind,73,0.000106
SevenCardsFlush_Pair,62,0.000090
SixCardsFlushStraight,62,0.000090
FourOfKind_ThreeCardsFlushStraight,50,0.000073
SixCardsFlush_ThreeOfKind,46,0.000067
FourCardsFlushStraight_FiveCardsStraight,41,0.000060
SixCardsFlush_ThreeCardsFlushStraight,35,0.000051
EightCardsFlush,33,0.000048
FourCardsFlushStraight_FiveCardsFlush,19,0.000028
FourCardsFlushStraight_ThreeOfKind_Pair,14,0.000020
FourCardsFlushStraight_ThreeCardsFlushStraight_Pair,14,0.000020
SixCardsFlushStraight_Pair,12,0.000017
FiveCardsFlushStraight_ThreeCardsFlushStraight,11,0.000016
FourOfKind_FiveCardsStraight,10,0.000015
FiveCardsFlushStraight_Pair*2,10,0.000015
ThreeCardsFlushStraight*2_ThreeOfKind,8,0.000012
FiveCardsFlushStraight_ThreeOfKind,7,0.000010
FourOfKind_FiveCardsFlush,6,0.000009
SevenCardsFlushStraight,5,0.000007
ThreeCardsFlushStraight_ThreeOfKind*2,4,0.000006
FourCardsFlushStraight*2,3,0.000004
FiveCardsFlushStraight_FourCardsFlushStraight,2,0.000003
ThreeCardsFlushStraight*3,2,0.000003
FourOfKind_FourCardsFlushStraight,2,0.000003
SevenCardsFlushStraight_Pair,1,0.000001
FourOfKind_ThreeOfKind_Pair,1,0.000001
ThreeOfKind*3,1,0.000001
FiveCardsFlushStraight_FourOfKind,1,0.000001";
    }
}
