namespace GenericPoker.FourCard
{
    public class FourCardBigJoker : FourCardPokerCard, IFourCardSpecialCard
    {
        public static readonly int[] _bAllowReplacedNumbers = new int[] { 1, 5, 6 };
        public int[] AllowReplacedNumbers => _bAllowReplacedNumbers;
        
        public override string CardStr => "BigJoker";
        
        public override bool IsJoker => true;
        
        public override bool IsPair(FourCardPokerCard anotherCard)
        {
            return anotherCard.IsJoker;
        }
    }
}