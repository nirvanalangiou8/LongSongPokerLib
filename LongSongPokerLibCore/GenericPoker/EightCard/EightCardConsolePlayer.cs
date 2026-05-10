using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.EightCard
{
    public interface IPlayerFactory<out TPlayer>
    {
        TPlayer Create(string name);
    }
    
    public class EightCardPlayerFactory : IPlayerFactory<EightCardConsolePlayer>
    {
        public EightCardConsolePlayer Create(string name)
        {
            return new EightCardConsolePlayer(name);
        }
    }
    
    public class EightCardConsolePlayer : ConsolePlayer<EightCardPokerCard> 
    {
        private PokerHandCalculator _pokerHandCalculator;
        
        public EightCardConsolePlayer(string playerName) : base(playerName)
        {
            _pokerHandCalculator = new PokerHandCalculator();
        }
        
        public EightCardConsolePlayer() : base()
        {
            _pokerHandCalculator = new PokerHandCalculator();
        }
        
        public override List<PokerHandStructure> ProcessHands()
        {
            var castedList = _pokerCards.Cast<EightCardPokerCard>().ToList();
            _pokerHandCalculator.SetupCards(castedList);
            // TBR
            //_pokerHandCalculator.SetupCards(_pokerCards);
            return _pokerHandCalculator.Test8Cards();
        }
    }

}