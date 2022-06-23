using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ReuploaderMod.Misc {
    internal static class GCHelper {
        internal static void Collect() {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        internal static void BlockingCollect() {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);
        }

        internal static async Task BlockingCollectAsync() {
            var memory = GC.GetTotalMemory(false);
            Console.WriteLine($"Allocated memory: {(memory / 1024) / 1024} MiB");
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            if (memory > GC.GetTotalMemory(true))
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Console.WriteLine($"Allocated memory after collection: {(GC.GetTotalMemory(false) / 1024) / 1024} MiB");
        }
    }
}
