using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GenericPoker
{


    public class UtilFunc
    {
        
        // return any random number from min to max (including min and max)
        public static int GetRandomInt(int min, int max)
        {
            Random rng = new Random(12345); // Create a new instance of Random
            return rng.Next(min, max+1);
        }
        
        
        public static List<List<T>> GetPermutation<T>(List<T> inputList, int selectCount)
        {
            var resultList = new List<List<T>>();
            RecursivePermute(inputList, selectCount, new List<T>(), resultList);
            return resultList;
        }
        
        public static void RecursivePermute<T>( List<T> inputList, int selectCount, List<T> currentList, List<List<T>> resultList)
        {
            if (currentList.Count + inputList.Count < selectCount) return;
            
            if (currentList.Count >= selectCount)
            {
                // currentList is shared, so need to record at the moment when you added. If not, other hierachy loop
                // will containminate it.
                resultList.Add(new List<T>(currentList));
                return;
            }
            
            for(var i = 0; i < inputList.Count; i++)
            {
                currentList.Add(inputList[i]);
                
                // Get the sublist starting after the current item to loop through hierachically.
                var remainingList = inputList.Skip(i + 1).ToList();
                
                RecursivePermute(remainingList, selectCount, currentList, resultList);
                // remove previous add , so this current list shared in hierarchically. Doing so, it will just not
                // contaminate the recursive hierarchical visit. Each level remove one as each level only add one
                // item in the currentList. 
                currentList.RemoveAt(currentList.Count-1);
            }
        }

        public static List<T> GetExcludeList<T>(List<T> wholeList, List<T> excludeList, IEqualityComparer<T> comparer = null) 
        {
            HashSet <T> wholeSet = new HashSet<T>(wholeList, comparer);
            HashSet <T> excludeSet = new HashSet<T>(excludeList, comparer);
            wholeSet.ExceptWith(excludeSet);
            return new List<T>(wholeSet);
        }

        public static List<(List<T> Selected, List<T> Remaining)> GetPermutationAllowedDuplicated<T>(List<T> inputList, int selectCount)
        {
            var resultList = new List<(List<T> Selected, List<T> Remaining)>();
            RecursivePermuteAllowedDuplicated(inputList, 0, selectCount, new List<T>(), new List<T>(inputList), resultList);
            return resultList;
        }

        private static void RecursivePermuteAllowedDuplicated<T>(List<T> wholeList, int startIndex, int selectCount, List<T> currentSelected, List<T> currentRemaining, List<(List<T> Selected, List<T> Remaining)> resultList)
        {
            if (currentSelected.Count == selectCount)
            {
                resultList.Add((new List<T>(currentSelected), new List<T>(currentRemaining)));
                return;
            }

            for (int i = startIndex; i < wholeList.Count; i++)
            {
                var item = wholeList[i];
                currentSelected.Add(item);
                
                var nextRemaining = new List<T>(currentRemaining);
                int indexInRemaining = nextRemaining.IndexOf(item); 
                if (indexInRemaining >= 0)
                {
                    nextRemaining.RemoveAt(indexInRemaining);
                }

                RecursivePermuteAllowedDuplicated(wholeList, i + 1, selectCount, currentSelected, nextRemaining, resultList);
                
                currentSelected.RemoveAt(currentSelected.Count - 1);
            }
        }

        public static List<List<List<T>>> GenerateSlotPartitions<T>(List<T> items, int slotCount)
        {
            var results = new List<List<List<T>>>();
            var current = new List<List<T>>();
            for (int i = 0; i < slotCount; i++)
            {
                current.Add(new List<T>());
            }

            void PartitionHelper(int itemIndex)
            {
                if (itemIndex == items.Count)
                {
                    var copy = current.Select(slot => new List<T>(slot)).ToList();
                    results.Add(copy);
                    return;
                }

                var item = items[itemIndex];
                for (int slotIdx = 0; slotIdx < slotCount; slotIdx++)
                {
                    current[slotIdx].Add(item);
                    PartitionHelper(itemIndex + 1);
                    current[slotIdx].RemoveAt(current[slotIdx].Count - 1);
                }
            }

            PartitionHelper(0);
            return results;
        }

        /// <summary>
        /// Filters out candidate items whose components are dominated by another item with the same component count.
        /// Item S2 dominates S1 (so S1 is removed) if for all component positions k: S2[k] >= S1[k],
        /// and for at least one position: S2[k] > S1[k].
        /// Optional compatibilityCheck ensures components at each position can be meaningfully compared for dominance.
        /// </summary>
        public static List<TItem> FilterDominated<TItem, TComponent>(
            IEnumerable<TItem> items,
            Func<TItem, IReadOnlyList<TComponent>> componentSelector,
            IComparer<TComponent>? comparer = null,
            Func<TComponent, TComponent, bool>? compatibilityCheck = null)
        {
            if (items == null) return new List<TItem>();
            var cmp = comparer ?? Comparer<TComponent>.Default;

            var itemList = items.ToList();
            var filtered = new List<TItem>();

            for (int i = 0; i < itemList.Count; i++)
            {
                var item1 = itemList[i];
                var comps1 = componentSelector(item1);
                bool isDominated = false;

                for (int j = 0; j < itemList.Count; j++)
                {
                    if (i == j) continue;
                    var item2 = itemList[j];
                    var comps2 = componentSelector(item2);

                    if (comps1.Count != comps2.Count) continue;

                    bool compatible = true;
                    if (compatibilityCheck != null)
                    {
                        for (int k = 0; k < comps1.Count; k++)
                        {
                            if (!compatibilityCheck(comps1[k], comps2[k]))
                            {
                                compatible = false;
                                break;
                            }
                        }
                    }

                    if (!compatible) continue;

                    bool allGreaterOrEqual = true;
                    bool atLeastOneStrictlyGreater = false;

                    for (int k = 0; k < comps1.Count; k++)
                    {
                        int compareRes = cmp.Compare(comps2[k], comps1[k]);
                        if (compareRes < 0)
                        {
                            allGreaterOrEqual = false;
                            break;
                        }
                        if (compareRes > 0)
                        {
                            atLeastOneStrictlyGreater = true;
                        }
                    }

                    if (allGreaterOrEqual && atLeastOneStrictlyGreater)
                    {
                        isDominated = true;
                        break;
                    }
                }

                if (!isDominated)
                {
                    filtered.Add(item1);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Filters out candidate solutions that are dominated (obviously loser solutions).
        /// A solution S2 dominates S1 if S2 is >= S1 in all positions, and strictly better in at least one position.
        /// </summary>
        public static List<List<T>> FilterDominatedSolutions<T>(
            IEnumerable<List<T>> solutions,
            IComparer<T>? comparer = null)
        {
            if (solutions == null) return new List<List<T>>();

            var cmp = comparer ?? Comparer<T>.Default;
            var list = new List<List<T>>();
            var seen = new HashSet<string>();
            foreach (var sol in solutions)
            {
                if (sol == null) continue;
                var key = string.Join(",", sol.Select(x => x?.ToString() ?? string.Empty));
                if (seen.Add(key))
                {
                    list.Add(sol);
                }
            }

            return FilterDominated(list, sol => sol, cmp);
        }
    }
}


/*
        public static void Shuffle<T>(List<T> list)
        {
            // Iterate over the list from last to first
            for (var i = list.Count - 1; i > 0; i--)
            {
                // Pick a random index from 0 to i
                //int j = rng.Next(0, i + 1);
                int j = GetRandomInt(0, i);

                // Swap list[i] with the element at random index
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
*/