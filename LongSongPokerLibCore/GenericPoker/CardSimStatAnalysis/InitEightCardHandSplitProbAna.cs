using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace LongSongPokerLibCore.GenericPoker
{
    class InitEightCardHandSplitProbAna
    {
        public static void Run()
        {
            Main(new string[0]);
        }

        static void Main(string[] args)
        {
            
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            // inputPath is at LongSongPokerLibCore/stats_result.csv
            string inputPath = Path.Combine(projectRoot, "LongSongPokerLibCore", "stats_result.csv");
            
            // Resolve outputPath to the directory where this source file resides.
            string sourceFileDir = Path.Combine(projectRoot, "GenericPoker", "CardSimStatAnalysis");
            string outputPath = Path.Combine(sourceFileDir, "front_back_stats.csv");

            if (!File.Exists(inputPath))
            {
                // Fallback to projectRoot/stats_result.csv if it's there
                inputPath = Path.Combine(projectRoot, "stats_result.csv");
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Input file not found: {inputPath}");
                return;
            }

            var frontHandStats = new Dictionary<EightCardOverAllHandRank, double>();
            var backHandStats = new Dictionary<EightCardOverAllHandRank, double>();
            long totalInputCount = 0;

            var lines = File.ReadAllLines(inputPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("Hand Type"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                string handName = parts[0];
                if (!long.TryParse(parts[1], out long count)) continue;

                totalInputCount += count;

                if (handName == "Nothing")
                {
                    backHandStats[EightCardOverAllHandRank.Nothing] = backHandStats.GetValueOrDefault(EightCardOverAllHandRank.Nothing) + count;
                    frontHandStats[EightCardOverAllHandRank.Nothing] = frontHandStats.GetValueOrDefault(EightCardOverAllHandRank.Nothing) + count;
                    continue;
                }
                // change below check to if handName contains FourCardsFlushStraight 
                if (handName.Contains("Pair*4"))
                {
                    Console.WriteLine("Pair*4");
                }
                
                if (handName.Contains("SevenCardsFlushStraight"))
                {
                    Console.WriteLine("SevenCardsFlushStraight");
                }
               

                var components = ParseHandName(handName);
                

                var solutions = SplitHand(components);
                if (solutions.Count > 0)
                {
                    double perSolutionCount = (double)count / solutions.Count;
                    foreach (var sol in solutions)
                    {
                        frontHandStats[sol.Item1] = frontHandStats.GetValueOrDefault(sol.Item1) + perSolutionCount;
                        backHandStats[sol.Item2] = backHandStats.GetValueOrDefault(sol.Item2) + perSolutionCount;
                    }
                }
                else
                {
                    // If no valid split found (should not happen with legal hands), fallback to None
                    // This is for sanity check
                    frontHandStats[EightCardOverAllHandRank.None] = frontHandStats.GetValueOrDefault(EightCardOverAllHandRank.None) + count;
                    backHandStats[EightCardOverAllHandRank.None] = backHandStats.GetValueOrDefault(EightCardOverAllHandRank.None) + count;
                }
            }

            SaveStats(outputPath, frontHandStats, backHandStats);
            Console.WriteLine($"Analysis completed. Results saved to {outputPath}");

            double totalFront = frontHandStats.Values.Sum();
            double totalBack = backHandStats.Values.Sum();
            Console.WriteLine($"Total Input Appearance Count: {totalInputCount}");
            Console.WriteLine($"Total Front Stat Count: {totalFront:F2}");
            Console.WriteLine($"Total Back Stat Count: {totalBack:F2}");

            bool frontMatch = Math.Abs(totalFront - totalInputCount) < 0.001;
            bool backMatch = Math.Abs(totalBack - totalInputCount) < 0.001;

            if (frontMatch && backMatch)
            {
                Console.WriteLine("input and stat count check sum correct.");
            }
            else
            {
                if (!frontMatch) Console.WriteLine($"ERROR: Front stat count ({totalFront:F2}) does not match input count ({totalInputCount})!");
                if (!backMatch) Console.WriteLine($"ERROR: Back stat count ({totalBack:F2}) does not match input count ({totalInputCount})!");
            }
        }

        static List<EightCardsCompType> ParseHandName(string handName)
        {
            var comps = new List<EightCardsCompType>();
            var parts = handName.Split('_');
            foreach (var part in parts)
            {
                string typeStr = part;
                int count = 1;
                if (part.Contains('*'))
                {
                    var subParts = part.Split('*');
                    typeStr = subParts[0];
                    count = int.Parse(subParts[1]);
                }

                if (Enum.TryParse<EightCardsCompType>(typeStr, out var compType))
                {
                    for (int i = 0; i < count; i++)
                        comps.Add(compType);
                }
            }
            // Sort by rank descending to help balanced strategy
            return comps.OrderByDescending(c => GetCompPower(c)).ToList();
        }

        static int GetCompPower(EightCardsCompType comp)
        {
            // Simple power mapping for sorting components
            // Higher rank components should be used in back hand usually.
            if (comp == EightCardsCompType.Pair) return 1;
            if (comp == EightCardsCompType.ThreeOfKind) return 10;
            if (comp == EightCardsCompType.ThreeCardsFlushStraight) return 15;
            if (comp == EightCardsCompType.FourCardsFlushStraight) return 25;
            if (comp == EightCardsCompType.FourOfKind) return 30;
            // Add more if needed from EightCardsCompType
            return (int)comp; 
        }

        static List<(EightCardOverAllHandRank, EightCardOverAllHandRank)> SplitHand(List<EightCardsCompType> comps)
        {
            if (comps == null || comps.Count == 0) return new List<(EightCardOverAllHandRank, EightCardOverAllHandRank)>();

            // 4. Input for this function is a list of comp, and you can sort the components 
            // from high comp to low to easier for you to map which valid hand based on combo components.
            comps.Sort((a, b) => GetCompPower(b).CompareTo(GetCompPower(a)));

            // 1. Explore all possible legal split hand solutions based on input hand components.
            var solutions = new List<(EightCardOverAllHandRank, EightCardOverAllHandRank)>();

            // 2. Use UtilFunc.GetPermutation to get all possible split component groups.
            // Always select no more half number of components count.
            int maxFrontCount = comps.Count / 2;

            for (int selectCount = 0; selectCount <= maxFrontCount; selectCount++)
            {
                var possibleGroups = UtilFunc.GetPermutationAllowedDuplicated(comps, selectCount);
                foreach (var group in possibleGroups)
                {
                    var frontGroup = group.Selected;
                    var backGroup = group.Remaining;

                    var frontRank = MapToRank(frontGroup);
                    var backRank = MapToRank(backGroup);

                    // 3. check the return for MapToRank, if they are None, then it's invalid, skip this solution.
                    if (frontRank == EightCardOverAllHandRank.None || backRank == EightCardOverAllHandRank.None) continue;

                    // 5. in the inner loop always check if front > back, if it is, then swap the front and back as valid solution.
                    if ((int)frontRank > (int)backRank)
                    {
                        // Try swapping
                        var swappedFrontRank = backRank;
                        var swappedBackRank = frontRank;

                        if ((int)swappedBackRank >= (int)swappedFrontRank)
                        {
                            solutions.Add((swappedFrontRank, swappedBackRank));
                        }
                    }
                    else
                    {
                        solutions.Add((frontRank, backRank));
                    }
                }
            }

            return solutions.Distinct().ToList();
        }

        static EightCardOverAllHandRank MapToRank(List<EightCardsCompType> comps)
        {
            if (comps == null || comps.Count == 0) return EightCardOverAllHandRank.Nothing;
            
            // Sort high to low
            comps.Sort((a, b) => GetCompPower(b).CompareTo(GetCompPower(a)));

            if (comps.Count == 1)
            {
                if (Enum.TryParse<EightCardOverAllHandRank>(comps[0].ToString(), out var result))
                {
                    return result;
                }
            }
            
            if (comps.Count == 2)
            {
                var c1 = comps[0];
                var c2 = comps[1];

                if (c1 == EightCardsCompType.ThreeOfKind && c2 == EightCardsCompType.Pair) return EightCardOverAllHandRank.FullHouse;
                if (c1 == EightCardsCompType.ThreeCardsFlushStraight && c2 == EightCardsCompType.Pair) return EightCardOverAllHandRank.Mansion;
                if (c1 == EightCardsCompType.Pair && c2 == EightCardsCompType.Pair) return EightCardOverAllHandRank.TwoPairs;
            }

            return EightCardOverAllHandRank.None;
        }

        static void SaveStats(string path, Dictionary<EightCardOverAllHandRank, double> front, Dictionary<EightCardOverAllHandRank, double> back)
        {
            double totalFront = front.Values.Sum();
            double totalBack = back.Values.Sum();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Hand Position,Rank,Count,Probablities,Win/NoLose probablity");
                
                // Front Hand
                var sortedFront = front.OrderByDescending(e => (int)e.Key).ToList();
                double cumulativeFront = 0;
                var frontLines = new List<string>();
                
                // Start from bottom (Nothing) to accumulate
                for (int i = sortedFront.Count - 1; i >= 0; i--)
                {
                    var entry = sortedFront[i];
                    double prob = totalFront > 0 ? entry.Value / totalFront : 0;
                    cumulativeFront += prob;
                    frontLines.Add($"Front,{entry.Key},{entry.Value:F2},{prob:P8},{cumulativeFront:P8}");
                }
                
                // Reverse to have strongest at top
                frontLines.Reverse();
                foreach (var line in frontLines) writer.WriteLine(line);

                // Back Hand
                var sortedBack = back.OrderByDescending(e => (int)e.Key).ToList();
                double cumulativeBack = 0;
                var backLines = new List<string>();

                for (int i = sortedBack.Count - 1; i >= 0; i--)
                {
                    var entry = sortedBack[i];
                    double prob = totalBack > 0 ? entry.Value / totalBack : 0;
                    cumulativeBack += prob;
                    backLines.Add($"Back,{entry.Key},{entry.Value:F2},{prob:P8},{cumulativeBack:P8}");
                }

                backLines.Reverse();
                foreach (var line in backLines) writer.WriteLine(line);
            }
        }
    }
}
