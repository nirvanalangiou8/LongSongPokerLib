namespace GenericPoker.EightCard
{
    public class JokerCardMajor : JokerCard, IJokerFlushable, IJokerStraightable
    {
        private JokerType _jokerType;
        private PokerSuit _replacedSuit = PokerSuit.NoSuit;
        private int _replacedNumber = 0;
        private IJokerFlushable _jokerFlushableImplementation;

        public override string CardStr => $"MajorJoker";
        public override bool IsNumberable => true;
        
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

        public override bool MatchSuit(PokerSuit inputSuit)
        {
            var retBool = ((int)_suit & (int)inputSuit) != 0;
            if (retBool) SetSuitSub(inputSuit);
            return retBool;
        }
        
        public void CheckFlush()
        {
            //Debug.Log("DBG get into joker card major.");
            throw new System.NotImplementedException();
        }
        
        public void CheckStraight()
        {
            throw new System.NotImplementedException();
        }
        
    }
    
    public class JokerCardMinor : JokerCard, IJokerFlushable, IJokerStraightable
    {
        private JokerType _jokerType;
        private PokerSuit _replacedSuit = PokerSuit.NoSuit;
        private int _replacedNumber = 0;
        public override bool IsNumberable => true;
        
        public override int Number => _replacedNumber != 0 ? _replacedNumber : _number;
        
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

        public void SetStraightSub(int number)
        {
            _number = number;
            _replacedNumber = number;
        }
        
        
        public void SetSuitSub(PokerSuit inputSuit)
        {
            _replacedSuit = inputSuit;
        }

        public override bool MatchSuit(PokerSuit inputSuit)
        {
            var retBool = ((int)_suit & (int)inputSuit) != 0;
            if (retBool) SetSuitSub(inputSuit);
            return retBool;
        }
        
        public void CheckFlush()
        {
            throw new System.NotImplementedException();
        }
        
        public void CheckStraight()
        {
            throw new System.NotImplementedException();
        }

        public override string CardStr => $"MinorJoker";
        
    }
    
    
    public class JokerCardSuit : JokerCard, IJokerFlushable
    {
        private JokerType _jokerType;
        private PokerSuit _replacedSuit = PokerSuit.NoSuit;
        public override bool IsNumberable => false;

        public override string CardStr => $"SuitJoker";
        
        public override string CardUnitTestStr => $"{CardStr}{PokerConst.PokerSuitToSymbol[_replacedSuit]}";
        
        
        public void SetSuitSub(PokerSuit inputSuit)
        {
            _replacedSuit = inputSuit;
        }

        public override bool MatchSuit(PokerSuit inputSuit)
        {
            var retBool = ((int)_suit & (int)inputSuit) != 0;
            if (retBool) SetSuitSub(inputSuit);
            return retBool;
        }
        
        public void CheckFlush()
        {
            throw new System.NotImplementedException();
        }
        
        public void CheckStraight()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class JokerCardStraight : JokerCard, IJokerStraightable
    {
        private JokerType _jokerType;
        private int _replacedNumber = 0;
        public override string CardStr => $"StraightJoker";
        public override string CardUnitTestStr => $"{CardStr}_{PokerConst.PokerNumberNameDict[_number]}";
        public override bool IsNumberable => true;

        public override int Number => _replacedNumber != 0 ? _replacedNumber : _number;
        
        public void SetStraightSub(int number)
        {
            _number = number;
        }
        
        public void CheckFlush()
        {
            throw new System.NotImplementedException();
        }
        
        public void CheckStraight()
        {
            throw new System.NotImplementedException();
        }
    }
}

