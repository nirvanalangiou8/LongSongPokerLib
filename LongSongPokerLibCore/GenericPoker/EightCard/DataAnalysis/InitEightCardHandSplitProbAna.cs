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

            var frontHandStats = new Dictionary<EightCardFrontHandQualifiedHandAndRank, long>();
            var backHandStats = new Dictionary<EightCardBackHandQualifiedHandAndRank, long>();

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

                var components = ParseHandName(handName);
                var (front, back) = SplitHand(components);

                if (front.HasValue) 
                    frontHandStats[front.Value] += count;
                else
                    frontHandStats[EightCardFrontHandQualifiedHandAndRank.Nothing] += count;

                if (back.HasValue) 
                    backHandStats[back.Value] += count;
                else
                    backHandStats[EightCardBackHandQualifiedHandAndRank.Nothing] += count;
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

        static (EightCardFrontHandQualifiedHandAndRank?, EightCardBackHandQualifiedHandAndRank?) SplitHand(List<EightCardsCompType> comps)
        {
            // The ground rule is back hand > front hand.
            // Balanced strategy: if you can split components, always split them but follow rules.
            
            if (comps.Count == 0) return (null, EightCardBackHandQualifiedHandAndRank.Nothing);

            // Special case for 4 components (usually 4 pairs)
            if (comps.Count == 4 && comps.All(c => c == EightCardsCompType.Pair))
            {
                // Two pairs in front, two pairs in back.
                // But FrontHand only has 'TwoPairs', and BackHand has 'TwoPairs'.
                return (EightCardFrontHandQualifiedHandAndRank.TwoPairs, EightCardBackHandQualifiedHandAndRank.TwoPairs);
            }

            if (comps.Count == 3)
            {
                // Try splitting: 1 comp in front, 2 in back? Or 1 in front, 1 in back (leaving 1)?
                // The issue description says "if you can split your components, always split them".
                // Usually this means Front hand gets 1 component, Back hand gets 1 or 2.
                
                // Try Front = comps[1], Back = comps[0] + comps[2]? 
                // Wait, components are sorted descending. comps[0] is strongest.
                // To keep Back > Front, we should put comps[0] in Back.
                // To split: maybe Front = comps[1], Back = comps[0]?
                
                var frontComp = comps[1];
                var backComp = comps[0];
                
                var frontRank = MapToFrontRank(frontComp);
                var backRank = MapToBackRank(backComp, comps.Count > 2 ? comps[2] : EightCardsCompType.None);
                
                if (IsBackStronger(frontRank, backRank))
                {
                    return (frontRank, backRank);
                }
                
                // If not stronger, try putting stronger one in back and nothing in front? 
                // But balanced says split if possible.
            }

            if (comps.Count == 2)
            {
                // Split 1 and 1
                var frontComp = comps[1];
                var backComp = comps[0];
                
                var frontRank = MapToFrontRank(frontComp);
                var backRank = MapToBackRank(backComp, EightCardsCompType.None);
                
                if (IsBackStronger(frontRank, backRank))
                {
                    return (frontRank, backRank);
                }
            }

            if (comps.Count == 1)
            {
                // Cannot split. All goes to back.
                return (null, MapToBackRank(comps[0], EightCardsCompType.None));
            }

            // Default fallback
            return (null, EightCardBackHandQualifiedHandAndRank.Nothing);
        }

        static EightCardFrontHandQualifiedHandAndRank? MapToFrontRank(EightCardsCompType comp)
        {
            if (comp == EightCardsCompType.Pair) return EightCardFrontHandQualifiedHandAndRank.Pair;
            if (comp == EightCardsCompType.ThreeOfKind) return EightCardFrontHandQualifiedHandAndRank.ThreeOfKind;
            if (comp == EightCardsCompType.ThreeCardsFlushStraight) return EightCardFrontHandQualifiedHandAndRank.ThreeCardsFlushStraight;
            if (comp == EightCardsCompType.FourCardsFlushStraight) return EightCardFrontHandQualifiedHandAndRank.FourCardsFlushStraight;
            if (comp == EightCardsCompType.FourOfKind) return EightCardFrontHandQualifiedHandAndRank.FourOfKind;
            
            // If it's something like Two Pairs (split from components)
            // But components here are single types.
            return null;
        }

        static EightCardBackHandQualifiedHandAndRank? MapToBackRank(EightCardsCompType comp, EightCardsCompType extra)
        {
            // Combined rank logic
            if (comp == EightCardsCompType.ThreeOfKind && extra == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.FullHouse;
            if (comp == EightCardsCompType.ThreeCardsFlushStraight && extra == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.ThreeCardsFlushStraightAndPair;
            if (comp == EightCardsCompType.Pair && extra == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.TwoPairs;

            // Single rank mapping
            if (comp == EightCardsCompType.Pair) return EightCardBackHandQualifiedHandAndRank.Pair;
            if (comp == EightCardsCompType.ThreeOfKind) return EightCardBackHandQualifiedHandAndRank.ThreeOfKind;
            if (comp == EightCardsCompType.ThreeCardsFlushStraight) return EightCardBackHandQualifiedHandAndRank.ThreeCardsFlushStraight;
            if (comp == EightCardsCompType.FourCardsFlushStraight) return EightCardBackHandQualifiedHandAndRank.FourCardsFlushStraight;
            if (comp == EightCardsCompType.FourOfKind) return EightCardBackHandQualifiedHandAndRank.FourOfKind;
            if (comp == EightCardsCompType.FiveCardsStraight) return EightCardBackHandQualifiedHandAndRank.FiveCardsStraight;
            if (comp == EightCardsCompType.FiveCardsFlush) return EightCardBackHandQualifiedHandAndRank.FiveCardsFlush;
            if (comp == EightCardsCompType.SixCardsStraight) return EightCardBackHandQualifiedHandAndRank.SixCardsStraight;
            if (comp == EightCardsCompType.SixCardsFlush) return EightCardBackHandQualifiedHandAndRank.SixCardsFlush;
            if (comp == EightCardsCompType.SevenCardsStraight) return EightCardBackHandQualifiedHandAndRank.SevenCardsStraight;
            if (comp == EightCardsCompType.SevenCardsFlush) return EightCardBackHandQualifiedHandAndRank.SevenCardsFlush;
            if (comp == EightCardsCompType.EightCardsStraight) return EightCardBackHandQualifiedHandAndRank.EightCardsStraight;
            if (comp == EightCardsCompType.EightCardsFlush) return EightCardBackHandQualifiedHandAndRank.EightCardsFlush;
            if (comp == EightCardsCompType.FiveCardsFlushStraight) return EightCardBackHandQualifiedHandAndRank.FiveCardsFlushStraight;
            if (comp == EightCardsCompType.SixCardsFlushStraight) return EightCardBackHandQualifiedHandAndRank.SixCardsFlushStraight;
            
            return EightCardBackHandQualifiedHandAndRank.Nothing;
        }

        static bool IsBackStronger(EightCardFrontHandQualifiedHandAndRank? front, EightCardBackHandQualifiedHandAndRank? back)
        {
            if (!front.HasValue) return true;
            if (!back.HasValue) return false;

            // Use the Power Dict from EightCardSubBattleHand if possible, or a simplified one here.
            // Since we can't easily access the internal Dict without instantiation or making it public.
            // Let's use a simplified heuristic based on the enum order (assuming higher value = stronger)
            // or explicit values from EightCardSubBattleHand.
            
            int frontPower = GetFrontPower(front.Value);
            int backPower = GetBackPower(back.Value);
            
            return backPower >= frontPower; 
        }

        static int GetFrontPower(EightCardFrontHandQualifiedHandAndRank rank)
        {
            switch(rank) {
                case EightCardFrontHandQualifiedHandAndRank.Pair: return 1;
                case EightCardFrontHandQualifiedHandAndRank.TwoPairs: return 2;
                case EightCardFrontHandQualifiedHandAndRank.ThreeOfKind: return 15;
                case EightCardFrontHandQualifiedHandAndRank.ThreeCardsFlushStraight: return 24;
                case EightCardFrontHandQualifiedHandAndRank.FourOfKind: return 32;
                case EightCardFrontHandQualifiedHandAndRank.FourCardsFlushStraight: return 40;
                default: return 0;
            }
        }

        static int GetBackPower(EightCardBackHandQualifiedHandAndRank rank)
        {
            switch(rank) {
                case EightCardBackHandQualifiedHandAndRank.Nothing: return 0;
                case EightCardBackHandQualifiedHandAndRank.Pair: return 1;
                case EightCardBackHandQualifiedHandAndRank.TwoPairs: return 2;
                case EightCardBackHandQualifiedHandAndRank.ThreeOfKind: return 10;
                case EightCardBackHandQualifiedHandAndRank.FullHouse: return 28;
                case EightCardBackHandQualifiedHandAndRank.ThreeCardsFlushStraight: return 32;
                case EightCardBackHandQualifiedHandAndRank.FiveCardsStraight: return 24;
                case EightCardBackHandQualifiedHandAndRank.FiveCardsFlush: return 40;
                case EightCardBackHandQualifiedHandAndRank.ThreeCardsFlushStraightAndPair: return 48; // Mansion-like
                case EightCardBackHandQualifiedHandAndRank.SixCardsStraight: return 62;
                case EightCardBackHandQualifiedHandAndRank.FourOfKind: return 80;
                case EightCardBackHandQualifiedHandAndRank.FourCardsFlushStraight: return 100;
                case EightCardBackHandQualifiedHandAndRank.SixCardsFlush: return 120;
                case EightCardBackHandQualifiedHandAndRank.SevenCardsStraight: return 200;
                case EightCardBackHandQualifiedHandAndRank.FiveCardsFlushStraight: return 360;
                case EightCardBackHandQualifiedHandAndRank.EightCardsStraight: return 500;
                case EightCardBackHandQualifiedHandAndRank.SevenCardsFlush: return 800;
                case EightCardBackHandQualifiedHandAndRank.SixCardsFlushStraight: return 1000;
                case EightCardBackHandQualifiedHandAndRank.EightCardsFlush: return 2000;
                default: return 0;
            }
        }

        static void SaveStats(string path, Dictionary<EightCardFrontHandQualifiedHandAndRank, long> front, Dictionary<EightCardBackHandQualifiedHandAndRank, long> back)
        {
            long totalFront = front.Values.Sum();
            long totalBack = back.Values.Sum();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Hand Position,Rank,Count,Probablities,Win/NoLose probablity");
                
                // Front Hand
                var sortedFront = front.OrderByDescending(e => GetFrontPower(e.Key)).ToList();
                double cumulativeFront = 0;
                var frontLines = new List<string>();
                
                // Start from bottom (Nothing) to accumulate
                for (int i = sortedFront.Count - 1; i >= 0; i--)
                {
                    var entry = sortedFront[i];
                    double prob = totalFront > 0 ? (double)entry.Value / totalFront : 0;
                    cumulativeFront += prob;
                    frontLines.Add($"Front,{entry.Key},{entry.Value},{prob:P4},{cumulativeFront:P4}");
                }
                
                // Reverse to have strongest at top
                frontLines.Reverse();
                foreach (var line in frontLines) writer.WriteLine(line);

                // Back Hand
                var sortedBack = back.OrderByDescending(e => GetBackPower(e.Key)).ToList();
                double cumulativeBack = 0;
                var backLines = new List<string>();

                for (int i = sortedBack.Count - 1; i >= 0; i--)
                {
                    var entry = sortedBack[i];
                    double prob = totalBack > 0 ? (double)entry.Value / totalBack : 0;
                    cumulativeBack += prob;
                    backLines.Add($"Back,{entry.Key},{entry.Value},{prob:P4},{cumulativeBack:P4}");
                }

                backLines.Reverse();
                foreach (var line in backLines) writer.WriteLine(line);
            }
        }
    }
}
