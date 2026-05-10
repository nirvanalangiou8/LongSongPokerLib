using System;
using System.Collections.Generic;
//using EightCardsProbTest;

namespace GenericPoker.FourCard
{
    // This is for Four card battle hands which will contains first and second hands.
    
    public enum SubHandPosition
    {
        LowHand,
        HighHand
    }


     public class FourCardHands : IComparable<FourCardHands>
    {
        private FourCardSubBattleHand _lowHand;
        private FourCardSubBattleHand _highHand;
        protected List<FourCardPokerCard> _cards;
        //public int TotalPower => _lowHand.HandPower + _highHand.HandPower;
        public int TotalPower => (_lowHand.HandPower + _highHand.HandPower)/FourCardSubBattleHand.SquareRankBracketDigits;
        public FourCardSubBattleHand LowHand => _lowHand;
        public FourCardSubBattleHand HighHand => _highHand;
        
        
        public FourCardHands(FourCardSubBattleHand lowHand, FourCardSubBattleHand highHand)
        {
            _lowHand = lowHand;
            _highHand = highHand;
        }

        public FourCardHands(List<FourCardPokerCard> inputCards)
        {
            _cards = new List<FourCardPokerCard>(inputCards);
            ArrangeHand();
            if (_lowHand.IsPair() && _highHand.IsPair())
            {
                Console.WriteLine($"DBG both pair detected. hand power is {TotalPower}");
            }
        }

        public string GetFourCardsStr(string separator = "_")
        {
            return _lowHand.GetHandString(separator) + separator + _highHand.GetHandString(separator);
        }

        
        // This is default arrangement strategy by arrange hand with maximum total hand powers, and also consider the balance which is
        // trying to  find minimum hand power different between first and second hands.
        public void ArrangeHand()
        {
            var bestTotalHandPower = float.MinValue;
            var twoSelectedCardList = UtilFunc.GetPermutation(_cards, 2);
            FourCardSubBattleHand bestLowHand = null;
            FourCardSubBattleHand bestHighHand = null;
            foreach (var twoCards in twoSelectedCardList)
            {
                var lowHand = new FourCardSubBattleHand(twoCards);
                var highHand = new FourCardSubBattleHand(UtilFunc.GetExcludeList(_cards, twoCards));
                if (lowHand.HandPower <= highHand.HandPower)
                {
                    var totalHandPower = lowHand.HandPower + highHand.HandPower;
                    if (totalHandPower > bestTotalHandPower)
                    {
                        bestTotalHandPower = totalHandPower;
                        bestLowHand = lowHand;
                        bestHighHand = highHand;
                    }
                    else if (totalHandPower == bestTotalHandPower)
                    {
                        if ((highHand.HandPower - lowHand.HandPower) < (bestHighHand.HandPower - bestLowHand.HandPower))
                        {
                            bestLowHand = lowHand;
                            bestHighHand = highHand;
                        }
                    }
                }
            }
            _lowHand = bestLowHand;
            _highHand = bestHighHand;
        }

        public int CompareTo(FourCardHands other)
        {
            if (other == null) return 1;
            if (_lowHand == other._lowHand && _highHand == other._highHand) return 0;
            if (TotalPower == other.TotalPower)
            {
                return 0;
            } else {
                return other.TotalPower.CompareTo(other.TotalPower);
            }
            return 0;
        }
        
        public static bool operator >(FourCardHands left, FourCardHands right) => left.CompareTo(right) > 0;
        public static bool operator <(FourCardHands left, FourCardHands right) => left.CompareTo(right) < 0;
        public static bool operator >=(FourCardHands left, FourCardHands right) => left.CompareTo(right) >= 0;
        public static bool operator <=(FourCardHands left, FourCardHands right) => left.CompareTo(right) <= 0;
        public static bool operator ==(FourCardHands left, FourCardHands right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(FourCardHands left, FourCardHands right) => !(left == right);
        
        
        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object obj)
        {
            if (obj is FourCardHands other)
            {
                if (TotalPower != other.TotalPower) return false;
                else return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }

    }
}