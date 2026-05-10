using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GenericPoker
{
    static class IListExtensions
    {
        public static void Swap<T>(
            this IList<T> list,
            int firstIndex,
            int secondIndex
        )
        {
            Contract.Requires(list != null);
            Contract.Requires(firstIndex >= 0 && firstIndex < list.Count);
            Contract.Requires(secondIndex >= 0 && secondIndex < list.Count);
            if (firstIndex == secondIndex)
            {
                return;
            }
            T temp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = temp;
        }
    }

    public class UtilFunc
    {
        
        // return any random number from min to max (including min and max)
        public static int GetRandomInt(int min, int max)
        {
            Random rng = new Random(12345); // Create a new instance of Random
            return rng.Next(min, max+1);
        }
        
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
        
        
        public static int SelectRange(int inputValue, Dictionary<(int, int), int> rangeDict) 
        {
            foreach (var kvp in rangeDict)
            {
                var range = kvp.Key;
                if (inputValue >= range.Item1 && inputValue < range.Item2)
                {
                    return kvp.Value;
                }
            }
            return 0;
        }
        
        public static void CustomPrint(bool verbose = true, params object[] args)
        {
            if (verbose)
            {
                Console.WriteLine(string.Join(" ", args));
            }
        }
        
    }
}