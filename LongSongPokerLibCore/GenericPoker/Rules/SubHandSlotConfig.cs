namespace GenericPoker
{
    public record SubHandSlotConfig(BattleSubHandOrder Order, int CardCount, string SlotName = "")
    {
        public override string ToString() => string.IsNullOrEmpty(SlotName)
            ? $"{Order}: {CardCount} cards"
            : $"{SlotName} ({Order}): {CardCount} cards";
    }
}
