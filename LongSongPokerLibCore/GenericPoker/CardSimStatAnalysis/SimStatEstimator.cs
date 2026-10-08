using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimStatEstimator
    {
        private List<BasePokerCard> _allPokerCards;
        private int _minFlushStraightCards = 3;
        private int _minStraightCards = 5;
        private int _minFlushCards = 5;
        
        public static readonly int MaxPokerNumber = 20;
        
        private static readonly Dictionary<string, BaseCompType> SimCardsCompTypeDict = new()
			{
				{ "2_Kind", BaseCompType.Pair },
				{ "3_Kind", BaseCompType.ThreeOfKind },
				{ "4_Kind", BaseCompType.FourOfKind },
				{ "5_Kind", BaseCompType.FiveOfKind },
				{ "6_Kind", BaseCompType.SixOfKind },
				{ "7_Kind", BaseCompType.SevenOfKind },
				{ "8_Kind", BaseCompType.EightOfKind },
				//{ "3_Flush", BaseCompType.ThreeCardsFlush },
				//{ "3_Straight", BaseCompType.ThreeCardsStraight },
				//{ "3_PairInFlush", BaseCompType.ThreeCardsPairInFlush},
				{ "3_FlushStraight", BaseCompType.ThreeCardsFlushStraight },
				//{ "4_Flush", BaseCompType.FourCardsFlush },
				//{ "4_PairInFlush", BaseCompType.FourCardsPairInFlush},
				//{ "4_TwoPairsInFlush", BaseCompType.FourCardsTwoPairsInFlush},
				//{ "4_Straight", BaseCompType.FourCardStraight },
				{ "4_FlushStraight", BaseCompType.FourCardsFlushStraight },
				{ "5_Flush", BaseCompType.FiveCardsFlush },
				//{ "5_PairInFlush", BaseCompType.FiveCardsPairInFlush},
				//{ "5_TwoPairsInFlush", BaseCompType.FiveCardsTwoPairsInFlush},
				{ "5_Straight", BaseCompType.FiveCardsStraight },
				{ "5_FlushStraight", BaseCompType.FiveCardsFlushStraight },
				{ "6_Flush", BaseCompType.SixCardsFlush },
				{ "6_PairInFlush", BaseCompType.SixCardsPairInFlush},
				{ "6_TwoPairsInFlush", BaseCompType.SixCardsTwoPairsInFlush},
				{ "6_ThreePairsInFlush", BaseCompType.SixCardsThreePairsInFlush},
				{ "7_Flush", BaseCompType.SevenCardsFlush },
				{ "7_PairInFlush", BaseCompType.SevenCardsPairInFlush},
				{ "7_TwoPairsInFlush", BaseCompType.SevenCardsTwoPairsInFlush},
				{ "7_ThreePairsInFlush", BaseCompType.SevenCardsThreePairsInFlush},
				{ "8_Flush", BaseCompType.EightCardsFlush },
				{ "9_Flush", BaseCompType.NineCardsFlush },
				{ "10_Flush", BaseCompType.TenCardsFlush },
				{ "8_PairInFlush", BaseCompType.EightCardsPairInFlush},
				{ "8_TwoPairsInFlush", BaseCompType.EightCardsTwoPairsInFlush},
				{ "8_ThreePairsInFlush", BaseCompType.EightCardsThreePairsInFlush},
				{ "8_FourPairsInFlush", BaseCompType.EightCardsFourPairsInFlush},
				{ "6_Straight", BaseCompType.SixCardsStraight },
				{ "7_Straight", BaseCompType.SevenCardsStraight },
				{ "8_Straight", BaseCompType.EightCardsStraight },
				{ "9_Straight", BaseCompType.NineCardsStraight },
				{ "10_Straight", BaseCompType.TenCardsStraight },
				{ "6_FlushStraight", BaseCompType.SixCardsFlushStraight },
				{ "7_FlushStraight", BaseCompType.SevenCardsFlushStraight },
				{ "8_FlushStraight", BaseCompType.EightCardsFlushStraight },
				{ "9_FlushStraight", BaseCompType.NineCardsFlushStraight },
				{ "10_FlushStraight", BaseCompType.TenCardsFlushStraight }
			};

        public void SetupCards(List<BasePokerCard> inputPokerCardList)
        {
            _allPokerCards = new List<BasePokerCard>(inputPokerCardList);
        }
        
        public string GetHandString()
        {
            return string.Join(",", _allPokerCards.Select(card => card.CardStr));
        }

        private bool checkCompsCardCount(List<PokerHandStructure> allComps)
        {
	        
	        foreach (var structure in allComps)
	        {
		        var totalCards = 0;
		        
		        if (structure.FinalCompsStr == "None")
		        {
			        continue;
		        }

		        // Split by underscore to get individual composition patterns
		        var compParts = structure.FinalCompsStr.Split('_');

		        foreach (var compPart in compParts)
		        {
			        // Parse pattern like "Pair*2" or "ThreeCardsFlushStraight"
			        var parts = compPart.Split('*');
			        var compTypeName = parts[0];
			        var multiplier = parts.Length > 1 ? int.Parse(parts[1]) : 1;

			        // Find the card count from the composition type name
			        // The key format is like "2_Kind" for Pair, "3_FlushStraight" for ThreeCardsFlushStraight
			        var cardCount = 0;
			        foreach (var kvp in SimCardsCompTypeDict)
			        {
				        if (kvp.Value.ToString() == compTypeName)
				        {
					        // Extract the number prefix from the key (e.g., "2" from "2_Kind")
					        var keyParts = kvp.Key.Split('_');
					        cardCount = int.Parse(keyParts[0]);
					        break;
				        }
			        }

			        totalCards += cardCount * multiplier;
		        }
		        if (totalCards > _allPokerCards.Count) 
		        {
			        Console.WriteLine("Structure final string is " + structure.FinalCompsStr + " where the hand string is " + GetHandString());
			        return false;
		        }
	        }

	        return true;
        }
        
        public List<PokerHandStructure> TestSimCards()
        {
            var allCandidateComps = new List<PokerHandStructure>();

            RecursiveArrangeHands(_allPokerCards, new PokerHandStructure(), allCandidateComps);
            if (allCandidateComps.Count == 0)
            {
                var nothingHand = new PokerHandStructure();
                nothingHand.SetRemainingCards(_allPokerCards);
                allCandidateComps.Add(nothingHand);
            }
            
            foreach (var res in allCandidateComps)
            {
                res.SortCompsAndClassify();
            }
            
            if (!checkCompsCardCount(allCandidateComps))
            {
                Environment.Exit(1);
            }
            
            allCandidateComps.Sort((c1, c2) => c2.CompareTo(c1));
            
            
            // remove the duplicate set of arrangement by their final comp str.
            var uniqueCandidates = allCandidateComps.DistinctBy(c => c.FinalCompsStr).ToList();
            return uniqueCandidates;
        }
        
        
        /// <summary>
        /// Groups cards by their numerical rank (number) and filters based on a minimum count per rank.
        /// </summary>
        /// <param name="minCardCountInGroup">Minimum number of cards of the same rank required.</param>
        /// <param name="noneJokerCards">The collection of cards (excluding jokers) to group.</param>
        /// <returns>A list of card groups, each containing cards of the same rank, ordered by rank descending.</returns>
        private List<List<BasePokerCard>> _getNumberGroups(int minCardCountInGroup, List<BasePokerCard> noneJokerCards)
        {
            // 使用 Dictionary 根據牌面點數（Number）進行分組
            var rankGroupsDict = new Dictionary<int, List<BasePokerCard>>();

            // 遍歷所有非鬼牌，直接在一次 Pass 中完成分組，避免建立不必要的暫存排序清單
            foreach (var card in noneJokerCards)
            {
                if (!rankGroupsDict.TryGetValue(card.Number, out var group))
                {
                    group = new List<BasePokerCard>();
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
        private void RecursiveArrangeHands(List<BasePokerCard> remainingCards,
            PokerHandStructure currentHandCandidates, List<PokerHandStructure> results)
        {
            var candidateProcessed = false;
            
            // make copy of input currentHandCandidates to prevent step one finding polute the other step initial clean currentHandCandidates.
            var accumHandStructures = new PokerHandStructure(currentHandCandidates);
            
            //1. Sort the number in each suit, try to find suits first and find straight by the way to see if we have flush Straight.
            var flushGroups = _evaluateFlushGroups(_minFlushStraightCards, remainingCards);
            candidateProcessed = ArrangeFlushOrFlushStraight(flushGroups, remainingCards, accumHandStructures, results, candidateProcessed);
           
			
            // 2. Continue to find Sort majorly for straight
            accumHandStructures = new PokerHandStructure(currentHandCandidates);
            var numberGroupList = _getNumberGroups(1, remainingCards);
            var allStraightClusters = GetAllStraightCluster(_minStraightCards, numberGroupList);
            candidateProcessed = ArrangeStraightComps(allStraightClusters, remainingCards, accumHandStructures,
	            results, candidateProcessed);
            
            // 3. Continue to find kinds. Get all kinds group to performance any pair or threeOFkind or fourOFkind, etc.
            accumHandStructures = new PokerHandStructure(currentHandCandidates);
            var allKindGroups = GetKindGroups(2, remainingCards);
            candidateProcessed = ArrangeKindComps(allKindGroups, remainingCards, accumHandStructures, results, candidateProcessed);
          
			
            // When code comes here, it means there are nothing else worthy to record, so that put all current into Results.
            // If bFoundStructure is true, it means those process function has already handled those RecursiveXXX for remaning.
            // Only process if bFoundStructure is false, means notthing else to record, so formally process currentHandCandidates.
            if (!candidateProcessed && accumHandStructures.Components.Count > 0)
            {
                var newCandidateComps = new PokerHandStructure(accumHandStructures);
                newCandidateComps.SetRemainingCards(remainingCards);
                results.Add(newCandidateComps);
            }
            
            // Process if all remaining cards are not touched (meaning = _allPokerCards.count), so it's nothing. 
            if (!candidateProcessed && remainingCards.Count == _allPokerCards.Count)
            {
	            var newCandidateComps = new PokerHandStructure(accumHandStructures);
	            var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
		            { CompRank = BaseCompType.Nothing, Cards = remainingCards };
	            newCandidateComps.AddComp(newHandCandidateData);
	            results.Add(newCandidateComps);
            }
        }
        
        private BaseCompType  DetermineCompType(int numCards, CompType compType)
        {
            var keyStr = $"{numCards}_{compType.ToString()}";
            var retCompType = SimCardsCompTypeDict.TryGetValue(keyStr, out var value) ? value : BaseCompType.None;
            return retCompType;
        }
        
        
        /// <summary>
        /// Attempts to arrange remaining cards into kind-based components (pairs, triples, etc.).
        /// Part of the recursive hand-splitting logic.
        /// </summary>
        /// <param name="allKindGroups">Cards grouped by rank with at least 2 cards.</param>
        /// <param name="remainingCards">Available cards.</param>
        /// <param name="accumHandStructures">Current state of hand composition.</param>
        /// <param name="results">Output list of complete hand structures.</param>
        /// <param name="candidateProcessed">A flag indicating if any valid hand component was found in this branch.</param>
        /// <returns>True if a hand component was successfully added.</returns>
        private bool ArrangeKindComps(List<List<BasePokerCard>> allKindGroups, List<BasePokerCard> remainingCards,
            PokerHandStructure accumHandStructures, List<PokerHandStructure> results, bool candidateProcessed)
        {
	        // Do filter out any kindGroup count < 2, so we have at least pair in the group. such as pair, threeofkind, fourofkind
	        var allQualifiedGroups = allKindGroups.Where(x => x.Count >= 2).ToList();

	        if (allQualifiedGroups.Count > 0)
	        {
		        // Do loop through allQualifiedGroups to add into currentHandCandidates	
		        foreach (var kindGroup in allQualifiedGroups)
		        {
			        var handType = DetermineCompType(kindGroup.Count, CompType.Kind);
			        var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
				        { CompRank = handType, Cards = kindGroup };
			        accumHandStructures.AddComp(newHandCandidateData);
		        }

		        // Count out the newRemainCards by subtracting the cards from remainingCards by allQualifiedGroups. 
		        var allCardsToRemove = allQualifiedGroups.SelectMany(group => group).ToList();
		        var newRemainCards = UtilFunc.GetExcludeList(remainingCards, allCardsToRemove, new PokerCardComparer());
		        RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
		        return true;
	        }
	        else
	        {
		        return candidateProcessed;    
	        }

        }
        
        
		/// <summary>
		/// Attempts to arrange remaining cards into flush or flush-straight components.
		/// Part of the recursive hand-splitting logic.
		/// </summary>
		/// <param name="flushGroups">Pre-evaluated suit groups.</param>
		/// <param name="remainingCards">Available cards.</param>
		/// <param name="accumHandStructures">Current state of hand composition.</param>
		/// <param name="results">Output list of complete hand structures.</param>
		/// <param name="candidateProcessed">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeFlushOrFlushStraight(List<List<BasePokerCard>> flushGroups, List<BasePokerCard> remainingCards,
            PokerHandStructure accumHandStructures, List<PokerHandStructure> results, bool candidateProcessed)
        {
	        
			foreach (var flushGroup in flushGroups)
			{
				var flushGroupForStraightFinding = flushGroup.Select(card => new List<BasePokerCard> { card }).ToList();

				//var stillFlushCandidate = false;
				var allStraightClusters = GetAllStraightCluster(_minFlushStraightCards, flushGroupForStraightFinding);

				if (allStraightClusters.Count > 0)
				{

					//var accumHandStructuresCopy = new PokerHandStructure(accumHandStructures);

					//var accumAllRepresentedStraightCards = new List<BasePokerCard>();
					//var componentAddCount = 0;
					foreach (var straightCluster in allStraightClusters)
					{
						// Loop through allStraightClusters and select the first item from each sub list to form a straight
						var straight = straightCluster.Select(subList => subList[0]).ToList();
						//accumAllRepresentedStraightCards.AddRange(straight);
						var handType = DetermineCompType(straight.Count, CompType.FlushStraight);
						var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
							{ CompRank = handType, Cards = straight };
						accumHandStructures.AddComp(newHandCandidateData);
						//componentAddCount++;
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, straight,
							new PokerCardComparer());
					
						RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
					
						// need to roll back one from previous add, so that other loop member can have a clean start.
						accumHandStructures.RemoveLast();
					}
					
					
					// if the first cluster's straight is not all flush count, it means, it has regular flush (not full long straight flush), 
					// so set up this full flush count as flush.
					if (allStraightClusters[0].Count != flushGroup.Count && flushGroup.Count >= _minFlushCards)
					{
						// copy back the original hand structure before doing partial straight flush.
						//accumHandStructures = new PokerHandStructure(accumHandStructuresCopy);
						var handType = DetermineCompType(flushGroup.Count, CompType.Flush);
						var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
							{ CompRank = handType, Cards = flushGroup };
						accumHandStructures.AddComp(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, flushGroup,
							new PokerCardComparer());
						RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
						accumHandStructures.RemoveLast();
					}
					
					candidateProcessed = true;
				}
				else if (flushGroup.Count >= _minFlushCards )
				{
					var handType = DetermineCompType(flushGroup.Count, CompType.Flush);
					var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
						{ CompRank = handType, Cards = flushGroup };
					accumHandStructures.AddComp(newHandCandidateData);
					var newRemainCards = UtilFunc.GetExcludeList(remainingCards, flushGroup,
						new PokerCardComparer());
					RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
					accumHandStructures.RemoveLast();
					candidateProcessed = true;
				}
				
			}
            return candidateProcessed;
        }

		
        
        /// <summary>
		/// Attempts to arrange remaining cards into straight components based on pre-calculated clusters.
		/// Part of the recursive hand-splitting logic.
		/// </summary>
		/// <param name="allStraightClusters">Groups of consecutive ranks.</param>
		/// <param name="remainingCards">Available cards.</param>
		/// <param name="accumHandStructures">Current state of hand composition.</param>
		/// <param name="results">Output list of complete hand structures.</param>
		/// <param name="candidateProcessed">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeStraightComps(List<List<List<BasePokerCard>>> allStraightClusters, List<BasePokerCard> remainingCards,
            PokerHandStructure accumHandStructures, List<PokerHandStructure> results, bool candidateProcessed)
        {
	        
	        if (allStraightClusters.Count == 0)
	        {
	            return candidateProcessed;
	        }
	        
	        var accumAllRepresentedStraightCards = new List<BasePokerCard>();
	        foreach (var straightCluster in allStraightClusters)
	        {
				// Loop through allStraightClusters and select the first item from each sub list to form a straight
				var straight = straightCluster.Select(subList => subList[0]).ToList();
				accumAllRepresentedStraightCards.AddRange(straight);
				var isAllSameSuit = straight.All(card => card.Suit == straight[0].Suit);
				var compType = isAllSameSuit ? CompType.FlushStraight : CompType.Straight;
				var handType = DetermineCompType(straight.Count, compType);
				var newHandCandidateData = new PokerCardComponent<BaseCompType, BasePokerCard>
						{ CompRank = handType, Cards = straight };
				accumHandStructures.AddComp(newHandCandidateData);
				var newRemainCards = UtilFunc.GetExcludeList(remainingCards, accumAllRepresentedStraightCards, new PokerCardComparer());
				RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
				accumHandStructures.RemoveLast();
	        }
	        
	        
            return true;
        }
        
        
        private List<List<BasePokerCard>> GetKindGroups(int minCardCountInGroup, List<BasePokerCard> noneJokerCards)
        {
	        var numberGroups = _getNumberGroups(minCardCountInGroup, noneJokerCards);
	        numberGroups.Sort((x, y) => y.Count.CompareTo(x.Count));
	        return numberGroups;
        }
        
        /// <summary>
		/// Groups all provided cards by their suit and filters those that meet the minimum count requirement.
		/// </summary>
		/// <param name="minCardCountInGroup">Minimum number of cards of the same suit required.</param>
		/// <param name="allPokerCards">The collection of cards to evaluate.</param>
		/// <returns>A list of card groups, each containing cards of the same suit, ordered by group size.</returns>
		private List<List<BasePokerCard>> _evaluateFlushGroups(int minCardCountInGroup, List<BasePokerCard> allPokerCards)
		{

			var sortedList = allPokerCards.OrderByDescending(item => item.PokerCardPower).ToList();

			var suitGroups = new List<List<BasePokerCard>>();
			
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
				List<BasePokerCard> sameSuitCards = sortedList.FindAll(e => e.Suit == pokerSuit);
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

        

        /// <summary>
        /// Identifies clusters of consecutive card ranks that are long enough to potentially form straights.
        /// It handles Ace-low logic by duplicating Ace groups as rank 1 if necessary.
        /// </summary>
        /// <param name="straightCount">The minimum required length for a straight.</param>
        /// <param name="kindGroupList">List of cards grouped by their numerical rank.</param>
        /// <returns>A list of clusters, where each cluster is a list of consecutive rank groups.</returns>
        /// So the return would be like this <{<8>, <7,7>}, {<4>, <3,3>, <2,2>}>
        private static List<List<List<BasePokerCard>>> GetAllStraightCluster(int straightCount, List<List<BasePokerCard>> kindGroupList)
        {
	        if (kindGroupList.Count == 0)
	        {
		        return new List<List<List<BasePokerCard>>>();
	        } 
	        
	        // if we have ace kind group, we copy them in the bottom of kind group list and make all ace becomes "1" 
	        // so that to let 3,2,1 straight become available.
	        if (kindGroupList[0][0] is AcePokerCard)
	        //if (kindGroupList[0][0] is AceCard)
	        {
		        var newKindGroup = new List<BasePokerCard>();
		        foreach (var ace in kindGroupList[0])
		        {
			        var newAce = BasePokerCard.CreateInstance(ace);
			        ((IJokerStraightable)newAce).SetStraightSub(1);
			        newKindGroup.Add(newAce);
		        }
		        kindGroupList.Add(newKindGroup);
	        }
			
	        // Clustering numberGroups
	        // The following is to collect all possible straight clusters. Each cluster contains continuous of kind groups.
	        // Initially the accum is empty, so add the first kindGroup as the initial accum.
	        // Then check if the next kind group's represented number is neighebor of accum last group's number.
	        // If it is, then straight continutes, and add that new kind group into accum's last claster, if not
	        // add that new kind group as new accum's list. and go on.to achieve all possible straight clusters.
	        // Then if the input is <<8>,<7,7>, <4>, <3,3>, <2,2>>, then the return would be
	        // <{<8>, <7,7>}, {<4>, <3,3>, <2,2>}>
	        var numberClusters = kindGroupList
		        .Aggregate(new List<List<List<BasePokerCard>>>(), (accum, numGroup) =>
		        {
			        // check if empty initial accum is empty, then start the input numGroup as accum,
			        // of 
			        if (accum.Count == 0 || accum.Last().Last()[0].Number - numGroup[0].Number != 1)
				        accum.Add(new List<List<BasePokerCard>> { numGroup });
			        else
				        accum.Last().Add(numGroup);
			        return accum;
		        });
			
	        //numberClusters contains various clusters. In each cluster, there are continuous of kind groups.
	        // Filter out all qualified cluster's member count number >= StraightCount, and also sorted with higher StraightCount
	        var qualifiedClusters = 
		        numberClusters.Where(cluster => cluster.Count >= straightCount)
			        .OrderByDescending(x => x.Count).ToList();
			
	        return qualifiedClusters;
        }
        
    }
}
