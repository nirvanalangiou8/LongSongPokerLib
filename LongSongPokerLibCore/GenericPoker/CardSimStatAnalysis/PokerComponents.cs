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
            return comp switch
            {
                SimCardsCompType.Pair => 1,
                SimCardsCompType.ThreeCardsStraight => 2,
                SimCardsCompType.ThreeCardsFlush => 3,
                SimCardsCompType.FourCardStraight => 4,
                SimCardsCompType.FourCardsFlush => 5,
                SimCardsCompType.ThreeOfKind => 10,
                SimCardsCompType.ThreeCardsFlushStraight => 15,
                SimCardsCompType.FiveCardsStraight => 18,
                SimCardsCompType.FiveCardsFlush => 20,
                SimCardsCompType.FourCardsFlushStraight => 25,
                SimCardsCompType.FourOfKind => 30,
                SimCardsCompType.SixCardsStraight => 32,
                SimCardsCompType.SixCardsFlush => 34,
                SimCardsCompType.FiveCardsFlushStraight => 35,
                SimCardsCompType.SevenCardsStraight => 36,
                SimCardsCompType.SevenCardsFlush => 38,
                SimCardsCompType.SixCardsFlushStraight => 40,
                SimCardsCompType.EightCardsStraight => 42,
                SimCardsCompType.EightCardsFlush => 44,
                SimCardsCompType.SevenCardsFlushStraight => 46,
                SimCardsCompType.NineCardsStraight => 48,
                SimCardsCompType.NineCardsFlush => 50,
                SimCardsCompType.EightCardsFlushStraight => 52,
                SimCardsCompType.NineCardsFlushStraight => 55,
                _ => (int)comp
            };
        }

        /// <summary>
        /// Breaks this component down into all valid candidate sets of smaller atomic components.
        /// </summary>
        public List<List<PokerComponents>> BreakDown()
        {
            var result = new List<List<PokerComponents>>();

            switch (CompType)
            {
                // Flushes
                case SimCardsCompType.NineCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.NineCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush), new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush), new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush), new(SimCardsCompType.ThreeCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush) });
                    break;

                case SimCardsCompType.EightCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush), new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush) });
                    break;

                case SimCardsCompType.SevenCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush) });
                    break;

                case SimCardsCompType.SixCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush), new(SimCardsCompType.ThreeCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush) });
                    break;

                case SimCardsCompType.FiveCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush) });
                    break;

                case SimCardsCompType.FourCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlush) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush) });
                    break;

                case SimCardsCompType.ThreeCardsFlush:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlush) });
                    break;

                // Flush Straights
                case SimCardsCompType.NineCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.NineCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight), new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight), new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight) });
                    break;

                case SimCardsCompType.EightCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight), new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight) });
                    break;

                case SimCardsCompType.SevenCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight) });
                    break;

                case SimCardsCompType.SixCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight), new(SimCardsCompType.ThreeCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight) });
                    break;

                case SimCardsCompType.FiveCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight) });
                    break;

                case SimCardsCompType.FourCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardsFlushStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight) });
                    break;

                case SimCardsCompType.ThreeCardsFlushStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsFlushStraight) });
                    break;

                // Straights
                case SimCardsCompType.NineCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.NineCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight), new(SimCardsCompType.FourCardStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsStraight), new(SimCardsCompType.ThreeCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight), new(SimCardsCompType.ThreeCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight) });
                    break;

                case SimCardsCompType.EightCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.EightCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight), new(SimCardsCompType.ThreeCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardStraight), new(SimCardsCompType.FourCardStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight) });
                    break;

                case SimCardsCompType.SevenCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SevenCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardStraight), new(SimCardsCompType.ThreeCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight) });
                    break;

                case SimCardsCompType.SixCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.SixCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsStraight), new(SimCardsCompType.ThreeCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardStraight) });
                    break;

                case SimCardsCompType.FiveCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FiveCardsStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsStraight) });
                    break;

                case SimCardsCompType.FourCardStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourCardStraight) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsStraight) });
                    break;

                case SimCardsCompType.ThreeCardsStraight:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeCardsStraight) });
                    break;

                // Sets / Multiples
                case SimCardsCompType.FourOfKind:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.FourOfKind) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.Pair), new(SimCardsCompType.Pair) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeOfKind) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.Pair) });
                    break;

                case SimCardsCompType.ThreeOfKind:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.ThreeOfKind) });
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.Pair) });
                    break;

                case SimCardsCompType.Pair:
                    result.Add(new List<PokerComponents> { new(SimCardsCompType.Pair) });
                    break;

                default:
                    result.Add(new List<PokerComponents> { new(CompType, CardCount) });
                    break;
            }

            return result;
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
