using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using GenericPoker.EightCard;
using GenericPoker;

namespace GenericPoker.SevenCard
{
	public class HandAnalyzer
	{
		
		private readonly List<EightCardPokerCard> _allPokerCards;
		private readonly List<EightCardPokerCard> _noneJokerCards;
		private readonly List<EightCardPokerCard> _jokerCards;
		private PokerRankTypes _bestRank;


		// This is the possible estimated max poker number, consider A is 14, small joker is 15, larger joker is 16, and some reverse space.
		// This number might be used for computing the possible Single Card Rank comparision, or other usage. See which function use this to understand
		// the purpose of this variable.
		public static readonly int MaxPokerNumber = 20;
		private int _minFlushStraightCards = 5;

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
				{ (6, CompType.Straight), PokerCardCompRank.SixCardStraight },
				{ (7, CompType.Straight), PokerCardCompRank.SevenCardStraight },
				{ (6, CompType.FlushStraight), PokerCardCompRank.SixCardFlushStraight },
				{ (7, CompType.FlushStraight), PokerCardCompRank.SevenCardFlushStraight },
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

		public HandAnalyzer()
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
				if (card is EightCardJokerCard) _jokerCards.Add(card);
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
					newCardList.Add(EightCardJokerCard.CreateInstance(jokerType, deckID: deckNumber));
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


			//--- Finally, sort them out base on their CompPower order.
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
						var newJoker = EightCardJokerCard.CreateInstance((EightCardJokerCard)jokerCard);
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
				flushableJokerCards.OrderByDescending(obj => ((EightCardJokerCard)obj).PokerCardPower).ToList();


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
						var newJoker = EightCardJokerCard.CreateInstance((EightCardJokerCard)pair.jokerCard);
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
		
		/*
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
		*/
		
		private List<List<EightCardPokerCard>> GetNumberGroups(int minCardCountInGroup, List<EightCardPokerCard> noneJokerCards)
		{
			// 使用 Dictionary 根據牌面點數（Number）進行分組
			var rankGroupsDict = new Dictionary<int, List<EightCardPokerCard>>();

			// 遍歷所有非鬼牌，直接在一次 Pass 中完成分組，避免建立不必要的暫存排序清單
			foreach (var card in noneJokerCards)
			{
				if (!rankGroupsDict.TryGetValue(card.Number, out var group))
				{
					group = new List<EightCardPokerCard>();
					rankGroupsDict[card.Number] = group;
				}
				group.Add(card);
			}

			// 針對每個點位分組執行原地排序（In-place Sort），以極小化堆積記憶體（Heap Allocation）配置
			foreach (var group in rankGroupsDict.Values)
			{
				group.Sort((x, y) => y.CompareTo(x)); // 排序優先級：點數相同時，按花色權重降序排列
			}

			// 透過 LINQ 鍊式呼叫一次完成過濾、點數降序排序與清單轉換
			return rankGroupsDict
				.Where(kv => kv.Value.Count >= minCardCountInGroup) // 僅保留符合最小張數條件的分組（例如：找對子則至少需 2 張）
				.OrderByDescending(kv => kv.Key) // 依據點數由大到小排序，確保回傳結果符合 Power 順序
				.Select(kv => kv.Value) // 提取出分組內的卡片清單
				.ToList(); // 將結果實體化（Materialize）為 List 以供後續遞歸運算使用
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



		/*
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
	    */
