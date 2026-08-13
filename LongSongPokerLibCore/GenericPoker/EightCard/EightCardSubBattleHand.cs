using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.EightCard
{
    
    public enum BattleHandEnum
    {
        FirstHand = 3,
        SecondHand = 5,
    }
    
    public enum EightCardsBattleHandRank
    {
        Nothing,
        Pair,
        TwoPairs,
        FourCardStraight,
        FourCardsFlush,
        ThreeCardsPairInFlush,
        ThreeOfKind,
        TownHouse, // ThreeCardsPairInFlush_Pair
        FourCardsPairInFlush,
        FiveCardsStraight,
        FullHouse,
        ThreeCardsFlushStraight,
        FiveCardsFlush,
        Mansion, // ThreeCardsFlushStraight_Pair,
        FiveCardsPairInFlush,
        SixCardsStraight,
        FourOfKind,
        FourCardsFlushStraight,
        SixCardsFlush,
        SixCardsPairInFlush,
        SevenCardsStraight,
        FourCardsTwoPairsInFlush,
        FiveCardsTwoPairsInFlush,
        SixCardsTwoPairsInFlush,
        FiveCardsFlushStraight,
        SevenCardsPairInFlush,
        EightCardsStraight,
        FiveOfKind,
        SevenCardsFlush,
        SevenCardsTwoPairsInFlush,
        SixCardsFlushStraight,
        SixCardsThreePairsInFlush,
        EightCardsPairInFlush,
        SevenCardsThreePairsInFlush,
        EightCardsTwoPairsInFlush,
        SixOfKind,
        EightCardsFlush,
        SevenCardsFlushStraight,
        EightCardsThreePairsInFlush,
        SevenOfKind,
        EightCardsFlushStraight,
        EightCardsFourPairsInFlush,
        EightOfKind
    }
    
    
    public class EightCardSubBattleHand : BaseSubBattleHand <EightCardPokerCard>
    {
        
        public static readonly Dictionary<(BattleHandEnum, EightCardsBattleHandRank), int> EightCardsBattleHandPowerDict =
        new()
        {
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.Nothing ), 0},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.Pair ), 1},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.TwoPairs ), 2},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardStraight ), 4},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsFlush ), 6},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeCardsPairInFlush ), 9},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeOfKind ), 15},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsPairInFlush ), 20},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.ThreeCardsFlushStraight ), 24},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourOfKind ), 32},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsFlushStraight ), 40},
            { (BattleHandEnum.FirstHand, EightCardsBattleHandRank.FourCardsTwoPairsInFlush ), 50},
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Nothing), 0 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Pair), 1 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.TwoPairs), 2 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardStraight), 4 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsFlush), 6 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeCardsPairInFlush), 8 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeOfKind), 10 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.TownHouse), 16 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsPairInFlush), 20 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsStraight), 24 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FullHouse), 28 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.ThreeCardsFlushStraight), 32 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsFlush), 40 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.Mansion), 48 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsPairInFlush), 56 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsStraight), 62 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourOfKind), 80 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsFlushStraight), 100 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsFlush), 120 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsPairInFlush), 140 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsStraight), 200 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FourCardsTwoPairsInFlush), 220 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsTwoPairsInFlush), 240 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsTwoPairsInFlush), 300 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveCardsFlushStraight), 360 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsPairInFlush), 400 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsStraight), 500 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.FiveOfKind), 700 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsFlush), 800 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsTwoPairsInFlush), 900 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsFlushStraight), 1000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixCardsThreePairsInFlush), 1200 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsPairInFlush), 2000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsThreePairsInFlush), 3000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsTwoPairsInFlush), 5000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SixOfKind), 10000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFlush), 20000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenCardsFlushStraight), 40000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsThreePairsInFlush), 80000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.SevenOfKind), 200000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFlushStraight), 400000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightCardsFourPairsInFlush), 2000000 },
            { (BattleHandEnum.SecondHand, EightCardsBattleHandRank.EightOfKind), 10000000 }
        };
        
        private BattleHandEnum _battleHandEnum;
        //private List<PokerCard> _cards;
        private EightCardsBattleHandRank _battleHandRank;
        private List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> _components;

        //public override int HandPower => EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
        public EightCardsBattleHandRank BattleHandRank => _battleHandRank;
        public List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> Components => _components;
        
        private void Init()
        {
            _components = [];
        }
        public EightCardSubBattleHand(BattleHandEnum battleHandEnum, EightCardsBattleHandRank inputRank, 
            params PokerCardComponent<EightCardsCompType, EightCardPokerCard>[] inputCombos)
        {
            Init();
            foreach(var comp in inputCombos)
            {
                _components.Add(comp);
                Cards.AddRange(comp.Cards);
            }
            _battleHandRank = inputRank;
            _battleHandEnum = battleHandEnum;
            _handPower = EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
        }

        public List<EightCardPokerCard> AddMinorCards(List<EightCardPokerCard> remainingCards)
        {
            var retCards = new List<EightCardPokerCard>(remainingCards);
            foreach (var card in remainingCards)
            {
                if (Cards.Count >= (int)_battleHandEnum) break;
                Cards.Add(card);
                retCards.RemoveAt(0);
            }
            return retCards;
        }
        
        
        public List<EightCardPokerCard> AddOneMinorCard(List<EightCardPokerCard> remainingCards)
        {
            var retCards = new List<EightCardPokerCard>(remainingCards);
            if (remainingCards.Count == 0 || Cards.Count >= (int)_battleHandEnum) return retCards;
            Cards.Add(retCards[0]);
            retCards.RemoveAt(0);
            return retCards;
        }
        
        public override int CompareTo(BaseSubBattleHand<EightCardPokerCard> other)
        {
            if (other == null) return 1;
            
            if (other is not EightCardSubBattleHand otherEightCard)
                throw new ArgumentException("Cannot compare different hand types");
            
            if (BattleHandRank != otherEightCard.BattleHandRank)
            {
                int myPower = EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
                int otherPower = EightCardsBattleHandPowerDict[(otherEightCard._battleHandEnum, otherEightCard.BattleHandRank)];
                if (myPower != otherPower) return myPower.CompareTo(otherPower);
                return BattleHandRank.CompareTo(otherEightCard.BattleHandRank);
            }

            // If BattleHandRank are same, check components
            int componentCount = Math.Min(_components.Count, otherEightCard.Components.Count);
            for (int i = 0; i < componentCount; i++)
            {
                int cmp = _components[i].CompareTo(otherEightCard.Components[i]);
                if (cmp != 0) return cmp;
            }
            
            if (_components.Count != otherEightCard.Components.Count)
                return _components.Count.CompareTo(otherEightCard.Components.Count);

            // If components are also same (or both empty as in 'Nothing' rank), compare all cards
            var mySortedCards = Cards.OrderByDescending(c => c.Number == 1 ? 14 : c.Number).ThenByDescending(c => c.Suit).ToList();
            var otherSortedCards = otherEightCard.Cards.OrderByDescending(c => c.Number == 1 ? 14 : c.Number).ThenByDescending(c => c.Suit).ToList();
            
            // Debug check for the specific problematic case
            if (mySortedCards.Count == 3 && mySortedCards[0].Number == 13 && otherSortedCards.Count == 5 && otherSortedCards[0].Number == 12)
            {
                 // Console.WriteLine($"Comparing Front K-high with Back Q-high: {mySortedCards[0].Number} vs {otherSortedCards[0].Number}");
            }

            for (int i = 0; i < Math.Min(mySortedCards.Count, otherSortedCards.Count); i++)
            {
                int myNum = mySortedCards[i].Number == 1 ? 14 : mySortedCards[i].Number;
                int otherNum = otherSortedCards[i].Number == 1 ? 14 : otherSortedCards[i].Number;
                
                if (myNum > otherNum) return 1;
                if (myNum < otherNum) return -1;
            }

            return mySortedCards.Count.CompareTo(otherSortedCards.Count);
        }
        
        
        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object obj)
        {
            if (obj is EightCardSubBattleHand other)
            {
                if (BattleHandRank != other.BattleHandRank) return false;
                foreach (var (comp1, comp2) in 
                         _components.Zip(other.Components, (comp1, comp2) => (comp1, comp2)))
                {
                    if (comp1.Equals(comp2) == false) return false;
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }
        
    }
    
}



/*
     public static bool operator >(EightCardSubBattleHand left, EightCardSubBattleHand right) => left.CompareTo(right) > 0;
     public static bool operator <(EightCardSubBattleHand left, EightCardSubBattleHand right) => left.CompareTo(right) < 0;
     public static bool operator >=(EightCardSubBattleHand left, EightCardSubBattleHand right) => left.CompareTo(right) >= 0;
     public static bool operator <=(EightCardSubBattleHand left, EightCardSubBattleHand right) => left.CompareTo(right) <= 0;
     public static bool operator ==(EightCardSubBattleHand left, EightCardSubBattleHand right) => left?.Equals(right) ?? right is null;
     public static bool operator !=(EightCardSubBattleHand left, EightCardSubBattleHand right) => !(left == right);
     */