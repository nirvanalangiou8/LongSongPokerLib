using System.Collections.Generic;
using System.Linq;

namespace GenericPoker.FourCard
{
    public class FourCardSubBattleHand : BaseSubBattleHand<FourCardPokerCard>
    {
        public static readonly int RankBracketDigits = 16;
        public static readonly int SquareRankBracketDigits = RankBracketDigits * RankBracketDigits;
        public static readonly int PairCompensatePoints = 8;
        
        public string HandStrOnlyNum => $"{Cards[0].CardStrNumOnly}:{Cards[1].CardStrNumOnly}";
        
        public FourCardSubBattleHand(List<FourCardPokerCard> cards)
                : base(cards.ToList())
        {
            // Sort the cards, so later we can always assume the first card is larger than the second one.
            _cards = _cards.OrderByDescending(x => x.PokerCardPower).ToList();
            (_handPower, _handName) = CalcHandPower();
        }

        public bool IsPair()
        {
            return Cards[0].IsPair(Cards[1]);
        }
        
        private (int, string) CalcHandPower()
        {
            var handPower = 0;
            var handName = "";
            if (Cards[0].IsPair(Cards[1]))
            {
                handPower = (Cards[0].Number + FourCardSubBattleHand.PairCompensatePoints) *
                            FourCardSubBattleHand.SquareRankBracketDigits;
                handName = PokerConst.PokerNumberNameDict[Cards[0].Number] + "-Pair";
            } else {
                var bestPts = ((FourCardPokerCard)Cards[0]).DecideBestPts(Cards[1]);
                    handPower = bestPts * FourCardSubBattleHand.SquareRankBracketDigits +
                                Cards[0].Number * FourCardSubBattleHand.RankBracketDigits +
                                Cards[1].Number;
                handName = $"{bestPts}-Pts";
            }

            return (handPower, handName);
        }
        
        public override int CompareTo(BaseSubBattleHand<FourCardPokerCard> other)
        {
            return 0;
        }
    }
}