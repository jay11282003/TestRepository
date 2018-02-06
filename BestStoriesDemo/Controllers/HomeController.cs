using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using BestStories.Model;
using System.Reflection;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Caching.Memory;
using BestStories.Helper;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BestStories.Controllers
{
    public class HomeController : BaseController
    {
        private IHostingEnvironment hostingEnvironment;
        private IMemoryCache _cache;
        private const string key = "Stories-Cache";
        private string storyurl = "";
        private IHttpAccessClass _httpAccess;

        public HomeController(IHostingEnvironment env, IMemoryCache memoryCache, IConfiguration config,IHttpAccessClass httpAccessClass)
        {
             hostingEnvironment = env;
            _cache = memoryCache;
             storyurl = config["ConnectionStrings:UrlConnection"];
            _httpAccess = httpAccessClass;
        }

        private IEnumerable<StoryViewModel> SetGetMemoryCache
        {
            get
            {
               if (_cache.TryGetValue(key, out IEnumerable<StoryViewModel> Stories))
                {
                    return Stories;
                }

                //fetch the data from the object
                Stories = _httpAccess.GetStories(storyurl).Result;

                //Save the received data in cache
                _cache.Set(key, Stories,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                return Stories;
            }
        }

        //////private List<StoryViewModel> SetGetMemoryCache
        //////{
        //////    get
        //////    {
        //////        List<StoryViewModel> Stories;
        //////        if (!_cache.TryGetValue(key, out Stories))

        //////        {
        //////            //fetch the data from the object

        //////            Stories = _httpAccess.GetStories(storyurl).Result;

        //////            //Save the received data in cache

        //////            _cache.Set(key, Stories,
        //////                new MemoryCacheEntryOptions()
        //////                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
        //////        }
        //////        else
        //////        {
        //////            //fetch the data from the cache

        //////            Stories = _cache.Get(key) as List<StoryViewModel>;
        //////        }
        //////        return Stories;
        //////    }
        //////}


        ////right way to use cache but I',m using as property that why I use above property

        // public async Task<IEnumerable<StoryViewModel>> GetStories()
        //{
        //    if (_cache.TryGetValue(key, out IEnumerable<StoryViewModel> bestStories))
        //    {
        //        return bestStories;
        //    }

        //    bestStories = await _httpAccess.GetStories(storyurl);

        //    _cache.Set(key, bestStories, new MemoryCacheEntryOptions()
        //                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));

        //    return bestStories;
        //}

        [HttpGet]
        public virtual async Task<IActionResult> Load(String sort, String order, String search, Int32 limit, Int32 offset, String ExtraParam)
        {
          // Get entity fieldnames
          List<String> columnNames = typeof(StoryViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

          // Create a seperate list for searchable field names   
          List<String> searchFields = new List<String>(columnNames);

          // Perform filtering
            var items = await SearchItems(SetGetMemoryCache.AsQueryable(), search, searchFields);

      
        // Sort the filtered items and apply paging
                return Content(ItemsToJson(items, columnNames, sort, order, limit, offset), "application/json");
        }
    }
}
