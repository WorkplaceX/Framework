namespace Framework
{
    using Framework.Server;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal static class UtilStopwatch
    {
        /// <summary>
        /// (RequestId, (RequestPath), Collection).
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, Collection> RequestIdToStopwatchCollectionList = new ConcurrentDictionary<Guid, Collection>();

        /// <summary>
        /// Stopwatch collection for (RequestId, RequestPath). If more than one request of same path has to be processed, a second stopwatch Collection class is created.
        /// </summary>
        private class Collection
        {
            public Collection()
            {
                this.Id = Interlocked.Increment(ref id); // Thread safe counter
            }

            private static int id = 0;
        
            public int Id;

            /// <summary>
            /// Gets NavigatePath. This stopwatch Collection is used only for this NavigatePath (request) path.
            /// </summary>
            public string NavigatePath;

            /// <summary>
            /// Gets RequestCount. Number of requests this stopwatch Collection has been used for.
            /// </summary>
            public int RequestCount;

            /// <summary>
            /// (Name, Stopwatch).
            /// </summary>
            public readonly Dictionary<string, Item> List = new Dictionary<string, Item>();

            /// <summary>
            /// Gets or sets IsReleased. If true, StopwatchCollection has been released and can be reaused.
            /// </summary>
            public bool IsReleased;

            /// <summary>
            /// Gets LogText. Used to add some logging information at the end of request.
            /// </summary>
            public StringBuilder LogText = new StringBuilder();
        }

        private class Item
        {
            public Stopwatch Stopwatch;

            /// <summary>
            /// Gets IsStart. Ensure sequence start, stop, start, stop is always correct.
            /// </summary>
            public bool IsStart;

            /// <summary>
            /// Gets StartCount. Number of times this stopwatch has been started. It is resetted every 10 requests.
            /// Same stopwatch (name) can be started and stopped multiple times in one request. 
            /// </summary>
            public int StartCount;
        }

        private static Collection CollectionCurrent
        {
            get
            {
                var result = RequestIdToStopwatchCollectionList.GetOrAdd(requestId.Value, (key) =>
                {
                    var released = RequestIdToStopwatchCollectionList.Where(item => item.Value.IsReleased == true && item.Value.NavigatePath == UtilServer.Context.Request.Path.Value).FirstOrDefault();
                    if (!released.Equals(default(KeyValuePair<Guid, Collection>))) // released != null
                    {
                        released.Value.IsReleased = false;
                        if (RequestIdToStopwatchCollectionList.TryRemove(released.Key, out Collection stopwatchCollection))
                        {
                            UtilFramework.Assert(released.Value == stopwatchCollection);
                            return stopwatchCollection;
                        }
                    }
                    return new Collection { NavigatePath = UtilServer.Context?.Request.Path.Value }; // Also called by command cli deployDb
                });

                return result;
            }
        }

        private static Dictionary<string, Item> StopwatchList
        {
            get
            {
                return CollectionCurrent.List;
            }
        }

        internal static void TimeStart(string name)
        {
            if (!StopwatchList.ContainsKey(name))
            {
                StopwatchList[name] = new Item { Stopwatch = new Stopwatch() };
            }
            UtilFramework.Assert(StopwatchList[name].IsStart == false);
            StopwatchList[name].Stopwatch.Start();
            StopwatchList[name].IsStart = true;
            StopwatchList[name].StartCount += 1;
        }

        internal static void TimeStop(string name)
        {
            UtilFramework.Assert(StopwatchList[name].IsStart == true);
            StopwatchList[name].Stopwatch.Stop();
            StopwatchList[name].IsStart = false;
        }

        /// <summary>
        /// Write logging information to the end of the request.
        /// </summary>
        internal static void Log(string text)
        {
            CollectionCurrent.LogText.AppendLine(text);
        }

        /// <summary>
        /// Log stopwatch to console.
        /// </summary>
        internal static void TimeLog()
        {
            int pathCount = RequestIdToStopwatchCollectionList.Where(item => item.Value.NavigatePath == UtilServer.Context.Request.Path.Value).Count();
            var collection = CollectionCurrent;

            StringBuilder result = new StringBuilder();

            result.AppendLine(string.Format("CollectionId={0}/{1}; Path={2}; RequestCount={3}; PathCount={4};", collection.Id, RequestIdToStopwatchCollectionList.Count, collection.NavigatePath, collection.RequestCount, pathCount));
            foreach (var item in collection.List.OrderBy(item => item.Key)) // Order by stopwatch name.
            {
                // Calculate average time per max 10 requests.
                double second = ((double)item.Value.Stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) / collection.RequestCount;
                result.AppendLine(string.Format("Time={0:000.0}ms; Name={1}; StartCount={2};", second * 1000, item.Key, item.Value.StartCount));
            }

            result.AppendLine(collection.LogText.ToString());

            UtilServer.Logger(typeof(UtilStopwatch).Name).LogInformation(result.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        /// <summary>
        /// Gets requestId. Id of current request.
        /// </summary>
        private static readonly AsyncLocal<Guid> requestId = new AsyncLocal<Guid>();

        /// <summary>
        /// Bind unused stopwatch to this request.
        /// </summary>
        internal static void RequestBind()
        {
            UtilFramework.Assert(requestId.Value == Guid.Empty);
            requestId.Value = Guid.NewGuid();

            var collection = CollectionCurrent;
            collection.RequestCount += 1;
            collection.LogText.Clear();

            // Average time is calculated based on max every 10 requests.
            if (CollectionCurrent.RequestCount > 10)
            {
                CollectionCurrent.RequestCount = 1;
                foreach (var item in CollectionCurrent.List)
                {
                    item.Value.Stopwatch.Reset();
                    item.Value.StartCount = 0;
                }
            }
        }

        /// <summary>
        /// Release stopwatch used for this request.
        /// </summary>
        internal static void RequestRelease()
        {
            RequestIdToStopwatchCollectionList[requestId.Value].IsReleased = true;
            foreach (var item in RequestIdToStopwatchCollectionList[requestId.Value].List)
            {
                item.Value.IsStart = false;
                item.Value.Stopwatch.Stop();
            }
        }
    }
}
