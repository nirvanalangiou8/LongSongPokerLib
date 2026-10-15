namespace GenericPoker
{


    
    public interface IJoker
    {
        int JokerPower { get; }
    }

    public interface IJokerFlushable : IJoker
    {
        void SetSuitSub(PokerSuit inputSuit);
    }

    public interface IJokerStraightable : IJoker
    {
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