using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    public abstract class BaseCardRule : ICardRule
    {

        public static ICardRule Default { get; } = EightCardRule.Default;

        private List<SubHandSlotConfig> _subHandSlots = new()
        {
            new SubHandSlotConfig(BattleSubHandOrder.FirstHand, 3, "Front"),
            new SubHandSlotConfig(BattleSubHandOrder.SecondHand, 5, "Back")
        };

        public virtual IReadOnlyList<SubHandSlotConfig> SubHandSlots
        {
            get => _subHandSlots;
            set
            {
                _subHandSlots = value?.ToList() ?? new List<SubHandSlotConfig>();
                _subHandsCardCount = _subHandSlots.ToDictionary(s => s.Order, s => s.CardCount);
            }
        }

        private Dictionary<BattleSubHandOrder, int> _subHandsCardCount = new()
        {
            { BattleSubHandOrder.FirstHand, 3 },
            { BattleSubHandOrder.SecondHand, 5 }
        };

        public virtual Dictionary<BattleSubHandOrder, int> SubHandsCardCount
        {
            get => _subHandsCardCount;
            set
            {
                _subHandsCardCount = value ?? new Dictionary<BattleSubHandOrder, int>();
                _subHandSlots = _subHandsCardCount.Select(kvp => new SubHandSlotConfig(kvp.Key, kvp.Value)).ToList();
            }
        }
        
        public virtual int CardCount
        {
            get => SubHandsCardCount.Values.Sum();
        }

        public virtual int TotalCardCount => CardCount;
        
        public int MinStraightCount { get;  set; } = 5;
        public int MinFlushCount { get;  set; } = 5;
        public int MinFlushStraightCount { get;  set; } = 3;
        public int MinKindCount { get;  set; } = 2;
        
        public string WinRateTableFile { get; set; } = string.Empty;
        public List<Dictionary<PokerOverAllHandRank, (double min, double max)>> WinRateCdfBands { get; set; } = new();
        public List<List<(PokerOverAllHandRank rank, long count)>> WinRateStatCountLadders { get; set; } = new();

        public virtual void LoadWinRateTable(string? fileName = null)
        {
            string? targetFile = fileName ?? WinRateTableFile;
            if (string.IsNullOrWhiteSpace(targetFile)) return;

            string? resolvedPath = ResolveWinRateTablePath(targetFile);
            if (string.IsNullOrEmpty(resolvedPath) || !File.Exists(resolvedPath))
            {
                return;
            }

            var laddersByOrder = new Dictionary<int, List<(PokerOverAllHandRank rank, long count)>>();

            var lines = File.ReadAllLines(resolvedPath);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("Hand Position", StringComparison.OrdinalIgnoreCase))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                string posStr = parts[0].Trim();
                string rankStr = parts[1].Trim();
                string countStr = parts[2].Trim();

                int orderIdx;
                if (Enum.TryParse<BattleSubHandOrder>(posStr, true, out var order))
                {
                    orderIdx = (int)order;
                }
                else if (int.TryParse(posStr, out int parsedIdx))
                {
                    orderIdx = parsedIdx;
                }
                else if (posStr.Equals("Front", StringComparison.OrdinalIgnoreCase))
                {
                    orderIdx = (int)BattleSubHandOrder.FirstHand;
                }
                else if (posStr.Equals("Middle", StringComparison.OrdinalIgnoreCase) || posStr.Equals("Mid", StringComparison.OrdinalIgnoreCase))
                {
                    orderIdx = (int)BattleSubHandOrder.SecondHand;
                }
                else if (posStr.Equals("Back", StringComparison.OrdinalIgnoreCase))
                {
                    orderIdx = SubHandSlots.Count > 2 ? (int)BattleSubHandOrder.ThirdHand : (int)BattleSubHandOrder.SecondHand;
                }
                else
                {
                    continue;
                }

                if (!Enum.TryParse<PokerOverAllHandRank>(rankStr, true, out var rank) || rank == PokerOverAllHandRank.None)
                    continue;

                if (!long.TryParse(countStr, out long count))
                {
                    if (double.TryParse(countStr, out double dCount))
                        count = (long)Math.Round(dCount);
                    else
                        continue;
                }

                if (!laddersByOrder.TryGetValue(orderIdx, out var list))
                {
                    list = new List<(PokerOverAllHandRank rank, long count)>();
                    laddersByOrder[orderIdx] = list;
                }
                list.Add((rank, count));
            }

            int maxOrder = laddersByOrder.Keys.Count > 0 ? laddersByOrder.Keys.Max() : -1;
            var newLadders = new List<List<(PokerOverAllHandRank rank, long count)>>();
            var newBands = new List<Dictionary<PokerOverAllHandRank, (double min, double max)>>();
            for (int i = 0; i <= maxOrder; i++)
            {
                if (laddersByOrder.TryGetValue(i, out var ladder))
                {
                    ladder.Sort((a, b) => ((int)a.rank).CompareTo((int)b.rank));
                    newLadders.Add(ladder);
                    newBands.Add(BuildCdfBands(ladder));
                }
                else
                {
                    newLadders.Add(new List<(PokerOverAllHandRank rank, long count)>());
                    newBands.Add(new Dictionary<PokerOverAllHandRank, (double min, double max)>());
                }
            }

            WinRateStatCountLadders = newLadders;
            WinRateCdfBands = newBands;
        }

        private static Dictionary<PokerOverAllHandRank, (double min, double max)> BuildCdfBands(
            IEnumerable<(PokerOverAllHandRank rank, long count)> ladder)
        {
            double total = 0;
            foreach (var entry in ladder) total += entry.count;
            if (total <= 0) total = 1;

            var dict = new Dictionary<PokerOverAllHandRank, (double min, double max)>();
            double cum = 0;
            foreach (var entry in ladder)
            {
                double min = cum / total;
                cum += entry.count;
                double max = cum / total;
                if (dict.TryGetValue(entry.rank, out var old))
                    dict[entry.rank] = (Math.Min(old.min, min), Math.Max(old.max, max));
                else
                    dict[entry.rank] = (min, max);
            }
            return dict;
        }

        public static string? ResolveWinRateTablePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;

            string cleanName = filePath.Trim();
            if (cleanName.StartsWith("~"))
            {
                cleanName = cleanName.TrimStart('~', '/', '\\');
            }

            string fileName = Path.GetFileName(cleanName);
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            // Try direct file existence
            if (File.Exists(filePath)) return Path.GetFullPath(filePath);
            if (File.Exists(cleanName)) return Path.GetFullPath(cleanName);

            string relativeDataPath = Path.Combine("GenericPoker", "CardSimStatAnalysis", "Data", fileName);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidate = Path.GetFullPath(Path.Combine(baseDir, relativeDataPath));
            if (File.Exists(candidate)) return candidate;

            candidate = Path.GetFullPath(Path.Combine(baseDir, fileName));
            if (File.Exists(candidate)) return candidate;

            string currentWorkingDir = Directory.GetCurrentDirectory();
            candidate = Path.GetFullPath(Path.Combine(currentWorkingDir, relativeDataPath));
            if (File.Exists(candidate)) return candidate;

            candidate = Path.GetFullPath(Path.Combine(currentWorkingDir, fileName));
            if (File.Exists(candidate)) return candidate;

            var dirsToSearch = new[] { new DirectoryInfo(baseDir), new DirectoryInfo(currentWorkingDir) };
            foreach (var root in dirsToSearch)
            {
                DirectoryInfo? dir = root;
                while (dir != null)
                {
                    candidate = Path.GetFullPath(Path.Combine(dir.FullName, "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                    if (File.Exists(candidate)) return candidate;

                    candidate = Path.GetFullPath(Path.Combine(dir.FullName, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data", fileName));
                    if (File.Exists(candidate)) return candidate;

                    candidate = Path.GetFullPath(Path.Combine(dir.FullName, fileName));
                    if (File.Exists(candidate)) return candidate;

                    dir = dir.Parent;
                }
            }

            return null;
        }
         

        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<BaseCompType>? components, BattleSubHandOrder? subHandOrder = null)
        {
            if (components == null) return PokerOverAllHandRank.Nothing;
            var list = components
                .Where(c => c != BaseCompType.Nothing && c != BaseCompType.None)
                .ToList();
            if (list.Count == 0) return PokerOverAllHandRank.Nothing;

            // Sort high power to low power
            list.Sort((a, b) => ((int)b).CompareTo((int)a));

            

            switch (list.Count)
            {
                case 1 when Enum.TryParse<PokerOverAllHandRank>(list[0].ToString(), out var directRank):
                    return directRank;
                case 1:
                    return PokerOverAllHandRank.None;
                case 2:
                {
                    var c1 = list[0];
                    var c2 = list[1];

                    if (c1 == BaseCompType.ThreeOfKind && c2 == BaseCompType.Pair) return PokerOverAllHandRank.FullHouse;
                    if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.Pair) return PokerOverAllHandRank.Mansion;
                    if (c1 == BaseCompType.Pair && c2 == BaseCompType.Pair) return PokerOverAllHandRank.TwoPairs;
                    if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.ThreeCardsFlushStraight) return PokerOverAllHandRank.ThreeCardsFlushStraight;

                    return PokerOverAllHandRank.None;
                }
                default:
                    return PokerOverAllHandRank.None;
            }
        }

        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components, BattleSubHandOrder? subHandOrder = null) => AssembleHandRank(components?.Select(c => c.CompType), subHandOrder);
        public virtual PokerOverAllHandRank AssembleHandRank(IEnumerable<PokerCardComponent>? components, BattleSubHandOrder? subHandOrder = null) => AssembleHandRank(components?.Select(c => c.CompRank), subHandOrder);


    }
}


/*
            if (subHandOrder.HasValue && SubHandsCardCount.TryGetValue(subHandOrder.Value, out int slotCardCount))
            {
                if (slotCardCount == 1)
                {
                    // 1-card slot cannot hold any multi-card components
                    return PokerOverAllHandRank.None;
                }

                if (slotCardCount == 3)
                {
                    if (list.Count == 1)
                    {
                        var c = list[0];
                        if (c == BaseCompType.Pair || c == BaseCompType.ThreeOfKind || c == BaseCompType.ThreeCardsFlushStraight)
                        {
                            if (Enum.TryParse<PokerOverAllHandRank>(c.ToString(), out var directRank))
                                return directRank;
                        }
                        return PokerOverAllHandRank.None;
                    }

                    if (list.Count == 2)
                    {
                        if (list[0] == BaseCompType.Pair && list[1] == BaseCompType.Pair)
                            return PokerOverAllHandRank.TwoPairs;
                        return PokerOverAllHandRank.None;
                    }

                    return PokerOverAllHandRank.None;
                }

                if (slotCardCount == 4)
                {
                    if (list.Count == 1)
                    {
                        var c = list[0];
                        if (c == BaseCompType.Pair || c == BaseCompType.ThreeOfKind || c == BaseCompType.ThreeCardsFlushStraight ||
                            c == BaseCompType.FourCardsFlushStraight || c == BaseCompType.FourOfKind)
                        {
                            if (Enum.TryParse<PokerOverAllHandRank>(c.ToString(), out var directRank))
                                return directRank;
                        }
                        return PokerOverAllHandRank.None;
                    }

                    if (list.Count == 2)
                    {
                        if (list[0] == BaseCompType.Pair && list[1] == BaseCompType.Pair)
                            return PokerOverAllHandRank.TwoPairs;
                        return PokerOverAllHandRank.None;
                    }

                    return PokerOverAllHandRank.None;
                }

                if (slotCardCount == 5)
                {
                    if (list.Count == 1)
                    {
                        var c = list[0];
                        if (c == BaseCompType.SixCardsStraight || c == BaseCompType.SixCardsFlush ||
                            c == BaseCompType.SevenCardsStraight || c == BaseCompType.SevenCardsFlush ||
                            c == BaseCompType.EightCardsStraight || c == BaseCompType.EightCardsFlush ||
                            c == BaseCompType.SixCardsFlushStraight || c == BaseCompType.SevenCardsFlushStraight ||
                            c == BaseCompType.EightCardsFlushStraight)
                        {
                            return PokerOverAllHandRank.None;
                        }

                        if (Enum.TryParse<PokerOverAllHandRank>(c.ToString(), out var directRank))
                            return directRank;
                        return PokerOverAllHandRank.None;
                    }

                    if (list.Count == 2)
                    {
                        var c1 = list[0];
                        var c2 = list[1];

                        if (c1 == BaseCompType.ThreeOfKind && c2 == BaseCompType.Pair) return PokerOverAllHandRank.FullHouse;
                        if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.Pair) return PokerOverAllHandRank.Mansion;
                        if (c1 == BaseCompType.Pair && c2 == BaseCompType.Pair) return PokerOverAllHandRank.TwoPairs;
                        if (c1 == BaseCompType.ThreeCardsFlushStraight && c2 == BaseCompType.ThreeCardsFlushStraight) return PokerOverAllHandRank.ThreeCardsFlushStraight;

                        return PokerOverAllHandRank.None;
                    }

                    return PokerOverAllHandRank.None;
                }
            }*/