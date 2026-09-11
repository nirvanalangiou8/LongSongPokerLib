using GenericPoker.EightCard;

namespace GenericPoker
{
    
    public enum EightCardJokerType
    {
        StraightJoker = 1, // Can be replaced to form straight
        SuitJoker = 2, // Can be replaced to form suit
        DrawCardJoker = 3, // Can be any card in the draw card pool
        MinorJoker = 4, // Can be Ace, replaced as flush or straight
        MajorJoker = 5,	// Can be any card you wish. 
    }
    
    

    public class AceCard : EightCardPokerCard , IJokerStraightable
    {
        private int _replacedNumber = 0;
        public int JokerPower => 100; // High value means low priority in sorting compared to real jokers
        
        public override int Number => _replacedNumber != 0 ? _replacedNumber : _number;

        public void SetAceFourteenNumber(int number)
        {
            _number = number;
            _replacedNumber = number;
        }
        
/*
        public override int DecideBestFourCardAceNumber(EightCardPokerCard anotherCard)
        {
            var retPts = 0;
            var totalPts1 = (14 + anotherCard.Number) % 10;
            var totalPts2 = (1 + anotherCard.Number) % 10;
            if (totalPts1 > totalPts2) {
                _replacedNumber = 14;
                retPts = totalPts1;
            } else {
                retPts = totalPts2;
            }

            return retPts;
        }
*/
        
        public void SetStraightSub(int number)
        {
            _number = number;
            _replacedNumber = number;
        }
        
        /*
        public void CheckStraight()
        {
            throw new System.NotImplementedException();
        }*/
    }
    
    
    public class EightCardJokerCard : EightCardPokerCard, IJoker
    {
        protected JokerType _jokerType;

        public override string CardStr => $"Joker";
        
        public override string CardUnitTestStr => CardStr;

        public int JokerPower => (int)_jokerType;
        
        public static EightCardJokerCard CreateInstance(EightCardJokerCard another)
        {
            return CreateInstance(another._jokerType, another.ObjectID, deckID : another.DeckID);
        }
        
        public static EightCardJokerCard CreateInstance(JokerType jokerType, int objectID = 0, int deckID = 1)
        {
            EightCardJokerCard data;
            
            var id = PokerConst.TotalRegularPokerCardsWithoutJokers + (int)jokerType;
            
            int number;
            PokerSuit pokerSuit;
            
            switch (jokerType)
            {
                case JokerType.MajorJoker:
                    data = new EightCardJokerCardMajor();
                    number = PokerConst.MaxTotalCountInSameSuit + (int)jokerType;
                    pokerSuit = PokerSuit.Wild;
                    break;
                case JokerType.MinorJoker:
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
                default:
                    data = new EightCardJokerCardSuit();
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
