namespace GenericPoker
{
    public class JokerCard : BasePokerCard, IJoker
    {
        protected JokerType _jokerType;

        public override string CardStr => "Joker";
        
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
                    data = new JokerCardMajor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.MinorJoker:
                    data = new JokerCardMinor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.SuitJoker:
                    data = new JokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.StraightJoker:
                    data = new JokerCardStraight();
                    number = 0;
                    pokerSuit = PokerSuit.NoSuit;
                    break;
                case JokerType.DrawCardJoker:
                    // TO BE REVISED
                    data = new JokerCardSuit();
                    number = 0;
                    pokerSuit = PokerSuit.Wild;
                    break;
                default:
                    data = new JokerCardSuit();
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
