namespace GenericPoker
{


    
    public interface IJokerFlushable 
    {
        /*bool MatchSuit(PokerSuit inputSuit)
        {
            var retBool = ((int)_suit & (int)inputSuit) != 0;
            if (retBool) SetSuitSub(inputSuit);
            return retBool;
        }*/
        void SetSuitSub(PokerSuit inputSuit);
        
    }

    public interface IJokerStraightable 
    {
        //void CheckStraight();
        void SetStraightSub(int number);
    }
    
    
    public interface IAceable : INumberReplaceable
    {
        void CheckAce();
    }

    public interface IAKable : INumberReplaceable
    {
        void CheckAce();
    }
    
    public interface INumberReplaceable
    {
        void CheckAce();
    }
    
}