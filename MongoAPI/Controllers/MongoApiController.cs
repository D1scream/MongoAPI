using Amazon.Runtime.Documents;
using Castle.Core;
using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
                List<KeyValuePair<string, string>> quer = new List<KeyValuePair<string, string>>();
                
                foreach(var key in request.Query.Keys)
                {
                    string values = request.Query[key].ToString();
                    KeyValuePair<string, string> temp = new KeyValuePair<string, string>(key, values);
                    quer.Add(temp);
                }

                BsonDocument filter = new BsonDocument();

                foreach (KeyValuePair<string, string> val in quer)
                {
                    if (double.TryParse(val.Key, out double d))
                    {
                        filter.Add(val.Key, d);
                    }
                    else if (int.TryParse(val.Key, out int n))
                    {
                        filter.Add(val.Key, n);
                    }
                    else if (val.Value.Contains("[") && val.Value.Contains("]"))
                    {
                        string k = val.Value.Remove(0, 1);
                        k = k.Remove(k.Length - 1, 1);
                        string[] ks = k.Split(',');
                        filter.Add(new BsonDocument("genres", new BsonDocument { { "$all", new BsonArray(ks) } }));
                    }
                    else
                    {
                        filter.Add(val.Key, val.Value);
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
        public async Task AddMovie(HttpResponse response, HttpRequest request)
        {
            try
            {
                var doc = request.BodyReader.AsStream() ;
                string i = new StreamReader(doc, Encoding.UTF8).ReadToEnd();
                
                BsonDocument s = BsonSerializer.Deserialize<BsonDocument>(i);
                db.GetCollection<BsonDocument>(collectionName).InsertOne(s);
                await response.WriteAsJsonAsync(BsonToJson(s));
            }
            catch (Exception ex)
            {
                await response.WriteAsJsonAsync(new { message = ex.Message });
            }
        }
        public async Task UpdateMovie(HttpResponse response, HttpRequest request)
        {
            try
            {
                var doc = request.BodyReader.AsStream();
                string i = new StreamReader(doc, Encoding.UTF8).ReadToEnd();
                BsonDocument s = BsonSerializer.Deserialize<BsonDocument>(i);
                string id = s.GetValue("_id").AsString;
                ObjectId idO = new ObjectId(id);
                s.Remove("_id");
                s.Add("_id", idO);
                {
                    db.GetCollection<BsonDocument>(collectionName).ReplaceOne(new BsonDocument("_id", s.GetValue("_id")), s);
                }
                await response.WriteAsJsonAsync(new {message =  "Updated" });
            }
            catch (Exception ex)
            {
                await response.WriteAsJsonAsync(new { message = ex.Message });
            }
        }
        public async Task DeleteMovie(string id, HttpResponse response)
        {
            try
            {
                ObjectId idO = new ObjectId(id);
                if(db.GetCollection<BsonDocument>(collectionName).Find(new BsonDocument("_id", idO)).Count()== 0){
                    await response.WriteAsJsonAsync(new { message = "Document isn't exist" });
                    return;
                }
                    db.GetCollection<BsonDocument>(collectionName).DeleteOne(new BsonDocument("_id", idO));
                await response.WriteAsJsonAsync(new { message = "Deleted" });

            }
            catch (Exception ex)
            {
                await response.WriteAsJsonAsync(new { message = ex.Message });
            }
        }

        string BsonToJson(BsonDocument bson)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BsonBinaryWriter(stream))
                {
                    BsonSerializer.Serialize(writer, typeof(BsonDocument), bson);
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new Newtonsoft.Json.Bson.BsonReader(stream))
                {
                    var sb = new StringBuilder();
                    var sw = new StringWriter(sb);
                    using (var jWriter = new JsonTextWriter(sw))
                    {
                        jWriter.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                        jWriter.WriteToken(reader);
                    }
                    return sb.ToString();
                }
            }
        }
    }
}