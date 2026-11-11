
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProjectFlow.Models.Enums;

namespace ProjectFlow.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public ProjectStatus Status { get; set; }
        = ProjectStatus.Planned;

    [BsonElement("startDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [BsonElement("endDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? EndDate { get; set; }

    [BsonElement("memberIds")]
    public List<string> MemberIds { get; set; } = new();
}
