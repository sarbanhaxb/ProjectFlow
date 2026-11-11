
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProjectFlow.Models.Enums;

namespace ProjectFlow.Models;

public class TaskItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("projectId")]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public Enums.TaskStatus Status { get; set; }
        = Enums.TaskStatus.ToDo;

    [BsonElement("priority")]
    [BsonRepresentation(BsonType.String)]
    public TaskPriority Priority { get; set; }
        = TaskPriority.Medium;

    [BsonElement("assigneeId")]
    public string? AssigneeId { get; set; }

    [BsonElement("dueDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DueDate { get; set; }
}
