namespace GenericPoker
{
    public class BaseJokerCardSuit : BaseJokerCard, IJokerFlushable
    {
        private PokerSuit _replacedSuit = PokerSuit.NoSuit;
        
        public override bool IsNumberable => false;
        public override string CardStr => "SuitJoker";
        public override string CardUnitTestStr => $"{CardStr}{PokerConst.PokerSuitToSymbol[_replacedSuit]}";

        public void SetSuitSub(PokerSuit inputSuit)
        {
            _replacedSuit = inputSuit;
        }
    }
}
