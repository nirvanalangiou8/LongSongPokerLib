using GenericPoker.EightCard;

namespace GenericPoker
{
    public class HandSplitResult
    {
        public BaseSubBattleHand FrontHand { get; set; }
        public BaseSubBattleHand BackHand { get; set; }
        public double FrontWinRate { get; set; }
        public double BackWinRate { get; set; }
        public double TotalScore { get; set; }

        public HandSplitResult(BaseSubBattleHand frontHand, BaseSubBattleHand backHand, double frontWinRate, double backWinRate, double totalScore)
        {
            FrontHand = frontHand;
            BackHand = backHand;
            FrontWinRate = frontWinRate;
            BackWinRate = backWinRate;
            TotalScore = totalScore;
        }
    }
}
