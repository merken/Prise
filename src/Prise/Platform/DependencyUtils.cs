using System.Collections.Generic;

namespace Prise
{
    public static class DependencyUtils
    {
        public static IEnumerable<T> AddRangeToList<T>(this List<T> list, IEnumerable<T> range)
        {
            if (range != null)
                list.AddRange(range);
            return list;
        }
    }
}
