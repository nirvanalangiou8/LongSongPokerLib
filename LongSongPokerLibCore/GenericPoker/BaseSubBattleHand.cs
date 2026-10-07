using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericPoker
{
    
    
   
    
    public class BaseSubBattleHand<TCard>: IComparable<BaseSubBattleHand<TCard>> where TCard : BasePokerCard
    {
        //protected List<TCard> Cards;
        protected List<TCard> _cards;
        public List<TCard> Cards {
            get => _cards;
        }
        private PokerOverAllHandRank _battleHandRank;
        protected int _handPower;
        protected string _handName;
        
        public virtual int HandPower => _handPower;
        public string HandName => _handName;

        protected BaseSubBattleHand()
        {
            _cards = new List<TCard>();
        }
        
        protected BaseSubBattleHand(List<TCard> cards)
        {
            _cards = new List<TCard>(cards);
        }
        
        public string GetHandString(string separator = "_")
        {
            return string.Join(separator, Cards.Select(card => card.CardStr));    
        }
        
        public PokerOverAllHandRank BattleHandRank => _battleHandRank;
        
        
        public virtual void AddMinorCards(List<TCard> remainingCards)
        {
            if (remainingCards != null)
            {
                _cards.AddRange(remainingCards);
            }
        }

        public virtual int CompareTo(BaseSubBattleHand<TCard>? other)
        {
            return 0;
        }
        
        public static bool operator >(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => left.CompareTo(right) > 0;
        public static bool operator <(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => left.CompareTo(right) < 0;
        public static bool operator >=(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => left.CompareTo(right) >= 0;
        public static bool operator <=(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => left.CompareTo(right) <= 0;
        public static bool operator ==(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(BaseSubBattleHand<TCard> left, BaseSubBattleHand<TCard> right) => !(left == right);
        
        // Override Equals and GetHashCode for proper equality checks
        public override bool Equals(object? obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }
        
    }

    
    public class BaseSubBattleHand<TRank, TCompEnum, TCard> : BaseSubBattleHand<TCard>
        where TRank : Enum
        where TCompEnum : Enum
        where TCard : BasePokerCard
    {
        private TRank _battleHandRank;
        public new TRank BattleHandRank => _battleHandRank;

        public List<PokerCardComponent<TCompEnum, TCard>> Components { get; protected set; }

        public BaseSubBattleHand() : base()
        {
            Components = new List<PokerCardComponent<TCompEnum, TCard>>();
        }

        public BaseSubBattleHand(List<TCard> cards) : base(cards)
        {
            Components = new List<PokerCardComponent<TCompEnum, TCard>>();
        }

        public BaseSubBattleHand(TRank inputRank, params PokerCardComponent<TCompEnum, TCard>[] inputCombos)
        {
            Components = new List<PokerCardComponent<TCompEnum, TCard>>();
            _cards = new List<TCard>();
            if (inputCombos != null)
            {
                foreach (var comp in inputCombos)
                {
                    Components.Add(comp);
                    if (comp.Cards != null)
                    {
                        _cards.AddRange(comp.Cards);
                    }
                }
            }
            _battleHandRank = inputRank;
        }

        public override void AddMinorCards(List<TCard> remainingCards)
        {
            if (remainingCards != null)
            {
                _cards.AddRange(remainingCards);
            }
        }

        public override int CompareTo(BaseSubBattleHand<TCard> other)
        {
            if (other is BaseSubBattleHand<TRank, TCompEnum, TCard> otherHand)
            {
                int rankCompare = ((IComparable)BattleHandRank).CompareTo(otherHand.BattleHandRank);
                if (rankCompare != 0) return rankCompare;
                
                // Tie-breaker logic would go here
            }
            return base.CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseSubBattleHand<TRank, TCompEnum, TCard> other)
            {
                return EqualityComparer<TRank>.Default.Equals(BattleHandRank, other.BattleHandRank) &&
                       Cards.SequenceEqual(other.Cards);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return BattleHandRank != null ? BattleHandRank.GetHashCode() : 0;
        }
    }
}