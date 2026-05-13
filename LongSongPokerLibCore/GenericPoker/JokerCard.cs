
using GenericPoker.EightCard;

namespace GenericPoker
{
    public enum JokerType
    {
        StraightJoker = 1, // Can be replaced to form straight
        SuitJoker = 2, // Can be replaced to form suit
        DrawCardJoker = 3, // Can be any card in the draw card pool
        MinorJoker = 4, // Can be Ace, replaced as flush or straight
        MajorJoker = 5,	// Can be any card you wish. 
    }

    
    
    public class JokerCard : BasePokerCard
    {
     
        private JokerType _jokerType;

        public override string CardStr => $"Joker";
        
        public override string CardUnitTestStr => CardStr;

        public int JokerPower => (int)_jokerType;
        
        public static JokerCard CreateInstance(JokerCard another)
        {
            return CreateInstance(another._jokerType, another.ObjectID, deckID : another.DeckID);
        }
        
        public static JokerCard CreateInstance(JokerType jokerType, int objectID = 0, int deckID = 1)
        {
            JokerCard data;
            
            var id = PokerConst.TotalRegularPokerCardsWithoutJokers + (int)jokerType;
            
            int number;
            PokerSuit pokerSuit;
            
            switch (jokerType)
            {
                case JokerType.MajorJoker:
                    data = new GodJoker();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                /*case JokerType.MinorJoker:
                    data = new EightCardJokerCardMinor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.SuitJoker:
                    data = new EightCardJokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.StraightJoker:
                    data = new EightCardJokerCardStraight();
                    number = 0;
                    pokerSuit = PokerSuit.NoSuit;
                    break;
                case JokerType.DrawCardJoker:
                    // TO BE REVISED
                    data = new EightCardJokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                */
                default:
                    data = new GodJoker();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
            }
            data._jokerType = jokerType;
            data.Init(id, number, pokerSuit, objectID, deckID : deckID);
            return data;
        }
    }
    
}