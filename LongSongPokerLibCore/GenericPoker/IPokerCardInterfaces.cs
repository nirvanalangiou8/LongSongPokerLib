namespace GenericPoker
{
    public interface IJokerFlushable
    {
        void CheckFlush();
        bool MatchSuit(PokerSuit inputSuit);
        void SetSuitSub(PokerSuit inputSuit);
        
    }

    public interface IJokerStraightable
    {
        void CheckStraight();
        void SetStraightSub(int number);
    }
    
    
    public interface IAceable
    {
        void CheckAce();
    }

}