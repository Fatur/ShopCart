using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Polly;
using ShoppingCart.Controllers;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShoppingCart
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private static Policy exponentialRetryPolicy =
         Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), (ex, _) => Console.WriteLine(ex.ToString()));

        private static string productCatalogueBaseUrl = @"http://localhost:5002";
        private static string getProductPathTemplate = "/api/products?{0}";
        private readonly ICache m_cache = null;

        public ProductCatalogClient(ICache cache)
        {
            this.m_cache = cache;
        }
        public Task<IEnumerable<ShoppingCartItem>> 
            GetCartItems(int[] productCatalogueIds)=>
                exponentialRetryPolicy
                    .ExecuteAsync(async () =>await GetItemsFromCatalogueService(productCatalogueIds).ConfigureAwait(false));

        private async Task<IEnumerable<ShoppingCartItem>>
          GetItemsFromCatalogueService(int[] productCatalogueIds)
        {
            var response = await
              RequestProductFromProductCatalogue(productCatalogueIds).ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        private  async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
        {
            var arrayParams=productCatalogueIds.Select(id =>String.Format("productids={0}",id)).ToArray();
            var productsResource = string.Format(getProductPathTemplate, string.Join("&", arrayParams));
            var response = this.m_cache.Get(productsResource) as HttpResponseMessage;
            if (response == null)
                response = await GetFromProductCatalogService(productsResource, response);
            return response;
        }

        private async Task<HttpResponseMessage> GetFromProductCatalogService(string productsResource, HttpResponseMessage response)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(productCatalogueBaseUrl);
                response = await httpClient.GetAsync(productsResource).ConfigureAwait(false);
                AddToCache(productsResource, response);
            }

            return response;
        }

        private void AddToCache(string productsResource, HttpResponseMessage response)
        {
            var cacheHeader = response
                .Headers
                .FirstOrDefault(h => h.Key.ToLower() == "cache-control");
            if (string.IsNullOrEmpty(cacheHeader.Key))
                return;
            var maxAge =
              CacheControlHeaderValue.Parse(cacheHeader.Value.FirstOrDefault())
                .MaxAge;
            if (maxAge.HasValue)
                this.m_cache.Add(key: productsResource, value: response, ttl: maxAge.Value);
        }

        private  async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products =
              JsonConvert.DeserializeObject<List<ProductCatalogueProduct>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return
              products
                .Select(p => new ShoppingCartItem(0,
                  int.Parse(p.ProductId),
                  p.ProductName,
                  p.ProductDescription,
                  p.Price
              ));
        }

        private class ProductCatalogueProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
}
