using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace ShoppingCart
{
    [Route("api/events")]
    public class EventsFeedController : Controller
    {
        private IEventStore m_store;

        public EventsFeedController(IEventStore store)
        {
            this.m_store = store;
        }
        [HttpGet]
        public async Task<IEnumerable<Event>> Get(long firstSeq = 0, long endSeq = 0)
        {
            return await this.m_store.GetEvents(firstSeq, endSeq);
        }


    }
}
