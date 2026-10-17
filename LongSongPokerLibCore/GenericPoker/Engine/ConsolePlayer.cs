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

        public virtual HandSplitResult? EvaluateBestSplitHand()
        {
            return EvaluateBestSplitHand(_pokerCards, Rule);
        }

        public static HandSplitResult? EvaluateBestSplitHand(string inputCardStr, ICardRule? rule = null)
        {
            var cards = inputCardStr.Split(',')
                .Select(s => BasePokerCard.CreateInstance(s.Trim()))
                .Cast<BasePokerCard>()
                .ToList();

            return EvaluateBestSplitHand(cards, rule);
        }

        public static HandSplitResult? EvaluateBestSplitHand(IEnumerable<BasePokerCard> inputCards, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? EightCardRule.Default;
            var cards = inputCards.Select(c => c as BasePokerCard ?? BasePokerCard.CreateInstance(c.CardStr)).ToList();
            if (cards.Count != effectiveRule.CardCount)
            {
                return null;
            }

            int frontCount = effectiveRule.SubHandsCardCount.TryGetValue(BattleSubHandOrder.FirstHand, out var fc) ? fc : 3;

            BaseSubBattleHand bestFrontHand = null, bestBackHand = null;
            double bestFront = 0, bestBack = 0, bestTotal = double.NegativeInfinity;

            foreach (var frontIdx in Combinations(cards.Count, frontCount))
            {
                var frontCards = frontIdx.Select(i => cards[i]).ToList();
                var backCards = Enumerable.Range(0, cards.Count)
                    .Where(i => !frontIdx.Contains(i))
                    .Select(i => cards[i])
                    .ToList();

                var frontHand = EvaluateBestSingleHand(frontCards, BattleSubHandOrder.FirstHand, effectiveRule);
                var backHand = EvaluateBestSingleHand(backCards, BattleSubHandOrder.SecondHand, effectiveRule);

                if (backHand.CompareTo(frontHand) < 0) continue;

                double front = IWinRateHandArrangeStrategy.GetSubHandWinRate(frontHand, BattleSubHandOrder.FirstHand, effectiveRule);
                double back = IWinRateHandArrangeStrategy.GetSubHandWinRate(backHand, BattleSubHandOrder.SecondHand, effectiveRule);
                double total = front + back;

                if (total > bestTotal)
                {
                    bestTotal = total;
                    bestFront = front;
                    bestBack = back;
                    bestFrontHand = frontHand;
                    bestBackHand = backHand;
                }
            }

            if (bestFrontHand == null || bestBackHand == null) return null;
            return new HandSplitResult(bestFrontHand, bestBackHand, bestFront, bestBack, bestTotal);
        }

        public static BaseSubBattleHand EvaluateBestSingleHand(List<BasePokerCard> cards, BattleSubHandOrder which, ICardRule? rule = null)
        {
            var effectiveRule = rule ?? EightCardRule.Default;
            var best = BuildSingleHand(which, PokerOverAllHandRank.Nothing,
                new List<PokerCardComponent>(),
                cards,
                effectiveRule);

            var calc = new PokerHandCalculator(effectiveRule);
            calc.SetupCards(cards);
            calc.MinFlushStraightCards = effectiveRule.MinFlushStraightCount;

            var structures = calc.Test8Cards();

            foreach (var st in structures)
            {
                var comps = st.Components;
                if (comps.Count == 0) continue;

                PokerOverAllHandRank rank;
                var usedComps = new List<PokerCardComponent>();

                if (comps.Count >= 2 &&
                    (PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
                         (comps[0].CompRank, comps[1].CompRank), out rank) ||
                     PokerConst.EightCardsCompComboToBattleRankDict.TryGetValue(
                         (comps[1].CompRank, comps[0].CompRank), out rank)))
                {
                    usedComps.Add(comps[0]);
                    usedComps.Add(comps[1]);
                }
                else
                {
                    rank = PokerHandStructure.ConvertCompRankToBattleRank(comps[0].CompRank);
                    usedComps.Add(comps[0]);
                }

                if (!PokerConst.EightCardsBattleHandPowerDict.ContainsKey((which, rank)))
                    continue;

                var usedSet = new HashSet<BasePokerCard>(usedComps.SelectMany(c => c.Cards));
                var leftovers = cards.Where(c => !usedSet.Contains(c)).ToList();

                var cand = BuildSingleHand(which, rank, usedComps, leftovers, effectiveRule);
                if (cand.CompareTo(best) > 0) best = cand;
            }

            return best;
        }

        public static BaseSubBattleHand BuildSingleHand(
            BattleSubHandOrder which,
            PokerOverAllHandRank rank,
            List<PokerCardComponent> comps,
            List<BasePokerCard> kickers,
            ICardRule? rule = null)
        {
            var effectiveRule = rule ?? EightCardRule.Default;
            int count = effectiveRule?.SubHandsCardCount != null &&
                        effectiveRule.SubHandsCardCount.TryGetValue(which, out var c) ? c : (which == BattleSubHandOrder.FirstHand ? 3 : 5);
            var hand = new BaseSubBattleHand(which, rank, count, comps.ToArray());
            var sorted = kickers.OrderByDescending(k => k.PokerCardPower).ToList();
            hand.AddMinorCards(sorted);
            return hand;
        }

        public static IEnumerable<int[]> Combinations(int n, int k)
        {
            var idx = new int[k];
            for (int i = 0; i < k; i++) idx[i] = i;

            while (true)
            {
                yield return (int[])idx.Clone();

                int pos = k - 1;
                while (pos >= 0 && idx[pos] == n - k + pos) pos--;
                if (pos < 0) break;

                idx[pos]++;
                for (int i = pos + 1; i < k; i++) idx[i] = idx[i - 1] + 1;
            }
        }
    }
}
