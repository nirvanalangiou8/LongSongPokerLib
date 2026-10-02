using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.EightCard
{
    /*
    public class EightCardConsolePlayer : ConsolePlayer
    {
        private PokerHandCalculator _pokerHandCalculator;

        public EightCardConsolePlayer(string playerName) : base(playerName, EightCardRule.Default)
        {
            _pokerHandCalculator = new PokerHandCalculator();
        }

        public override List<PokerHandStructure> ProcessHands()
        {
            var castedList = _pokerCards.Select(c => c as EightCardPokerCard ?? EightCardPokerCard.CreateInstance(c.CardStr)).ToList();
            _pokerHandCalculator.SetupCards(castedList);
            return _pokerHandCalculator.Test8Cards();
        }
    }

    public class EightCardPlayerFactory : IPlayerFactory<EightCardConsolePlayer>
    {
        public EightCardConsolePlayer Create(string name)
        {
            return new EightCardConsolePlayer(name);
        }
    }*/
}
