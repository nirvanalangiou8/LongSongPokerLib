using System.Linq;

namespace GenericPoker.FourCard
{
    public class FourCardAceCard: FourCardPokerCard, IFourCardSpecialCard
    {
        private static readonly int[] _aceAllowReplacedNumbers = new int[] { 1, 14 };
        
        public int[] AllowReplacedNumbers => _aceAllowReplacedNumbers;
        
    }
}