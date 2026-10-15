using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker
{
    public class BaseSubBattleHand : IComparable<BaseSubBattleHand>
    {
        protected List<BasePokerCard> _cards;
        public List<BasePokerCard> Cards => _cards;

        protected BattleSubHandOrder _battleSubHandOrder;
        
        protected int _designatedCardCount;
        public int DesignatedCardCount
        {
            get => _designatedCardCount > 0 ? _designatedCardCount : (_battleSubHandOrder == BattleSubHandOrder.FirstHand ? 3 : 5);
            set => _designatedCardCount = value;
        }
        
        // help me create a varaible called KickerWeighter as    
        public int KickerWeighter { get; set; }
        
        public BattleSubHandOrder BattleSubHandOrder
        {
            get => _battleSubHandOrder;
            set => _battleSubHandOrder = value;
        }

        protected PokerOverAllHandRank _battleHandRank;
        public PokerOverAllHandRank BattleHandRank
        {
            get => _battleHandRank;
            set => _battleHandRank = value;
        }

        protected List<PokerCardComponent> _components;
        public List<PokerCardComponent> Components => _components;

        protected int _handPower;
        public virtual int HandPower => _handPower;

        protected string _handName;
        public string HandName => _handName;

        public BaseSubBattleHand()
        {
            _cards = new List<BasePokerCard>();
            _components = new List<PokerCardComponent>();
        }

        public BaseSubBattleHand(List<BasePokerCard> cards, int designatedCardCount = 0)
        {
            _designatedCardCount = designatedCardCount;
            _cards = cards != null ? new List<BasePokerCard>(cards) : new List<BasePokerCard>();
            _components = new List<PokerCardComponent>();
        }

        public BaseSubBattleHand(BattleSubHandOrder battleSubHandOrder, int designatedCardCount = 0)
        {
            _designatedCardCount = designatedCardCount;
            _battleSubHandOrder = battleSubHandOrder;
            _cards = new List<BasePokerCard>();
            _components = new List<PokerCardComponent>();
        }

        public BaseSubBattleHand(BattleSubHandOrder battleSubHandOrder, PokerOverAllHandRank inputRank, 
            params PokerCardComponent[] inputCombos) : this(battleSubHandOrder, inputRank, 0, inputCombos)
        {
        }

        public BaseSubBattleHand(BattleSubHandOrder battleSubHandOrder, PokerOverAllHandRank inputRank,
            int designatedCardCount, params PokerCardComponent[] inputCombos)
        {
            _designatedCardCount = designatedCardCount;
            _cards = new List<BasePokerCard>();
            _components = new List<PokerCardComponent>();
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
            _battleSubHandOrder = battleSubHandOrder;
            if (PokerConst.EightCardsBattleHandPowerDict.TryGetValue((_battleSubHandOrder, BattleHandRank), out var power))
            {
                _handPower = power;
            }
            else
            {
                _handPower = 0;
            }
        }

        public string GetHandString(string separator = ",", string leftBrace = "[", string rightBrace = "]")
        {
            var compCards = _components?.SelectMany(c => c.Cards ?? Enumerable.Empty<BasePokerCard>()).ToList() ?? new List<BasePokerCard>();
            if (compCards.Count > 0)
            {
                var compStr = $"{leftBrace}{string.Join(separator, compCards.Select(card => card.CardStr))}{rightBrace}";
                var kickerCards = _cards.Skip(compCards.Count).ToList();
                if (kickerCards.Count > 0)
                {
                    return $"{compStr}{separator}{string.Join(separator, kickerCards.Select(card => card.CardStr))}";
                }
                return compStr;
            }

            return string.Join(separator, Cards.Select(card => card.CardStr));
        }

        // add minor cards, and return the number of cards added. If count is not assigned, add as manay as there still has
        // room and also the inputCards still have those amount of cars.
        // If count is assigned, add those count but still make sure there still has room and also enough count in inputCards.
        public virtual int AddMinorCards(List<BasePokerCard>? inputCards, int count = int.MaxValue)
        {
            var actualTakeCount = 0;

            if (inputCards == null || inputCards.Count == 0 || count <= 0)
            {
                return actualTakeCount;
            }

            int remainingCapacity = Math.Max(0, DesignatedCardCount - Cards.Count);
            if (remainingCapacity == 0)
            {
                return actualTakeCount;
            }

            actualTakeCount = Math.Min(Math.Min(count, remainingCapacity), inputCards.Count);

            if (actualTakeCount > 0)
            {
                Cards.AddRange(inputCards.GetRange(0, actualTakeCount));
                inputCards.RemoveRange(0, actualTakeCount);
            }

            return actualTakeCount;
        }
        
        /*
        public virtual int AddMinorCards(List<BasePokerCard> inputCards, int count = int.MaxValue)
        {
            int added = 0;
            if (inputCards == null) return added;
            var retCards = new List<BasePokerCard>(inputCards);

            int maxCards = (int)_battleSubHandOrder > 0 ? (int)_battleSubHandOrder : int.MaxValue;
            while (retCards.Count > 0 && Cards.Count < maxCards && added < count)
            {
                Cards.Add(retCards[0]);
                retCards.RemoveAt(0);
                added++;
            }
            return added;
        }*/

        public virtual int CompareTo(BaseSubBattleHand? other)
        {
            if (other == null) return 1;

            if (BattleHandRank != other.BattleHandRank)
            {
                int myPower = PokerConst.EightCardsBattleHandPowerDict.TryGetValue((_battleSubHandOrder, BattleHandRank), out var p1) ? p1 : HandPower;
                int otherPower = PokerConst.EightCardsBattleHandPowerDict.TryGetValue((other._battleSubHandOrder, other.BattleHandRank), out var p2) ? p2 : other.HandPower;
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
                /*
                if (BattleHandRank != other.BattleHandRank) return false;
                if (_components.Count != other._components.Count) return false;
                for (int i = 0; i < _components.Count; i++)
                {
                    if (!_components[i].Equals(other._components[i])) return false;
                }*/

                if (Cards.Count != other.Cards.Count) return false;
                for (int i = 0; i < Cards.Count; i++)
                {
                    if (!Cards[i].Equivelent(other.Cards[i])) return false;
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