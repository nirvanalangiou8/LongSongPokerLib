
using System.Collections.Generic;

namespace GenericPoker
{
	/*
	public enum PokerBitTypes
	{
		BitPair = 0b_0000_0001,
		BitTwoPair = 0b_0000_0010,
		BitThreeOfKind = 0b_0000_0100,
		BitStraight = 0b_0000_1000,
		BitFlush = 0b_0001_0000,
		BitFourOfKind = 0b_0010_0000,
		BitFiveOfKind = 0b_0100_0000
	}*/


	public enum BaseCompType
	{
		Nothing,
		Pair,
		ThreeCardsFlush,
		ThreeCardsPairInFlush,
		ThreeCardsStraight,
		FourCardsFlush,
		FourCardStraight,
		FiveCardsStraight,
		FiveCardsFlush,
		FourCardsPairInFlush,
		FourCardsTwoPairsInFlush,
		ThreeOfKind,
		FiveCardsPairInFlush,
		FiveCardsTwoPairsInFlush,
		ThreeCardsFlushStraight,
		FourCardsFlushStraight,
		FourOfKind,
		FiveCardsFlushStraight,
		SixCardsFlush,
		SixCardsStraight,
		SixCardsPairInFlush,
		SixCardsTwoPairsInFlush,
		SixCardsFlushStraight,
		SixCardsThreePairsInFlush,
		SevenCardsFlush,
		SevenCardsStraight,
		SevenCardsPairInFlush,
		SevenCardsTwoPairsInFlush,
		SevenCardsThreePairsInFlush,
		SevenCardsFlushStraight,
		EightCardsFlush,
		EightCardsPairInFlush,
		EightCardsTwoPairsInFlush,
		EightCardsStraight,
		FiveOfKind,
		EightCardsFlushStraight,
		EightCardsThreePairsInFlush,
		EightCardsFourPairsInFlush,
		SixOfKind,
		SevenOfKind,
		EightOfKind,
		NineCardsFlush,
		NineCardsStraight,
		NineCardsFlushStraight,
		NineOfKind,
		TenCardsFlush,
		TenCardsStraight,
		TenCardsFlushStraight,
		TenOfKind,
		None,
	}
	
		

	public enum PokerRankTypes
	{
		FiveOfKind = 500,
		RoyalFlushStraight = 300,
		FlushStraight = 200,
		FourOfKind = 150,
		FullHouse = 80,
		Flush = 40,
		Straight = 30,
		FourCardsFlushStraight = 25,
		ThreeOfKind = 20,
		TwoPairs = 15,
		ThreeCardsFlushStraight = 12,
		FourCardsFlush = 10,
		FourCardStraight = 8,
		OnePair = 5,
		Nothing = 0
	}

	public enum CompType
	{
		Kind,
		Flush,
		Straight,
		FullHouse,
		FlushStraight,
	}
	
	public enum BattleHandEnum
	{
		FirstHand = 3,
		SecondHand = 5,
	}

	
	
	public enum PokerOverAllHandRank
    {
        None,
        Nothing,
        Pair,
        TwoPairs,
        ThreeOfKind,
        ThreeCardsFlushStraight,
        FiveCardsStraight,
        FiveCardsFlush,
        FullHouse,
        Mansion,
        SixCardsStraight,
        SixCardsFlush,
        SevenCardsStraight,
        FourCardsFlushStraight,
        FourOfKind,
        FiveCardsFlushStraight,
        EightCardsStraight,
        SevenCardsFlush,
        NineCardsStraight,
        SixCardsFlushStraight,
        EightCardsFlush,
        SevenCardsFlushStraight,
        EightCardsFlushStraight,
        NineCardsFlush,
        NineCardsFlushStraight
    }
	
	public enum PokerCardCompRank
	{
		ThreeCardFlush,
		ThreeCardStraight,
		FourCardFlush,
		FourCardStraight,
		ThreeOfKind,
		ThreeCardFlushStraight,
		FiveCardStraight,
		FiveCardFlush,
		FullHouse,
		FourOfKind,
		FourCardFlushStraight,
		FiveCardFlushStraight,
		FiveOfKind,

		// Following, the associated hasn't precisely calculated and just use temporary values  
		SixCardStraight,
		SixCardFlush,
		SixCardFlushStraight,
		SevenCardStraight,
		SevenCardFlush,
		SevenCardFlushStraight,
		EightCardStraight,
		EightCardFlush,
		EightCardFlushStraight,
		SixOfKind,
		SevenOfKind,
		EightOfKind,
		NineCardStraight,
		NineCardFlush,
		NineCardFlushStraight,
		TenCardStraight,
		TenCardFlush,
		TenCardFlushStraight,
		ElevenCardStraight,
		ElevenCardFlush,
		ElevenFlushStraight,
		TwelveCardStraight,
		TwelveCardFlush,
		TwelveFlushStraight,
		ThirteenCardStraight,
		ThirteenCardFlush,
		ThirteenCardFlushStraight,
		FourteenCardStraight,
	}

		
	public enum PokerSuit
	{
		NoSuit = 0, // Some joker can not be replaced as suit, but straight, so set this extra options.
		Club = 1,
		Diamond = 2,
		Heart = 4,
		Spade = 8, 
		Star = 15,
		Wild = 31,
	}
	//🂿 ♣ ♠️♠️♣️ ❤️, 🃏⭐ ♠️, 🔶 ♣️ ✖️
	
	public static partial class PokerConst
	{
		public const int MaxTotalCountInSameSuit = 13;
		public const int TotalRegularSuitCount = 4;
		public const int TotalRegularPokerCardsWithoutJokers = MaxTotalCountInSameSuit * TotalRegularSuitCount;
		public const int AceBigNumber = PokerConst.MaxTotalCountInSameSuit + 1;
		
		public static readonly Dictionary<int, string> PokerNumberNameDict = new Dictionary<int, string> {
			{ 1, "A" }, { 2, "2" }, { 3, "3" }, { 4, "4" }, { 5, "5" }, { 6, "6" },
			{ 7, "7" }, { 8, "8" }, { 9, "9" }, { 10, "10" }, { 11, "J" }, { 12, "Q" }, { 13, "K" }, {14, "A"}, {15, "Joker"}, {16, "Joker"}, {17, "Joker"}, {18, "Joker"},};
    
		public static readonly Dictionary<string, int> PokerStringToNumberDict = new Dictionary<string, int> {
			{"A", 14 }, {"2", 2}, {"3", 3}, {"4", 4}, {"5", 5}, {"6", 6 }, {"7", 7}, {"8", 8}, {"9", 9}, 
			{"10", 10}, {"J", 11}, {"Q", 12}, {"K", 13}, {"Joker", 15}};
		
		public static readonly Dictionary<PokerSuit, string> PokerSuitToSymbol = new Dictionary<PokerSuit, string> {
			{PokerSuit.NoSuit , "✖️" }, { PokerSuit.Club, "♣️" }, { PokerSuit.Diamond, "🔶" }, { PokerSuit.Heart, "❤️" }, 
			{ PokerSuit.Spade, "♠️" }, { PokerSuit.Star, "⭐" }, { PokerSuit.Wild, "🃏" },
		};
		
		public static readonly Dictionary<string, PokerSuit> SymbolToPokerSuit = new Dictionary<string, PokerSuit> {
			{"✖️", PokerSuit.NoSuit}, {"♣️", PokerSuit.Club}, {"🔶", PokerSuit.Diamond }, {"❤️", PokerSuit.Heart}, 
			{"♠️", PokerSuit.Spade}, {"⭐",  PokerSuit.Star}, {"🃏", PokerSuit.Wild},
		};
		
		public enum PokerCardRangeGroup
		{
			Royal = 0b0100,
			MiddleClass = 0b010,
			LowerClass = 0b001,
		}
		
		public static readonly Dictionary<PokerCardRangeGroup, (int, int)> MatchCardRangeNumberGroupDict = new Dictionary<PokerCardRangeGroup, (int, int)>
		{
			{ PokerCardRangeGroup.Royal, (10,14) },
			{ PokerCardRangeGroup.MiddleClass, (6,9)},
			{ PokerCardRangeGroup.LowerClass, (1,5) },
		};
		
		public static readonly Dictionary<(int, CompType), PokerCardCompRank> PokerCompNameDict =
			new Dictionary<(int, CompType), PokerCardCompRank>
			{
				{ (3, CompType.Kind), PokerCardCompRank.ThreeOfKind },
				{ (3, CompType.Flush), PokerCardCompRank.ThreeCardFlush },
				{ (3, CompType.Straight), PokerCardCompRank.ThreeCardStraight },
				{ (3, CompType.FlushStraight), PokerCardCompRank.ThreeCardFlushStraight },
				{ (4, CompType.Kind), PokerCardCompRank.FourOfKind },
				{ (4, CompType.Flush), PokerCardCompRank.FourCardFlush },
				{ (4, CompType.Straight), PokerCardCompRank.FourCardStraight },
				{ (4, CompType.FlushStraight), PokerCardCompRank.FourCardFlushStraight },
				{ (5, CompType.Kind), PokerCardCompRank.FiveOfKind },
				{ (6, CompType.Kind), PokerCardCompRank.SixOfKind },
				{ (7, CompType.Kind), PokerCardCompRank.SevenOfKind },
				{ (8, CompType.Kind), PokerCardCompRank.EightOfKind },
				{ (5, CompType.Flush), PokerCardCompRank.FiveCardFlush },
				{ (5, CompType.Straight), PokerCardCompRank.FiveCardStraight },
				{ (5, CompType.FullHouse), PokerCardCompRank.FullHouse },
				{ (5, CompType.FlushStraight), PokerCardCompRank.FiveCardFlushStraight },
				{ (6, CompType.Flush), PokerCardCompRank.SixCardFlush },
				{ (7, CompType.Flush), PokerCardCompRank.SevenCardFlush },
				{ (8, CompType.Flush), PokerCardCompRank.EightCardFlush },
				{ (6, CompType.Straight), PokerCardCompRank.SixCardStraight },
				{ (7, CompType.Straight), PokerCardCompRank.SevenCardStraight },
				{ (8, CompType.Straight), PokerCardCompRank.EightCardStraight },
				{ (6, CompType.FlushStraight), PokerCardCompRank.SixCardFlushStraight },
				{ (7, CompType.FlushStraight), PokerCardCompRank.SevenCardFlushStraight },
				{ (8, CompType.FlushStraight), PokerCardCompRank.EightCardFlushStraight },
				{ (13, CompType.Straight), PokerCardCompRank.ThirteenCardStraight },
				{ (13, CompType.FlushStraight), PokerCardCompRank.ThirteenCardFlushStraight },
				{ (14, CompType.Straight), PokerCardCompRank.FourteenCardStraight },
			};

        public static readonly Dictionary<(BattleHandEnum, PokerOverAllHandRank), int> EightCardsBattleHandPowerDict =
        new()
        {
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.Nothing ), 0},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.Pair ), 1},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.TwoPairs ), 2},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.ThreeCardsFlushStraight ), 24},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.ThreeOfKind ), 15},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.FourOfKind ), 32},
            { (BattleHandEnum.FirstHand, PokerOverAllHandRank.FourCardsFlushStraight ), 40},
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Nothing), 0 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Pair), 1 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.TwoPairs), 2 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.ThreeOfKind), 10 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsStraight), 24 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FullHouse), 28 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.ThreeCardsFlushStraight), 32 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsFlush), 40 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.Mansion), 48 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsStraight), 62 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FourOfKind), 80 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FourCardsFlushStraight), 100 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsFlush), 120 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsStraight), 200 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.FiveCardsFlushStraight), 360 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsStraight), 500 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsFlush), 800 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SixCardsFlushStraight), 1000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsFlush), 20000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.SevenCardsFlushStraight), 40000 },
            { (BattleHandEnum.SecondHand, PokerOverAllHandRank.EightCardsFlushStraight), 400000 },
        };
        
		
	}
	
		
}

