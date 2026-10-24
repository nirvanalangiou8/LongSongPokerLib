namespace GenericPoker
{
    public class BaseJokerCardStraight : BaseJokerCard, IJokerStraightable
    {
        private int _replacedNumber = 0;
        
        public override bool IsNumberable => true;
        public override string CardStr => "StraightJoker";
        public override string CardUnitTestStr => $"{CardStr}_{PokerConst.PokerNumberNameDict[_number]}";

        public override int Number => _replacedNumber != 0 ? _replacedNumber : _number;

        public void SetStraightSub(int number)
        {
            _number = number;
            _replacedNumber = number;
        }
    }
}
