using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimConsolePlayer : ConsolePlayer
    {
        private SimStatEstimator _pokerHandCalculator;

        public SimConsolePlayer(string playerName) : base(playerName)
        {
            _pokerHandCalculator = new SimStatEstimator();
        }

        public List<PokerHandStructure> ProcessSimHands()
        {
            var simCards = _pokerCards.Select(c => c as BasePokerCard ?? BasePokerCard.CreateInstance(c.CardStr)).ToList();
            _pokerHandCalculator.SetupCards(simCards);
            return _pokerHandCalculator.TestSimCards();
        }

        public override List<PokerHandStructure> ProcessHands()
        {
            return new List<PokerHandStructure>();
        }
    }

    public class SimCardPlayerFactory : IPlayerFactory<SimConsolePlayer>
    {
        public SimConsolePlayer Create(string name)
        {
            return new SimConsolePlayer(name);
        }
    }
}
