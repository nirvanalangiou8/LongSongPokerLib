using System;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.CardSimStatAnalysis;
using NUnit.Framework;

namespace GenericPoker.CardSimStatAnalysis.UnitTest
{
    [TestFixture]
    public class SimHandSplitProbAnaTest
    {
        private const string SourceData8CardsPath = "../Data/stats_result_8cards_for_unittest.csv";
        private const string SourceData9CardsPath = "../Data/stats_result_9cards_for_unittest.csv";

        private static string ResolveInputPath(string inputPath)
        {
            if (File.Exists(inputPath)) return Path.GetFullPath(inputPath);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidate = Path.GetFullPath(Path.Combine(baseDir, inputPath));
            if (File.Exists(candidate)) return candidate;

            DirectoryInfo? currentDir = new DirectoryInfo(baseDir);
            while (currentDir != null)
            {
                candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, inputPath));
                if (File.Exists(candidate)) return candidate;

                candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "GenericPoker", "CardSimStatAnalysis", inputPath));
                if (File.Exists(candidate)) return candidate;

                candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", inputPath));
                if (File.Exists(candidate)) return candidate;

                string fileName = Path.GetFileName(inputPath);
                candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                if (File.Exists(candidate)) return candidate;

                candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                if (File.Exists(candidate)) return candidate;

                currentDir = currentDir.Parent;
            }

            return inputPath;
        }

        private static string LoadCleanCsvResult(string filePath)
        {
            var lines = File.ReadAllLines(filePath)
                .Where(line => !line.TrimStart().StartsWith("#"));
            return string.Join("\n", lines);
        }

        [Test]
        public void TestEightCardHandSplitProbAna()
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempOutputPath = Path.Combine(projectDirectory, "temp_front_back_stats.csv");

            try
            {
                PostSimStatHandSplitProbAna.Run(ResolveInputPath(SourceData8CardsPath), tempOutputPath, new EightCardRule());

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");

                string csvResult = LoadCleanCsvResult(tempOutputPath);

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
                PostSimStatHandSplitProbAna.Run(ResolveInputPath(SourceData9CardsPath), tempOutputPath, new NineCardTwoHandsRule());

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");

                string csvResult = LoadCleanCsvResult(tempOutputPath);

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

        [Test]
        public void TestNineCardThreeHandSplitProbAna()
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempOutputPath = Path.Combine(projectDirectory, "temp_threehands_stats_9cards.csv");

            try
            {
                var stats = PostSimStatHandSplitProbAna.Run(ResolveInputPath(SourceData9CardsPath), tempOutputPath, new NineCardThreeHandsRule());

                Assert.That(File.Exists(tempOutputPath), Is.True, $"File {tempOutputPath} does not exist");
                Assert.That(stats.ContainsKey(BattleSubHandOrder.FirstHand), Is.True);
                Assert.That(stats.ContainsKey(BattleSubHandOrder.SecondHand), Is.True);
                Assert.That(stats.ContainsKey(BattleSubHandOrder.ThirdHand), Is.True);

                string csvResult = LoadCleanCsvResult(tempOutputPath);
                Console.WriteLine("=== ACTUAL 9-CARDS 3-HANDS RUN RESULT ===");
                Console.WriteLine(csvResult);
                Console.WriteLine("=========================================");

                Assert.That(csvResult.Contains("FirstHand,"), Is.True);
                Assert.That(csvResult.Contains("SecondHand,"), Is.True);
                Assert.That(csvResult.Contains("ThirdHand,"), Is.True);
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
            @"Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities
FirstHand,FourOfKind,377,3593591176,0.0000104908984227%,100.0000000000000000%
FirstHand,FourCardsFlushStraight,4838,3593590799,0.0001346285585381%,99.9999895091015722%
FirstHand,ThreeCardsFlushStraight,6266009,3593585961,0.1743662173328979%,99.9998548805430376%
FirstHand,ThreeOfKind,8647866,3587319952,0.2406469065751067%,99.8254886632101379%
FirstHand,TwoPairs,21900282,3578672086,0.6094260845880928%,99.5848417566350341%
FirstHand,Pair,930370320,3556771804,25.8897096089708345%,98.9754156720469402%
FirstHand,Nothing,2626401484,2626401484,73.0857060630761057%,73.0857060630761057%
SecondHand,EightCardsFlushStraight,21,3593591178,0.0000005843736519%,100.0000000000000000%
SecondHand,SevenCardsFlushStraight,2757,3593591157,0.0000767199122949%,99.9999994156263505%
SecondHand,EightCardsFlush,20725,3593588400,0.0005767211397579%,99.9999226957140497%
SecondHand,SixCardsFlushStraight,69858,3593567675,0.0019439606939062%,99.9993459745742941%
SecondHand,SevenCardsFlush,1062546,3593497817,0.0295678041092965%,99.9974020138803876%
SecondHand,EightCardsStraight,1830960,3592435271,0.0509507038866623%,99.9678342097710892%
SecondHand,FiveCardsFlushStraight,2312679,3590604311,0.0643556510868082%,99.9168835058844351%
SecondHand,FourOfKind,6843389,3588291632,0.1904331533841494%,99.8525278547976236%
SecondHand,FourCardsFlushStraight,29661450,3581448243,0.8253985645776203%,99.6620947014134728%
SecondHand,SevenCardsStraight,14354277,3551786793,0.3994410128752826%,98.8366961368358554%
SecondHand,SixCardsFlush,20753672,3537432516,0.5775190045838876%,98.4372551239605653%
SecondHand,SixCardsStraight,65679045,3516678844,1.8276715894141701%,97.8597361193766835%
SecondHand,Mansion,76893581,3450999799,2.1397420349522019%,96.0320645299625131%
SecondHand,FullHouse,91254774,3374106218,2.5393755015501656%,93.8923224950103052%
SecondHand,FiveCardsFlush,196836536,3282851444,5.4774326363286725%,91.3529469934601490%
SecondHand,FiveCardsStraight,224750900,3086014908,6.2542144853851817%,85.8755143571314772%
SecondHand,ThreeCardsFlushStraight,226346673,2861264008,6.2986205661260675%,79.6212998717462872%
SecondHand,ThreeOfKind,253128801,2634917335,7.0438953253685890%,73.3226793056202197%
SecondHand,TwoPairs,648950476,2381788534,18.0585504542887659%,66.2787839802516321%
SecondHand,Pair,1535935713,1732838058,42.7409696017458318%,48.2202335259628689%
SecondHand,Nothing,196902345,196902345,5.4792639242170357%,5.4792639242170357%";

        public static readonly string Expected9CardsSplitStatsResult = 
            @"Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities
FirstHand,FourOfKind,2720,3434254586,0.0000792020490003%,100.0000000000000000%
FirstHand,FourCardsFlushStraight,34338,3434251866,0.0009998676318285%,99.9999207979510030%
FirstHand,Mansion,90511,3434217528,0.0026355355356873%,99.9989209303191684%
FirstHand,FullHouse,97890,3434127017,0.0028504002120011%,99.9962853947834795%
FirstHand,FiveCardsFlush,143020,3434029127,0.0041645136205985%,99.9934349945714840%
FirstHand,FiveCardsStraight,230418,3433886107,0.0067094035759409%,99.9892704809508870%
FirstHand,ThreeCardsFlushStraight,22753289,3433655689,0.6625393787855889%,99.9825610773749429%
FirstHand,ThreeOfKind,27050557,3410902400,0.7876689488971975%,99.3200216985893558%
FirstHand,TwoPairs,95800950,3383851843,2.7895704177127653%,98.5323527496921536%
FirstHand,Pair,1298946600,3288050893,37.8232471551542659%,95.7427823319793925%
FirstHand,Nothing,1989104293,1989104293,57.9195351768251210%,57.9195351768251210%
SecondHand,NineCardsFlushStraight,3,3434254584,0.0000000873552012%,100.0000000000000000%
SecondHand,NineCardsFlush,1928,3434254581,0.0000561402759418%,99.9999999126447991%
SecondHand,EightCardsFlushStraight,267,3434252653,0.0000077746129027%,99.9999437723688600%
SecondHand,SevenCardsFlushStraight,9972,3434252386,0.0002903686886365%,99.9999359977559510%
SecondHand,EightCardsFlush,135037,3434242414,0.0039320614327525%,99.9996456290673130%
SecondHand,SixCardsFlushStraight,167120,3434107377,0.0048662670722958%,99.9957135676345699%
SecondHand,NineCardsStraight,1074138,3433940257,0.0312771803524511%,99.9908473005622689%
SecondHand,SevenCardsFlush,3514281,3432866119,0.1023302412224428%,99.9595701202098175%
SecondHand,EightCardsStraight,7834539,3429351838,0.2281292434317677%,99.8572398789873805%
SecondHand,FiveCardsFlushStraight,4189012,3421517299,0.1219773286324308%,99.6291106355556066%
SecondHand,FourOfKind,11801406,3417328287,0.3436380650107331%,99.5071333069231789%
SecondHand,FourCardsFlushStraight,43079344,3405526881,1.2544015869034362%,99.1634952419124471%
SecondHand,SevenCardsStraight,33906556,3362447537,0.9873046732752064%,97.9090936550090052%
SecondHand,SixCardsFlush,44243101,3328540981,1.2882883291799662%,96.9217889817338030%
SecondHand,SixCardsStraight,110197038,3284297880,3.2087614736951021%,95.6335006525538378%
SecondHand,Mansion,122450494,3174100842,3.5655625115997513%,92.4247391788587280%
SecondHand,FullHouse,146673024,3051650348,4.2708838384708403%,88.8591766672589767%
SecondHand,FiveCardsFlush,306257661,2904977324,8.9177331938883428%,84.5882928287881364%
SecondHand,FiveCardsStraight,300313882,2598719663,8.7446598571680037%,75.6705596348998033%
SecondHand,ThreeCardsFlushStraight,236290408,2298405781,6.8803987072147701%,66.9258997777318010%
SecondHand,ThreeOfKind,262036582,2062115373,7.6300861101216491%,60.0455010705170267%
SecondHand,TwoPairs,780161346,1800078791,22.7170504375164284%,52.4154149603953790%
SecondHand,Pair,977217606,1019917445,28.4550135145135163%,29.6983645228789506%
SecondHand,Nothing,42699839,42699839,1.2433510083654299%,1.2433510083654299%";
    }
}
