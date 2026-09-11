using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.CardSimStatAnalysis
{
    public class PokerComponents : IComparable<PokerComponents>, IEquatable<PokerComponents>
    {
        public SimCardsCompType CompType { get; set; }
        public int CardCount { get; set; }
        public int Power => GetCompPower(CompType);

        public PokerComponents()
        {
            CompType = SimCardsCompType.Nothing;
            CardCount = 0;
        }

        public PokerComponents(SimCardsCompType compType)
        {
            CompType = compType;
            CardCount = GetDefaultCardCount(compType);
        }

        public PokerComponents(SimCardsCompType compType, int cardCount)
        {
            CompType = compType;
            CardCount = cardCount;
        }

        public static int GetDefaultCardCount(SimCardsCompType comp)
        {
            return comp switch
            {
                SimCardsCompType.Pair => 2,
                SimCardsCompType.ThreeOfKind => 3,
                SimCardsCompType.FourOfKind => 4,
                SimCardsCompType.FiveOfKind => 5,
                SimCardsCompType.SixOfKind => 6,
                SimCardsCompType.SevenOfKind => 7,
                SimCardsCompType.EightOfKind => 8,
                SimCardsCompType.NineOfKind => 9,
                SimCardsCompType.TenOfKind => 10,

                SimCardsCompType.ThreeCardsFlush => 3,
                SimCardsCompType.FourCardsFlush => 4,
                SimCardsCompType.FiveCardsFlush => 5,
                SimCardsCompType.SixCardsFlush => 6,
                SimCardsCompType.SevenCardsFlush => 7,
                SimCardsCompType.EightCardsFlush => 8,
                SimCardsCompType.NineCardsFlush => 9,
                SimCardsCompType.TenCardsFlush => 10,

                SimCardsCompType.ThreeCardsStraight => 3,
                SimCardsCompType.FourCardStraight => 4,
                SimCardsCompType.FiveCardsStraight => 5,
                SimCardsCompType.SixCardsStraight => 6,
                SimCardsCompType.SevenCardsStraight => 7,
                SimCardsCompType.EightCardsStraight => 8,
                SimCardsCompType.NineCardsStraight => 9,
                SimCardsCompType.TenCardsStraight => 10,

                SimCardsCompType.ThreeCardsFlushStraight => 3,
                SimCardsCompType.FourCardsFlushStraight => 4,
                SimCardsCompType.FiveCardsFlushStraight => 5,
                SimCardsCompType.SixCardsFlushStraight => 6,
                SimCardsCompType.SevenCardsFlushStraight => 7,
                SimCardsCompType.EightCardsFlushStraight => 8,
                SimCardsCompType.NineCardsFlushStraight => 9,
                SimCardsCompType.TenCardsFlushStraight => 10,

                _ => 0
            };
        }

        public static int GetCompPower(SimCardsCompType comp)
        {
            return (int)comp;
        }

        public enum ComponentCategory
        {
            Kind,
            FlushStraight,
            Flush,
            Straight,
            Other
        }

        public static ComponentCategory GetComponentCategory(SimCardsCompType type)
        {
            return type switch
            {
                SimCardsCompType.Pair or
                SimCardsCompType.ThreeOfKind or
                SimCardsCompType.FourOfKind or
                SimCardsCompType.FiveOfKind or
                SimCardsCompType.SixOfKind or
                SimCardsCompType.SevenOfKind or
                SimCardsCompType.EightOfKind or
                SimCardsCompType.NineOfKind or
                SimCardsCompType.TenOfKind => ComponentCategory.Kind,

                SimCardsCompType.ThreeCardsFlushStraight or
                SimCardsCompType.FourCardsFlushStraight or
                SimCardsCompType.FiveCardsFlushStraight or
                SimCardsCompType.SixCardsFlushStraight or
                SimCardsCompType.SevenCardsFlushStraight or
                SimCardsCompType.EightCardsFlushStraight or
                SimCardsCompType.NineCardsFlushStraight or
                SimCardsCompType.TenCardsFlushStraight => ComponentCategory.FlushStraight,

                SimCardsCompType.ThreeCardsFlush or
                SimCardsCompType.FourCardsFlush or
                SimCardsCompType.FiveCardsFlush or
                SimCardsCompType.SixCardsFlush or
                SimCardsCompType.SevenCardsFlush or
                SimCardsCompType.EightCardsFlush or
                SimCardsCompType.NineCardsFlush or
                SimCardsCompType.TenCardsFlush => ComponentCategory.Flush,

                SimCardsCompType.ThreeCardsStraight or
                SimCardsCompType.FourCardStraight or
                SimCardsCompType.FiveCardsStraight or
                SimCardsCompType.SixCardsStraight or
                SimCardsCompType.SevenCardsStraight or
                SimCardsCompType.EightCardsStraight or
                SimCardsCompType.NineCardsStraight or
                SimCardsCompType.TenCardsStraight => ComponentCategory.Straight,

                _ => ComponentCategory.Other
            };
        }

        public static SimCardsCompType GetComponentType(ComponentCategory category, int count)
        {
            return category switch
            {
                ComponentCategory.Kind => count switch
                {
                    2 => SimCardsCompType.Pair,
                    3 => SimCardsCompType.ThreeOfKind,
                    4 => SimCardsCompType.FourOfKind,
                    5 => SimCardsCompType.FiveOfKind,
                    6 => SimCardsCompType.SixOfKind,
                    7 => SimCardsCompType.SevenOfKind,
                    8 => SimCardsCompType.EightOfKind,
                    9 => SimCardsCompType.NineOfKind,
                    10 => SimCardsCompType.TenOfKind,
                    _ => SimCardsCompType.Nothing
                },
                ComponentCategory.FlushStraight => count switch
                {
                    3 => SimCardsCompType.ThreeCardsFlushStraight,
                    4 => SimCardsCompType.FourCardsFlushStraight,
                    5 => SimCardsCompType.FiveCardsFlushStraight,
                    6 => SimCardsCompType.SixCardsFlushStraight,
                    7 => SimCardsCompType.SevenCardsFlushStraight,
                    8 => SimCardsCompType.EightCardsFlushStraight,
                    9 => SimCardsCompType.NineCardsFlushStraight,
                    10 => SimCardsCompType.TenCardsFlushStraight,
                    _ => SimCardsCompType.Nothing
                },
                ComponentCategory.Flush => count switch
                {
                    3 => SimCardsCompType.ThreeCardsFlush,
                    4 => SimCardsCompType.FourCardsFlush,
                    5 => SimCardsCompType.FiveCardsFlush,
                    6 => SimCardsCompType.SixCardsFlush,
                    7 => SimCardsCompType.SevenCardsFlush,
                    8 => SimCardsCompType.EightCardsFlush,
                    9 => SimCardsCompType.NineCardsFlush,
                    10 => SimCardsCompType.TenCardsFlush,
                    _ => SimCardsCompType.Nothing
                },
                ComponentCategory.Straight => count switch
                {
                    3 => SimCardsCompType.ThreeCardsStraight,
                    4 => SimCardsCompType.FourCardStraight,
                    5 => SimCardsCompType.FiveCardsStraight,
                    6 => SimCardsCompType.SixCardsStraight,
                    7 => SimCardsCompType.SevenCardsStraight,
                    8 => SimCardsCompType.EightCardsStraight,
                    9 => SimCardsCompType.NineCardsStraight,
                    10 => SimCardsCompType.TenCardsStraight,
                    _ => SimCardsCompType.Nothing
                },
                _ => SimCardsCompType.Nothing
            };
        }

        /// <summary>
        /// Breaks this component down into all valid candidate sets of smaller atomic components using integer partition mathematics.
        /// </summary>
        public List<List<PokerComponents>> BreakDown(
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            var category = GetComponentCategory(CompType);
            int totalCards = CardCount > 0 ? CardCount : GetDefaultCardCount(CompType);

            if (category == ComponentCategory.Other || totalCards <= 0)
            {
                return new List<List<PokerComponents>> { new() { new(CompType, totalCards) } };
            }

            int minCards = category switch
            {
                ComponentCategory.FlushStraight => minFlushStraightCards > 0 ? minFlushStraightCards : SimPokerHandCalculator._minFlushStraightCards,
                ComponentCategory.Flush => minFlushCards > 0 ? minFlushCards : SimPokerHandCalculator._minFlushCards,
                ComponentCategory.Straight => minStraightCards > 0 ? minStraightCards : SimPokerHandCalculator._minStraightCards,
                ComponentCategory.Kind => minKindCards > 0 ? minKindCards : SimPokerHandCalculator._minKindCards,
                _ => 1
            };

            var partitions = new List<List<int>>();
            GeneratePartitions(totalCards, totalCards, new List<int>(), partitions, minCards);

            var result = new List<List<PokerComponents>>();
            var seen = new HashSet<string>();

            foreach (var partition in partitions)
            {
                var validParts = partition.Where(p => p >= minCards).ToList();
                if (validParts.Count == 0)
                {
                    continue;
                }

                string key = string.Join(",", validParts);
                if (seen.Add(key))
                {
                    var breakdownOption = validParts.Select(p => new PokerComponents(GetComponentType(category, p), p)).ToList();
                    result.Add(breakdownOption);
                }
            }

            if (result.Count == 0)
            {
                result.Add(new List<PokerComponents> { new(CompType, totalCards) });
            }

            return result;
        }

        private static void GeneratePartitions(int remaining, int maxVal, List<int> current, List<List<int>> partitions, int minCards = 1)
        {
            if (remaining < minCards)
            {
                partitions.Add(new List<int>(current));
                return;
            }

            for (int i = Math.Min(remaining, maxVal); i >= minCards; i--)
            {
                current.Add(i);
                GeneratePartitions(remaining - i, i, current, partitions, minCards);
                current.RemoveAt(current.Count - 1);
            }
        }

        /// <summary>
        /// Computes the Cartesian Product of alternative candidate breakdowns across all component groups.
        /// </summary>
        public static List<List<List<PokerComponents>>> CartesianProduct(List<List<List<PokerComponents>>> sequences)
        {
            var result = new List<List<List<PokerComponents>>> { new() };

            foreach (var sequence in sequences)
            {
                var temp = new List<List<List<PokerComponents>>>();
                foreach (var existing in result)
                {
                    foreach (var item in sequence)
                    {
                        var copy = new List<List<PokerComponents>>(existing) { item };
                        temp.Add(copy);
                    }
                }
                result = temp;
            }

            return result;
        }

        public int CompareTo(PokerComponents? other)
        {
            if (other is null) return 1;
            return Power.CompareTo(other.Power);
        }

        public bool Equals(PokerComponents? other)
        {
            if (other is null) return false;
            return CompType == other.CompType && CardCount == other.CardCount;
        }

        public override bool Equals(object? obj)
        {
            return obj is PokerComponents other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CompType, CardCount);
        }

        public override string ToString()
        {
            return CompType.ToString();
        }
    }
}
