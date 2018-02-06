using BestStories.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BestStories.Helper
{
    public class HttpAccessClass: IHttpAccessClass,IDisposable
    {
        private HttpClient client;
        
        public HttpAccessClass()
        {
            client = new HttpClient();
        }
        
        public async Task<List<StoryViewModel>> GetStories(string storyurl)
        {

            List<StoryViewModel> Stories = new List<StoryViewModel>();

           var response = await client.GetAsync(storyurl + "topstories.json");

            if (!response.IsSuccessStatusCode)
            { return null; }

            var response2 = await response.Content.ReadAsStringAsync();
            var RootObjects = JsonConvert.DeserializeObject<List<string>>(response2);

            //slower
            //foreach (var rootObject in RootObjects)
            //{
            //    StoryViewModel sModel = new StoryViewModel();

            //    var result = await client.GetAsync(storyurl + "item/" + rootObject + ".json");

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var result2 = await result.Content.ReadAsStringAsync();
            //        JObject jo = JObject.Parse(result2);
            //        sModel.Author = jo.Property("by").Value.ToString();
            //        sModel.Title = jo.Property("title").Value.ToString();
            //        sModel.Id = (int)jo.Property("id").Value;
            //    }

            //    Stories.Add(sModel);
            //}

            //faster
            Parallel.ForEach(RootObjects, rootObject =>
                {
                    StoryViewModel sModel = new StoryViewModel();

                    var result = client.GetAsync(storyurl + "item/" + rootObject + ".json").Result;

                    if (response.IsSuccessStatusCode)

                    {
                        var result2 = result.Content.ReadAsStringAsync().Result;
                        JObject jo = JObject.Parse(result2);
                        sModel.Author = jo.Property("by").Value.ToString();
                        sModel.Title = jo.Property("title").Value.ToString();
                        sModel.Id = (int)jo.Property("id").Value;
                    }

                    Stories.Add(sModel);
                }
            );

            return Stories;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    public interface IHttpAccessClass
    {
        Task<List<StoryViewModel>> GetStories(string storyurl);
    }
}
