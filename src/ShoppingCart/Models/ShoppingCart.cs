using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class ShoppingCart
    {
        private HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

        public int UserId { get; }
        public IEnumerable<ShoppingCartItem> Items { get { return items; } }

        public ShoppingCart(int userId)
        {
            this.UserId = userId;
        }

        public ShoppingCart(int userId, IEnumerable<ShoppingCartItem> items1) : this(userId)
        {
             foreach(var item in items1)
            {
                this.items.Add(item);
            }
        }

        public void AddItems(
          IEnumerable<ShoppingCartItem> shoppingCartItems,
          IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
            {
                item.ShoppingCartId = this.UserId;
                if (this.items.Add(item))
                    eventStore.Raise(
                      "ShoppingCartItemAdded",
                      new { UserId, item });
            }
        }

        public void RemoveItems(
          int[] productCatalogueIds,
          IEventStore eventStore)
        {
            items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogId));
            eventStore.Raise("ShoppingCartItemDeleted", productCatalogueIds);
        }
    }

    public class ShoppingCartItem
    {
        public int ShoppingCartId { get; set; }
        public int ProductCatalogId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public Money Price { get; }
        public ShoppingCartItem()
        {

        }
        public ShoppingCartItem(
          int shoppingCartId,
          int productCatalogueId,
          string productName,
          string description,
          Money price)
        {
            this.ShoppingCartId = shoppingCartId;
            this.ProductCatalogId = productCatalogueId;
            this.ProductName = productName;
            this.ProductDescription = description;
            this.Price = price;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = obj as ShoppingCartItem;
            return this.ProductCatalogId.Equals(that.ProductCatalogId);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.ProductCatalogId.GetHashCode();
        }
    }

    public class Money
    {
        public string Currency { get; }
        public decimal Amount { get; }

        public Money(string currency, decimal amount)
        {
            this.Currency = currency;
            this.Amount = amount;
        }
    }
}
