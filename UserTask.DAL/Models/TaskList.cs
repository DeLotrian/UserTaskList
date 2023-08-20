using MongoDB.Bson.Serialization.Attributes;

namespace UserTask.DAL.Models
{
    public class TaskList
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;

        [BsonElement("CreationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public List<string>? Tasks { get; set; } = new List<string>();
        public List<string>? AttachedUsers { get; set; } = new List<string>();
    }
}