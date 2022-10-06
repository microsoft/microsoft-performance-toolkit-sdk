using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Performance.SDK
{
    internal class Utils
    {
        internal static bool EnumerableComparer<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            bool nullList1 = list1 == null;
            bool nullList2 = list2 == null;

            if (list1 == null && list2 == null)
            {
                return true;
            }
            else if ((list1 == null) != (list2 == null))
            {
                return false;
            }

            var length1 = list1.Count();
            var length2 = list2.Count();
            if (!length1.Equals(length2))
            {
                return false;
            }

            for (int idx = 0; idx < length1; idx++)
            {
                if (!list1.ElementAt(idx).Equals(list2.ElementAt(idx)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
