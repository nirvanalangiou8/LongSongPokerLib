using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.EightCard
{
    

    
    public class EightCardSubBattleHand : BaseSubBattleHand <BasePokerCard>
    {
       
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
            _handPower = PokerConst.EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
        }

        public List<BasePokerCard> AddMinorCards(List<BasePokerCard> remainingCards, int count = int.MaxValue)
        {
            if (remainingCards == null) return new List<BasePokerCard>();
            var retCards = new List<BasePokerCard>(remainingCards);
            int added = 0;
            while (retCards.Count > 0 && Cards.Count < (int)_battleHandEnum && added < count)
            {
                Cards.Add(retCards[0]);
                retCards.RemoveAt(0);
                added++;
            }
            return retCards;
        }
        
        public override int CompareTo(BaseSubBattleHand<BasePokerCard> other)
        {
            if (other == null) return 1;
            
            if (other is not EightCardSubBattleHand otherEightCard)
                throw new ArgumentException("Cannot compare different hand types");
            
            if (BattleHandRank != otherEightCard.BattleHandRank)
            {
                int myPower = PokerConst.EightCardsBattleHandPowerDict[(_battleHandEnum, BattleHandRank)];
                int otherPower = PokerConst.EightCardsBattleHandPowerDict[(otherEightCard._battleHandEnum, otherEightCard.BattleHandRank)];
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
