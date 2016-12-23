using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private IProductStore m_store;
        public ProductsController(IProductStore store)
        {
            this.m_store = store;
        }
        [HttpGet]
        public IActionResult GetProducts(int[] productIds)
        {

            var products = m_store.GetProductsByIds(productIds);
            this.Response.Headers.Add("cache-control", "max-age=86400");
            return new ObjectResult(products);
        }
    }
}
