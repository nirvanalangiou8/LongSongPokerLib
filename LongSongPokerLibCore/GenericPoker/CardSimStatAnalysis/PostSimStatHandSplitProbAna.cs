using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class PostSimStatHandSplitProbAna
    {
        public static Dictionary<BattleSubHandOrder, Dictionary<PokerOverAllHandRank, double>> Run(string inputPath, string? outputPath = null, ICardRule? rule = null)
        {
            return Analyze(inputPath, outputPath, rule);
        }

        public static Dictionary<BattleSubHandOrder, Dictionary<PokerOverAllHandRank, double>> Analyze(string inputPath, string? outputPath = null, ICardRule? rule = null)
        {
            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                Console.WriteLine($"Input file not found: {inputPath}");
                return new Dictionary<BattleSubHandOrder, Dictionary<PokerOverAllHandRank, double>>();
            }

            var effectiveRule = rule ?? ((inputPath.Contains("9cards") || inputPath.Contains("9_cards") || inputPath.Contains("9card")) ? NineCardTwoHandRule.Default : EightCardRule.Default);
            var slots = effectiveRule.SubHandSlots;
            int slotCount = slots.Count;

            var handStats = new Dictionary<BattleSubHandOrder, Dictionary<PokerOverAllHandRank, double>>();
            for (int i = 0; i < slotCount; i++)
            {
                handStats[slots[i].Order] = new Dictionary<PokerOverAllHandRank, double>();
            }

            long totalInputCount = 0;
            var lines = File.ReadAllLines(inputPath);
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
                    for (int i = 0; i < slotCount; i++)
                    {
                        var order = slots[i].Order;
                        handStats[order][PokerOverAllHandRank.Nothing] = handStats[order].GetValueOrDefault(PokerOverAllHandRank.Nothing) + count;
                    }
                    continue;
                }

                var components = ParseHandName(handName, effectiveRule);

                var solutions = SplitHand(components, effectiveRule);
                if (solutions.Count > 0)
                {
                    double perSolutionCount = (double)count / solutions.Count;
                    foreach (var sol in solutions)
                    {
                        for (int i = 0; i < slotCount; i++)
                        {
                            var order = slots[i].Order;
                            var rank = sol[i];
                            handStats[order][rank] = handStats[order].GetValueOrDefault(rank) + perSolutionCount;
                        }
                    }
                }
                else
                {
                    // If no valid split found (should not happen with legal hands), fallback to None
                    // This is for sanity check
                    for (int i = 0; i < slotCount; i++)
                    {
                        var order = slots[i].Order;
                        handStats[order][PokerOverAllHandRank.None] = handStats[order].GetValueOrDefault(PokerOverAllHandRank.None) + count;
                    }
                }
            }

            if (!string.IsNullOrEmpty(outputPath))
            {
                SaveStats(outputPath, handStats, effectiveRule, inputPath, headerNotes);
                Console.WriteLine($"Analysis completed. Results saved to {outputPath}");
            }

            Console.WriteLine($"Total Input Appearance Count: {totalInputCount}");
            bool allMatched = true;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = slots[i];
                string posName = slot.Order.ToString();
                //GetPositionName(i, slotCount, slot.Order);
                double totalSlot = handStats[slot.Order].Values.Sum();
                Console.WriteLine($"Total {posName} Stat Count: {totalSlot:F2}");
                if (Math.Abs(totalSlot - totalInputCount) >= 0.001)
                {
                    allMatched = false;
                    Console.WriteLine($"ERROR: {posName} stat count ({totalSlot:F2}) does not match input count ({totalInputCount})!");
                }
            }

            if (allMatched)
            {
                Console.WriteLine("input and stat count check sum correct.");
            }

            return handStats;
        }

        /*public static string GetPositionName(int slotIndex, int totalSlots, BattleSubHandOrder order)
        {
            if (totalSlots <= 2)
            {
                return slotIndex == 0 ? "Front" : "Back";
            }
            return order.ToString();
        }*/

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

        public static List<List<PokerOverAllHandRank>> SplitHand(
            List<PokerComponents> comps,
            ICardRule? rule = null,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            var effectiveRule = rule ?? comps?.FirstOrDefault(c => c.Rule != null)?.Rule ?? EightCardRule.Default;
            var slots = effectiveRule.SubHandSlots;
            int slotCount = slots.Count;

            if (comps == null || comps.Count == 0)
            {
                var nothingSolution = new List<PokerOverAllHandRank>();
                for (int i = 0; i < slotCount; i++)
                {
                    nothingSolution.Add(PokerOverAllHandRank.Nothing);
                }
                return new List<List<PokerOverAllHandRank>> { nothingSolution };
            }

            // 1. Break down each component into atomic sub-components based on card constraints.
            var breakdownSequences = comps
                .Select(c => c.BreakDown(effectiveRule, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards))
                .ToList();

            // 2. Generate Cartesian Product of all candidate breakdowns across components.
            var candidateCombinations = PokerComponents.CartesianProduct(breakdownSequences);

            var solutions = new List<List<PokerOverAllHandRank>>();

            foreach (var combination in candidateCombinations)
            {
                // Flatten the candidate components
                var atomicComps = combination.SelectMany(c => c).ToList();

                // Sort atomic components by power descending
                atomicComps.Sort((a, b) => b.Power.CompareTo(a.Power));

                var allPartitions = UtilFunc.GenerateSlotPartitions(atomicComps, slotCount);

                foreach (var partition in allPartitions)
                {
                    bool validRanks = true;
                    var rankList = new List<PokerOverAllHandRank>();

                    for (int i = 0; i < slotCount; i++)
                    {
                        var slot = slots[i];
                        var slotComps = partition[i];

                        var rank = effectiveRule.AssembleHandRank(slotComps, slot.Order);
                        if (rank == PokerOverAllHandRank.None)
                        {
                            validRanks = false;
                            break;
                        }

                        rankList.Add(rank);
                    }

                    if (!validRanks) continue;

                    // Deployment ordering check: rankList[0] <= rankList[1] <= ... <= rankList[slotCount - 1]
                    bool validDeployment = true;
                    for (int i = 0; i < rankList.Count - 1; i++)
                    {
                        if ((int)rankList[i] > (int)rankList[i + 1])
                        {
                            validDeployment = false;
                            break;
                        }
                    }

                    if (!validDeployment) continue;

                    solutions.Add(rankList);
                }
            }

            return FilterDominatedSolutions(solutions);
        }

        /// <summary>
        /// Filters out candidate solutions that are dominated (obviously loser solutions).
        /// A solution S2 dominates S1 if S2 is >= S1 in all hand ranks, and strictly better in at least one rank.
        /// </summary>
        public static List<List<PokerOverAllHandRank>> FilterDominatedSolutions(
            IEnumerable<List<PokerOverAllHandRank>> solutions)
        {
            if (solutions == null) return new List<List<PokerOverAllHandRank>>();

            var list = new List<List<PokerOverAllHandRank>>();
            var seen = new HashSet<string>();
            foreach (var sol in solutions)
            {
                var key = string.Join(",", sol.Select(r => (int)r));
                if (seen.Add(key))
                {
                    list.Add(sol);
                }
            }

            var filtered = new List<List<PokerOverAllHandRank>>();

            for (int i = 0; i < list.Count; i++)
            {
                var s1 = list[i];
                bool isDominated = false;

                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    var s2 = list[j];

                    if (s1.Count != s2.Count) continue;

                    bool allGreaterOrEqual = true;
                    bool atLeastOneStrictlyGreater = false;

                    for (int k = 0; k < s1.Count; k++)
                    {
                        if ((int)s2[k] < (int)s1[k])
                        {
                            allGreaterOrEqual = false;
                            break;
                        }
                        if ((int)s2[k] > (int)s1[k])
                        {
                            atLeastOneStrictlyGreater = true;
                        }
                    }

                    if (allGreaterOrEqual && atLeastOneStrictlyGreater)
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

        
        public static void SaveStats(string path, Dictionary<BattleSubHandOrder, Dictionary<PokerOverAllHandRank, double>> handStats, ICardRule? rule = null, string? inputPath = null, List<string>? headerNotes = null)
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var effectiveRule = rule ?? EightCardRule.Default;
            var slots = effectiveRule.SubHandSlots;
            int slotCount = slots.Count;

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

                for (int slotIdx = 0; slotIdx < slotCount; slotIdx++)
                {
                    var slot = slots[slotIdx];
                    string posName = slot.Order.ToString();
                    //GetPositionName(slotIdx, slotCount, slot.Order);
                    var dict = handStats.TryGetValue(slot.Order, out var stats) ? stats : new Dictionary<PokerOverAllHandRank, double>();

                    var sorted = dict.OrderByDescending(e => (int)e.Key).ToList();
                    long totalCount = sorted.Sum(e => (long)Math.Round(e.Value, MidpointRounding.AwayFromZero));

                    long cumulativeCount = 0;
                    var posLines = new List<string>();

                    for (int i = sorted.Count - 1; i >= 0; i--)
                    {
                        var entry = sorted[i];
                        long count = (long)Math.Round(entry.Value, MidpointRounding.AwayFromZero);
                        cumulativeCount += count;
                        double prob = totalCount > 0 ? (double)count / totalCount : 0;
                        double winNoLoseProb = totalCount > 0 ? (double)cumulativeCount / totalCount : 0;
                        posLines.Add($"{posName},{entry.Key},{count},{cumulativeCount},{prob:P16},{winNoLoseProb:P16}");
                    }

                    posLines.Reverse();
                    foreach (var line in posLines) writer.WriteLine(line);
                }
            }
        }
    }
}


/*public static List<List<PokerOverAllHandRank>> SplitHand(
            List<BaseCompType> compTypes,
            ICardRule? rule = null,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            if (compTypes == null) return new List<List<PokerOverAllHandRank>>();
            return SplitHand(compTypes.Select(t => new PokerComponents(t, rule)).ToList(), rule, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }*/