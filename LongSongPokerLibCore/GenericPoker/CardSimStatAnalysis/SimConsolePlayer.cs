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

        public List<SimPokerHandStructure> ProcessSimHands()
        {
            var simCards = _pokerCards.Select(c => c as BasePokerCard ?? BasePokerCard.CreateInstance(c.CardStr)).ToList();
            _pokerHandCalculator.SetupCards(simCards);
            return _pokerHandCalculator.TestSimCards();
        }

        public override List<EightCard.PokerHandStructure> ProcessHands()
        {
            return new List<EightCard.PokerHandStructure>();
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
