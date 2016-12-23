using System;
using System.Collections.Generic;

namespace ProductCatalog
{
    public interface IProductStore
    {
        IEnumerable<ProductCatalogProduct> GetProductsByIds(int[] productIds);
    }


    public class InMemoryProductCatalogStore : IProductStore
    {
        public IEnumerable<ProductCatalogProduct> GetProductsByIds(int[] productIds)
        {
            var list = new List<ProductCatalogProduct>();
            foreach(int id in productIds)
            {
                list.Add(new ProductCatalogProduct() { ProductId = id, ProductName = "foo" + id, ProductDescription = "bar", Price = new Money() });
            }
            return list;
        }
    }
}