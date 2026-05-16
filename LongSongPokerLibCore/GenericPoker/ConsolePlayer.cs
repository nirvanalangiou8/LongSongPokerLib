using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using GenericPoker.EightCard;

namespace GenericPoker
{
    
    public class ConsolePlayer<TCard> where TCard: BasePokerCard
    {
        public string PlayerName { get; set; }
        
        protected List<TCard> _pokerCards;
        
        public List<TCard> Cards => _pokerCards;
        
        public ConsolePlayer(string playerName)
        {
            PlayerName = playerName;
            _pokerCards = new List<TCard>();
        }
        
/*
        public ConsolePlayer()
        {
            _pokerCards = new List<TCard>();
        }
*/
        
        public void SetCards(List<TCard> inputCards)
        {
            _pokerCards.AddRange(inputCards);
        }
        
        public void ClearCards()
        {
            _pokerCards.Clear();
        }
        
        

        public virtual List<PokerHandStructure> ProcessHands()
        {
            return null;
        }

        
    }
}