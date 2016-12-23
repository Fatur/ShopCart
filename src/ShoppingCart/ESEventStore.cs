using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingCart
{
    public class ESEventStore : IEventStore
    {
        //private const string connectionString ="discover://http://127.0.0.1:2113/";
        private IEventStoreConnection connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback,2113));
        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            await connection.ConnectAsync().ConfigureAwait(false);
            var result = await connection.ReadStreamEventsForwardAsync(
                "ShoppingCart",
                start: (int)firstEventSequenceNumber,
                count: (int)(lastEventSequenceNumber - firstEventSequenceNumber),
                resolveLinkTos: false).ConfigureAwait(false);
            return result.Events
                        .Select(ev =>
                            new
                            {
                                Content=JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ev.Event.Data)),
                                MetaData=JsonConvert.DeserializeObject<EventMetaData>(Encoding.UTF8.GetString(ev.Event.Data))
                            })
                         .Select((ev,i)=>
                                new Event(
                                    i+firstEventSequenceNumber,
                                    ev.MetaData.OccuredAt,
                                    ev.MetaData.EventName,
                                    ev.Content));
        }

        public async Task Raise(string eventName, object content)
        {
            await connection.ConnectAsync().ConfigureAwait(false);
            var contentJson = JsonConvert.SerializeObject(content);
            var metaDataJson = JsonConvert.SerializeObject(new EventMetaData
            {
                OccuredAt = DateTimeOffset.Now,
                EventName = eventName
            });
            var eventData = new EventData(Guid.NewGuid(),"ShoppingCartEvent",isJson: true, 
                data: Encoding.UTF8.GetBytes(contentJson),
                metadata: Encoding.UTF8.GetBytes(metaDataJson));
            await connection.AppendToStreamAsync("ShoppingCart", ExpectedVersion.Any, eventData);
        }
    }

    internal class EventMetaData
    {
        public string EventName { get; set; }
        public DateTimeOffset OccuredAt { get; set; }
    }
}
