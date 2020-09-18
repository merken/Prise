using System.Collections.Generic;

namespace Prise.Utils
{
    public static class LinqUtils
    {
        public static List<T> AddRangeToList<T>(this List<T> list, IEnumerable<T> range)
        {
            if (range != null)
                list.AddRange(range);
            return list;
        }

        public static List<T> AddToList<T>(this List<T> list, params T[] itemsToAdd) => list.AddRangeToList(itemsToAdd);
    }
}