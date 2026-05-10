using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.FourCard;
using GenericPoker.EightCard;

namespace GenericPoker
{
    
    public class BaseSubBattleHand<TCard>: IComparable<BaseSubBattleHand<TCard>> where TCard : BasePokerCard
    {
        //protected List<TCard> Cards;
        protected List<TCard> _cards;
        public List<TCard> Cards {
            get => _cards;
            //set
            //{
            //    _cards = value;
            //}
        }
        private EightCardsBattleHandRank _battleHandRank;
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

        // return example 6_Club_10_Heart
       
        
        public EightCardsBattleHandRank BattleHandRank => _battleHandRank;
        
        
        public virtual int CompareTo(BaseSubBattleHand<TCard> other)
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
        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }
        
    }
}