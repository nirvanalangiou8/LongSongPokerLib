using System;
using GenericPoker.EightCard;
using GenericPoker.CardSimStatAnalysis;

namespace GenericPoker
{
    //public class AceCard : EightCardPokerCard , IJokerStraightable
    public class AcePokerCard : SimPokerCard , IJokerStraightable
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
}