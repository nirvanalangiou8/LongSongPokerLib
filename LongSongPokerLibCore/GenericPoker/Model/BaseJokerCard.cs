namespace GenericPoker
{
    public class BaseJokerCard : BasePokerCard, IJoker
    {
        protected JokerType _jokerType;

        public override string CardStr => "Joker";
        
        public override string CardUnitTestStr => CardStr;

        public int JokerPower => (int)_jokerType;
        
        public static BaseJokerCard CreateInstance(BaseJokerCard another)
        {
            return CreateInstance(another._jokerType, another.ObjectID, deckID : another.DeckID);
        }
        
        public static BaseJokerCard CreateInstance(JokerType jokerType, int objectID = 0, int deckID = 1)
        {
            BaseJokerCard data;
            
            var id = PokerConst.TotalRegularPokerCardsWithoutJokers + (int)jokerType;
            
            int number;
            PokerSuit pokerSuit;
            
            switch (jokerType)
            {
                case JokerType.MajorJoker:
                    data = new BaseJokerCardMajor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.MinorJoker:
                    data = new BaseJokerCardMinor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.SuitJoker:
                    data = new BaseJokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.StraightJoker:
                    data = new BaseJokerCardStraight();
                    number = 0;
                    pokerSuit = PokerSuit.NoSuit;
                    break;
                case JokerType.DrawCardJoker:
                    // TO BE REVISED
                    data = new BaseJokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                default:
                    data = new BaseJokerCardSuit();
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
