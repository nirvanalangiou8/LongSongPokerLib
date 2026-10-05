using System;
using System.Collections.Generic;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class PokerComponents : IComparable<PokerComponents>, IEquatable<PokerComponents>
    {
        public BaseCompType CompType { get; set; }
        public int CardCount { get; set; }
        public int Power => (int)CompType;
        public ICardRule? Rule { get; set; }

        public PokerComponents()
        {
            CompType = BaseCompType.Nothing;
            CardCount = 0;
        }

        public PokerComponents(BaseCompType compType, ICardRule? rule = null)
        {
            CompType = compType;
            CardCount = GetDefaultCardCount(compType);
            Rule = rule;
        }

        public PokerComponents(BaseCompType compType, int cardCount, ICardRule? rule = null)
        {
            CompType = compType;
            CardCount = cardCount;
            Rule = rule;
        }

        public static int GetDefaultCardCount(BaseCompType comp)
        {
            return comp switch
            {
                BaseCompType.Pair => 2,
                BaseCompType.ThreeOfKind => 3,
                BaseCompType.FourOfKind => 4,
                BaseCompType.FiveOfKind => 5,
                BaseCompType.SixOfKind => 6,
                BaseCompType.SevenOfKind => 7,
                BaseCompType.EightOfKind => 8,
                BaseCompType.NineOfKind => 9,
                BaseCompType.TenOfKind => 10,

                BaseCompType.ThreeCardsFlush => 3,
                BaseCompType.FourCardsFlush => 4,
                BaseCompType.FiveCardsFlush => 5,
                BaseCompType.SixCardsFlush => 6,
                BaseCompType.SevenCardsFlush => 7,
                BaseCompType.EightCardsFlush => 8,
                BaseCompType.NineCardsFlush => 9,
                BaseCompType.TenCardsFlush => 10,

                BaseCompType.ThreeCardsStraight => 3,
                BaseCompType.FourCardStraight => 4,
                BaseCompType.FiveCardsStraight => 5,
                BaseCompType.SixCardsStraight => 6,
                BaseCompType.SevenCardsStraight => 7,
                BaseCompType.EightCardsStraight => 8,
                BaseCompType.NineCardsStraight => 9,
                BaseCompType.TenCardsStraight => 10,

                BaseCompType.ThreeCardsFlushStraight => 3,
                BaseCompType.FourCardsFlushStraight => 4,
                BaseCompType.FiveCardsFlushStraight => 5,
                BaseCompType.SixCardsFlushStraight => 6,
                BaseCompType.SevenCardsFlushStraight => 7,
                BaseCompType.EightCardsFlushStraight => 8,
                BaseCompType.NineCardsFlushStraight => 9,
                BaseCompType.TenCardsFlushStraight => 10,

                _ => 0
            };
        }

        public static int GetCompPower(BaseCompType comp)
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

        public static ComponentCategory GetComponentCategory(BaseCompType type)
        {
            return type switch
            {
                BaseCompType.Pair or
                BaseCompType.ThreeOfKind or
                BaseCompType.FourOfKind or
                BaseCompType.FiveOfKind or
                BaseCompType.SixOfKind or
                BaseCompType.SevenOfKind or
                BaseCompType.EightOfKind or
                BaseCompType.NineOfKind or
                BaseCompType.TenOfKind => ComponentCategory.Kind,

                BaseCompType.ThreeCardsFlushStraight or
                BaseCompType.FourCardsFlushStraight or
                BaseCompType.FiveCardsFlushStraight or
                BaseCompType.SixCardsFlushStraight or
                BaseCompType.SevenCardsFlushStraight or
                BaseCompType.EightCardsFlushStraight or
                BaseCompType.NineCardsFlushStraight or
                BaseCompType.TenCardsFlushStraight => ComponentCategory.FlushStraight,

                BaseCompType.ThreeCardsFlush or
                BaseCompType.FourCardsFlush or
                BaseCompType.FiveCardsFlush or
                BaseCompType.SixCardsFlush or
                BaseCompType.SevenCardsFlush or
                BaseCompType.EightCardsFlush or
                BaseCompType.NineCardsFlush or
                BaseCompType.TenCardsFlush => ComponentCategory.Flush,

                BaseCompType.ThreeCardsStraight or
                BaseCompType.FourCardStraight or
                BaseCompType.FiveCardsStraight or
                BaseCompType.SixCardsStraight or
                BaseCompType.SevenCardsStraight or
                BaseCompType.EightCardsStraight or
                BaseCompType.NineCardsStraight or
                BaseCompType.TenCardsStraight => ComponentCategory.Straight,

                _ => ComponentCategory.Other
            };
        }

        public static BaseCompType GetComponentType(ComponentCategory category, int count)
        {
            return category switch
            {
                ComponentCategory.Kind => count switch
                {
                    2 => BaseCompType.Pair,
                    3 => BaseCompType.ThreeOfKind,
                    4 => BaseCompType.FourOfKind,
                    5 => BaseCompType.FiveOfKind,
                    6 => BaseCompType.SixOfKind,
                    7 => BaseCompType.SevenOfKind,
                    8 => BaseCompType.EightOfKind,
                    9 => BaseCompType.NineOfKind,
                    10 => BaseCompType.TenOfKind,
                    _ => BaseCompType.Nothing
                },
                ComponentCategory.FlushStraight => count switch
                {
                    3 => BaseCompType.ThreeCardsFlushStraight,
                    4 => BaseCompType.FourCardsFlushStraight,
                    5 => BaseCompType.FiveCardsFlushStraight,
                    6 => BaseCompType.SixCardsFlushStraight,
                    7 => BaseCompType.SevenCardsFlushStraight,
                    8 => BaseCompType.EightCardsFlushStraight,
                    9 => BaseCompType.NineCardsFlushStraight,
                    10 => BaseCompType.TenCardsFlushStraight,
                    _ => BaseCompType.Nothing
                },
                ComponentCategory.Flush => count switch
                {
                    3 => BaseCompType.ThreeCardsFlush,
                    4 => BaseCompType.FourCardsFlush,
                    5 => BaseCompType.FiveCardsFlush,
                    6 => BaseCompType.SixCardsFlush,
                    7 => BaseCompType.SevenCardsFlush,
                    8 => BaseCompType.EightCardsFlush,
                    9 => BaseCompType.NineCardsFlush,
                    10 => BaseCompType.TenCardsFlush,
                    _ => BaseCompType.Nothing
                },
                ComponentCategory.Straight => count switch
                {
                    3 => BaseCompType.ThreeCardsStraight,
                    4 => BaseCompType.FourCardStraight,
                    5 => BaseCompType.FiveCardsStraight,
                    6 => BaseCompType.SixCardsStraight,
                    7 => BaseCompType.SevenCardsStraight,
                    8 => BaseCompType.EightCardsStraight,
                    9 => BaseCompType.NineCardsStraight,
                    10 => BaseCompType.TenCardsStraight,
                    _ => BaseCompType.Nothing
                },
                _ => BaseCompType.Nothing
            };
        }

        /// <summary>
        /// Breaks this component down into all valid candidate sets of smaller atomic components using integer partition mathematics.
        /// </summary>
        /*
        public List<List<PokerComponents>> BreakDown(
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            return BreakDown(null, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards);
        }*/

        /// <summary>
        /// Breaks this component down into all valid candidate sets of smaller atomic components using integer partition mathematics.
        /// </summary>
        public List<List<PokerComponents>> BreakDown(
            ICardRule? rule,
            int minFlushStraightCards = -1,
            int minFlushCards = -1,
            int minStraightCards = -1,
            int minKindCards = -1)
        {
            var category = GetComponentCategory(CompType);
            int totalCards = CardCount > 0 ? CardCount : GetDefaultCardCount(CompType);
            var effectiveRule = rule ?? Rule ?? EightCardRule.Default;

            if (category == ComponentCategory.Other || totalCards <= 0)
            {
                return new List<List<PokerComponents>> { new() { new(CompType, totalCards, effectiveRule) } };
            }

            int minCards = category switch
            {
                ComponentCategory.FlushStraight => minFlushStraightCards > 0 ? minFlushStraightCards : effectiveRule.MinFlushStraightCount,
                ComponentCategory.Flush => minFlushCards > 0 ? minFlushCards : effectiveRule.MinFlushCount,
                ComponentCategory.Straight => minStraightCards > 0 ? minStraightCards : effectiveRule.MinStraightCount,
                ComponentCategory.Kind => minKindCards > 0 ? minKindCards : effectiveRule.MinKindCount,
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
                    var breakdownOption = validParts.Select(p => new PokerComponents(GetComponentType(category, p), p, effectiveRule)).ToList();
                    result.Add(breakdownOption);
                }
            }

            if (result.Count == 0)
            {
                result.Add(new List<PokerComponents> { new(CompType, totalCards, effectiveRule) });
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
