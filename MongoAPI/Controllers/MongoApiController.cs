using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoAPI.Controllers
{
    public class MongoApiController
    {
        IMongoDatabase db;
        WebApplication app;
        string collectionName;
        public MongoApiController(string dbName, string collectionName)
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            db = client.GetDatabase("ShadrinDb");
            this.collectionName = "Movies";
        }
        public async Task GetMovies(HttpResponse response, HttpRequest request)
        {
            try
            {
                Dictionary<string,string> quer= new Dictionary<string,string>();
                
                foreach(var key in request.Query.Keys)
                {
                    Console.WriteLine(request.Query[key]);
                    quer.Add(key, request.Query[key].ToString());
                }

                BsonDocument filter = new BsonDocument();
                foreach(string key in quer.Keys)
                {
                    if (int.TryParse(quer[key],out int n))
                    {
                        filter.Add(key, n);
                    }
                    else if (double.TryParse(quer[key], out double d))
                    {
                        filter.Add(key, d);
                    }
                    else
                    {
                        filter.Add(key, quer[key]);
                    }
                }
                Console.WriteLine(filter.ToString());
                    List<Movie> collection = await db.GetCollection<Movie>(collectionName).Find(filter).ToListAsync();
                    await response.WriteAsJsonAsync(collection);
                
            }
            catch (Exception ex)
            {
                await response.WriteAsJsonAsync(new { message = ex.Message });

            }
        }

        
    }
}