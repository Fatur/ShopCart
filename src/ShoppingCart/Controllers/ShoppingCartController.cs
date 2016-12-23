using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers
{
    [Route("api/[controller]")]
    public class ShoppingCartController : Controller
    {
        private IShoppingCartStore m_shopCartStore = null;
        private IProductCatalogClient m_catalog = null;
        private IEventStore m_eventStore = null;
        public ShoppingCartController(IShoppingCartStore store, IProductCatalogClient catalog, IEventStore eventStore)
        {
            this.m_shopCartStore = store;
            this.m_catalog = catalog;
            this.m_eventStore = eventStore;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(int userId)
        {
            var cart = await this.m_shopCartStore.Get(userId);
            if (cart == null)
                return NotFound();
            return new ObjectResult(cart);
        }
        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddItems(int userId, [FromBody] int[] productIds)
        {
            var cart = await m_shopCartStore.Get(userId);
            var cartItems = await m_catalog.GetCartItems(productIds).ConfigureAwait(false);
            cart.AddItems(cartItems,m_eventStore);
            await m_shopCartStore.Save(cart);
            return CreatedAtRoute(new { userId = userId }, cart);
        }
        [HttpDelete("{userId}/items")]
        public async Task<IActionResult> DeleteItem(int userId,[FromBody] int[] productIds)
        {
            var cart = await m_shopCartStore.Get(userId);
            if (cart == null) return NotFound();

            cart.RemoveItems(productIds, this.m_eventStore);
            await m_shopCartStore.Save(cart);
            return new NoContentResult();
        }
    }
}
