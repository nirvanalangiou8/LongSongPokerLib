using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using GenericPoker;

namespace GenericPoker.EightCard
{
	public class PokerHandCalculator
	{
		//public List<PokerCard> rankSortedCards;
		//public PokerRankTypes BestRank => _bestRank;
		//private HandProcessData _tempEvaluateData;

		private readonly List<EightCardPokerCard> _allPokerCards;
		private readonly List<EightCardPokerCard> _noneJokerCards;
		private readonly List<EightCardPokerCard> _jokerCards;
		private PokerRankTypes _bestRank;


		// This is the possible estimated max poker number, consider A is 14, small joker is 15, larger joker is 16, and some reverse space.
		// This number might be used for computing the possible Single Card Rank comparision, or other usage. See which function use this to understand
		// the purpose of this variable.
		public static readonly int MaxPokerNumber = 20;
		private int _minFlushStraightCards = 3;

		public int MinFlushStraightCards
		{
			get { return _minFlushStraightCards; }
			set { _minFlushStraightCards = value; }
		}

		//private Dictionary<string, int> evaluationDecompDict = new Dictionary<string, int>();

		public static List<string> genericHandRank = new List<string>
		{
			"FiveOfKind", "RoyalFlushStraight", "FlushStraight", "FourOfKind",
			"FullHouse", "Flush", "Straight", "FourCardsFlushStraight", "ThreeOfKind",
			"ThreeCardsFlushStraight", "TwoPairs", "ThreeCardsFlush",
			"ThreeCardStraight", "OnePair"
		};

		private static readonly Dictionary<(int, CompType), PokerCardCompRank> PokerCompNameDict =
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

		
		
		public static readonly Dictionary<(EightCardsCompType, EightCardsCompType), EightCardsBattleHandRank> EightCardsCompComboToBattleRankDict =
			new()
			{
				{ (EightCardsCompType.Pair, EightCardsCompType.Pair ), EightCardsBattleHandRank.TwoPairs},
				{ (EightCardsCompType.ThreeCardsPairInFlush, EightCardsCompType.Pair ), EightCardsBattleHandRank.TownHouse},
				{ (EightCardsCompType.ThreeOfKind, EightCardsCompType.Pair ), EightCardsBattleHandRank.FullHouse},
				{ (EightCardsCompType.ThreeCardsFlushStraight, EightCardsCompType.Pair ), EightCardsBattleHandRank.Mansion},
			};
		
		
		private static readonly Dictionary<string, EightCardsCompType> EightCardsCompTypeDict = new()
			{
				{ ("2_Kind"), EightCardsCompType.Pair },
				{ "3_Kind", EightCardsCompType.ThreeOfKind },
				{ "4_Kind", EightCardsCompType.FourOfKind },
				{ "5_Kind", EightCardsCompType.FiveOfKind },
				{ "6_Kind", EightCardsCompType.SixOfKind },
				{ "7_Kind", EightCardsCompType.SevenOfKind },
				{ "8_Kind", EightCardsCompType.EightOfKind },
				{ "3_Flush", EightCardsCompType.ThreeCardsFlush },
				{ "3_Straight", EightCardsCompType.ThreeCardsStraight },
				{ "3_PairInFlush", EightCardsCompType.ThreeCardsPairInFlush},
				{ "3_FlushStraight", EightCardsCompType.ThreeCardsFlushStraight },
				{ "4_Flush", EightCardsCompType.FourCardsFlush },
				{ "4_PairInFlush", EightCardsCompType.FourCardsPairInFlush},
				{ "4_TwoPairsInFlush", EightCardsCompType.FourCardsTwoPairsInFlush},
				{ "4_Straight", EightCardsCompType.FourCardStraight },
				{ "4_FlushStraight", EightCardsCompType.FourCardsFlushStraight },
				{ "5_Flush", EightCardsCompType.FiveCardsFlush },
				{ "5_PairInFlush", EightCardsCompType.FiveCardsPairInFlush},
				{ "5_TwoPairsInFlush", EightCardsCompType.FiveCardsTwoPairsInFlush},
				{ "5_Straight", EightCardsCompType.FiveCardsStraight },
				{ "5_FlushStraight", EightCardsCompType.FiveCardsFlushStraight },
				{ "6_Flush", EightCardsCompType.SixCardsFlush },
				{ "6_PairInFlush", EightCardsCompType.SixCardsPairInFlush},
				{ "6_TwoPairsInFlush", EightCardsCompType.SixCardsTwoPairsInFlush},
				{ "6_ThreePairsInFlush", EightCardsCompType.SixCardsThreePairsInFlush},
				{ "7_Flush", EightCardsCompType.SevenCardsFlush },
				{ "7_PairInFlush", EightCardsCompType.SevenCardsPairInFlush},
				{ "7_TwoPairsInFlush", EightCardsCompType.SevenCardsTwoPairsInFlush},
				{ "7_ThreePairsInFlush", EightCardsCompType.SevenCardsThreePairsInFlush},
				{ "8_Flush", EightCardsCompType.EightCardsFlush },
				{ "8_PairInFlush", EightCardsCompType.EightCardsPairInFlush},
				{ "8_TwoPairsInFlush", EightCardsCompType.EightCardsTwoPairsInFlush},
				{ "8_ThreePairsInFlush", EightCardsCompType.EightCardsThreePairsInFlush},
				{ "8_FourPairsInFlush", EightCardsCompType.EightCardsFourPairsInFlush},
				{ "6_Straight", EightCardsCompType.SixCardsStraight },
				{ "7_Straight", EightCardsCompType.SevenCardsStraight },
				{ "8_Straight", EightCardsCompType.EightCardsStraight },
				{ "6_FlushStraight", EightCardsCompType.SixCardsFlushStraight },
				{ "7_FlushStraight", EightCardsCompType.SevenCardsFlushStraight },
				{ "8_FlushStraight", EightCardsCompType.EightCardsFlushStraight }
			};

		public PokerHandCalculator()
		{
			_allPokerCards = new List<EightCardPokerCard>();
			_noneJokerCards = new List<EightCardPokerCard>();
			_jokerCards = new List<EightCardPokerCard>();

			_bestRank = PokerRankTypes.Nothing;

			var test = new List<PokerCardComponent<EightCardsCompType>>();
		     var b = new PokerHandStructure(test);

		     //InitTempEvaluateData();
		}
		
		
		
		public void SetupCards(List<EightCardPokerCard> inputPokerCardList)
		{
			_allPokerCards.Clear();
			_noneJokerCards.Clear();
			_jokerCards.Clear();

			_allPokerCards.AddRange(inputPokerCardList);

			foreach (var card in _allPokerCards)
			{
				if (card is JokerCard) _jokerCards.Add(card);
				else _noneJokerCards.Add(card);
			}
		}
		
		
		public static PokerHandCalculator CreateInstance(string wholeCardStr)
		{
			var inputCardStrs = wholeCardStr.Split(',');
			var newCardList = new List<EightCardPokerCard>();
			
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
					newCardList.Add(JokerCard.CreateInstance(jokerType, deckID: deckNumber));
				} else {
					newCardList.Add(EightCardPokerCard.CreateInstance(splitDeckStrs[0], deckID :deckNumber));
				}
			}

			var data = new PokerHandCalculator();
			data.SetupCards(newCardList);
			return data;
		}

		public List<EightCardPokerCard> SortWithSuits()
		{
			var sortedSubLists = _evaluateFlushGroups(1, _allPokerCards);
			var flattenedList = sortedSubLists.SelectMany(sublist => sublist).ToList();
			return flattenedList;
		}

		public List<EightCardPokerCard> SortWithKinds()
		{

			var sortedList = GetKindGroups(1, _noneJokerCards);

			// step4 : flatten all list in list to a global flatten list.
			var allSortedCards = sortedList.SelectMany(pair => pair).ToList();

			return allSortedCards;
		}

		private List<PokerCardComponent<PokerCardCompRank> > ListsToPokerComp(List<List<EightCardPokerCard>> inputListInList,
			PokerCardCompRank pokerCardCompRank)
		{
			return inputListInList
				.Select(subList => new PokerCardComponent<PokerCardCompRank> { CompRank = pokerCardCompRank, Cards = subList }).ToList();
		}

		public List<PokerCardComponent<PokerCardCompRank>> GetAllPermuteComps(int cardCountInComp)
		{
			var allPokerCardComps = new List<PokerCardComponent<PokerCardCompRank>>();
			var permutes = new List<List<EightCardPokerCard>>();

			//=============================
			// ======= process kinds ======
			//============================
			List<List<EightCardPokerCard>> kindGroupLists = GetNumberGroups(cardCountInComp, _noneJokerCards);
			foreach (var gList in kindGroupLists)
			{
				permutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(gList, cardCountInComp));
			}

			allPokerCardComps.AddRange(ListsToPokerComp(permutes,
				PokerCompNameDict[(cardCountInComp, CompType.Kind)]));

			//==========================
			//====== process flush =====
			//==========================
			permutes.Clear();
			var flushGroupLists = _evaluateFlushGroups(cardCountInComp, _allPokerCards);
			foreach (var gList in flushGroupLists)
			{
				permutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(gList, cardCountInComp));
			}

			// filter out flush straight as later, we will have dedicated method to collect flush straight?
			allPokerCardComps.AddRange(ListsToPokerComp(permutes,
				PokerCompNameDict[(cardCountInComp, CompType.Flush)]));

			//======= Straight ====
			// process straight might be a bit trickier than kinds and flush.
			//====================
			permutes.Clear();
			kindGroupLists = GetNumberGroups(1, _noneJokerCards);
			var straightableJokerCards = _jokerCards.Where(card => card is IJokerStraightable).ToList();
			ProcessPermuteStraight(cardCountInComp, kindGroupLists, straightableJokerCards, permutes);

			allPokerCardComps.AddRange(ListsToPokerComp(permutes,
				PokerCompNameDict[(cardCountInComp, CompType.Straight)]));


			//==== Flush Straight ===
			// For flush straight, we get flush groups, and check each flush group to sort out all possible
			// straights to collect flush straight hands.
			//======================
			permutes.Clear();
			foreach (var gList in flushGroupLists)
			{
				// Need to convert the single List into ListInList to cater the straight searching.
				var wrapperListInList = gList.Select(item => new List<EightCardPokerCard> { item }).ToList();
				RecursivePermuteStraight(cardCountInComp, wrapperListInList, new List<EightCardPokerCard>(), permutes);
			}

			allPokerCardComps.AddRange(ListsToPokerComp(permutes,
				PokerCompNameDict[(cardCountInComp, CompType.FlushStraight)]));

			// After running above, we have flush and straight, and flush straight, So sometimes,
			// the same Comp will be double of tripple count as flush, straight or flush-straight.
			// Ex: A-Spade, K-Spade, Q-Spade, can be flush, straight, and flush-straight. Then we will pick these three
			// cards as highest Comp to represent, which is flush-straight.
			// Then our below algorithm is to group by same Comp cards -- Using CompUniqueKey(), and if there are multi Comps
			// in each group which are duplicated, but different CompRank, then we sort each duplicated group, and pick the first one
			// which has best CompRank, which is flush-straight in the example.
			allPokerCardComps = allPokerCardComps.GroupBy(cardComp => cardComp.CompUniqueKey())
				.Select(cardCompDupGroups =>
					cardCompDupGroups.OrderByDescending(cardComp => cardComp.CompRank).First())
				.ToList();

			//--- Finally sort them out base on their CompPower order.
			//allPokerCardComps = allPokerCardComps.OrderByDescending(obj => obj.CompPower).ToList();
			allPokerCardComps.Sort((x, y) => y.CompareTo(x));


			return allPokerCardComps;
		}


		public List<PokerCardComponent<PokerCardCompRank>> GetAllFlushStraightComps(int cardCountInComp)
		{
			var allComps = new List<PokerCardComponent<PokerCardCompRank>>();
			var permutes = new List<List<EightCardPokerCard>>();

			// process flush
			permutes.Clear();

			var straightAndFlushableJokers =
				_jokerCards.Where(card => (card is IJokerStraightable and IJokerFlushable)).ToList();
			var natureSuitCardNeeded = cardCountInComp - straightAndFlushableJokers.Count < 1
				? 1
				: cardCountInComp - straightAndFlushableJokers.Count;
			List<List<EightCardPokerCard>> flushGroupLists = _evaluateFlushGroups(natureSuitCardNeeded, _allPokerCards);

			//	var straightAndFlushableJokerList = straightAndFlushableJokers.Select(item => new List<PokerCard> { item }).ToList(); 

			// For flush straight, we get flush groups, and check each flush group to sort out all possible
			// straights to collect flush straight hands.
			permutes.Clear();
			foreach (var flushGroup in flushGroupLists)
			{
				// Need to convert the single List into ListInList to cater the straight searching.
				var wrapperListInList = flushGroup.Select(item => new List<EightCardPokerCard> { item }).ToList();
				var representedSuit = flushGroup[0].Suit;
				ProcessPermuteStraight(cardCountInComp, wrapperListInList, straightAndFlushableJokers, permutes,
					representedSuit);
			}

			allComps.AddRange(ListsToPokerComp(permutes,
				PokerCompNameDict[(cardCountInComp, CompType.FlushStraight)]));

			//--- Finally sort them out base on their CompPower
			//allComps = allComps.OrderByDescending(obj => obj.CompPower).ToList();
			allComps.Sort((x, y) => y.CompareTo(x));

			return allComps;
		}

		public List<PokerCardComponent<PokerCardCompRank>> GetAllStraightComps(int cardCountInComp)
		{
			var allComps = new List<PokerCardComponent<PokerCardCompRank>>();
			var permutes = new List<List<EightCardPokerCard>>();


			var kindGroupLists = GetNumberGroups(1, _noneJokerCards);
			var straightableJokerCards = _jokerCards.Where(card => card is IJokerStraightable).ToList();
			ProcessPermuteStraight(cardCountInComp, kindGroupLists, straightableJokerCards, permutes);
			allComps.AddRange(ListsToPokerComp(permutes, PokerCompNameDict[(cardCountInComp, CompType.Straight)]));


			//--- Finally sort them out base on their CompPower order.
			allComps.Sort((x, y) => y.CompareTo(x));

			return allComps;
		}


		public List<PokerHandStructure> Test8Cards()
		{
			var allCandidateComps = new List<PokerHandStructure>();
			RecursiveEvaluateCards(_allPokerCards, new PokerHandStructure(), allCandidateComps);
			//RecursiveArrangeHands(_allPokerCards, new PokerHandStructure(), allCandidateComps);
			foreach (var res in allCandidateComps)
			{
				res.SortCompsAndClassify();
			}
			
			allCandidateComps.Sort((c1, c2) => c2.CompareTo(c1));
			var uniqueCandidates = allCandidateComps.Distinct().ToList();
			
			return uniqueCandidates;
		}

		public EightCardHands Test8CardsTwoHandsDeploy()
		{
			var allCandidateComps = new List<PokerHandStructure>();
			
			RecursiveArrangeHands(_allPokerCards, new PokerHandStructure(), allCandidateComps);
			
			foreach (var res in allCandidateComps) res.SortCompsAndClassify();
			
			allCandidateComps.Sort((c1, c2) => c2.CompareTo(c1));
			var uniqueCandidates = allCandidateComps.Distinct().ToList();
			var allPokerHands = new List<EightCardHands>();
			var arrangeStrategy = new BalancedStrategy();
			
			foreach (var pokerStructure in uniqueCandidates)
				allPokerHands.Add(pokerStructure.ArrangeHands(arrangeStrategy));
			
			allPokerHands.Sort((c1, c2) => c2.CompareTo(c1));
			
			return allPokerHands[0];
		}


		private List<List<EightCardPokerCard>> GetSuitCompWithJokers(List<EightCardPokerCard> inputCards, int cardCountInComp,
			List<EightCardPokerCard> jokerCards, PokerSuit subSuit)
		{
			var permutes = new List<List<EightCardPokerCard>>();
			var rangeNumbers = Enumerable.Range(1, 14).Reverse().ToList();
			var retCompPermutes = new List<List<EightCardPokerCard>>();

			var jokerNeeded = cardCountInComp - 1;
			for (var jokerCountInvolved = jokerNeeded; jokerCountInvolved > 0; jokerCountInvolved--)
			{

				var pokerCardCountInvoled = cardCountInComp - jokerCountInvolved;
				if (jokerCountInvolved > jokerCards.Count || pokerCardCountInvoled > inputCards.Count)
					continue;
				permutes.Clear();
				permutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(inputCards, pokerCardCountInvoled));
				foreach (var suitComp in permutes)
				{
					var newCompCards = new List<EightCardPokerCard>(suitComp);
					var cardNums = newCompCards.Select(card => card.Number).ToList();
					var jokerReplacedNumCandidate = rangeNumbers.Where(x => !cardNums.Contains(x)).ToList();

					foreach (var (jokerCard, index) in jokerCards.Select((value, i) => (value, i)))
					{
						var newJoker = JokerCard.CreateInstance((JokerCard)jokerCard);
						((IJokerFlushable)newJoker).SetSuitSub(subSuit);
						if (newJoker.IsNumberable)
							((IJokerStraightable)newJoker).SetStraightSub(jokerReplacedNumCandidate[index]);
						newCompCards.Add(newJoker);
						if (newCompCards.Count >= cardCountInComp) break;
					}

					newCompCards = newCompCards.OrderByDescending(obj => obj.Number).ToList();
					retCompPermutes.Add(newCompCards);
				}
			}

			return retCompPermutes;
		}
		
		public List<PokerCardComponent<PokerCardCompRank>> GetAllFlushComps(int cardCountInComp)
		{

			var allComps = new List<PokerCardComponent<PokerCardCompRank>>();
			var permutes = new List<List<EightCardPokerCard>>();

			// process flush
			permutes.Clear();
			List<List<EightCardPokerCard>> flushGroupLists = _evaluateFlushGroups(cardCountInComp, _allPokerCards);

			// TO DO , need to reconside this, this will also include Minor/major joker as they are also flushable.
			var flushableJokerCards = _jokerCards.Where(card => card is IJokerFlushable).ToList();


			flushableJokerCards =
				flushableJokerCards.OrderByDescending(obj => ((JokerCard)obj).PokerCardPower).ToList();


			foreach (var gList in flushGroupLists)
			{
				var replacedSuit = gList[0].Suit;
				// insert numerbaleJokerCards. into the list, to fill whatever the gaps from ace, K, Q, to 3,2.
				var suitCompWithJokers =
					GetSuitCompWithJokers(gList, cardCountInComp, flushableJokerCards, replacedSuit);
				permutes.AddRange(suitCompWithJokers);
				
				permutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(gList, cardCountInComp));

			}

			// filter out flush straight as later, we will have dedicated method to collect flush straight?
			allComps.AddRange(ListsToPokerComp(permutes, PokerCompNameDict[(cardCountInComp, CompType.Flush)]));

			//--- Finally sort them out base on their CompPower order.
			//allComps = allComps.OrderByDescending(obj => obj.CompPower).ToList();
			allComps.Sort((x, y) => y.CompareTo(x));

			return allComps;
		}

		// Conisder multiple jokers on your hand.
		private void ProcessPermuteStraight(int straightCount, List<List<EightCardPokerCard>> kindGroupList,
			List<EightCardPokerCard> straightableJokerCards, List<List<EightCardPokerCard>> permutes,
			PokerSuit assignedSuit = PokerSuit.NoSuit)
		{

			// if we have ace kind group, we copy them in the bottom of kindgroup list and make all ace becomes "1" 
			// so that to let 3,2,1 straight become available.
			if (kindGroupList[0][0] is AceCard)
			{
				var newKindGroup = new List<EightCardPokerCard>();
				foreach (var ace in kindGroupList[0])
				{
					var newAce = EightCardPokerCard.CreateInstance(ace);
					((IJokerStraightable)newAce).SetStraightSub(1);
					newKindGroup.Add(newAce);
				}

				kindGroupList.Add(newKindGroup);
			}

			// For jokers Involved, we need another approach
			var jokerNeeded = straightCount - 1;
			for (var jokerCountInvolved = jokerNeeded; jokerCountInvolved >= 0; jokerCountInvolved--)
			{
				var pokerCardCountInvolved = straightCount - jokerCountInvolved;
				// 1. check if total jokers are over needed cards for straight, 
				// 2. check if pokerCard needed > actually involved card for straight. Ex: if jokerneed = 1, then pokerCardinvolved will be 2, 
				//    But there are only 1 card in the group, so not possible to form straight because cards are short.
				if (jokerCountInvolved > straightableJokerCards.Count || pokerCardCountInvolved > kindGroupList.Count)
					continue;
				
				var tempPermutes = UtilFunc.GetPermutation<List<EightCardPokerCard>>(kindGroupList, pokerCardCountInvolved);
				foreach (var cardPermute in tempPermutes)
				{
					var numberList = cardPermute.Select(group => group[0].Number).ToList();
					var gapNumbers = numberList
						.Zip(numberList.Skip(1), (a, b) => Enumerable.Range(b + 1, a - b - 1).Reverse())
						.SelectMany(g => g)
						.ToList();

					// Filter out all impossible cases that gap count are larger than jokerCount. In doing so, we made sure 
					// jokers can fulfill the gaps, and form straight.
					if (gapNumbers.Count > jokerCountInvolved) continue;

					// Now if the gapNumbers + regular PokerCard count (cardPermute.Count) < straightCount, we still need to find other gaps for jokers.
					// These other gaps are nor regular gap between poker Cards but the number=15 and number=1 pseudo boundary.
					// These extra boundary gaps from 15 to first number, and last number to 1.
					// if gapNumbers count + total poker Card count < straightCount, then we will add extra gap number
					// from 15 to first number, and last number to 1. 
					if (gapNumbers.Count + cardPermute.Count < straightCount)
					{
						// Add upper gap to boundary number 15, gap between first to 15 is Range(first+1, 15-first-1) <- see Range function
						gapNumbers.AddRange(Enumerable.Range(numberList[0] + 1,
							PokerConst.AceBigNumber + 1 - numberList[0] - 1));
						// add lower gap to boundary number 0, gap between 0 to last is Range(0+1, Last-0-1)
						gapNumbers.AddRange(Enumerable.Range(0 + 1, numberList.Last() - 0 - 1).Reverse());
					}

					// The gapNumbers come to here might be something like K, A, J, 10, etc... So we trim the required gap number
					// for joker first. In this example, K, A, and then sorted them. Since the reserved number are all neededs, so 
					// sorted them will not cause to include unwanted gap numbers. 
					gapNumbers = gapNumbers.GetRange(0, jokerCountInvolved).OrderByDescending(n => n).ToList();


					foreach (var pair in straightableJokerCards.Zip(
						         gapNumbers, (obj, num) => new { jokerCard = obj, Number = num }))
					{
						var newJoker = JokerCard.CreateInstance((JokerCard)pair.jokerCard);
						((IJokerStraightable)newJoker).SetStraightSub(pair.Number);
						if (assignedSuit != PokerSuit.NoSuit && newJoker is IJokerFlushable)
							((IJokerFlushable)newJoker).SetSuitSub(assignedSuit);
						cardPermute.Add(new List<EightCardPokerCard> { newJoker });
					}

					var groupPermuteWithJokers = cardPermute.OrderByDescending(objList => objList[0].Number).ToList();

					RecursivePermuteStraight(straightCount, groupPermuteWithJokers,
						new List<EightCardPokerCard>(), permutes);

				}
			}
		}

		
		private List<List<List<EightCardPokerCard>>> GetAllStraightCluster(int straightCount, List<List<EightCardPokerCard>> kindGroupList)
		{
			if (kindGroupList.Count == 0)
			{
				return new List<List<List<EightCardPokerCard>>>();
			} 
			// if we have ace kind group, we copy them in the bottom of kindgroup list and make all ace becomes "1" 
			// so that to let 3,2,1 straight become available.
			if (kindGroupList[0][0] is AceCard)
			{
				var newKindGroup = new List<EightCardPokerCard>();
				foreach (var ace in kindGroupList[0])
				{
					var newAce = EightCardPokerCard.CreateInstance(ace);
					((IJokerStraightable)newAce).SetStraightSub(1);
					newKindGroup.Add(newAce);
				}
				kindGroupList.Add(newKindGroup);
			}
			
			// Clustering numberGroups
			var numberClusters = kindGroupList
				.Aggregate(new List<List<List<EightCardPokerCard>>>(), (acc, numGroup) =>
				{
					if (acc.Count == 0 || acc.Last().Last()[0].Number - numGroup[0].Number != 1)
						acc.Add(new List<List<EightCardPokerCard>> { numGroup });
					else
						acc.Last().Add(numGroup);
					return acc;
				});
			
			//numberClusters contains various clusters. In each cluster, there are continuous of kind groups.
			// Filter out all qualified cluster number which is >= StraightCount, and also sorted with higher StraightCount
			var qualifiedClusters = 
				numberClusters.Where(cluster => cluster.Count >= straightCount)
				.OrderByDescending(x => x.Count).ToList();
			
			return qualifiedClusters;
		}
		
		

		// Evaluate the string is a bit of tricky. 
		// First step, evaluate call kind groups with at least 1 count, so the return looks like Ex: if cardCountInComp=3
		// (A-spade), (K-spade, K-club), (Q-Diamond, Q-club), (J-heart, J-Club), (10-Club).
		// Then we have (A-spade, K-spade, Q-Diamond), (A-spade, K-spade, Q-Club), (A-spade, K-club, Q-Diamond), 
		// (A-spade, K-club, Q-club) ...
		private static void RecursivePermuteStraight(int straightCount, List<List<EightCardPokerCard>> kindGroupList,
			List<EightCardPokerCard> currentList, List<List<EightCardPokerCard>> resultList)
		{

			if (currentList.Count + kindGroupList.Count < straightCount) return;

			if (currentList.Count >= straightCount)
			{
				// currentList is shared, so need to record at the moment when you added. If not, other hierarchy loop
				// will contaminate it.
				resultList.Add(new List<EightCardPokerCard>(currentList));
				return;
			}

			for (var i = 0; i < kindGroupList.Count; i++)
			{
				var kindGroup = kindGroupList[i];
				var remainingGroupList = kindGroupList.Skip(i + 1).ToList();

				// Need to optimize, why we need to check kindGroup.Count > 0, should it always > 0?
				if (currentList.Count > 0 && kindGroup.Count > 0)
				{
					if (!currentList.Last().IsNextNeighborNumber(kindGroup.First()))
					{
						break;
					}
				}

				foreach (var pokerCard in kindGroup)
				{
					currentList.Add(pokerCard);
					RecursivePermuteStraight(straightCount, remainingGroupList, currentList, resultList);
					// trace back by removing last element.
					currentList.RemoveAt(currentList.Count - 1);
				}
			}
		}


		private List<List<EightCardPokerCard>> _evaluateFlushGroups(int minCardCountInGroup, List<EightCardPokerCard> allPokerCards)
		{

			var sortedList = allPokerCards.OrderByDescending(item => item.PokerCardPower).ToList();

			var suitGroups = new List<List<EightCardPokerCard>>();


			// sort the Enum entry list by its associated values. also filter out other PokerSuit, and only 4 normal suits
			// are reserved.
			var sortedEnumValues = Enum.GetValues(typeof(PokerSuit))
				.Cast<PokerSuit>()
				.Where(e => (int)e <= (int)PokerSuit.Spade && (int)e >= (int)PokerSuit.Club)
				.OrderByDescending(e => (int)e)
				.ToList();

			// Loop through each PokerSuit, and collect all cards belong to related suit.
			foreach (var pokerSuit in sortedEnumValues)
			{
				//List <PokerCard> sameSuitCards = sortedList.FindAll(e => ((int)e.Suit & (int)pokerSuit) != 0 );
				List<EightCardPokerCard> sameSuitCards = sortedList.FindAll(e => e.Suit == pokerSuit);
				if (sameSuitCards.Count > 0)
				{
					suitGroups.Add(sameSuitCards);
				}
			}

			// now we have each suit group, sort them by number of members in each group. The larger count will be placed in front.
			// Ex: You have heart suit group with 4 cards will be placed in front of spade group with 3 cards.
			// Ex: (8-Heart, 5-Heart, 4-Heart, 3-Heart) -> (Ace-Spade, K-Spade, 9-Spade)
			//var sortedSubLists = _tempEvaluateData.SuitGroups.OrderByDescending(sublist => sublist.Count).ToList();
			var sortedSubLists = suitGroups.OrderByDescending(sublist => sublist.Count).ToList();

			return sortedSubLists.Where(subList => subList.Count >= minCardCountInGroup).ToList();

		}

		
		// search all possible "minCardCountInGroup" cards of kind group. Ex: minCardCountInGroup = 2, means at least 2 same cards, which is pair.
		private List<List<EightCardPokerCard>> GetNumberGroups_old(int minCardCountInGroup, List<EightCardPokerCard> noneJokerCards)
		{
			var rankGroupsDict = new Dictionary<int, List<EightCardPokerCard>>();

			var preSortedList = noneJokerCards.OrderByDescending(item => item.PokerCardPower).ToList();
			// Setp1 : place number in dictionary to achieve grouping concept.
			foreach (var card in preSortedList)
			{
				if (!rankGroupsDict.ContainsKey(card.Number))
					rankGroupsDict[card.Number] = new List<EightCardPokerCard>();

				rankGroupsDict[card.Number].Add(card);
			}

			// Step2 : in each group, each has same poker rank/number, but not in suit order, so that compare than in suit order.
			// sort kind group with suit colors, which means spade->heart->diamond->club
			foreach (var kvp in rankGroupsDict)
			{
				// reverse sort, (y compare x), so that higher is in front.
				kvp.Value.Sort((x, y) => y.CompareTo(x));
			}

			// Step3 : sort the dictionary key (which is rank/number), and convert to a list because the dictionary is hash-based and nor gurantee the order.	
			//var sortedList = rankGroupsDict.OrderByDescending(kv => kv.Value.Count)
			//	.ThenByDescending(kv => kv.Value[0].Number).ToList();
			var sortedList = rankGroupsDict.OrderByDescending(kv => kv.Value[0].Number).ToList();

			// step 4, only group member counts >= minCardCountInGroup
			return sortedList.Select(pair => pair.Value).Where(subList => subList.Count >= minCardCountInGroup)
				.ToList();
		}
		
		
		private List<List<EightCardPokerCard>> GetNumberGroups(int minCardCountInGroup, List<EightCardPokerCard> noneJokerCards)
		{
			// Use a dictionary to group cards by their number
			var rankGroupsDict = new Dictionary<int, List<EightCardPokerCard>>();

			// Avoid creating a new sorted list; group and sort in one pass
			foreach (var card in noneJokerCards)
			{
				if (!rankGroupsDict.TryGetValue(card.Number, out var group))
				{
					group = new List<EightCardPokerCard>();
					rankGroupsDict[card.Number] = group;
				}
				group.Add(card);
			}

			// Sort each group in-place to minimize memory allocations
			foreach (var group in rankGroupsDict.Values)
			{
				group.Sort((x, y) => y.CompareTo(x)); // Higher cards first
			}

			// Filter and sort the groups in one step, using yield to minimize intermediate lists
			return rankGroupsDict
				.Where(kv => kv.Value.Count >= minCardCountInGroup) // Filter groups
				.OrderByDescending(kv => kv.Key) // Sort by number descending
				.Select(kv => kv.Value) // Select the groups
				.ToList(); // Materialize the result as a list
		}
		

		private List<List<EightCardPokerCard>> GetKindGroups(int minCardCountInGroup, List<EightCardPokerCard> noneJokerCards)
		{
			var numberGroups = GetNumberGroups(minCardCountInGroup, noneJokerCards);
			numberGroups.Sort((x, y) => y.Count.CompareTo(x.Count));
			return numberGroups;
		}

		private EightCardsCompType EvaluteEightCardsCompType(int numCards, CompType CompType, int pairsInFlush = 0)
		{
			var keyStr = "";
			Dictionary<int, string> localDict = new Dictionary<int, string>{{1, "PairIn"}, {2, "TwoPairsIn"},  {3, "ThreePairsIn"}, {4, "FourPairsIn"}};
			keyStr = pairsInFlush == 0 ? $"{numCards}_{CompType.ToString()}" : 
				$"{numCards}_{localDict[pairsInFlush]}{CompType.ToString()}";
			var retCompType = EightCardsCompTypeDict.TryGetValue(keyStr, out var value) ? value : EightCardsCompType.None;
			
			return retCompType;
		}

		private void RecursiveEvaluateCards(List<EightCardPokerCard> remainingCards,
			PokerHandStructure currentHandStructure, List<PokerHandStructure> results)
		{

			var hasRank = false;

			var kindGroupList = GetNumberGroups(1, remainingCards);
			var flushGroups = _evaluateFlushGroups(_minFlushStraightCards, remainingCards);

			
			//1. Sort the number in each suit, try to find suit first and find straight by the way to see if we have flush Straight.
			foreach (var flushGroup in flushGroups)
			{
				for (var desiredCount = flushGroup.Count; desiredCount >= _minFlushStraightCards; desiredCount--)
				{
					var flushStraightPermutes = new List<List<EightCardPokerCard>>();
					var flushOnlyPermutes = new List<List<EightCardPokerCard>>();
					
					// To consider more general case for flush pairs, we need following codes to cluster them.
					//var wrapperListInList = flushGroup.Select(item => new List<PokerCard> { item }).ToList();
					var wrapperListInList = flushGroup
						.GroupBy(item => item) // Group by the item value
						.Select(group => group.ToList()) // Convert each group into a list
						.ToList(); //
					
					ProcessPermuteStraight(desiredCount, wrapperListInList, new List<EightCardPokerCard>(), flushStraightPermutes);

					if (flushStraightPermutes.Count > 0) { // Yes we have straight in suit group which implies @@flush-straight@@
						var handType = EvaluteEightCardsCompType(desiredCount, CompType.FlushStraight);
						if (handType == EightCardsCompType.None) continue;
						foreach (var permute in flushStraightPermutes)
						{
							var newHandCandidateData = new PokerCardComponent<EightCardsCompType>
								{ CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							var newRemainCards =
								UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveEvaluateCards(newRemainCards, currentHandStructure, results);
							currentHandStructure.RemoveLastComp();
							hasRank = true;
						}
					} else { // Yes we have straight in suit group which implies @@flush@@
						
						flushOnlyPermutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(flushGroup, desiredCount));
						
						foreach (var permute in flushOnlyPermutes)
						{
							int pairCount = permute
								.GroupBy(x => x)                // Group by item value
								.Count(g => g.Count() >= 2);
							
							var handType = EvaluteEightCardsCompType(desiredCount, CompType.Flush, pairCount);
							if (handType == EightCardsCompType.None) continue;
							var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							// TODO, if we have deck 2, then if we have 2 same J-spade, while remove one J-spade, will also remove the other becuase 
							// when do hash set, two J-spade will become single one.
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveEvaluateCards(newRemainCards, currentHandStructure, results);
							currentHandStructure.RemoveLastComp();
							hasRank = true;
						}
					}
				}
			}

			// 2. Sort majorly for straight
			var allStraightClusters = GetAllStraightCluster(_minFlushStraightCards, kindGroupList);
			foreach (var straightCluster in allStraightClusters)
			{
				//straightCluster is always a straight for at least _minFlushStraight count, we still need to loop through possible sub straight
				// Ex: we have 5 cards straights, 8,7,6,5,4, we still need to visit all sub straights, such as 3-card straight and 4 cards straight and also
				// full set of 5 cards straights.
				for (var targetSCount = straightCluster.Count; targetSCount >= _minFlushStraightCards; targetSCount--)
				{
					for (int selectID = 0; selectID <= straightCluster.Count - targetSCount; selectID++)
					{
						var targetStraightCluster = straightCluster.GetRange(selectID, targetSCount);
						var allPermutes = new List<List<EightCardPokerCard>>();
						var handType = EvaluteEightCardsCompType(targetStraightCluster.Count, CompType.Straight);
						if (handType == EightCardsCompType.None) continue;
						RecursivePermuteStraight(targetStraightCluster.Count, targetStraightCluster, new List<EightCardPokerCard>(), allPermutes);
						foreach (var permute in allPermutes)
						{
							var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveEvaluateCards(newRemainCards, currentHandStructure, results);
							currentHandStructure.RemoveLastComp();
						}		
					}		
				}
				hasRank = true;
			}
			
			// 3. Get all kinds group to performance any pair or threeOFkind or fourOFkind, etc..
			var allKindGroups = GetKindGroups(2, remainingCards);
			
			//for (var groupNum = allKindGroups.Count; groupNum >= 1; groupNum--)
			foreach(var kindGroup in allKindGroups)
			{
				for (var groupCardNum = kindGroup.Count; groupCardNum >= 2 ; groupCardNum--) {
					//var allPermutes = new List<List<PokerCard>>();
					var handType = EvaluteEightCardsCompType(groupCardNum, CompType.Kind);
					var allPermutes = UtilFunc.GetPermutation<EightCardPokerCard>(kindGroup, groupCardNum);
					foreach (var permute in allPermutes)
					{
						var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
						currentHandStructure.AddComp(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
						RecursiveEvaluateCards(newRemainCards, currentHandStructure, results);
						currentHandStructure.RemoveLastComp();
					}
				}
				hasRank = true;
			}
			
			// When code comes here, it means there are nothing else worthy to record, so that put all current into Results.
			if (hasRank == false && currentHandStructure.Components.Count > 0)
			{
				results.Add(new PokerHandStructure(currentHandStructure));
			}
		}

		private bool ArrangeFlushOrFlushStraight(List<List<EightCardPokerCard>> flushGroups, List<EightCardPokerCard> remainingCards,
            PokerHandStructure currentHandStructure, List<PokerHandStructure> results, bool hasRank)
        {
			foreach (var flushGroup in flushGroups)
			{
				for (var desiredCount = flushGroup.Count; desiredCount >= _minFlushStraightCards; desiredCount--)
				{
					var flushStraightPermutes = new List<List<EightCardPokerCard>>();
					var flushOnlyPermutes = new List<List<EightCardPokerCard>>();
					
					// To consider more general case for flush pairs, we need following codes to cluster them.
					//var wrapperListInList = flushGroup.Select(item => new List<PokerCard> { item }).ToList();
					var wrapperListInList = flushGroup
						.GroupBy(item => item) // Group by the item value
						.Select(group => group.ToList()) // Convert each group into a list
						.ToList(); //
					
					ProcessPermuteStraight(desiredCount, wrapperListInList, new List<EightCardPokerCard>(), flushStraightPermutes);

					if (flushStraightPermutes.Count > 0) { // Yes we have straight in suit group which implies @@flush-straight@@
						var handType = EvaluteEightCardsCompType(desiredCount, CompType.FlushStraight);
						if (handType == EightCardsCompType.None) continue;
						foreach (var permute in flushStraightPermutes)
						{
							var newHandCandidateData = new PokerCardComponent<EightCardsCompType>
								{ CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							var newRemainCards =
								UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveEvaluateCards(newRemainCards, currentHandStructure, results);
							currentHandStructure.RemoveLastComp();
							hasRank = true;
						}
					} else { // Yes we have straight in suit group which implies @@flush@@

						flushOnlyPermutes.AddRange(UtilFunc.GetPermutation<EightCardPokerCard>(flushGroup, desiredCount));
						
						
						foreach (var permute in flushOnlyPermutes)
						{
							int pairCount = permute
								.GroupBy(x => x)                // Group by item value
								.Count(g => g.Count() >= 2);
							
							var handType = EvaluteEightCardsCompType(desiredCount, CompType.Flush, pairCount);
							if (handType == EightCardsCompType.None) continue;
							var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							// TODO, if we have deck 2, then if we have 2 same J-spade, while remove one J-spade, will also remove the other becuase 
							// when do hash set, two J-spade will become single one.
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveArrangeHands(newRemainCards, currentHandStructure, results);
							currentHandStructure.RemoveLastComp();
							hasRank = true;
						}
					}
				}
			}
            return hasRank;
        }
		
		
		private bool ArrangeStraightComps(List<List<List<EightCardPokerCard>>> allStraightClusters, List<EightCardPokerCard> remainingCards,
            PokerHandStructure currentHandCandidates, List<PokerHandStructure> results, bool hasRank)
        {
          
            foreach (var straightCluster in allStraightClusters)
            {
                //straightCluster is always a straight for at least _minFlushStraight count, we still need to loop through possible sub straight
                // Ex: we have 5 cards straights, 8,7,6,5,4, we still need to visit all sub straights, such as 3-card straight and 4 cards straight and also
                // full set of 5 cards straights.
                for (var targetSCount = straightCluster.Count; targetSCount >= _minFlushStraightCards; targetSCount--)
                {
                    for (int selectID = 0; selectID <= straightCluster.Count - targetSCount; selectID++)
                    {
                        var targetStraightCluster = straightCluster.GetRange(selectID, targetSCount);
                        var allPermutes = new List<List<EightCardPokerCard>>();
                        var handType = EvaluteEightCardsCompType(targetStraightCluster.Count, CompType.Straight);
                        if (handType == EightCardsCompType.None) continue;
                        RecursivePermuteStraight(targetStraightCluster.Count, targetStraightCluster, new List<EightCardPokerCard>(), allPermutes);
                        foreach (var permute in allPermutes)
                        {
                            var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
                            currentHandCandidates.AddComp(newHandCandidateData);
                            var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
                            RecursiveArrangeHands(newRemainCards, currentHandCandidates, results);
                            currentHandCandidates.RemoveLastComp();
                        }
                    }
                }
                hasRank = true;
            }
            
            return hasRank;
        }
		
		private bool ArrangeKindComps(List<List<EightCardPokerCard>> allKindGroups, List<EightCardPokerCard> remainingCards,
			PokerHandStructure currentHandCandidates, List<PokerHandStructure> results, bool hasRank)
		{
			foreach (var kindGroup in allKindGroups)
			{
				for (var groupCardNum = kindGroup.Count; groupCardNum >= 2; groupCardNum--)
				{
					//var allPermutes = new List<List<PokerCard>>();
					var handType = EvaluteEightCardsCompType(groupCardNum, CompType.Kind);
					var allPermutes = UtilFunc.GetPermutation<EightCardPokerCard>(kindGroup, groupCardNum);
					foreach (var permute in allPermutes)
					{
						var newHandCandidateData = new PokerCardComponent<EightCardsCompType> { CompRank = handType, Cards = permute };
						currentHandCandidates.AddComp(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
						RecursiveArrangeHands(newRemainCards, currentHandCandidates, results);
						currentHandCandidates.RemoveLastComp();
					}
				}
				hasRank = true;
			}
			return hasRank;
		}
		
		private void RecursiveArrangeHands(List<EightCardPokerCard> remainingCards,
			PokerHandStructure currentHandCandidates, List<PokerHandStructure> results)
		{
			var hasRank = false;
			
			var kindGroupList = GetNumberGroups(1, remainingCards);
			var flushGroups = _evaluateFlushGroups(_minFlushStraightCards, remainingCards);
			
			//1. Sort the number in each suit, try to find suit first and find straight by the way to see if we have flush Straight.
			hasRank = ArrangeFlushOrFlushStraight(flushGroups, remainingCards, currentHandCandidates, results, hasRank);
			
			// 2. Sort majorly for straight
			var allStraightClusters = GetAllStraightCluster(_minFlushStraightCards, kindGroupList);
			hasRank = ArrangeStraightComps(allStraightClusters, remainingCards, currentHandCandidates, results, hasRank);
			
			// 3. Get all kinds group to performance any pair or threeOFkind or fourOFkind, etc.
			var allKindGroups = GetKindGroups(2, remainingCards);
			hasRank = ArrangeKindComps(allKindGroups, remainingCards, currentHandCandidates, results, hasRank);
			
			// When code comes here, it means there are nothing else worthy to record, so that put all current into Results.
			if (hasRank == false && currentHandCandidates.Components.Count > 0)
			{
				var newCandidateComps = new PokerHandStructure(currentHandCandidates);
				newCandidateComps.SetRemainingCards(remainingCards);
				results.Add(newCandidateComps);
			}
		}
		
	}
}





		// Select all possible group from input cardCount in each group.
		// For 3 cards, we can have 3 of kind, 3 cards straight, 3 cards of flush and 3 cards flush straight. etc..
		// For 4 cards, we can have 4 of kind, 4 cards of straight, 4 cards of flush and 4 cards of flush straight.
		// For 5 cards, we can have potential 5 of kind, five card straight, five card of flush, and 

		/*public void RankAndSort() {

			InitTempEvaluateData();
			_allPokerCards.Sort(AceBigCardCompare);
			RecursiveEvaluateCards(_allPokerCards);
			foreach (var card in rankSortedCards) {
				card.PrintCard();
			}

			for (var i = 0; i < rankSortedCards.Count; i++) {
				transform.Find(rankSortedCards[i].CardStr).transform.SetSiblingIndex(i);
			}

			//var handRankObj = transform.parent.transform.Find("HandRank").gameObject;
			//handRankObj.GetComponent<Text>().text = _bestRank.ToString();
		}*/




		/*
		private void EvaluateKinds(List <PokerCard> runCards) {

			for (int number = 14; number >=2; number-- ) {
				List <PokerCard> groupCards = runCards.FindAll(e => e.Number == number);
				if (groupCards.Count>1) {
					_tempEvaluateData.KindGroups.Add(groupCards);
				}
			}

			_tempEvaluateData.KindGroups.Sort(delegate(List <PokerCard> x, List <PokerCard> y)
			{
				if (x.Count == y.Count)  return x[0].CompareToAceBig(y[0]);
				else if (x.Count < y.Count) return 1;
				else return -1;
			});


			// According to group, determine how many paris, 3 kinds, etc.. to turn on associate bits.
			foreach (var group in _tempEvaluateData.KindGroups)
			{
				switch (group.Count)
				{
					case 2:
						_tempEvaluateData.PairCnt += 1;
						_tempEvaluateData.RankTypeBits |= (byte) PokerBitTypes.BitPair;
						_tempEvaluateData.KindCards.AddRange(group);
						Debug.Log("Evalute Kinds Count = Get into pairs");
						break;
					case 3:
						_tempEvaluateData.ThreeOfKindCnt += 1;
						_tempEvaluateData.RankTypeBits |= (byte) PokerBitTypes.BitThreeOfKind;
						_tempEvaluateData.KindCards.AddRange(group);
						Debug.Log("Evalute Kinds Count = Get into three od kind");
						break;
					case 4:
						_tempEvaluateData.FourOfKindCnt += 1;
						_tempEvaluateData.RankTypeBits |= (byte) PokerBitTypes.BitFourOfKind;
						_tempEvaluateData.KindCards.AddRange(group);
						break;
					case 5:
						_tempEvaluateData.FiveOfKindCnt += 1;
						_tempEvaluateData.RankTypeBits |= (byte) PokerBitTypes.BitFiveOfKind;
						_tempEvaluateData.KindCards.AddRange(group);
						break;
					case 1:
						_tempEvaluateData.NoneKindCards.AddRange( group);
						break;
					default:
						break;
				}
			}
		}*/

		/*
		private List <PokerCard> CheckStraight(List <PokerCard> runCards) {
			List <PokerCard> retStraightCards = new List <PokerCard>();

			if (runCards.Count <= 3) {
				return retStraightCards;
			}

			var list1 = runCards.GetRange(0, runCards.Count-1);
			var list2 = runCards.GetRange(1, runCards.Count-1);

			Debug.Log("From CheckStraight : run card count " + runCards.Count);
			Debug.Log("From CheckStraight : List1 count " + list1.Count);
			Debug.Log("From CheckStraight : List2 count " + list2.Count);

			int consectiveNumber = 0;

			foreach (var pair in list1.Zip(list2, (a, b) => new {card1 = a, card2 = b})) {
				int number1 = pair.card1.Number;
				int number2 = pair.card2.Number;
				if (number1 - number2 == 1) {
					consectiveNumber ++;
					if (retStraightCards.Count == 0) {
						retStraightCards.Add(pair.card1);
						retStraightCards.Add(pair.card2);
					} else {
						retStraightCards.Add(pair.card2);
					}
				} else {
					// larger than 3 meaning at least four card straight.
					if (consectiveNumber >= 3) {
						// break directly
						break;
					} else {
						consectiveNumber = 0;
						retStraightCards.Clear();
					}
				}
			}
			Debug.Log("From CheckStraight : " + retStraightCards.Count);
			foreach (var card in retStraightCards) {
				card.PrintCard();
			}
			return retStraightCards;
		}
	    */

/*

	private List<PokerCard> FillNumberGapWithJokers_old(List<PokerCard> inputCards, List<PokerCard> jokers, PokerSuit subSuit)
	{
		var cards = new List<PokerCard>(inputCards);

		while (jokers.Count > 0)
		{
			// Find the first occurrence of the gap and include index
			var searchGapResult = inputCards
				.Select((value, index) => new { Value = value, Index = index }) // Attach index
				.Zip(inputCards.Skip(1).Select((value, index) => new { Value = value, Index = index + 1 }),
					(current, next) => new { Current = current, Next = next })
				.FirstOrDefault(pair => pair.Current.Value.Number - pair.Next.Value.Number > 1);

			if (searchGapResult != null) {
				var fillSpace = searchGapResult.Current.Value.Number-searchGapResult.Next.Value.Number-1;
				var insertJokerCardCount = fillSpace > jokers.Count ? jokers.Count : fillSpace;

				// Pop out needed JokerCardCount from jokers.
				//var fillListFromJokers = jokers.Take(insertJokerCardCount).ToList();
				var fillListFromJokers = new List<PokerCard>();
				for (var i = 0; i < insertJokerCardCount; i++)
				{
					var newJoker = JokerCard.CreateInstance((JokerCard)jokers[i]);
					((IJokerFlushable)newJoker).SetSuitSub(subSuit);
					var replacedNumber = searchGapResult.Current.Value.Number - 1 - i;
					((IJokerStraightable)newJoker).SetStraightSub(replacedNumber);
					fillListFromJokers.Add(newJoker);
				}
				jokers.RemoveRange(0, insertJokerCardCount);

				// InsertRange was insert a sublist in front of this index
				cards.InsertRange(searchGapResult.Next.Index, fillListFromJokers);
			} else {
				// Normally, the code will not run to here as the joker consumed up to fill the gap before all the gap are filled.
				break;
			}
		}

		return cards;
	}
	*/

/*
private void Awake() {

}

// Start is called before the first frame update
private void Start()
{


}
*/

/*
		for (var loopCardNumber = PokerCard.AceBigNumber; loopCardNumber >= straightCount; loopCardNumber--)
		{
			var adjustKindGroupList = new List<List<PokerCard>>();
			var newJokerCards = new List<PokerCard>(straightableJokerCards);
			// The idea here is to scan each straightCount, ex: 3 cards.
			// If any number exist in regular pokerCard, fill that in the adjustKindGroupList, if not fill jokerCards.
			for (var cardNumber = loopCardNumber ; cardNumber > loopCardNumber-straightCount; cardNumber--)
			{
				if (numberToKindGroupDict.ContainsKey(cardNumber)) {
					adjustKindGroupList.Add(numberToKindGroupDict[cardNumber]);
				} else {
					if (newJokerCards.Count != 0) // If we still have straightable joker Card remains, fill in the list
					{
						// we need to create new jokerCard as dummy and change its number. If we do not use new copy, and share
						// the jokerCard and change its number, this number will be messed up when do the Comp sorting for same straight.
						var newJokerCard = JokerCard.CreateInstance((JokerCard)newJokerCards[0]);
						((IJokerStraightable)newJokerCard).SetStraightSub(cardNumber);
						adjustKindGroupList.Add(new List<PokerCard> { newJokerCard });
						newJokerCards.RemoveAt(0);
					}
				}
			}

			// only the Count fullfill the straight Count Will suffice the straight. Sometimes, the adjustKindGroupList has enough
			// pokerCard combining with jokerCards. Ex: 1 poker Card and 1 joker, total cards = 2,  will not suffice for 3 cards straight.
			if (adjustKindGroupList.Count == straightCount)
			{
				RecursivePermuteStraight(straightCount, adjustKindGroupList,
					new List<PokerCard>(),  permutes);
			}
		}
		*/


/*
		// step 1 , convert a kindGroupList into a dictionary.
		// Map to sorted dictionary
		var numberToKindGroupDict = kindGroupList
			.ToDictionary(group => group.First().Number, group => group);

		// step2, since 5,4,3,2,A can also be a straight, and in step1, we assume A is 14, so also copy A's kind group
		// as "1" in the last group.
		if (numberToKindGroupDict.ContainsKey(PokerCard.AceBigNumber))
			numberToKindGroupDict[1] = new List<PokerCard>(numberToKindGroupDict[PokerCard.AceBigNumber]);

		// Scan all possible straight from A-K-Q, K-Q-J, Q-J-10, all the way to 3-2-A. This Scan strategy is for none_jokers only.
		// For jokers involved we need different approaches.
		for (var loopCardNumber = PokerCard.AceBigNumber; loopCardNumber >= straightCount; loopCardNumber--)
		{
			var adjustKindGroupList = new List<List<PokerCard>>();
			var newJokerCards = new List<PokerCard>(straightableJokerCards);
			// The idea here is to scan each straightCount, ex: 3 cards.
			// If any number exist in regular pokerCard, fill that in the adjustKindGroupList, if not fill jokerCards.
			for (var cardNumber = loopCardNumber ; cardNumber > loopCardNumber-straightCount; cardNumber--)
			{
				if (numberToKindGroupDict.TryGetValue(cardNumber, out var value)) {
					adjustKindGroupList.Add(value);
				}
			}

			// only the Count fulfill the straight Count Will suffice the straight. Sometimes, the adjustKindGroupList has enough
			// pokerCard combining with jokerCards. Ex: 1 poker Card and 1 joker, total cards = 2,  will not suffice for 3 cards straight.
			if (adjustKindGroupList.Count == straightCount)
			{
				RecursivePermuteStraight(straightCount, adjustKindGroupList,
					new List<PokerCard>(),  permutes);
			}
		}
		*/
		
		
		/*
		private void getMaxPossibleStraightCount(List<List<PokerCard>> kindGroupList)
		{
			var representedCards = kindGroupList.Select(group => group[0]).ToList();
			
			// if we have ace kind group, we copy them in the bottom of kindgroup list and make all ace becomes "1" 
			// so that to let 3,2,1 straight become available.
			if (representedCards[0] is AceCard)
			{
				var newAce = PokerCard.CreateInstance(representedCards[0]);
				((IJokerStraightable)newAce).SetStraightSub(1);
				representedCards.Add(newAce);
			}
			
			var numberList = representedCards.Select(group => group.Number).ToList();
			var gapNumbers = numberList
				.Zip(numberList.Skip(1), (a, b) => Enumerable.Range(b + 1, a - b - 1).Reverse())
				.SelectMany(g => g)
				.ToList();
			
		}*/
		
		
		/*
		//2. Sort majorly for Straight
		if (_tempEvaluateData.NumberGroups.Count >=3 ) {
			Debug.Log("+++++++++" + _tempEvaluateData.NumberGroups.Count );
			var List1 = _tempEvaluateData.NumberGroups.GetRange(0, _tempEvaluateData.NumberGroups.Count-1);
			var List2 = _tempEvaluateData.NumberGroups.GetRange(1, _tempEvaluateData.NumberGroups.Count-1);

			int consectiveNumber = 0;
			List <PokerCard> straightColllectedCards = new List <PokerCard>();
			foreach (var pair in List1.Zip(List2, (a, b) => new {cardList1 = a, cardList2 = b})) {
				int number1 = pair.cardList1[0].Number;
				int number2 = pair.cardList2[0].Number;
				Debug.Log("-----  Straight log num1, num2 = " + number1 + " , " + number2 );
				if (number1 - number2 == 1) {
					consectiveNumber ++;
					// Always get number group last element as it's less suit power to use them to compose a straight
					if (straightColllectedCards.Count == 0) {
						straightColllectedCards.Add(pair.cardList1.Last());
						straightColllectedCards.Add(pair.cardList2.Last());
					} else {
						straightColllectedCards.Add(pair.cardList2.Last());
					}
				} else {
					if (consectiveNumber >=3) {
						break;
					} else {
						consectiveNumber = 0;
						straightColllectedCards.Clear();
					}
				}
			}
			if (consectiveNumber == 3) {
				newCandidate.HandType = PokerRankTypes.FourCardStraight;
				newCandidate.Cards = straightColllectedCards;
				Debug.Log("Get tino Straight  ==4");
				handCandidates.Add(newCandidate);
			} else if (consectiveNumber >= 4) {
				newCandidate.HandType = PokerRankTypes.Straight;
				newCandidate.Cards = straightColllectedCards;
				Debug.Log("Get tino Straight  >=5");
				handCandidates.Add(newCandidate);
			}
		}
*/

/*
			for (var desiredCount = 8; desiredCount >= 3; desiredCount--)
			{
				var permutes = new List<List<PokerCard>>();
				//var kindGroupLists = _evaluateKindGroups(1, _noneJokerCards);
				//var straightableJokerCards = _jokerCards.Where(card => card is IJokerStraightable).ToList();
			
				ProcessPermuteStraight(cardCountInComp, kindGroupList, new List<PokerCard>(), permutes);
				
			}

			


			//--- Finally sort them out base on their CompPower order.
			allComps.Sort((x, y) => y.CompareTo(x));
			var output = "";
			foreach (var Comp in allComps)
			{
				output += $"\"{Comp.CompString}\",";
			}

			return allComps;
			
			
			
			for (var desiredCount = flushGroup.Count; desiredCount >= 3; desiredCount--)
			{

				var flushStraightPermutes = new List<List<PokerCard>>();
				var flushOnlyPermutes = new List<List<PokerCard>>();
				var wrapperListInList = flushGroup.Select(item => new List<PokerCard> { item }).ToList();
				ProcessPermuteStraight(desiredCount, wrapperListInList, new List<PokerCard>(),
					flushStraightPermutes);

				if (flushStraightPermutes.Count >
				    0) // Yes we have straight in suit group which implies flush straight
				{
					var handType = EightCardsCompTypeDict[(desiredCount, CompType.FlushStraight)];
					foreach (var permute in flushStraightPermutes)
					{
						var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
						currentHandCandidates.Add(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
						RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
						currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
						hasRank = true;
					}
				}
				else
				{
					var handType = EightCardsCompTypeDict[(desiredCount, CompType.Flush)];
					UtilFunc.RecursivePermute<PokerCard>(flushGroup, desiredCount, new List<PokerCard>(),
						flushOnlyPermutes);
					foreach (var permute in flushOnlyPermutes)
					{
						var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
						currentHandCandidates.Add(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
						RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
						currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
						hasRank = true;
					}
				}

			}*/
			
						/*		
			// 3. Process Kind groups
			if (_tempEvaluateData.FiveOfKindCnt >= 1) {
				newCandidate.HandType = PokerRankTypes.FiveOfKind;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				handCandidates.Add(newCandidate);
			} else if (_tempEvaluateData.FourOfKindCnt >= 1) {
				newCandidate.HandType = PokerRankTypes.FiveOfKind;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				handCandidates.Add(newCandidate);
			} else if (_tempEvaluateData.ThreeOfKindCnt >= 1 && _tempEvaluateData.PairCnt >= 1) {
				newCandidate.HandType = PokerRankTypes.FullHouse;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				foreach (var group in _tempEvaluateData.KindGroups) {
					if (group.Count == 2) {
						newCandidate.Cards.AddRange(group);
						break;
					}
				}
				handCandidates.Add(newCandidate);
				Debug.Log("fullhouse count = " + newCandidate.Cards.Count);
			} else if (_tempEvaluateData.ThreeOfKindCnt >= 1) {
				newCandidate.HandType = PokerRankTypes.ThreeOfKind;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				handCandidates.Add(newCandidate);
			} else if (_tempEvaluateData.PairCnt >= 2) {
				newCandidate.HandType = PokerRankTypes.TwoPairs;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				newCandidate.Cards.AddRange(_tempEvaluateData.KindGroups[1]);
				handCandidates.Add(newCandidate);
			} else if (_tempEvaluateData.PairCnt == 1) {
				newCandidate.HandType = PokerRankTypes.OnePair;
				newCandidate.Cards = _tempEvaluateData.KindGroups[0];
				handCandidates.Add(newCandidate);
			}


			handCandidates.Sort((t1, t2) => t2.HandType.CompareTo(t1.HandType));

			if (handCandidates.Count > 0) {
				Debug.Log("The best rank = " + handCandidates[0].HandType.ToString() + " Count = " + handCandidates[0].Cards.Count);

				rankSortedCards.AddRange(handCandidates[0].Cards);
				HashSet <PokerCard> determinedSet = new HashSet<PokerCard>();
				foreach (var card in handCandidates[0].Cards) {
					determinedSet.Add(card);
				}
				HashSet <PokerCard> remainingSet = new HashSet<PokerCard>();
				foreach (var card in remainingCards) {
					remainingSet.Add(card);
				}

				remainingSet.ExceptWith(determinedSet);

				RecursiveEvaluateCards(new List<PokerCard>(remainingSet));

				if (handCandidates[0].HandType > _bestRank) {
					_bestRank = handCandidates[0].HandType;
				}
			} else {
				// Nothing in rank, quit the recursive loop and report the hand has nothing.
				Debug.Log("The best rank = Nothing.");
				rankSortedCards.AddRange(remainingCards);
			}
			*/

/*
	private void RecursiveEvaluateCards(List<PokerCard> remainingCards,
			List<HandCandidateData> currentHandCandidates, List<List<HandCandidateData>> results)
		{

			var hasRank = false;

			var kindGroupList = GetNumberGroups(1, remainingCards);
			var flushGroups = _evaluateFlushGroups(3, remainingCards);

			
			//1. Sort the number in each suit, try to find suit first and find straight by the way to see if we have flush Straight.
			foreach (var flushGroup in flushGroups)
			{

				for (var desiredCount = flushGroup.Count; desiredCount >= 3; desiredCount--)
				{
					var flushStraightPermutes = new List<List<PokerCard>>();
					var flushOnlyPermutes = new List<List<PokerCard>>();
					var wrapperListInList = flushGroup.Select(item => new List<PokerCard> { item }).ToList();
					ProcessPermuteStraight(desiredCount, wrapperListInList, new List<PokerCard>(),
						flushStraightPermutes);

					if (flushStraightPermutes.Count >
					    0) // Yes we have straight in suit group which implies flush straight
					{
						var handType = EightCardsCompTypeDict[(desiredCount, CompType.FlushStraight)];
						foreach (var permute in flushStraightPermutes)
						{
							var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
							currentHandCandidates.Add(newHandCandidateData);
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
							RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
							currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
							hasRank = true;
						}
					}
					else
					{
						var handType = EightCardsCompTypeDict[(desiredCount, CompType.Flush)];
						UtilFunc.RecursivePermute<PokerCard>(flushGroup, desiredCount, new List<PokerCard>(),
							flushOnlyPermutes);
						foreach (var permute in flushOnlyPermutes)
						{
							var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
							currentHandCandidates.Add(newHandCandidateData);
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
							RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
							currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
							hasRank = true;
						}
					}

				}
			}

			// 2. Sort majorly for straight
			var allStraightClusters = GetAllStraightCluster(3, kindGroupList);
			foreach (var straightCluster in allStraightClusters)
			{
				var allPermutes = new List<List<PokerCard>>();
				var handType = EightCardsCompTypeDict[(straightCluster.Count, CompType.Straight)];
				RecursivePermuteStraight(straightCluster.Count, straightCluster, new List<PokerCard>(), allPermutes);
				foreach (var permute in allPermutes)
				{
					var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
					currentHandCandidates.Add(newHandCandidateData);
					var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
					RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
					currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
				}
				hasRank = true;
			}
			
			// 3. Get all kinds group to performance any pair or threeOFkind or fourOFkind, etc..
			var allKindGroups = GetKindGroups(2, remainingCards);
			
			//for (var groupNum = allKindGroups.Count; groupNum >= 1; groupNum--)
			foreach(var kindGroup in allKindGroups)
			{
				for (var groupCardNum = kindGroup.Count; groupCardNum >= 2 ; groupCardNum--) {
					var allPermutes = new List<List<PokerCard>>();
					var handType = EightCardsCompTypeDict[(groupCardNum, CompType.Kind)];
					UtilFunc.RecursivePermute<PokerCard>(kindGroup, groupCardNum, new List<PokerCard>(),
						allPermutes);
					foreach (var permute in allPermutes)
					{
						var newHandCandidateData = new HandCandidateData { HandType = handType, Cards = permute };
						currentHandCandidates.Add(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute);
						RecursiveEvaluateCards(newRemainCards, currentHandCandidates, results);
						currentHandCandidates.RemoveAt(currentHandCandidates.Count - 1);
					}
				}
				hasRank = true;
			}
			
			// Process Three of Kind.
			
			
			// Process pairs.
			
			
			
			
			// When code comes here, it means there are nothing else worthy to record, so that put all current into Results.
			if (hasRank == false)
			{
				results.Add(new List<HandCandidateData>(currentHandCandidates));
			}
		}
*/


/*public bool Equals(CandidateComps x, CandidateComps y)
{
	foreach (var (Comp1, Comp2) in x.Comps.Zip(y.Comps, (a, b) => (a, b)))
	{
		bool equRes = Comp1.Equals(Comp2);
		if (equRes == false) return false;
	}
	return true;
}*/
		
/*public override bool Equals(CandidateComps another)
{

	return true;
}
*/

//var handStrings = new List<string>();
/*
foreach (var result in retResults)
{
	var CompTypeCountDict = new Dictionary<EightCardsCompType, int>();
	foreach (var handCandidate in result)
	{
		if (CompTypeCountDict.ContainsKey(handCandidate.HandType))
		{
			CompTypeCountDict[handCandidate.HandType] += 1;
		}
		else
		{
			CompTypeCountDict[handCandidate.HandType] = 1;
		}
	}

	var sortedCompTypeCountDict = CompTypeCountDict
		.OrderBy(kvp => kvp.Key) // Keys are enums, naturally sorted by declaration order
		.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	var resultString = string.Join(
		",",
		sortedCompTypeCountDict.Select(kvp => $"{kvp.Key}_{kvp.Value}")
	);
	handStrings.Add(resultString);
}
*/
//var uniqueStrings = handStrings.Distinct().ToList();


/*
	public struct HandCandidateData : IComparable<HandCandidateData>, IEqualityComparer<HandCandidateData>
	{
		public EightCardsCompType HandType;

		// Reset all temporary variables
		public List<PokerCard> Cards;
		public int CompareTo(HandCandidateData other)
		{
			throw new NotImplementedException();
		}

		public bool Equals(HandCandidateData x, HandCandidateData y)
		{
			throw new NotImplementedException();
		}

		public int GetHashCode(HandCandidateData obj)
		{
			throw new NotImplementedException();
		}
	};
	*/

/*
		private void InitTempEvaluateData()
		{
			_tempEvaluateData.KindGroups = new List<List<PokerCard>>();
			_tempEvaluateData.SuitGroups = new List<List<PokerCard>>();
			_tempEvaluateData.NumberGroups = new List<List<PokerCard>>();
			_tempEvaluateData.KindCards = new List<PokerCard>();
			_tempEvaluateData.NoneKindCards = new List<PokerCard>();
			_tempEvaluateData.PairCnt = 0;
			_tempEvaluateData.ThreeOfKindCnt = 0;
			_tempEvaluateData.FourOfKindCnt = 0;
			_tempEvaluateData.FiveOfKindCnt = 0;
			_tempEvaluateData.RankTypeBits = 0;
			rankSortedCards = new List<PokerCard>();
		}*/



						/*
						private void EvaluateStraightAndFlush(List<PokerCard> runCards)
						{

							foreach (PokerSuit pokerSuit in Enum.GetValues(typeof(PokerSuit)))
							{
								List<PokerCard> sameSuitCards = runCards.FindAll(e => e.Suit == pokerSuit);
								if (sameSuitCards.Count > 3)
								{
									_tempEvaluateData.SuitGroups.Add(sameSuitCards);
								}
							}

							// Sort with any suit with most cards.
							_tempEvaluateData.SuitGroups.Sort(delegate(List<PokerCard> x, List<PokerCard> y)
							{
								if (x.Count == y.Count) return 0;
								else if (x.Count > y.Count) return 1;
								else return -1;
							});


							//2. preparating data for straight with AceBig
							for (var number = 14; number >= 1; number--)
							{
								List<PokerCard> sameNumberCards = runCards.FindAll(e => e.Number == number);
								if (sameNumberCards.Count > 0)
								{
									//Debug.Log("YYY number sorted: " + sameNumberCards[0].number);
									_tempEvaluateData.NumberGroups.Add(sameNumberCards);
								}
							}

						}*/



						/*
						public struct HandProcessData
						{
							// Reset all temporary variables
							public List<List<PokerCard>> KindGroups;
							public List<List<PokerCard>> SuitGroups;

							// collect number groups from 1 to 14, ex: you have 2,5,5,5,7 then you will have single member group of 2,
							// and 3 members of group of number 5, and single member group of 7.
							// This variable is majorly facilitate the straight extraction.
							public List<List<PokerCard>> NumberGroups;

							public List<PokerCard> KindCards;
							public List<PokerCard> NoneKindCards;

							public int PairCnt;
							public int ThreeOfKindCnt;
							public int FourOfKindCnt;
							public int FiveOfKindCnt;
							public byte RankTypeBits;
							//public List <HandCandidateData> handCandidates;

						};
					*/

/*
		private void ResetEvaluateSet()
		{

			foreach (var t in _tempEvaluateData.KindGroups)
			{
				t.Clear();
			}

			_tempEvaluateData.KindGroups.Clear();

			foreach (var t in _tempEvaluateData.SuitGroups)
			{
				t.Clear();
			}

			_tempEvaluateData.SuitGroups.Clear();

			foreach (var t in _tempEvaluateData.NumberGroups)
			{
				t.Clear();
			}

			_tempEvaluateData.NumberGroups.Clear();

			_tempEvaluateData.KindCards.Clear();
			_tempEvaluateData.NoneKindCards.Clear();

			_tempEvaluateData.PairCnt = 0;
			_tempEvaluateData.ThreeOfKindCnt = 0;
			_tempEvaluateData.FourOfKindCnt = 0;
			_tempEvaluateData.FiveOfKindCnt = 0;
			_tempEvaluateData.RankTypeBits = 0;
		}*/

