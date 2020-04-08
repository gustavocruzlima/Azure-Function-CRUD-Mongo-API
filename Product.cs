using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndustryLog.API.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public int CompanyNumber { get; set; }
        public string ProductId { get; set; }
        public string Color { get; set; }
        public DateTime ReleaseDate { get; set; }

    }
}