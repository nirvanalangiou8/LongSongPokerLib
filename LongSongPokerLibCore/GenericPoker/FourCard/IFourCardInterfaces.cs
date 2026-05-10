namespace GenericPoker.FourCard
{
    public interface IFourCardSpecialCard
    {
        //void SetAceFourteenNumber(int number);
       // public int DecideBestFourCardAceNumber(EightCardPokerCard anotherCard);
        int[] AllowReplacedNumbers { get; }
        //public int DecideBestPts(BasePokerCard anotherCard);
        //public static readonly int[] AllowReplacedNumbers = new int[] { 1, 5, 6 };
    }
    
    /*
    public interface IFourCardSmallJoker
    {
        //void SetAceFourteenNumber(int number);
        
        
        // Decided the best pts for self and another Card. If any card of these two has replaced number, like Ace, joker cards.
        // We will explore all possible total pts and return the largest points.
        public int DecideBestPts(EightCardPokerCard anotherCard);
    }
    
    public interface IFourCardBigJoker
    {
        public EightCardPokerCard ReplacedCard { get; set; } 
        public int DecideBestNumber(EightCardPokerCard anotherCard);
    }*/
    
}