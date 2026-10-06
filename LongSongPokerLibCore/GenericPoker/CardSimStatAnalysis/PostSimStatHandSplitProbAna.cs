using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace GenericPoker.CardSimStatAnalysis
{
    public class PostSimStatHandSplitProbAna
    {
        public static (Dictionary<PokerOverAllHandRank, double> FrontStats, Dictionary<PokerOverAllHandRank, double> BackStats) Run(string? inputPath = null, string? outputPath = null, ICardRule? rule = null)
        {
            return Analyze(inputPath, outputPath, rule);
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

                // Check multiple ancestor directory depths (e.g. project root, solution root)
                DirectoryInfo? currentDir = new DirectoryInfo(baseDir);
                while (currentDir != null)
                {
                    candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, inputPath));
                    if (File.Exists(candidate))
                        return candidate;

                    candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "GenericPoker", "CardSimStatAnalysis", inputPath));
                    if (File.Exists(candidate))
                        return candidate;

                    candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", inputPath));
                    if (File.Exists(candidate))
                        return candidate;

                    string fileName = Path.GetFileName(inputPath);
                    candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                    if (File.Exists(candidate))
                        return candidate;

                    candidate = Path.GetFullPath(Path.Combine(currentDir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                    if (File.Exists(candidate))
                        return candidate;

                    currentDir = currentDir.Parent;
                }
            }

            // Default fallback
            string defaultBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo? fallbackDir = new DirectoryInfo(defaultBaseDir);
            while (fallbackDir != null)
            {
                string defaultCandidate = Path.GetFullPath(Path.Combine(fallbackDir.FullName, "GenericPoker", "CardSimStatAnalysis", "Data", "stats_result_8cards.csv"));
                if (File.Exists(defaultCandidate))
                    return defaultCandidate;

                defaultCandidate = Path.GetFullPath(Path.Combine(fallbackDir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", "stats_result_8cards.csv"));
                if (File.Exists(defaultCandidate))
                    return defaultCandidate;

                defaultCandidate = Path.GetFullPath(Path.Combine(fallbackDir.FullName, "LongSongPokerLibCore", "stats_result.csv"));
                if (File.Exists(defaultCandidate))
                    return defaultCandidate;

                defaultCandidate = Path.GetFullPath(Path.Combine(fallbackDir.FullName, "stats_result.csv"));
                if (File.Exists(defaultCandidate))
                    return defaultCandidate;

                fallbackDir = fallbackDir.Parent;
            }

            return inputPath ?? "";
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

        public static (Dictionary<PokerOverAllHandRank, double> FrontStats, Dictionary<PokerOverAllHandRank, double> BackStats) Analyze(string? inputPath = null, string? outputPath = null, ICardRule? rule = null)
        {
            string resolvedInputPath = ResolveInputPath(inputPath);
            string resolvedOutputPath = ResolveOutputPath(outputPath, resolvedInputPath);

            var effectiveRule = rule ?? ((resolvedInputPath.Contains("9cards") || resolvedInputPath.Contains("9_cards") || resolvedInputPath.Contains("9card")) ? NineCardRule.Default : EightCardRule.Default);

            if (!File.Exists(resolvedInputPath))
            {
                Console.WriteLine($"Input file not found: {resolvedInputPath}");
                return (new Dictionary<PokerOverAllHandRank, double>(), new Dictionary<PokerOverAllHandRank, double>());
            }

            var frontHandStats = new Dictionary<PokerOverAllHandRank, double>();
            var backHandStats = new Dictionary<PokerOverAllHandRank, double>();
            long totalInputCount = 0;

            var lines = File.ReadAllLines(resolvedInputPath);
            var headerNotes = new List<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#"))
                {
                    headerNotes.Add(line);
                    continue;
                }

                if (line.StartsWith("Hand Type"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                string handName = parts[0];
                if (!long.TryParse(parts[1], out long count)) continue;

                totalInputCount += count;

                if (handName == "Nothing")
                {
                    backHandStats[PokerOverAllHandRank.Nothing] = backHandStats.GetValueOrDefault(PokerOverAllHandRank.Nothing) + count;
                    frontHandStats[PokerOverAllHandRank.Nothing] = frontHandStats.GetValueOrDefault(PokerOverAllHandRank.Nothing) + count;
                    continue;
                }

                var components = ParseHandName(handName, effectiveRule);

                var solutions = SplitHand(components, effectiveRule);
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
                    frontHandStats[PokerOverAllHandRank.None] = frontHandStats.GetValueOrDefault(PokerOverAllHandRank.None) + count;
                    backHandStats[PokerOverAllHandRank.None] = backHandStats.GetValueOrDefault(PokerOverAllHandRank.None) + count;
                }
            }

            if (!string.IsNullOrEmpty(resolvedOutputPath))
            {
                SaveStats(resolvedOutputPath, frontHandStats, backHandStats, resolvedInputPath, headerNotes);
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

        public static List<PokerComponents> ParseHandName(string handName, ICardRule? rule = null)
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

                if (Enum.TryParse<BaseCompType>(typeStr, out var compType))
                {
                    for (int i = 0; i < count; i++)
                        comps.Add(new PokerComponents(compType, rule));
                }
            }
            // Sort by power descending to help balanced strategy
            return comps.OrderByDescending(c => c.Power).ToList();
        }

        public static List<(PokerOverAllHandRank, PokerOverAllHandRank)> SplitHand(
            List<PokerComponents> comps,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            return SplitHand(comps, null, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }

        public static List<(PokerOverAllHandRank, PokerOverAllHandRank)> SplitHand(
            List<PokerComponents> comps,
            ICardRule? rule,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            if (comps == null || comps.Count == 0) return new List<(PokerOverAllHandRank, PokerOverAllHandRank)>();

            var effectiveRule = rule ?? comps.FirstOrDefault(c => c.Rule != null)?.Rule ?? EightCardRule.Default;

            // 1. Break down each component into atomic sub-components based on card constraints.
            var breakdownSequences = comps
                .Select(c => c.BreakDown(effectiveRule, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards))
                .ToList();

            // 2. Generate Cartesian Product of all candidate breakdowns across components.
            var candidateCombinations = PokerComponents.CartesianProduct(breakdownSequences);

            var solutions = new List<(PokerOverAllHandRank, PokerOverAllHandRank)>();

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

                        var frontRank = effectiveRule.AssembleHandRank(frontGroup);
                        var backRank = effectiveRule.AssembleHandRank(backGroup);

                        // If either rank is None, it is an invalid split; skip it.
                        if (frontRank == PokerOverAllHandRank.None || backRank == PokerOverAllHandRank.None) continue;

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
        public static List<(PokerOverAllHandRank, PokerOverAllHandRank)> FilterDominatedSolutions(
            IEnumerable<(PokerOverAllHandRank Front, PokerOverAllHandRank Back)> solutions)
        {
            if (solutions == null) return new List<(PokerOverAllHandRank, PokerOverAllHandRank)>();

            var list = solutions.Distinct().ToList();
            var filtered = new List<(PokerOverAllHandRank Front, PokerOverAllHandRank Back)>();

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

        public static List<(PokerOverAllHandRank, PokerOverAllHandRank)> SplitHand(
            List<BaseCompType> compTypes,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            return SplitHand(compTypes, null, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }

        public static List<(PokerOverAllHandRank, PokerOverAllHandRank)> SplitHand(
            List<BaseCompType> compTypes,
            ICardRule? rule,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            if (compTypes == null) return new List<(PokerOverAllHandRank, PokerOverAllHandRank)>();
            return SplitHand(compTypes.Select(t => new PokerComponents(t, rule)).ToList(), rule, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }

        public static void SaveStats(string path, Dictionary<PokerOverAllHandRank, double> front, Dictionary<PokerOverAllHandRank, double> back, string? inputPath = null, List<string>? headerNotes = null)
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var sortedFront = front.OrderByDescending(e => (int)e.Key).ToList();
            long totalFrontCount = sortedFront.Sum(e => (long)Math.Round(e.Value, MidpointRounding.AwayFromZero));

            var sortedBack = back.OrderByDescending(e => (int)e.Key).ToList();
            long totalBackCount = sortedBack.Sum(e => (long)Math.Round(e.Value, MidpointRounding.AwayFromZero));

            using (var writer = new StreamWriter(path))
            {
                if (headerNotes != null)
                {
                    foreach (var note in headerNotes)
                    {
                        writer.WriteLine(note);
                    }
                }
                if (!string.IsNullOrEmpty(inputPath))
                {
                    writer.WriteLine($"# Source: {inputPath}");
                }

                writer.WriteLine("Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities");
                
                // Front Hand
                long cumulativeFrontCount = 0;
                var frontLines = new List<string>();
                
                // Start from bottom (Nothing) to accumulate
                for (int i = sortedFront.Count - 1; i >= 0; i--)
                {
                    var entry = sortedFront[i];
                    long count = (long)Math.Round(entry.Value, MidpointRounding.AwayFromZero);
                    cumulativeFrontCount += count;
                    double prob = totalFrontCount > 0 ? (double)count / totalFrontCount : 0;
                    double winNoLoseProb = totalFrontCount > 0 ? (double)cumulativeFrontCount / totalFrontCount : 0;
                    frontLines.Add($"Front,{entry.Key},{count},{cumulativeFrontCount},{prob:P16},{winNoLoseProb:P16}");
                }
                
                // Reverse to have strongest at top
                frontLines.Reverse();
                foreach (var line in frontLines) writer.WriteLine(line);

                // Back Hand
                long cumulativeBackCount = 0;
                var backLines = new List<string>();

                for (int i = sortedBack.Count - 1; i >= 0; i--)
                {
                    var entry = sortedBack[i];
                    long count = (long)Math.Round(entry.Value, MidpointRounding.AwayFromZero);
                    cumulativeBackCount += count;
                    double prob = totalBackCount > 0 ? (double)count / totalBackCount : 0;
                    double winNoLoseProb = totalBackCount > 0 ? (double)cumulativeBackCount / totalBackCount : 0;
                    backLines.Add($"Back,{entry.Key},{count},{cumulativeBackCount},{prob:P16},{winNoLoseProb:P16}");
                }

                backLines.Reverse();
                foreach (var line in backLines) writer.WriteLine(line);
            }
        }
    }
}
