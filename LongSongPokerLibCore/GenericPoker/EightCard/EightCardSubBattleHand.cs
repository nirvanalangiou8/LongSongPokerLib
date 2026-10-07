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
    
    
    public class EightCardSubBattleHand : BaseSubBattleHand <BasePokerCard>
    {
        public static readonly Dictionary<(BattleHandEnum, PokerOverAllHandRank), int> EightCardsBattleHandPowerDict =
        new()
        {
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.Nothing ), 0},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.Pair ), 1},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.TwoPairs ), 2},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.ThreeCardsFlushStraight ), 24},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.ThreeOfKind ), 15},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.FourOfKind ), 32},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.FourCardsFlushStraight ), 40},
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Nothing), 0 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Pair), 1 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.TwoPairs), 2 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.ThreeOfKind), 10 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsStraight), 24 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FullHouse), 28 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.ThreeCardsFlushStraight), 32 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsFlush), 40 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Mansion), 48 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsStraight), 62 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FourOfKind), 80 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FourCardsFlushStraight), 100 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsFlush), 120 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsStraight), 200 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsFlushStraight), 360 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsStraight), 500 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsFlush), 800 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsFlushStraight), 1000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsFlush), 20000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsFlushStraight), 40000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsFlushStraight), 400000 },
        };
        
        private BattleHandEnum _battleHandEnum;
        //private List<PokerCard> _cards;
        private PokerOverAllHandRank _battleHandRank;
        private List<PokerCardComponent<BaseCompType, BasePokerCard>> _components;

        //public override int HandPower => EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
        public PokerOverAllHandRank BattleHandRank => _battleHandRank;
        public List<PokerCardComponent<BaseCompType, BasePokerCard>> Components => _components;
        
        private void Init()
        {
            _components = [];
        }
        public EightCardSubBattleHand(BattleHandEnum battleHandEnum, PokerOverAllHandRank inputRank, 
            params PokerCardComponent<BaseCompType, BasePokerCard>[] inputCombos)
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

        public List<BasePokerCard> AddMinorCards(List<BasePokerCard> remainingCards)
        {
            var retCards = new List<BasePokerCard>(remainingCards);
            foreach (var card in remainingCards)
            {
                if (Cards.Count >= (int)_battleHandEnum) break;
                Cards.Add(card);
                retCards.RemoveAt(0);
            }
            return retCards;
        }
        
        
        public List<BasePokerCard> AddOneMinorCard(List<BasePokerCard> remainingCards)
        {
            var retCards = new List<BasePokerCard>(remainingCards);
            if (remainingCards.Count == 0 || Cards.Count >= (int)_battleHandEnum) return retCards;
            Cards.Add(retCards[0]);
            retCards.RemoveAt(0);
            return retCards;
        }
        
        public override int CompareTo(BaseSubBattleHand<BasePokerCard> other)
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
