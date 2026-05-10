namespace GenericPoker.FourCard
{
    public class FourCardSmallJoker : FourCardPokerCard, IFourCardSpecialCard
    {

        public static readonly int[] _sAllowReplacedNumbers = new int[] { 1, 5, 6 };
	    
        
        public int[] AllowReplacedNumbers => _sAllowReplacedNumbers;
        
        // Consider how many Joker's to count the maximum number of ModulatorScale.
        // This is majorly used for count the hand power whenever need to compare card to card by using decimal concepts.
        //public static readonly int PokerPowerModulatorScale = (PokerConst.AceBigNumber + 1)*PokerHandCalculator.MaxPokerNumber;
        
        public override string CardStr => "SmallJoker";
        
        public override bool IsJoker => true;
        
        public override bool IsPair(FourCardPokerCard anotherCard)
        {
            return anotherCard.IsJoker;
        }  
    }
}