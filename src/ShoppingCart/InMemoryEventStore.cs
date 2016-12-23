using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCart
{
    public class InMemoryEventStore : IEventStore
    {
        private static long currentSequenceNumber = 0;
        private static readonly IList<Event> database = new List<Event>();

        public Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            var result=database
              .Where(e =>
                e.SequenceNumber >= firstEventSequenceNumber &&
                e.SequenceNumber <= lastEventSequenceNumber)
              .OrderBy(e => e.SequenceNumber);
            return Task.FromResult<IEnumerable<Event>>(result);
        }

        public Task Raise(string eventName, object content)
        {
            var seqNumber = Interlocked.Increment(ref currentSequenceNumber);
            database.Add(
              new Event(
                seqNumber,
                DateTimeOffset.UtcNow,
                eventName,
                content));
            return Task.CompletedTask;
        }

       
    }
}
