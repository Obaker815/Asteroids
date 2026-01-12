namespace Asteroids
{
    internal static class Sorting
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1">The type of object in the ToSort array</typeparam>
        /// <typeparam name="T2">The value of each element to be sorted, this allows for types that aren't comparable to be sorted by a value returned by GetValue</typeparam>
        /// <param name="ToSort">The array of T1 objects to be sorted</param>
        /// <param name="GetValue">A function that takes a T1 and returns a T2</param>
        /// <param name="inverse">Boolean flag that will inverse sorting</param>
        /// <returns>The sorted T1 array</returns>
        public static T1[] Bubble<T1, T2>(T1[] ToSort, Func<T1, T2> GetValue, bool inverse = false)
        {
            if (ToSort.Length < 2) return ToSort;

            int swaps = 1;
            int iterations = 0;

            while (swaps > 0)
            {
                iterations++;
                swaps = 0;

                // Iterate through the unsorted values in the array
                for (int i = 0; i < ToSort.Length - iterations; i++)
                    // Compare the two values
                    if (Comparer<T2>.Default.Compare(
                        GetValue(ToSort[inverse? i: i + 1]), 
                        GetValue(ToSort[inverse? i + 1: i])) < 0)
                    {
                        // Tuple swap
                        (ToSort[i], ToSort[i + 1]) = (ToSort[i + 1], ToSort[i]);
                        swaps++;
                    }
            }
            return ToSort;
        }
    }
}
