using System;
using System.Collections.Generic;
using Random = System.Random;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using GenericPoker.EightCard;

namespace GenericPoker
{
    public class ConsoleCardDealer<TCard>  where TCard : BasePokerCard
    {
	    private readonly List<TCard> _pokerCards;
	    
	    private readonly List<TCard> _discardCards;
	    
	    public int TotalCards => _pokerCards.Count;
	    private readonly int _cardDecks;
	    
	    public List<TCard> RemainingCards => _pokerCards;
	    
	    public ConsoleCardDealer(int cardDecks = 1, bool addJokers = false)
	    {
		    _cardDecks = cardDecks;
		    _pokerCards = new List<TCard>();
		    _discardCards = new List<TCard>();
		    for (int i = 0; i < _cardDecks; i++)
		    {
			    for (var suitId = BasePokerCard.RegularSuitClubIndex; suitId <= BasePokerCard.RegularSuitSpadeIndex; suitId++)
			    {
				    var pokerSuit = (PokerSuit)Enum.GetValues(typeof(PokerSuit)).GetValue(suitId);
				    for (int number = 2; number <= PokerConst.AceBigNumber; number++)
				    {
					    int id = (suitId - 1) * PokerConst.AceBigNumber + number;
					    BasePokerCard newPokerCard;
  			    if (typeof(TCard) == typeof(EightCardPokerCard)) {
						    newPokerCard = EightCardPokerCard.CreateInstance(id, number, pokerSuit, deckID: i+1);
					    } else if (typeof(TCard).Name == "SevenCardPokerCard") {
                            // Using reflection or dynamic to avoid circular dependency if SevenCardPokerCard is in SevenCard namespace
                            // but since it is in same assembly, it should be fine if we add the namespace.
                            // For now, let's assume we'll add the namespace.
                            newPokerCard = (BasePokerCard)typeof(TCard).GetMethod("CreateInstance", new[] { typeof(int), typeof(int), typeof(PokerSuit), typeof(int), typeof(int) })
                                .Invoke(null, new object[] { id, number, pokerSuit, 0, i + 1 });
					    } else {
						    throw new InvalidOperationException($"Unsupported card type: {typeof(TCard).Name}");
					    }
					    
					    //var newPokerCard = EightCardPokerCard.CreateInstance(id, number, pokerSuit, deckID: i+1);
					    _pokerCards.Add((TCard)newPokerCard);
				    }
			    }
		    }

		    if (addJokers)
		    {
			    
		    }
		    
  
		    //UtilFunc.Shuffle(_pokerCards);
		    XRandom.Instance.Shuffle(_pokerCards);
		    
		    
		    var debug = true;
		    if (false)
		    {
			    //List<string> selectedCards = new List<string>() { "8_Spade", "8_Club", "8_Heart", "8_Diamond", "9_Heart", "9_Diamond", "5_Club", "5_Diamond" };
			    //List<string> selectedCards = new List<string>() { "10_Spade", "6_Club", "10_Club", "A_Diamond", "5_Heart", "6_Diamond", "A_Club", "7_Diamond" };
			    //List<string> selectedCards = new List<string>() { "10_Spade", "6_Club", "10_Club", "A_Diamond", "5_Heart", "6_Diamond", "A_Spade", "7_Diamond" };
			    //List<string> selectedCards = new List<string>() { "10_Spade", "9_Club", "8_Club", "10_Diamond", "9_Heart", "6_Heart", "5_Spade", "4_Diamond" };
			    

			    List<string> selectedCards = new List<string>()
			        {   "5♣️", "5❤️", "3❤️", "3♠️"}; //  The catch str FiveCardsStraight_1,FourCardStraight_1
			    
			    var insertCards = new List<TCard>();
			    foreach (var cardStr in selectedCards)
			    {
				    var searchedCard = _pokerCards.FirstOrDefault(c =>c.CardStr == cardStr);
				    _pokerCards.Remove(searchedCard);
					insertCards.Add(searchedCard);	
			    }
			    _pokerCards.InsertRange(0, insertCards);
		    }
		    
		    /*PokerCardController.CreateJokerCard(JokerType.MajorJoker, _cardPlaceHolderTransform);
		    PokerCardController.CreateJokerCard(JokerType.MinorJoker, _cardPlaceHolderTransform);
		    PokerCardController.CreateJokerCard(JokerType.StraightJoker, _cardPlaceHolderTransform);
		    PokerCardController.CreateJokerCard(JokerType.SuitJoker, _cardPlaceHolderTransform);*/

		    // Shuffle in gameobject children list to make them shuffled in real world.
		    
	    }
	    
		    
	    private void RecycleDiscardAndReShuffle()
	    {
		    _pokerCards.AddRange(_discardCards);
		    _discardCards.Clear();
		    ShuffleCards();
	    }
	    
	    public void DealCards(ConsolePlayer<TCard> player, int numberOfCards)
	    {
		    if (numberOfCards > _pokerCards.Count)
		    {
			    RecycleDiscardAndReShuffle();
		    }
		    // Get the first 'count' elements
		    var popCards = _pokerCards.GetRange(0, numberOfCards);

		    // Remove the first 'count' elements
		    _pokerCards.RemoveRange(0, numberOfCards);
		    
		    player.SetCards(popCards);
	    }

	    public void CollectCards(ConsolePlayer<TCard> player)
	    {
		    _discardCards.AddRange(player.Cards);
		    player.ClearCards();
	    }
	    
	    public void ShuffleCards()
	    {
		    //UtilFunc.Shuffle(_pokerCards);
		    XRandom.Instance.Shuffle(_pokerCards);
	    }

	    
    }
}