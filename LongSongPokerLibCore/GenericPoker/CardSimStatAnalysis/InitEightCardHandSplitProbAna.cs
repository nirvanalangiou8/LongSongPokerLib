using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace LongSongPokerLibCore.GenericPoker
{
    public class InitEightCardHandSplitProbAna
    {
        public static (Dictionary<EightCardOverAllHandRank, double> FrontStats, Dictionary<EightCardOverAllHandRank, double> BackStats) Run(string? inputPath = null, string? outputPath = null)
        {
            return Analyze(inputPath, outputPath);
        }

        public static string ResolveInputPath(string? inputPath)
        {
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (File.Exists(inputPath))
                    return Path.GetFullPath(inputPath);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string candidate = Path.GetFullPath(Path.Combine(baseDir, inputPath));
                if (File.Exists(candidate))
                    return candidate;

                string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                candidate = Path.GetFullPath(Path.Combine(projectRoot, inputPath));
                if (File.Exists(candidate))
                    return candidate;

                candidate = Path.GetFullPath(Path.Combine(projectRoot, "GenericPoker", "CardSimStatAnalysis", inputPath));
                if (File.Exists(candidate))
                    return candidate;

                candidate = Path.GetFullPath(Path.Combine(projectRoot, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", inputPath));
                if (File.Exists(candidate))
                    return candidate;

                string fileName = Path.GetFileName(inputPath);
                candidate = Path.GetFullPath(Path.Combine(projectRoot, "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                if (File.Exists(candidate))
                    return candidate;

                candidate = Path.GetFullPath(Path.Combine(projectRoot, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                if (File.Exists(candidate))
                    return candidate;
            }

            // Default fallback
            string defaultBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            string root = Path.GetFullPath(Path.Combine(defaultBaseDir, "..", "..", ".."));
            string defaultCandidate = Path.GetFullPath(Path.Combine(root, "GenericPoker", "CardSimStatAnalysis", "Data", "stats_result_8cards.csv"));
            if (File.Exists(defaultCandidate))
                return defaultCandidate;

            defaultCandidate = Path.GetFullPath(Path.Combine(root, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", "stats_result_8cards.csv"));
            if (File.Exists(defaultCandidate))
                return defaultCandidate;

            defaultCandidate = Path.GetFullPath(Path.Combine(root, "LongSongPokerLibCore", "stats_result.csv"));
            if (File.Exists(defaultCandidate))
                return defaultCandidate;

            defaultCandidate = Path.GetFullPath(Path.Combine(root, "stats_result.csv"));
            if (File.Exists(defaultCandidate))
                return defaultCandidate;

            return inputPath ?? defaultCandidate;
        }

        public static string ResolveOutputPath(string? outputPath)
        {
            if (!string.IsNullOrEmpty(outputPath))
            {
                if (Path.IsPathRooted(outputPath))
                    return outputPath;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                string sourceFileDir = Directory.Exists(Path.Combine(projectRoot, "GenericPoker", "CardSimStatAnalysis"))
                    ? Path.Combine(projectRoot, "GenericPoker", "CardSimStatAnalysis")
                    : Path.Combine(projectRoot, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis");

                return Path.GetFullPath(Path.Combine(sourceFileDir, outputPath));
            }

            string defaultBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            string root = Path.GetFullPath(Path.Combine(defaultBaseDir, "..", "..", ".."));
            string sourceDir = Directory.Exists(Path.Combine(root, "GenericPoker", "CardSimStatAnalysis"))
                ? Path.Combine(root, "GenericPoker", "CardSimStatAnalysis")
                : Path.Combine(root, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis");

            return Path.Combine(sourceDir, "front_back_stats.csv");
        }

        public static (Dictionary<EightCardOverAllHandRank, double> FrontStats, Dictionary<EightCardOverAllHandRank, double> BackStats) Analyze(string? inputPath = null, string? outputPath = null)
        {
            string resolvedInputPath = ResolveInputPath(inputPath);
            string resolvedOutputPath = ResolveOutputPath(outputPath);

            if (!File.Exists(resolvedInputPath))
            {
                Console.WriteLine($"Input file not found: {resolvedInputPath}");
                return (new Dictionary<EightCardOverAllHandRank, double>(), new Dictionary<EightCardOverAllHandRank, double>());
            }

            var frontHandStats = new Dictionary<EightCardOverAllHandRank, double>();
            var backHandStats = new Dictionary<EightCardOverAllHandRank, double>();
            long totalInputCount = 0;

            var lines = File.ReadAllLines(resolvedInputPath);
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

            if (!string.IsNullOrEmpty(resolvedOutputPath))
            {
                SaveStats(resolvedOutputPath, frontHandStats, backHandStats);
                Console.WriteLine($"Analysis completed. Results saved to {resolvedOutputPath}");
            }

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

            return (frontHandStats, backHandStats);
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

        public static void SaveStats(string path, Dictionary<EightCardOverAllHandRank, double> front, Dictionary<EightCardOverAllHandRank, double> back)
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

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
