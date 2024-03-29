using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoAPI.Models
{
    public class Movie
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string title { get; set; } = "";
        public string director { get; set; }
        public int year { get; set; }
        public string[] genres { get; set; }
        public double rating {  get; set; }

        public string Info()
        {
            return $"{_id}, {title}, {director}, {year}, {genres}, {rating}";
        }

    }
}
