using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker
{
    public class BaseSubBattleHand : IComparable<BaseSubBattleHand>
    {
        protected List<BasePokerCard> _cards;
        public List<BasePokerCard> Cards => _cards;

        protected BattleHandEnum _battleHandEnum;
        public BattleHandEnum BattleHandEnum
        {
            get => _battleHandEnum;
            set => _battleHandEnum = value;
        }

        protected PokerOverAllHandRank _battleHandRank;
        public PokerOverAllHandRank BattleHandRank
        {
            get => _battleHandRank;
            set => _battleHandRank = value;
        }

        protected List<PokerCardComponent<BaseCompType, BasePokerCard>> _components;
        public List<PokerCardComponent<BaseCompType, BasePokerCard>> Components => _components;

        protected int _handPower;
        public virtual int HandPower => _handPower;

        protected string _handName;
        public string HandName => _handName;

        public BaseSubBattleHand()
        {
            _cards = new List<BasePokerCard>();
            _components = new List<PokerCardComponent<BaseCompType, BasePokerCard>>();
        }

        public BaseSubBattleHand(List<BasePokerCard> cards)
        {
            _cards = cards != null ? new List<BasePokerCard>(cards) : new List<BasePokerCard>();
            _components = new List<PokerCardComponent<BaseCompType, BasePokerCard>>();
        }

        public BaseSubBattleHand(BattleHandEnum battleHandEnum, PokerOverAllHandRank inputRank, 
            params PokerCardComponent<BaseCompType, BasePokerCard>[] inputCombos)
        {
            _cards = new List<BasePokerCard>();
            _components = new List<PokerCardComponent<BaseCompType, BasePokerCard>>();
            if (inputCombos != null)
            {
                foreach (var comp in inputCombos)
                {
                    _components.Add(comp);
                    if (comp.Cards != null)
                    {
                        _cards.AddRange(comp.Cards);
                    }
                }
            }
            _battleHandRank = inputRank;
            _battleHandEnum = battleHandEnum;
            if (PokerConst.EightCardsBattleHandPowerDict.TryGetValue((_battleHandEnum, BattleHandRank), out var power))
            {
                _handPower = power;
            }
            else
            {
                _handPower = 0;
            }
        }

        public string GetHandString(string separator = "_")
        {
            return string.Join(separator, Cards.Select(card => card.CardStr));
        }

        public virtual List<BasePokerCard> AddMinorCards(List<BasePokerCard> remainingCards, int count = int.MaxValue)
        {
            if (remainingCards == null) return new List<BasePokerCard>();
            var retCards = new List<BasePokerCard>(remainingCards);
            int added = 0;
            int maxCards = (int)_battleHandEnum > 0 ? (int)_battleHandEnum : int.MaxValue;
            while (retCards.Count > 0 && Cards.Count < maxCards && added < count)
            {
                Cards.Add(retCards[0]);
                retCards.RemoveAt(0);
                added++;
            }
            return retCards;
        }

        public virtual int CompareTo(BaseSubBattleHand? other)
        {
            if (other == null) return 1;

            if (BattleHandRank != other.BattleHandRank)
            {
                int myPower = PokerConst.EightCardsBattleHandPowerDict.TryGetValue((_battleHandEnum, BattleHandRank), out var p1) ? p1 : HandPower;
                int otherPower = PokerConst.EightCardsBattleHandPowerDict.TryGetValue((other._battleHandEnum, other.BattleHandRank), out var p2) ? p2 : other.HandPower;
                if (myPower != otherPower) return myPower.CompareTo(otherPower);
                return BattleHandRank.CompareTo(other.BattleHandRank);
            }

            // If BattleHandRank are same, check components
            int componentCount = Math.Min(_components.Count, other.Components.Count);
            for (int i = 0; i < componentCount; i++)
            {
                int cmp = _components[i].CompareTo(other.Components[i]);
                if (cmp != 0) return cmp;
            }
            
            if (_components.Count != other.Components.Count)
                return _components.Count.CompareTo(other.Components.Count);

            // If components are also same (or both empty as in 'Nothing' rank), compare all cards
            var mySortedCards = Cards.OrderByDescending(c => c.Number == 1 ? 14 : c.Number).ThenByDescending(c => c.Suit).ToList();
            var otherSortedCards = other.Cards.OrderByDescending(c => c.Number == 1 ? 14 : c.Number).ThenByDescending(c => c.Suit).ToList();
            
            for (int i = 0; i < Math.Min(mySortedCards.Count, otherSortedCards.Count); i++)
            {
                int myNum = mySortedCards[i].Number == 1 ? 14 : mySortedCards[i].Number;
                int otherNum = otherSortedCards[i].Number == 1 ? 14 : otherSortedCards[i].Number;
                
                if (myNum > otherNum) return 1;
                if (myNum < otherNum) return -1;
            }

            return mySortedCards.Count.CompareTo(otherSortedCards.Count);
        }

        public static bool operator >(BaseSubBattleHand? left, BaseSubBattleHand? right) => left?.CompareTo(right) > 0;
        public static bool operator <(BaseSubBattleHand? left, BaseSubBattleHand? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >=(BaseSubBattleHand? left, BaseSubBattleHand? right) => left is null ? right is null : left.CompareTo(right) >= 0;
        public static bool operator <=(BaseSubBattleHand? left, BaseSubBattleHand? right) => left is null || left.CompareTo(right) <= 0;
        public static bool operator ==(BaseSubBattleHand? left, BaseSubBattleHand? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(BaseSubBattleHand? left, BaseSubBattleHand? right) => !(left == right);

        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object? obj)
        {
            if (obj is BaseSubBattleHand other)
            {
                if (BattleHandRank != other.BattleHandRank) return false;
                if (_components.Count != other._components.Count) return false;
                for (int i = 0; i < _components.Count; i++)
                {
                    if (!_components[i].Equals(other._components[i])) return false;
                }

                if (Cards.Count != other.Cards.Count) return false;
                for (int i = 0; i < Cards.Count; i++)
                {
                    if (!Cards[i].Equals(other.Cards[i])) return false;
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