using CachingWithSQL.DatabaseContext;
using CachingWithSQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CachingWithSQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMemoryCache cache;

        public CustomersController(ApplicationDbContext context, IMemoryCache cache)
        {
            this.context = context;
            this.cache = cache;
        }

        private const string CacheKey = "CustomerList";

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            if (!cache.TryGetValue(CacheKey, out IEnumerable<Customer> customers))
            {
                //Data not in cache. fetch from db
                customers = await context.Customers.ToListAsync();

                //define Expiration Policy
                var cacheOpt = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };

                //Store data in cache
                cache.Set(CacheKey, customers, cacheOpt);

                //var cachedData = GetCachedData();  // see cached data

                return Ok(new { source = "database", data = customers });
            }

            //Return cache data
            return Ok(new { source = "cache", data = customers });
        }

        [HttpPost("refresh-cache")]
        public async Task<IActionResult> RefreshCache()
        {
            var customers = await context.Customers.ToListAsync();

            var cacheOpt = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };

            cache.Set(CacheKey, customers, cacheOpt);
            return Ok(new { message = "cache refreshed", data = customers });
        }

        /*public IEnumerable<Customer> GetCachedData()
        {
            string CacheKey = "CustomerList";

            // Try to get the data from the cache
            if (cache.TryGetValue(CacheKey, out IEnumerable<Customer> cachedData))
            {
                return cachedData;  // Return the cached data if available
            }

            return null;  // No cached data found
        }*/
    }
}
