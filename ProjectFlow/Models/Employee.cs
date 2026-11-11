
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProjectFlow.Models;

public class Employee
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("position")]
    public string Position { get; set; } = string.Empty;
}
