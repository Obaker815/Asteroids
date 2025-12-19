namespace Asteroids
{
    internal static class Sorting
    {
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
                        GetValue(ToSort[inverse? i + 1: i]), 
                        GetValue(ToSort[inverse? i: i + 1])) > 0)
                    {
                        // Tuple swap
                        (ToSort[i], ToSort[i + 1]) = (ToSort[i + 1], ToSort[i]);
                        // Increment swap count
                        swaps++;
                    }
            }
            return ToSort;
        }
    }
}
