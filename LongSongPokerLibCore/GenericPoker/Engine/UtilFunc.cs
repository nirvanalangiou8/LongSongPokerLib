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

        /// <summary>
        /// ====================================================================================================
        /// INTEGER PARTITION GENERATION
        /// ====================================================================================================
        /// Recursively generates all integer partitions of a given total number such that:
        /// 1. Each partition part is greater than or equal to <paramref name="minCount"/>.
        /// 2. The exact sum of all elements in each sublist (partition) equals the original total number.
        /// 3. Incomplete sublists whose sum is less than the original input number are excluded.
        ///
        /// ----------------------------------------------------------------------------------------------------
        /// EXAMPLE CASES:
        /// ----------------------------------------------------------------------------------------------------
        /// Case 1: Total = 9, minCount = 3 for flushstraight
        /// Allowed / Generated partitions:
        ///   - [9]
        ///   - [6, 3]
        ///   - [5, 4]
        ///   - [3, 3, 3]
        /// Disallowed / Excluded partitions (sum &lt; 9 or part &lt; 3): 
        ///   - [8], [7], [6], [5], [4], [3] (sum &lt; 9)
        ///   - [5, 3] (sum = 8 &lt; 9)
        ///   - [6, 2, 1] (contains parts &lt; 3)
        ///
        /// Case 2: Total = 3, minCount = 2, normally for Kindgroup. we set allow partial subset as true. If not, we will only allow [3].
        ///  Then if we have 3 three of kind, we will miss the threeofkind+ one pair of fullhouse combination as threeofkind can not break into as one pair (count = 2)
        /// Allowed / Generated partitions:
        ///   - [3]
        ///   - [2] is allowed.
        
        
        /// ====================================================================================================
        /// </summary>
        /// <typeparam name="T">The numeric type of elements in the output partitions (e.g. int, long, double).</typeparam>
        /// <param name="remaining">The remaining value to partition in recursive steps (initially total number).</param>
        /// <param name="maxVal">The maximum value allowed for the next part (to maintain non-increasing order and avoid duplicates).</param>
        /// <param name="current">The current running accumulation of partition parts.</param>
        /// <param name="partitions">The output collection where valid full partitions are recorded.</param>
        /// <param name="minCount">The minimum threshold for each individual partition part (default is 1).</param>
        /// <param name="bAllowSubSet">Let the caller decide if we can allow subset.
        public static void GeneratePartitions<T>(int remaining, int maxVal, List<T> current, List<List<T>> partitions, int minCount = 1, bool bAllowSubSet = false)
        {
           
            if (remaining < minCount)
            {
                if (bAllowSubSet)
                {
                    partitions.Add(new List<T>(current));

                }
                else
                {
                    if (remaining == 0)
                    {
                        partitions.Add(new List<T>(current));
                    }
                }
                return;
            }
            
            for (int i = Math.Min(remaining, maxVal); i >= minCount; i--)
            {
                current.Add((T)Convert.ChangeType(i, typeof(T)));
                GeneratePartitions(remaining - i, i, current, partitions, minCount, bAllowSubSet);
                current.RemoveAt(current.Count - 1);
            }
        }
        
        /*
        private static void GeneratePartitions(int remaining, int maxVal, List<int> current, List<List<int>> partitions, int minCards = 1)
        {
            if (remaining < minCards)
            {
                partitions.Add(new List<int>(current));
                return;
            }

            for (int i = Math.Min(remaining, maxVal); i >= minCards; i--)
            {
                current.Add(i);
                GeneratePartitions(remaining - i, i, current, partitions, minCards);
                current.RemoveAt(current.Count - 1);
            }
        }*/

        /// <summary>
        /// Convenience helper that generates all valid integer partitions of <paramref name="total"/>
        /// where each part is >= <paramref name="minCount"/> and the sum of parts exactly equals <paramref name="total"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of elements in the output partitions (e.g. int, long, double).</typeparam>
        /// <param name="total">The total integer value to partition.</param>
        /// <param name="minCount">The minimum value for each partition part (default is 1).</param>
        /// <returns>A list of valid partitions, where each partition is a list of type <typeparamref name="T"/>.</returns>
        public static List<List<T>> GeneratePartitions<T>(int total, int minCount = 1)
        {
            var partitions = new List<List<T>>();
            if (total < minCount || total <= 0)
            {
                return partitions;
            }

            GeneratePartitions(total, total, new List<T>(), partitions, minCount);
            return partitions;
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