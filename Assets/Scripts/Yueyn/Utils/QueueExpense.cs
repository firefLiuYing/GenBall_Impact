using System.Collections;
using System.Collections.Generic;

namespace Yueyn.Utils
{
    public static class QueueExpense
    {
        public static bool IsNullOrEmpty<T>(this Queue<T> queue)=>queue == null || queue.Count == 0;
    }
}