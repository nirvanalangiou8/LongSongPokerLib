namespace GenericPoker
{
    public class SevenCardRule : BaseCardRule
    {
        public static new SevenCardRule Default { get; } = new();

        public SevenCardRule()
        {
            MinStraightCount = 5;
            MinFlushCount = 5;
            MinFlushStraightCount = 3;
            MinKindCount = 2;
            // temporarily use 8 cards data.
            WinRateTableFile = "prob_8cards_twohands";
            SubHandSlots = new List<SubHandSlotConfig>
            {
                new(BattleSubHandOrder.FirstHand, 2, "Front"),
                new(BattleSubHandOrder.SecondHand, 5, "Back")
            };
            LoadWinRateTable();
        }
    }
}