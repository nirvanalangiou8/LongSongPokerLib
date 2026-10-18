using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker
{
    public interface IPlayerFactory<out TPlayer> where TPlayer : ConsolePlayer
    {
        TPlayer Create(string name);
    }

    public class ConsolePlayer
    {
        public string PlayerName { get; set; }
        protected List<BasePokerCard> _pokerCards;
        public List<BasePokerCard> Cards => _pokerCards;
        public ICardRule Rule { get; set; }

        public ConsolePlayer(string playerName, ICardRule? rule = null)
        {
            PlayerName = playerName;
            Rule = rule ?? EightCardRule.Default;
            _pokerCards = new List<BasePokerCard>();
        }

        public void SetCards(IEnumerable<BasePokerCard> inputCards)
        {
            _pokerCards.AddRange(inputCards);
        }

        public void ClearCards()
        {
            _pokerCards.Clear();
        }

        public virtual List<PokerHandStructure> ProcessHands()
        {
            return null;
        }
        

        public static HandSplitResult? EvaluateBestSplitHand(string inputCardStr, ICardRule? rule = null)
        {
            var inputCards = PokerUtilFunc.ConvertStrToPokerCards(inputCardStr);
            var effectiveRule = rule ?? EightCardRule.Default;
            var battleHands = new BaseBattleHands(inputCards, effectiveRule);
            var result = battleHands.GetArrangeHands();
            if (result.FrontHand == null || result.BackHand == null) return null;

            double front = result.FrontHand.winScore;
            double back = result.BackHand.winScore;
            return new HandSplitResult(result.FrontHand, result.BackHand, front, back, front + back);
        }
    }
}
