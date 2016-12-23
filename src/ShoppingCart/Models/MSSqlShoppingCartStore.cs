using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace ShoppingCart.Models
{
    public class MSSqlShoppingCartStore : IShoppingCartStore
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=ShoppingCart;
                                            Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                            TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private const string sqlStatement = @"SELECT * from ShoppingCart, ShoppingCartItems 
                                            WHERE ShoppingCartItems.ShoppingCartId = ShoppingCart.ID
                                            AND ShoppingCart.UserId=@UserId";
        public async Task<ShoppingCart> Get(int userId)
        {
           using(var conn=new SqlConnection(connectionString))
            {
                var items = await conn.QueryAsync<ShoppingCartItem>(sqlStatement, new { UserId = userId });
                return new ShoppingCart(userId, items);
            }
        }
        private const string deleteAllForShoppingCartSql =
                                         @"delete item from ShoppingCartItems item
                                        inner join ShoppingCart cart on item.ShoppingCartId = cart.ID
                                        and cart.UserId=@UserId";
        private const string addAllForShoppingCartSql =
                                        @"insert into ShoppingCartItems 
                                        (ShoppingCartId, ProductCatalogId, ProductName, 
                                        ProductDescription, Amount, Currency)
                                        values 
                                        (@ShoppingCartId, @ProductCatalogId, @ProductName,
                                        @ProductDescription, @Amount, @Currency)";

        public async Task Save(ShoppingCart cart)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    await conn.ExecuteAsync(
                      deleteAllForShoppingCartSql,
                      new { UserId = cart.UserId },
                      tx).ConfigureAwait(false);
                    await conn.ExecuteAsync("Delete cart From ShoppingCart cart Where cart.UserId=@UserId",
                        new { UserId = cart.UserId },
                        tx).ConfigureAwait(false);
                    var newId = await conn.QueryFirstOrDefaultAsync<int>("Insert into ShoppingCart (UserId) values (@UserId);SELECT CAST(SCOPE_IDENTITY() as int)",
                        new { UserId = cart.UserId },
                        tx).ConfigureAwait(false);
                    await conn.ExecuteAsync(
                      addAllForShoppingCartSql,
                      ConvertToParameter(newId,cart.Items),
                      tx).ConfigureAwait(false);
                    tx.Commit();
                }
            }
        }

        private IEnumerable<object> ConvertToParameter(int id,IEnumerable<ShoppingCartItem> items)
        {
            return items.Select(x => new
            {
                ShoppingCartId = id,
                ProductCatalogId = x.ProductCatalogId,
                ProductName = x.ProductName,
                ProductDescription = x.ProductDescription,
                Amount = x.Price.Amount,
                Currency = x.Price.Currency
            });
        }
    }
}
