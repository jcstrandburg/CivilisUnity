using System.Collections.Generic;

namespace Neolithica.Utility
{
    public static class ListUtility
    {
        public static List<T> From<T>(params T[] elements)
        {
            return new List<T>(elements);
        }
    }
}