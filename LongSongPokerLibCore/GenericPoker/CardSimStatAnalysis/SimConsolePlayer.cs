using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimConsolePlayer : ConsolePlayer<SimPokerCard>
    {
        private SimPokerHandCalculator _pokerHandCalculator;

        public SimConsolePlayer(string playerName) : base(playerName)
        {
            _pokerHandCalculator = new SimPokerHandCalculator();
        }

        public List<SimPokerHandStructure> ProcessSimHands()
        {
            _pokerHandCalculator.SetupCards(_pokerCards);
            return _pokerHandCalculator.TestSimCards();
        }

        public override List<EightCard.PokerHandStructure> ProcessHands()
        {
            return new List<EightCard.PokerHandStructure>();
        }
    }

    public class SimCardPlayerFactory : EightCard.IPlayerFactory<SimConsolePlayer>
    {
        public SimConsolePlayer Create(string name)
        {
            return new SimConsolePlayer(name);
        }
    }
}
