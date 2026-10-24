namespace GenericPoker
{
    public class JokerCardMinor : JokerCard, IJokerFlushable, IJokerStraightable
    {
        private PokerSuit _replacedSuit = PokerSuit.NoSuit;
        private int _replacedNumber = 0;
        
        public override bool IsNumberable => true;
        public override string CardStr => "MinorJoker";

        public override string CardUnitTestStr
        {
            get
            {
                string retStr = CardStr;
                if (_replacedNumber != 0) retStr += $"_{PokerConst.PokerNumberNameDict[_number]}";
                if (_replacedSuit != PokerSuit.NoSuit) retStr += PokerConst.PokerSuitToSymbol[_replacedSuit];
                return retStr;
            }
        }

        public override int Number => _replacedNumber != 0 ? _replacedNumber : _number;

        public void SetStraightSub(int number)
        {
            _number = number;
            _replacedNumber = number;
        }

        public void SetSuitSub(PokerSuit inputSuit)
        {
            _replacedSuit = inputSuit;
        }
    }
}
