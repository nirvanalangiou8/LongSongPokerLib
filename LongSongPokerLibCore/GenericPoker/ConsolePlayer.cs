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
                .Select(s => EightCardPokerCard.CreateInstance(s.Trim()))
                .Cast<BasePokerCard>()
                .ToList();

            return EvaluateBestSplitHand(cards, rule);
        }

        public static HandSplitResult? EvaluateBestSplitHand(IEnumerable<BasePokerCard> inputCards, ICardRule? rule = null)
        {
            var cards = inputCards.Select(c => c as EightCardPokerCard ?? EightCardPokerCard.CreateInstance(c.CardStr)).ToList();
            if (cards.Count != 8)
            {
                return null;
            }

            EightCardSubBattleHand bestFrontHand = null, bestBackHand = null;
            double bestFront = 0, bestBack = 0, bestTotal = double.NegativeInfinity;

            foreach (var frontIdx in Combinations(cards.Count, 3))
            {
                var frontCards = frontIdx.Select(i => cards[i]).ToList();
                var backCards = Enumerable.Range(0, cards.Count)
                    .Where(i => !frontIdx.Contains(i))
                    .Select(i => cards[i])
                    .ToList();

                var frontHand = EvaluateBestSingleHand(frontCards, BattleHandEnum.FirstHand);
                var backHand = EvaluateBestSingleHand(backCards, BattleHandEnum.SecondHand);

                if (backHand.CompareTo(frontHand) < 0) continue;

                double front = WinRateStrategy.GetSubHandWinRate(frontHand);
                double back = WinRateStrategy.GetSubHandWinRate(backHand);
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

        public static EightCardSubBattleHand EvaluateBestSingleHand(List<EightCardPokerCard> cards, BattleHandEnum which)
        {
            var best = BuildSingleHand(which, EightCardsBattleHandRank.Nothing,
                new List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>(),
                cards);

            var calc = new PokerHandCalculator();
            calc.SetupCards(cards);
            calc.MinFlushStraightCards = 3;

            var structures = calc.Test8Cards();

            foreach (var st in structures)
            {
                var comps = st.Components;
                if (comps.Count == 0) continue;

                EightCardsBattleHandRank rank;
                var usedComps = new List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>>();

                if (comps.Count >= 2 &&
                    (PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
                         (comps[0].CompRank, comps[1].CompRank), out rank) ||
                     PokerHandCalculator.EightCardsCompComboToBattleRankDict.TryGetValue(
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

                if (!EightCardSubBattleHand.EightCardsBattleHandPowerDict.ContainsKey((which, rank)))
                    continue;

                var usedSet = new HashSet<EightCardPokerCard>(usedComps.SelectMany(c => c.Cards));
                var leftovers = cards.Where(c => !usedSet.Contains(c)).ToList();

                var cand = BuildSingleHand(which, rank, usedComps, leftovers);
                if (cand.CompareTo(best) > 0) best = cand;
            }

            return best;
        }

        public static EightCardSubBattleHand BuildSingleHand(
            BattleHandEnum which,
            EightCardsBattleHandRank rank,
            List<PokerCardComponent<EightCardsCompType, EightCardPokerCard>> comps,
            List<EightCardPokerCard> kickers)
        {
            var hand = new EightCardSubBattleHand(which, rank, comps.ToArray());
            var sorted = kickers.OrderByDescending(c => c.PokerCardPower).ToList();
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
