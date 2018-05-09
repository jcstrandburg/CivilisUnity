using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Neolithica.Extensions {
    public static class EnumerableExtensions {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source) =>
            source as IReadOnlyList<T> ?? new ReadOnlyCollection<T>(source.ToList());
    }
}