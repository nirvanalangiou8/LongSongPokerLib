using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimStatEstimator
    {
        private List<SimPokerCard> _allPokerCards;
        private int _minFlushStraightCards = 3;
        private int _minStraightCards = 5;
        private int _minFlushCards = 5;
        
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
				{ "9_Flush", SimCardsCompType.NineCardsFlush },
				{ "10_Flush", SimCardsCompType.TenCardsFlush },
				{ "8_PairInFlush", SimCardsCompType.EightCardsPairInFlush},
				{ "8_TwoPairsInFlush", SimCardsCompType.EightCardsTwoPairsInFlush},
				{ "8_ThreePairsInFlush", SimCardsCompType.EightCardsThreePairsInFlush},
				{ "8_FourPairsInFlush", SimCardsCompType.EightCardsFourPairsInFlush},
				{ "6_Straight", SimCardsCompType.SixCardsStraight },
				{ "7_Straight", SimCardsCompType.SevenCardsStraight },
				{ "8_Straight", SimCardsCompType.EightCardsStraight },
				{ "9_Straight", SimCardsCompType.NineCardsStraight },
				{ "10_Straight", SimCardsCompType.TenCardsStraight },
				{ "6_FlushStraight", SimCardsCompType.SixCardsFlushStraight },
				{ "7_FlushStraight", SimCardsCompType.SevenCardsFlushStraight },
				{ "8_FlushStraight", SimCardsCompType.EightCardsFlushStraight },
				{ "9_FlushStraight", SimCardsCompType.NineCardsFlushStraight },
				{ "10_FlushStraight", SimCardsCompType.TenCardsFlushStraight }
			};

        public void SetupCards(List<SimPokerCard> inputPokerCardList)
        {
            _allPokerCards = new List<SimPokerCard>(inputPokerCardList);
        }
        
        public string GetHandString()
        {
            return string.Join(",", _allPokerCards.Select(card => card.CardStr));
        }

        private bool checkCompsCardCount(List<SimPokerHandStructure> allComps)
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
        
        public List<SimPokerHandStructure> TestSimCards()
        {
            var allCandidateComps = new List<SimPokerHandStructure>();

            RecursiveArrangeHands(_allPokerCards, new SimPokerHandStructure(), allCandidateComps);
            if (allCandidateComps.Count == 0)
            {
                var nothingHand = new SimPokerHandStructure();
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
        private List<List<SimPokerCard>> _getNumberGroups(int minCardCountInGroup, List<SimPokerCard> noneJokerCards)
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
            
            // make copy of input currentHandCandidates
            var accumHandStructures = new SimPokerHandStructure(currentHandCandidates);
            
            //1. Sort the number in each suit, try to find suits first and find straight by the way to see if we have flush Straight.
            var flushGroups = _evaluateFlushGroups(_minFlushStraightCards, remainingCards);
            hasRank = ArrangeFlushOrFlushStraight(flushGroups, remainingCards, accumHandStructures, results, hasRank);
           
			
            // 2. Sort majorly for straight
            accumHandStructures = new SimPokerHandStructure(currentHandCandidates);
            var numberGroupList = _getNumberGroups(1, remainingCards);
            var allStraightClusters = GetAllStraightCluster(_minStraightCards, numberGroupList);
            hasRank = ArrangeStraightComps(allStraightClusters, remainingCards, accumHandStructures, results, hasRank);
          
			
            // 3. Get all kinds group to performance any pair or threeOFkind or fourOFkind, etc.
            accumHandStructures = new SimPokerHandStructure(currentHandCandidates);
            var allKindGroups = GetKindGroups(2, remainingCards);
            hasRank = ArrangeKindComps(allKindGroups, remainingCards, accumHandStructures, results, hasRank);
          
			
            // When code comes here, it means there are nothing else worthy to record, so that put all current into Results.
            // If hasRank is true, it means those process function has already handled those RecursiveXXX for remaning.
            // Only process if hasRank is false, means notthing else to record, so formally process currentHandCandidates.
            if (hasRank == false && currentHandCandidates.Components.Count > 0)
            {
                var newCandidateComps = new SimPokerHandStructure(accumHandStructures);
                newCandidateComps.SetRemainingCards(remainingCards);
                results.Add(newCandidateComps);
            }

            // Process if all remaining cards are not touches (meaning = _allPokerCards.count), so it's nothing. 
            if (hasRank == false && remainingCards.Count == _allPokerCards.Count)
            {
	            var newCandidateComps = new SimPokerHandStructure(accumHandStructures);
	            var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
		            { CompRank = SimCardsCompType.Nothing, Cards = remainingCards };
	            newCandidateComps.AddComp(newHandCandidateData);
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
        private bool ArrangeKindComps_old(List<List<SimPokerCard>> allKindGroups, List<SimPokerCard> remainingCards,
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
        /// Attempts to arrange remaining cards into kind-based components (pairs, triples, etc.).
        /// Part of the recursive hand-splitting logic.
        /// </summary>
        /// <param name="allKindGroups">Cards grouped by rank with at least 2 cards.</param>
        /// <param name="remainingCards">Available cards.</param>
        /// <param name="accumHandStructures">Current state of hand composition.</param>
        /// <param name="results">Output list of complete hand structures.</param>
        /// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
        /// <returns>True if a hand component was successfully added.</returns>
        private bool ArrangeKindComps(List<List<SimPokerCard>> allKindGroups, List<SimPokerCard> remainingCards,
            SimPokerHandStructure accumHandStructures, List<SimPokerHandStructure> results, bool hasRank)
        {
	        // Do filter out any kindGroup count < 2, so we have at least pair in the group. such as pair, threeofkind, fourofkind
	        var allQualifiedGroups = allKindGroups.Where(x => x.Count >= 2).ToList();

	        if (allQualifiedGroups.Count > 0)
	        {
		        // Do loop through allQualifiedGroups to add into currentHandCandidates	
		        foreach (var kindGroup in allQualifiedGroups)
		        {
			        var handType = DetermineCompType(kindGroup.Count, CompType.Kind);
			        var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
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
		        return hasRank;    
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
		/// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeFlushOrFlushStraight(List<List<SimPokerCard>> flushGroups, List<SimPokerCard> remainingCards,
            SimPokerHandStructure accumHandStructures, List<SimPokerHandStructure> results, bool hasRank)
        {
	        
			foreach (var flushGroup in flushGroups)
			{
				var flushGroupForStraightFinding = flushGroup.Select(card => new List<SimPokerCard> { card }).ToList();

				//var stillFlushCandidate = false;
				var allStraightClusters = GetAllStraightCluster(_minFlushStraightCards, flushGroupForStraightFinding);

				if (allStraightClusters.Count > 0)
				{

					//var accumHandStructuresCopy = new SimPokerHandStructure(accumHandStructures);

					//var accumAllRepresentedStraightCards = new List<SimPokerCard>();
					//var componentAddCount = 0;
					foreach (var straightCluster in allStraightClusters)
					{
						// Loop through allStraightClusters and select the first item from each sub list to form a straight
						var straight = straightCluster.Select(subList => subList[0]).ToList();
						//accumAllRepresentedStraightCards.AddRange(straight);
						var handType = DetermineCompType(straight.Count, CompType.FlushStraight);
						var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
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
						//accumHandStructures = new SimPokerHandStructure(accumHandStructuresCopy);
						var handType = DetermineCompType(flushGroup.Count, CompType.Flush);
						var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
							{ CompRank = handType, Cards = flushGroup };
						accumHandStructures.AddComp(newHandCandidateData);
						var newRemainCards = UtilFunc.GetExcludeList(remainingCards, flushGroup,
							new PokerCardComparer());
						RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
						accumHandStructures.RemoveLast();
					}
					
					hasRank = true;
				}
				else if (flushGroup.Count >= _minFlushCards )
				{
					var handType = DetermineCompType(flushGroup.Count, CompType.Flush);
					var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
						{ CompRank = handType, Cards = flushGroup };
					accumHandStructures.AddComp(newHandCandidateData);
					var newRemainCards = UtilFunc.GetExcludeList(remainingCards, flushGroup,
						new PokerCardComparer());
					RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
					accumHandStructures.RemoveLast();
					hasRank = true;
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
		/// <param name="accumHandStructures">Current state of hand composition.</param>
		/// <param name="results">Output list of complete hand structures.</param>
		/// <param name="hasRank">A flag indicating if any valid hand component was found in this branch.</param>
		/// <returns>True if a hand component was successfully added.</returns>
		private bool ArrangeStraightComps(List<List<List<SimPokerCard>>> allStraightClusters, List<SimPokerCard> remainingCards,
            SimPokerHandStructure accumHandStructures, List<SimPokerHandStructure> results, bool hasRank)
        {
	        
	        if (allStraightClusters.Count == 0)
	        {
	            return hasRank;
	        }
	        
	        var accumAllRepresentedStraightCards = new List<SimPokerCard>();
	        foreach (var straightCluster in allStraightClusters)
	        {
				// Loop through allStraightClusters and select the first item from each sub list to form a straight
				var straight = straightCluster.Select(subList => subList[0]).ToList();
				accumAllRepresentedStraightCards.AddRange(straight);
				var isAllSameSuit = straight.All(card => card.Suit == straight[0].Suit);
				var compType = isAllSameSuit ? CompType.FlushStraight : CompType.Straight;
				var handType = DetermineCompType(straight.Count, compType);
				var newHandCandidateData = new PokerCardComponent<SimCardsCompType, SimPokerCard>
						{ CompRank = handType, Cards = straight };
				accumHandStructures.AddComp(newHandCandidateData);
				var newRemainCards = UtilFunc.GetExcludeList(remainingCards, accumAllRepresentedStraightCards, new PokerCardComparer());
				RecursiveArrangeHands(newRemainCards, accumHandStructures, results);
				accumHandStructures.RemoveLast();
	        }
	        
	        
            return true;
        }
        
        
        private List<List<SimPokerCard>> GetKindGroups(int minCardCountInGroup, List<SimPokerCard> noneJokerCards)
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

        

        /// <summary>
        /// Identifies clusters of consecutive card ranks that are long enough to potentially form straights.
        /// It handles Ace-low logic by duplicating Ace groups as rank 1 if necessary.
        /// </summary>
        /// <param name="straightCount">The minimum required length for a straight.</param>
        /// <param name="kindGroupList">List of cards grouped by their numerical rank.</param>
        /// <returns>A list of clusters, where each cluster is a list of consecutive rank groups.</returns>
        /// So the return would be like this <{<8>, <7,7>}, {<4>, <3,3>, <2,2>}>
        private static List<List<List<SimPokerCard>>> GetAllStraightCluster(int straightCount, List<List<SimPokerCard>> kindGroupList)
        {
	        if (kindGroupList.Count == 0)
	        {
		        return new List<List<List<SimPokerCard>>>();
	        } 
	        
	        // if we have ace kind group, we copy them in the bottom of kind group list and make all ace becomes "1" 
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
	        // The following is to collect all possible straight clusters. Each cluster contains continuous of kind groups.
	        // Initially the accum is empty, so add the first kindGroup as the initial accum.
	        // Then check if the next kind group's represented number is neighebor of accum last group's number.
	        // If it is, then straight continutes, and add that new kind group into accum's last claster, if not
	        // add that new kind group as new accum's list. and go on.to achieve all possible straight clusters.
	        // Then if the input is <<8>,<7,7>, <4>, <3,3>, <2,2>>, then the return would be
	        // <{<8>, <7,7>}, {<4>, <3,3>, <2,2>}>
	        var numberClusters = kindGroupList
		        .Aggregate(new List<List<List<SimPokerCard>>>(), (accum, numGroup) =>
		        {
			        // check if empty initial accum is empty, then start the input numGroup as accum,
			        // of 
			        if (accum.Count == 0 || accum.Last().Last()[0].Number - numGroup[0].Number != 1)
				        accum.Add(new List<List<SimPokerCard>> { numGroup });
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
