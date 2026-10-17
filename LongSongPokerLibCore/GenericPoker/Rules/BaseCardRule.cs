using System;
using System.Collections.Generic;
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