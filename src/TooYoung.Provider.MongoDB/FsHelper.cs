using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace TooYoung.Provider.MongoDB
{
    public static class FsHelper
    {
        public static FSharpAsync<T> ToAsync<T>(this Task<T> task)
        {
            return FSharpAsync.AwaitTask(task);
        }
        
        public static FSharpAsync<Unit> ToAsync(this Task task)
        {
            return FSharpAsync.AwaitTask(task);
        }

        public static FSharpList<T> ToFsList<T>(this IEnumerable<T> seq)
        {
            return ListModule.OfSeq(seq);
        }
    }
}