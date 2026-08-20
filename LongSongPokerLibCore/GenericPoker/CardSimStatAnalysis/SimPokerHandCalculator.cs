using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimPokerHandCalculator
    {
        private List<SimPokerCard> _allPokerCards;
        private int _minFlushStraightCards = 3;
        
        public static readonly int MaxPokerNumber = 20;
        
        private static readonly Dictionary<string, SimCardsCompType> SimCardsCompTypeDict = new()
			{
				{ "2_Kind", SimCardsCompType.Pair },
				{ "3_Kind", SimCardsCompType.ThreeOfKind },
				{ "4_Kind", SimCardsCompType.FourOfKind },
				{ "5_Kind", SimCardsCompType.FiveOfKind },
				{ "6_Kind", SimCardsCompType.SixOfKind },
				{ "7_Kind", SimCardsCompType.SevenOfKind },
				{ "8_Kind", SimCardsCompType.EightOfKind },
				//{ "3_Flush", EightCardsCompType.ThreeCardsFlush },
				//{ "3_Straight", EightCardsCompType.ThreeCardsStraight },
				//{ "3_PairInFlush", EightCardsCompType.ThreeCardsPairInFlush},
				{ "3_FlushStraight", SimCardsCompType.ThreeCardsFlushStraight },
				//{ "4_Flush", EightCardsCompType.FourCardsFlush },
				//{ "4_PairInFlush", EightCardsCompType.FourCardsPairInFlush},
				//{ "4_TwoPairsInFlush", EightCardsCompType.FourCardsTwoPairsInFlush},
				//{ "4_Straight", EightCardsCompType.FourCardStraight },
				{ "4_FlushStraight", SimCardsCompType.FourCardsFlushStraight },
				{ "5_Flush", SimCardsCompType.FiveCardsFlush },
				//{ "5_PairInFlush", EightCardsCompType.FiveCardsPairInFlush},
				//{ "5_TwoPairsInFlush", EightCardsCompType.FiveCardsTwoPairsInFlush},
				{ "5_Straight", SimCardsCompType.FiveCardsStraight },
				{ "5_FlushStraight", SimCardsCompType.FiveCardsFlushStraight },
				{ "6_Flush", SimCardsCompType.SixCardsFlush },
				{ "6_PairInFlush", SimCardsCompType.SixCardsPairInFlush},
				{ "6_TwoPairsInFlush", SimCardsCompType.SixCardsTwoPairsInFlush},
				{ "6_ThreePairsInFlush", SimCardsCompType.SixCardsThreePairsInFlush},
				{ "7_Flush", SimCardsCompType.SevenCardsFlush },
				{ "7_PairInFlush", SimCardsCompType.SevenCardsPairInFlush},
				{ "7_TwoPairsInFlush", SimCardsCompType.SevenCardsTwoPairsInFlush},
				{ "7_ThreePairsInFlush", SimCardsCompType.SevenCardsThreePairsInFlush},
				{ "8_Flush", SimCardsCompType.EightCardsFlush },
				{ "8_PairInFlush", SimCardsCompType.EightCardsPairInFlush},
				{ "8_TwoPairsInFlush", SimCardsCompType.EightCardsTwoPairsInFlush},
				{ "8_ThreePairsInFlush", SimCardsCompType.EightCardsThreePairsInFlush},
				{ "8_FourPairsInFlush", SimCardsCompType.EightCardsFourPairsInFlush},
				{ "6_Straight", SimCardsCompType.SixCardsStraight },
				{ "7_Straight", SimCardsCompType.SevenCardsStraight },
				{ "8_Straight", SimCardsCompType.EightCardsStraight },
				{ "6_FlushStraight", SimCardsCompType.SixCardsFlushStraight },
				{ "7_FlushStraight", SimCardsCompType.SevenCardsFlushStraight },
				{ "8_FlushStraight", SimCardsCompType.EightCardsFlushStraight }
			};

        public void SetupCards(List<SimPokerCard> inputPokerCardList)
        {
            _allPokerCards = new List<SimPokerCard>(inputPokerCardList);
        }

        //static int counter = 0;
        
        public List<SimPokerHandStructure> TestSimCards()
        {

            
           // counter++;
            //Console.WriteLine($"Test8Cards called {counter} times.");
            
            var allCandidateComps = new List<SimPokerHandStructure>();
            //RecursiveEvaluateCards(_allPokerCards, new PokerHandStructure(), allCandidateComps);
            RecursiveArrangeHands(_allPokerCards, new SimPokerHandStructure(), allCandidateComps);
            foreach (var res in allCandidateComps)
            {
                res.SortCompsAndClassify();
            }
			
            allCandidateComps.Sort((c1, c2) => c2.CompareTo(c1));
            var uniqueCandidates = allCandidateComps.Distinct().ToList();
			
            
            // help me print finalcompstr if unuqiueCandidats has something
            /*
            if (uniqueCandidates.Count > 0)
            {
	            foreach (var candidate in uniqueCandidates)
	            {
		            Console.WriteLine("Counter--" + counter + "" + candidate.FinalCompsStr.ToString());
	            }
            }
            else
            {
	            Console.WriteLine("Counter--" + counter + "" + "Nothing in unqiueCandidates:");
            }*/
            
            return uniqueCandidates;
            
        }
        
        
        /// <summary>
        /// Groups cards by their numerical rank (number) and filters based on a minimum count per rank.
        /// </summary>
        /// <param name="minCardCountInGroup">Minimum number of cards of the same rank required.</param>
        /// <param name="noneJokerCards">The collection of cards (excluding jokers) to group.</param>
        /// <returns>A list of card groups, each containing cards of the same rank, ordered by rank descending.</returns>
        private List<List<SimPokerCard>> GetNumberGroups(int minCardCountInGroup, List<SimPokerCard> noneJokerCards)
        {
            // 使用 Dictionary 根據牌面點數（Number）進行分組
            var rankGroupsDict = new Dictionary<int, List<SimPokerCard>>();

            // 遍歷所有非鬼牌，直接在一次 Pass 中完成分組，避免建立不必要的暫存排序清單
            foreach (var card in noneJokerCards)
            {
                if (!rankGroupsDict.TryGetValue(card.Number, out var group))
                {
                    group = new List<SimPokerCard>();
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
        
        /// <summary>
        /// Main recursive entry point for partitioning a set of cards into various valid poker hand components.
        /// It sequentially tries flushes, straights, and kinds to find all possible valid hand layouts.
        /// </summary>
        /// <param name="remainingCards">The list of cards remaining to be partitioned.</param>
        /// <param name="currentHandCandidates">The current hand structure being built.</param>
        /// <param name="results">The list of all valid complete hand structures found.</param>
        private void RecursiveArrangeHands(List<SimPokerCard> remainingCards,
            SimPokerHandStructure currentHandCandidates, List<SimPokerHandStructure> results)
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
                var newCandidateComps = new SimPokerHandStructure(currentHandCandidates);
                newCandidateComps.SetRemainingCards(remainingCards);
                results.Add(newCandidateComps);
            }
        }
        
        private SimCardsCompType  DetermineCompType(int numCards, CompType compType)
        {
            var keyStr = $"{numCards}_{compType.ToString()}";
            var retCompType = SimCardsCompTypeDict.TryGetValue(keyStr, out var value) ? value : SimCardsCompType.None;
            return retCompType;
        }
        
        /// <summary>
        /// Attempts to arrange remaining cards into kind-based components (pairs, triples, etc.).
        /// Part of the recursive hand-splitting logic.
        /// </summary>
        /// <param name="allKindGroups">Cards grouped by rank with at least 2 cards.</param>
        /// <param name="remainingCards">Available cards.</param>
        /// <param name="currentHandCandidates">Current state of hand composition.</param>
        /// <param name="results">Output list of complete hand structures.</param>
        /// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
        /// <returns>True if a hand component was successfully added.</returns>
        private bool ArrangeKindComps(List<List<SimPokerCard>> allKindGroups, List<SimPokerCard> remainingCards,
            SimPokerHandStructure currentHandCandidates, List<SimPokerHandStructure> results, bool hasRank)
        {
            foreach (var kindGroup in allKindGroups)
            {
                for (var groupCardNum = kindGroup.Count; groupCardNum >= 2; groupCardNum--)
                {
                    //var allPermutes = new List<List<PokerCard>>();
                    var handType = DetermineCompType(groupCardNum, CompType.Kind);
                    var allPermutes = UtilFunc.GetPermutation<SimPokerCard>(kindGroup, groupCardNum);
                    foreach (var permute in allPermutes)
                    {
                        var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard> { CompRank = handType, Cards = permute };
                        currentHandCandidates.AddComp(newHandCandidateData);
                        var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
                        RecursiveArrangeHands(newRemainCards, currentHandCandidates, results);
                        currentHandCandidates.Components.RemoveAt(currentHandCandidates.Components.Count - 1);
                        
                    }
                }
                hasRank = true;
            }
            return hasRank;
        }
        
        
        /// <summary>
        /// Recursively generates all possible card combinations for a straight sequence.
        /// It navigates through rank groups to ensure consecutive numbers and collects 
        /// all valid permutations of length equal to straightCount.
        /// </summary>
        /// <param name="straightCount">The target number of cards in the straight.</param>
        /// <param name="kindGroupList">Available card groups to pick from.</param>
        /// <param name="currentList">The current accumulation of cards in the recursion.</param>
        /// <param name="resultList">Output list to store completed straight combinations.</param>
        private static void RecursivePermuteStraight(int straightCount, List<List<SimPokerCard>> kindGroupList,
	        List<SimPokerCard> currentList, List<List<SimPokerCard>> resultList)
        {

	        if (currentList.Count + kindGroupList.Count < straightCount) return;

	        if (currentList.Count >= straightCount)
	        {
		        // currentList is shared, so need to record at the moment when you added. If not, other hierarchy loop
		        // will contaminate it.
		        resultList.Add(new List<SimPokerCard>(currentList));
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

        
        /// <summary>
		/// Processes and generates all valid straight permutations from given card groups and jokers.
		/// This function handles gap filling with jokers, Ace-low (1) straight logic, 
		/// and optionally restricts the cards to a specific suit for flush-straights.
		/// </summary>
		/// <param name="straightCount">The target length of the straight.</param>
		/// <param name="kindGroupList">List of card groups grouped by rank.</param>
		/// <param name="straightableJokerCards">Available jokers that can be used in a straight.</param>
		/// <param name="permutes">Output list where valid straight combinations will be added.</param>
		/// <param name="assignedSuit">Optional suit restriction for flush-straights.</param>
		private void ProcessPermuteStraight(int straightCount, List<List<SimPokerCard>> kindGroupList,
			List<SimPokerCard> straightableJokerCards, List<List<SimPokerCard>> permutes,
			PokerSuit assignedSuit = PokerSuit.NoSuit)
		{

			// if we have ace kind group, we copy them in the bottom of kindgroup list and make all ace becomes "1" 
			// so that to let 3,2,1 straight become available.
			if (kindGroupList[0][0] is AcePokerCard)
			{
				var newKindGroup = new List<SimPokerCard>();
				foreach (var ace in kindGroupList[0])
				{
					var newAce = SimPokerCard.CreateInstance(ace);
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
				
				var tempPermutes = UtilFunc.GetPermutation<List<SimPokerCard>>(kindGroupList, pokerCardCountInvolved);
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
						var newJoker = SimPokerCard.CreateInstance((SimPokerCard)pair.jokerCard);
						((IJokerStraightable)newJoker).SetStraightSub(pair.Number);
						if (assignedSuit != PokerSuit.NoSuit && newJoker is IJokerFlushable)
							((IJokerFlushable)newJoker).SetSuitSub(assignedSuit);
						cardPermute.Add(new List<SimPokerCard> { newJoker });
					}

					var groupPermuteWithJokers = cardPermute.OrderByDescending(objList => objList[0].Number).ToList();

					RecursivePermuteStraight(straightCount, groupPermuteWithJokers,
						new List<SimPokerCard>(), permutes);

				}
			}
		}
        
		private SimCardsCompType DetermineCompTypeWithPairInFlush(int numCards, CompType CompType, int pairsInFlush = 0)
		{
			/*var keyStr = "";
			Dictionary<int, string> localDict = new Dictionary<int, string>{{1, "PairIn"}, {2, "TwoPairsIn"},  {3, "ThreePairsIn"}, {4, "FourPairsIn"}};
			keyStr = pairsInFlush == 0 ? $"{numCards}_{CompType.ToString()}" :
				$"{numCards}_{localDict[pairsInFlush]}{CompType.ToString()}";*/

			var keyStr = $"{numCards}_{CompType.ToString()}";
			var retCompType = SimCardsCompTypeDict.TryGetValue(keyStr, out var value) ? value : SimCardsCompType.None;
			
			return retCompType;
		}
       
        /// <summary>
		/// Attempts to arrange remaining cards into flush or flush-straight components.
		/// Part of the recursive hand-splitting logic.
		/// </summary>
		/// <param name="flushGroups">Pre-evaluated suit groups.</param>
		/// <param name="remainingCards">Available cards.</param>
		/// <param name="currentHandStructure">Current state of hand composition.</param>
		/// <param name="results">Output list of complete hand structures.</param>
		/// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeFlushOrFlushStraight(List<List<SimPokerCard>> flushGroups, List<SimPokerCard> remainingCards,
            SimPokerHandStructure currentHandStructure, List<SimPokerHandStructure> results, bool hasRank)
        {
			foreach (var flushGroup in flushGroups)
			{
				for (var desiredCount = flushGroup.Count; desiredCount >= _minFlushStraightCards; desiredCount--)
				{
					var flushStraightPermutes = new List<List<SimPokerCard>>();
					var flushOnlyPermutes = new List<List<SimPokerCard>>();
					
					// To consider more general case for flush pairs, we need following codes to cluster them.
					//var wrapperListInList = flushGroup.Select(item => new List<PokerCard> { item }).ToList();
					var wrapperListInList = flushGroup
						.GroupBy(item => item) // Group by the item value
						.Select(group => group.ToList()) // Convert each group into a list
						.ToList(); //
					
					ProcessPermuteStraight(desiredCount, wrapperListInList, new List<SimPokerCard>(), flushStraightPermutes);

					if (flushStraightPermutes.Count > 0) { // Yes we have straight in suit group which implies @@flush-straight@@
						var handType = DetermineCompType(desiredCount, CompType.FlushStraight);
						if (handType == SimCardsCompType.None) continue;
						foreach (var permute in flushStraightPermutes)
						{
							var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
								{ CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							var newRemainCards =
								UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveArrangeHands(newRemainCards, currentHandStructure, results);
							currentHandStructure.Components.RemoveAt(currentHandStructure.Components.Count - 1);
							//currentHandStructure.RemoveLastComp();
							
							
							hasRank = true;
						}
					} else { // no we don't have straight in suit group which implies @@flush@@

						flushOnlyPermutes.AddRange(UtilFunc.GetPermutation<SimPokerCard>(flushGroup, desiredCount));
						
						
						foreach (var permute in flushOnlyPermutes)
						{
							int pairCount = permute
								.GroupBy(x => x)                // Group by item value
								.Count(g => g.Count() >= 2);
							
							var handType = DetermineCompTypeWithPairInFlush(desiredCount, CompType.Flush, pairCount);
							if (handType == SimCardsCompType.None) continue;
							var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard> { CompRank = handType, Cards = permute };
							currentHandStructure.AddComp(newHandCandidateData);
							// TODO, if we have deck 2, then if we have 2 same J-spade, while remove one J-spade, will also remove the other becuase 
							// when do hash set, two J-spade will become single one.
							var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
							RecursiveArrangeHands(newRemainCards, currentHandStructure, results);
							currentHandStructure.Components.RemoveAt(currentHandStructure.Components.Count - 1);
							hasRank = true;
						}
					}
				}
			}
            return hasRank;
        }

        /// <summary>
		/// Attempts to arrange remaining cards into straight components based on pre-calculated clusters.
		/// Part of the recursive hand-splitting logic.
		/// </summary>
		/// <param name="allStraightClusters">Groups of consecutive ranks.</param>
		/// <param name="remainingCards">Available cards.</param>
		/// <param name="currentHandCandidates">Current state of hand composition.</param>
		/// <param name="results">Output list of complete hand structures.</param>
		/// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeStraightComps(List<List<List<SimPokerCard>>> allStraightClusters, List<SimPokerCard> remainingCards,
            SimPokerHandStructure currentHandCandidates, List<SimPokerHandStructure> results, bool hasRank)
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
                        var allPermutes = new List<List<SimPokerCard>>();
                        var handType = DetermineCompType(targetStraightCluster.Count, CompType.Straight);
                        if (handType == SimCardsCompType.None) continue;
                        RecursivePermuteStraight(targetStraightCluster.Count, targetStraightCluster, new List<SimPokerCard>(), allPermutes);
                        foreach (var permute in allPermutes)
                        {
                            var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard> { CompRank = handType, Cards = permute };
                            currentHandCandidates.AddComp(newHandCandidateData);
                            var newRemainCards = UtilFunc.GetExcludeList(remainingCards, permute, new PokerCardComparer());
                            RecursiveArrangeHands(newRemainCards, currentHandCandidates, results);
                            
                            currentHandCandidates.Components.RemoveAt(currentHandCandidates.Components.Count - 1);
                            
                        }
                    }
                }
                // TODO , need to comment out below as it's a big bug.
                hasRank = true;
            }
            
            return hasRank;
        }
        
        
        private List<List<SimPokerCard>> GetKindGroups(int minCardCountInGroup, List<SimPokerCard> noneJokerCards)
        {
	        var numberGroups = GetNumberGroups(minCardCountInGroup, noneJokerCards);
	        numberGroups.Sort((x, y) => y.Count.CompareTo(x.Count));
	        return numberGroups;
        }
        
        /// <summary>
		/// Groups all provided cards by their suit and filters those that meet the minimum count requirement.
		/// </summary>
		/// <param name="minCardCountInGroup">Minimum number of cards of the same suit required.</param>
		/// <param name="allPokerCards">The collection of cards to evaluate.</param>
		/// <returns>A list of card groups, each containing cards of the same suit, ordered by group size.</returns>
		private List<List<SimPokerCard>> _evaluateFlushGroups(int minCardCountInGroup, List<SimPokerCard> allPokerCards)
		{

			var sortedList = allPokerCards.OrderByDescending(item => item.PokerCardPower).ToList();

			var suitGroups = new List<List<SimPokerCard>>();


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
				List<SimPokerCard> sameSuitCards = sortedList.FindAll(e => e.Suit == pokerSuit);
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

        
		/*
        private List<List<List<SimCardPokerCard>>> GetAllStraightCluster(int minCount, List<List<SimCardPokerCard>> kindGroups)
        {
            // Simplified straight logic
            return new List<List<List<SimCardPokerCard>>>();
        }*/
        
        /// <summary>
        /// Identifies clusters of consecutive card ranks that are long enough to potentially form straights.
        /// It handles Ace-low logic by duplicating Ace groups as rank 1 if necessary.
        /// </summary>
        /// <param name="straightCount">The minimum required length for a straight.</param>
        /// <param name="kindGroupList">List of cards grouped by their numerical rank.</param>
        /// <returns>A list of clusters, where each cluster is a list of consecutive rank groups.</returns>
        private List<List<List<SimPokerCard>>> GetAllStraightCluster(int straightCount, List<List<SimPokerCard>> kindGroupList)
        {
	        if (kindGroupList.Count == 0)
	        {
		        return new List<List<List<SimPokerCard>>>();
	        } 
	        // if we have ace kind group, we copy them in the bottom of kindgroup list and make all ace becomes "1" 
	        // so that to let 3,2,1 straight become available.
	        if (kindGroupList[0][0] is AcePokerCard)
	        {
		        var newKindGroup = new List<SimPokerCard>();
		        foreach (var ace in kindGroupList[0])
		        {
			        var newAce = SimPokerCard.CreateInstance(ace);
			        ((IJokerStraightable)newAce).SetStraightSub(1);
			        newKindGroup.Add(newAce);
		        }
		        kindGroupList.Add(newKindGroup);
	        }
			
	        // Clustering numberGroups
	        var numberClusters = kindGroupList
		        .Aggregate(new List<List<List<SimPokerCard>>>(), (acc, numGroup) =>
		        {
			        if (acc.Count == 0 || acc.Last().Last()[0].Number - numGroup[0].Number != 1)
				        acc.Add(new List<List<SimPokerCard>> { numGroup });
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
        

        private SimCardsCompType MapToCompRank(int count, CompType type)
        {
            if (type == CompType.Kind)
            {
                if (count == 2) return SimCardsCompType.Pair;
                if (count == 3) return SimCardsCompType.ThreeOfKind;
                if (count == 4) return SimCardsCompType.FourOfKind;
                if (count == 5) return SimCardsCompType.FiveOfKind;
            }
            return SimCardsCompType.None;
        }
    }
}
