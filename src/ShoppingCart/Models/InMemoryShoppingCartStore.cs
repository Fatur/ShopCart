using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class InMemoryShoppingCartStore : IShoppingCartStore
    {
        private static readonly Dictionary<int, ShoppingCart> database = new Dictionary<int, ShoppingCart>();
        public Task<ShoppingCart> Get(int userId)
        {
            if (!database.ContainsKey(userId))
                database[userId] = new ShoppingCart(userId);
            return Task.FromResult<ShoppingCart>(database[userId]);
        }

        public Task Save(ShoppingCart cart)
        {
            database[cart.UserId] = cart;
            return Task.CompletedTask;
        }
    }
}
