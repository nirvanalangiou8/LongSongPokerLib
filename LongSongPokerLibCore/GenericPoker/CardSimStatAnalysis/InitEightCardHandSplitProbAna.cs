using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace GenericPoker.CardSimStatAnalysis
{
    public class InitEightCardHandSplitProbAna
    {
        public static (Dictionary<SimCardOverAllHandRank, double> FrontStats, Dictionary<SimCardOverAllHandRank, double> BackStats) Run(string? inputPath = null, string? outputPath = null)
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

        public static string ResolveOutputPath(string? outputPath, string? inputPath = null)
        {
            if (!string.IsNullOrEmpty(outputPath))
            {
                if (Path.IsPathRooted(outputPath))
                    return Path.GetFullPath(outputPath);

                string? inputToResolve = !string.IsNullOrEmpty(inputPath) ? inputPath : null;
                string resolvedInput = ResolveInputPath(inputToResolve);
                string? inputDir = Path.GetDirectoryName(resolvedInput);
                if (!string.IsNullOrEmpty(inputDir))
                {
                    return Path.GetFullPath(Path.Combine(inputDir, outputPath));
                }

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

        public static (Dictionary<SimCardOverAllHandRank, double> FrontStats, Dictionary<SimCardOverAllHandRank, double> BackStats) Analyze(string? inputPath = null, string? outputPath = null)
        {
            string resolvedInputPath = ResolveInputPath(inputPath);
            string resolvedOutputPath = ResolveOutputPath(outputPath, resolvedInputPath);

            if (!File.Exists(resolvedInputPath))
            {
                Console.WriteLine($"Input file not found: {resolvedInputPath}");
                return (new Dictionary<SimCardOverAllHandRank, double>(), new Dictionary<SimCardOverAllHandRank, double>());
            }

            var frontHandStats = new Dictionary<SimCardOverAllHandRank, double>();
            var backHandStats = new Dictionary<SimCardOverAllHandRank, double>();
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
                    backHandStats[SimCardOverAllHandRank.Nothing] = backHandStats.GetValueOrDefault(SimCardOverAllHandRank.Nothing) + count;
                    frontHandStats[SimCardOverAllHandRank.Nothing] = frontHandStats.GetValueOrDefault(SimCardOverAllHandRank.Nothing) + count;
                    continue;
                }

                if (handName == "ThreeCardsFlushStraight_ThreeOfKind*2")
                {
                    Console.WriteLine($"Invalid hand name: {handName}");
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
                    frontHandStats[SimCardOverAllHandRank.None] = frontHandStats.GetValueOrDefault(SimCardOverAllHandRank.None) + count;
                    backHandStats[SimCardOverAllHandRank.None] = backHandStats.GetValueOrDefault(SimCardOverAllHandRank.None) + count;
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

        public static List<PokerComponents> ParseHandName(string handName)
        {
            var comps = new List<PokerComponents>();
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

                if (Enum.TryParse<SimCardsCompType>(typeStr, out var compType))
                {
                    for (int i = 0; i < count; i++)
                        comps.Add(new PokerComponents(compType));
                }
            }
            // Sort by power descending to help balanced strategy
            return comps.OrderByDescending(c => c.Power).ToList();
        }

        public static List<(SimCardOverAllHandRank, SimCardOverAllHandRank)> SplitHand(
            List<PokerComponents> comps,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            if (comps == null || comps.Count == 0) return new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

            // 1. Break down each component into atomic sub-components based on card constraints.
            var breakdownSequences = comps
                .Select(c => c.BreakDown(minFlushStraightCards, minFlushCards, minStraightCards, minKindCards))
                .ToList();

            // 2. Generate Cartesian Product of all candidate breakdowns across components.
            var candidateCombinations = PokerComponents.CartesianProduct(breakdownSequences);

            var solutions = new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

            foreach (var combination in candidateCombinations)
            {
                // Flatten the candidate components
                var atomicComps = combination.SelectMany(c => c).ToList();

                // Sort atomic components by power descending
                atomicComps.Sort((a, b) => b.Power.CompareTo(a.Power));

                // Explore all possible split component groups (up to half total count for front hand)
                int maxFrontCount = atomicComps.Count / 2;

                for (int selectCount = 0; selectCount <= maxFrontCount; selectCount++)
                {
                    var possibleGroups = UtilFunc.GetPermutationAllowedDuplicated(atomicComps, selectCount);
                    foreach (var group in possibleGroups)
                    {
                        var frontGroup = group.Selected;
                        var backGroup = group.Remaining;

                        var frontRank = AssemblyComponent.AssembleHandRank(frontGroup);
                        var backRank = AssemblyComponent.AssembleHandRank(backGroup);

                        // If either rank is None, it is an invalid split; skip it.
                        if (frontRank == SimCardOverAllHandRank.None || backRank == SimCardOverAllHandRank.None) continue;

                        // Ensure back rank >= front rank (swap if necessary)
                        if ((int)frontRank > (int)backRank)
                        {
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
            }

            var uniqueSolutions = solutions.Distinct().ToList();
            return FilterDominatedSolutions(uniqueSolutions);
        }

        /// <summary>
        /// Filters out candidate solutions that are dominated (obviously loser solutions).
        /// A solution (F2, B2) is dominated by (F1, B1) if F1 >= F2 and B1 >= B2, and at least one inequality is strict.
        /// </summary>
        public static List<(SimCardOverAllHandRank, SimCardOverAllHandRank)> FilterDominatedSolutions(
            IEnumerable<(SimCardOverAllHandRank Front, SimCardOverAllHandRank Back)> solutions)
        {
            if (solutions == null) return new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

            var list = solutions.Distinct().ToList();
            var filtered = new List<(SimCardOverAllHandRank Front, SimCardOverAllHandRank Back)>();

            for (int i = 0; i < list.Count; i++)
            {
                var s1 = list[i];
                bool isDominated = false;

                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    var s2 = list[j];

                    // s2 dominates s1 if s2 is >= s1 in both front and back ranks, and strictly better in at least one rank
                    if ((int)s2.Front >= (int)s1.Front && (int)s2.Back >= (int)s1.Back &&
                        ((int)s2.Front > (int)s1.Front || (int)s2.Back > (int)s1.Back))
                    {
                        isDominated = true;
                        break;
                    }
                }

                if (!isDominated)
                {
                    filtered.Add(s1);
                }
            }

            return filtered;
        }

        public static List<(SimCardOverAllHandRank, SimCardOverAllHandRank)> SplitHand(
            List<SimCardsCompType> compTypes,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            if (compTypes == null) return new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();
            return SplitHand(compTypes.Select(t => new PokerComponents(t)).ToList(), minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }

        public static void SaveStats(string path, Dictionary<SimCardOverAllHandRank, double> front, Dictionary<SimCardOverAllHandRank, double> back)
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
