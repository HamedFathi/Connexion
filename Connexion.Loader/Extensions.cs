using System.Collections.Generic;
using System.Linq;

namespace Connexion.Loader
{
    internal static class Extensions
    {
        internal static IEnumerable<string> GetDuplicates(this IEnumerable<string> list)
        {
            var duplicates = list.GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key);
            return duplicates;
        }
    }
}
