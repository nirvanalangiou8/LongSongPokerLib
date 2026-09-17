using GenericPoker.EightCard;

namespace GenericPoker
{
    public class HandSplitResult
    {
        public EightCardSubBattleHand FrontHand { get; set; }
        public EightCardSubBattleHand BackHand { get; set; }
        public double FrontWinRate { get; set; }
        public double BackWinRate { get; set; }
        public double TotalScore { get; set; }

        public HandSplitResult(EightCardSubBattleHand frontHand, EightCardSubBattleHand backHand, double frontWinRate, double backWinRate, double totalScore)
        {
            FrontHand = frontHand;
            BackHand = backHand;
            FrontWinRate = frontWinRate;
            BackWinRate = backWinRate;
            TotalScore = totalScore;
        }
    }
}
