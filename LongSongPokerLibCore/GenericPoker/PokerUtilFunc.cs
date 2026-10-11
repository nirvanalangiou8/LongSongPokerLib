namespace GenericPoker;

public class PokerUtilFunc
{
    public static List<BasePokerCard> ConvertStrToPokerCards(string wholeCardStr)
    {
        var inputCardStrs = wholeCardStr.Split(',');
        var newCardList = new List<BasePokerCard>();
			
        foreach (var str in inputCardStrs) {
            var splitDeckStrs = str.Split('@');
            int deckNumber = 1;
				
            if (splitDeckStrs.Length == 2)
            {   
                deckNumber =  int.Parse(splitDeckStrs[1]);
            }
            // The TryParse not only parse enum string but if also it's potential number. 
            // If the input string is a number like "222", the first check, (TryParse) always be valid, and return 222 as JokerType.
            // So we need the second check to ensure it's real enum string matched.
            if (Enum.TryParse<JokerType>(splitDeckStrs[0], out var jokerType) && Enum.IsDefined(typeof(JokerType), jokerType))
            {
                newCardList.Add(BaseJokerCard.CreateInstance(jokerType, deckID: deckNumber));
            } else {
                newCardList.Add(BasePokerCard.CreateInstance(splitDeckStrs[0], deckID :deckNumber));
            }
        }
        return newCardList;
    }
}