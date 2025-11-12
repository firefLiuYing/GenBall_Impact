using System.Collections.Generic;

namespace Yueyn.Utils
{
    public static class ListExpense
    {
        public static bool IsNullOrEmpty<T>(this List<T> list)=>list == null || list.Count == 0;
    }
}