using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;

namespace LongSongPokerLibCore.GenericPoker.EightCard.DataAnalysis
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
            string inputPath = Path.Combine(projectRoot, "stats_result.csv");
            string outputPath = Path.Combine(projectRoot, "GenericPoker", "EightCard", "DataAnalysis", "front_back_stats.csv");

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Input file not found: {inputPath}");
                return;
            }

            var frontHandStats = new Dictionary<EightCardFrontHandQualifiedHandAndRank, double>();
            var backHandStats = new Dictionary<EightCardBackHandQualifiedHandAndRank, double>();

            // Initialize stats dictionaries
            foreach (EightCardFrontHandQualifiedHandAndRank rank in Enum.GetValues(typeof(EightCardFrontHandQualifiedHandAndRank)))
                frontHandStats[rank] = 0;
            foreach (EightCardBackHandQualifiedHandAndRank rank in Enum.GetValues(typeof(EightCardBackHandQualifiedHandAndRank)))
                backHandStats[rank] = 0;

            var lines = File.ReadAllLines(inputPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("Hand Type"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                string handName = parts[0];
                if (!long.TryParse(parts[1], out long count)) continue;

                if (handName == "Nothing")
                {
                    backHandStats[EightCardBackHandQualifiedHandAndRank.Nothing] += count;
                    frontHandStats[EightCardFrontHandQualifiedHandAndRank.Nothing] += count;
                    continue;
                }
                // change below check to if handName contains FourCardsFlushStraight 
                if (handName.Contains("FourCardsFlushStraight*2")  || handName.Contains("FourOfKind_FourCardsFlushStraight"))
                {
                    Console.WriteLine("FourCardsFlushStraight*2");
                }
                
                if (handName.Contains("ThreeOfKind_Pair*2"))
                {
                    Console.WriteLine("ThreeOfKind_Pair*2");
                }
               

                var components = ParseHandName(handName);
                

                var solutions = SplitHand(components);
                if (solutions.Count > 0)
                {
                    double perSolutionCount = (double)count / solutions.Count;
                    foreach (var sol in solutions)
                    {
                        frontHandStats[sol.Item1] += perSolutionCount;
                        backHandStats[sol.Item2] += perSolutionCount;
                    }
                }
                else
                {
                    // If no valid split found (should not happen with legal hands), fallback to None
                    frontHandStats[EightCardFrontHandQualifiedHandAndRank.None] += count;
                    backHandStats[EightCardBackHandQualifiedHandAndRank.None] += count;
                }
            }

            SaveStats(outputPath, frontHandStats, backHandStats);
            Console.WriteLine($"Analysis completed. Results saved to {outputPath}");
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

        static List<(EightCardFrontHandQualifiedHandAndRank, EightCardBackHandQualifiedHandAndRank)> SplitHand(List<EightCardsCompType> comps)
        {
            if (comps == null || comps.Count == 0) return new List<(EightCardFrontHandQualifiedHandAndRank, EightCardBackHandQualifiedHandAndRank)>();

            // 4. Input for this function is a list of comp, and you can sort the components 
            // from high comp to low to easier for you to map which valid hand based on combo components.
            comps.Sort((a, b) => GetCompPower(b).CompareTo(GetCompPower(a)));

            // 1. Explore all possible legal split hand solutions based on input hand components.
            var solutions = new List<(EightCardFrontHandQualifiedHandAndRank, EightCardBackHandQualifiedHandAndRank)>();

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

                    var frontRank = MapToFrontRank(frontGroup);
                    var backRank = MapToBackRank(backGroup);

                    // 3. check the return for MaptoFront/BackRank , if they are None, then it's invalid, skip this solution.
                    if (frontRank == EightCardFrontHandQualifiedHandAndRank.None && frontGroup.Count > 0) continue;
                    if (backRank == EightCardBackHandQualifiedHandAndRank.None) continue;

                    // 5. in the inner loop always check if front > back, if it is, then swap the front and back as valid solution.
                    if (!IsBackStronger(frontRank, backRank))
                    {
                        // Try swapping
                        var swappedFrontRank = MapToFrontRank(backGroup);
                        var swappedBackRank = MapToBackRank(frontGroup);

                        if (swappedFrontRank != EightCardFrontHandQualifiedHandAndRank.None || backGroup.Count == 0)
                        {
                            if (swappedBackRank != EightCardBackHandQualifiedHandAndRank.None)
                            {
                                if (IsBackStronger(swappedFrontRank, swappedBackRank))
                                {
                                    solutions.Add((swappedFrontRank, swappedBackRank));
                                }
                            }
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

        static EightCardFrontHandQualifiedHandAndRank MapToFrontRank(List<EightCardsCompType> comps)
        {
            if (comps == null || comps.Count == 0) return EightCardFrontHandQualifiedHandAndRank.Nothing;
            
            // Sort high to low
            comps.Sort((a, b) => GetCompPower(b).CompareTo(GetCompPower(a)));

            if (comps.Count == 1)
            {
                var comp = comps[0];
                if (comp == EightCardsCompType.Pair) return EightCardFrontHandQualifiedHandAndRank.Pair;
                if (comp == EightCardsCompType.ThreeOfKind) return EightCardFrontHandQualifiedHandAndRank.ThreeOfKind;
                if (comp == EightCardsCompType.ThreeCardsFlushStraight) return EightCardFrontHandQualifiedHandAndRank.ThreeCardsFlushStraight;
                if (comp == EightCardsCompType.FourCardsFlushStraight) return EightCardFrontHandQualifiedHandAndRank.FourCardsFlushStraight;
                if (comp == EightCardsCompType.FourOfKind) return EightCardFrontHandQualifiedHandAndRank.FourOfKind;
            }
            
            if (comps.Count == 2)
            {
                if (comps[0] == EightCardsCompType.Pair && comps[1] == EightCardsCompType.Pair) 
                    return EightCardFrontHandQualifiedHandAndRank.TwoPairs;
            }

            return EightCardFrontHandQualifiedHandAndRank.None;
        }

        static EightCardBackHandQualifiedHandAndRank MapToBackRank(List<EightCardsCompType> comps)
        {
            if (comps == null || comps.Count == 0) return EightCardBackHandQualifiedHandAndRank.Nothing;

            // Sort high to low
            comps.Sort((a, b) => GetCompPower(b).CompareTo(GetCompPower(a)));

               
            // Handle combinations first
            if (comps.Count == 2)
            {
                var c1 = comps[0];
                var c2 = comps[1];

                if (c1 == EightCardsCompType.ThreeOfKind && c2 == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.FullHouse;
                if (c1 == EightCardsCompType.ThreeCardsFlushStraight && c2 == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.Mansion;
                if (c1 == EightCardsCompType.Pair && c2 == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.TwoPairs;
                // if none of above cases, return None.
                return EightCardBackHandQualifiedHandAndRank.None;
            } else if (comps.Count == 1)
            {
                // Single rank mapping
                var comp = comps[0];
                if (comp == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.Pair;
                if (comp == EightCardsCompType.ThreeOfKind) return EightCardBackHandQualifiedHandAndRank.ThreeOfKind;
                if (comp == EightCardsCompType.ThreeCardsFlushStraight)
                    return EightCardBackHandQualifiedHandAndRank.ThreeCardsFlushStraight;
                if (comp == EightCardsCompType.FourCardsFlushStraight)
                    return EightCardBackHandQualifiedHandAndRank.FourCardsFlushStraight;
                if (comp == EightCardsCompType.FourOfKind) return EightCardBackHandQualifiedHandAndRank.FourOfKind;
                if (comp == EightCardsCompType.FiveCardsStraight)
                    return EightCardBackHandQualifiedHandAndRank.FiveCardsStraight;
                if (comp == EightCardsCompType.FiveCardsFlush)
                    return EightCardBackHandQualifiedHandAndRank.FiveCardsFlush;
                if (comp == EightCardsCompType.SixCardsStraight)
                    return EightCardBackHandQualifiedHandAndRank.SixCardsStraight;
                if (comp == EightCardsCompType.SixCardsFlush)
                    return EightCardBackHandQualifiedHandAndRank.SixCardsFlush;
                if (comp == EightCardsCompType.SevenCardsStraight)
                    return EightCardBackHandQualifiedHandAndRank.SevenCardsStraight;
                if (comp == EightCardsCompType.SevenCardsFlush)
                    return EightCardBackHandQualifiedHandAndRank.SevenCardsFlush;
                if (comp == EightCardsCompType.EightCardsStraight)
                    return EightCardBackHandQualifiedHandAndRank.EightCardsStraight;
                if (comp == EightCardsCompType.EightCardsFlush)
                    return EightCardBackHandQualifiedHandAndRank.EightCardsFlush;
                if (comp == EightCardsCompType.FiveCardsFlushStraight)
                    return EightCardBackHandQualifiedHandAndRank.FiveCardsFlushStraight;
                if (comp == EightCardsCompType.SixCardsFlushStraight)
                    return EightCardBackHandQualifiedHandAndRank.SixCardsFlushStraight;
            }

            return EightCardBackHandQualifiedHandAndRank.None;
        }

        static bool IsBackStronger(EightCardFrontHandQualifiedHandAndRank front, EightCardBackHandQualifiedHandAndRank back)
        {
            if (front == EightCardFrontHandQualifiedHandAndRank.None) return true;
            if (back == EightCardBackHandQualifiedHandAndRank.None) return false;

            var frontOverall = MapToOverallRank(front);
            var backOverall = MapToOverallRank(back);
            
            return (int)backOverall >= (int)frontOverall; 
        }

        static EightCardOverAllHandRank MapToOverallRank(EightCardFrontHandQualifiedHandAndRank rank)
        {
            if (Enum.TryParse<EightCardOverAllHandRank>(rank.ToString(), out var result))
            {
                return result;
            }
            return EightCardOverAllHandRank.None;
        }

        static EightCardOverAllHandRank MapToOverallRank(EightCardBackHandQualifiedHandAndRank rank)
        {
            if (Enum.TryParse<EightCardOverAllHandRank>(rank.ToString(), out var result))
            {
                return result;
            }
            return EightCardOverAllHandRank.None;
        }

        static void SaveStats(string path, Dictionary<EightCardFrontHandQualifiedHandAndRank, double> front, Dictionary<EightCardBackHandQualifiedHandAndRank, double> back)
        {
            double totalFront = front.Values.Sum();
            double totalBack = back.Values.Sum();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Hand Position,Rank,Count,Probablities,Win/NoLose probablity");
                
                // Front Hand
                var sortedFront = front.OrderByDescending(e => (int)MapToOverallRank(e.Key)).ToList();
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
                var sortedBack = back.OrderByDescending(e => (int)MapToOverallRank(e.Key)).ToList();
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
