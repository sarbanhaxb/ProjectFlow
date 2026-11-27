
using MongoDB.Driver;
using ProjectFlow.Models;

namespace ProjectFlow.Services;

public class MongoDbIndexService
{
    private readonly IMongoCollection<Employee> _employees;
    private readonly IMongoCollection<TaskItem> _tasks;

    public MongoDbIndexService(MongoDbService mongoDbService)
    {
        _employees = mongoDbService.Database
            .GetCollection<Employee>("users");

        _tasks = mongoDbService.Database
            .GetCollection<TaskItem>("tasks");
    }

    public async Task CreateIndexesAsync()
    {
        // Уникальный индекс email сотрудников
        var emailIndex = new CreateIndexModel<Employee>(
            Builders<Employee>.IndexKeys
                .Ascending(e => e.Email),
            new CreateIndexOptions
            {
                Name = "ux_users_email",
                Unique = true
            });

        await _employees.Indexes.CreateOneAsync(emailIndex);

        // Индекс задач по проекту
        var projectIndex = new CreateIndexModel<TaskItem>(
            Builders<TaskItem>.IndexKeys
                .Ascending(t => t.ProjectId),
            new CreateIndexOptions
            {
                Name = "ix_tasks_projectId"
            });

        // Индекс задач по исполнителю
        var assigneeIndex = new CreateIndexModel<TaskItem>(
            Builders<TaskItem>.IndexKeys
                .Ascending(t => t.AssigneeId),
            new CreateIndexOptions
            {
                Name = "ix_tasks_assigneeId"
            });

        await _tasks.Indexes.CreateManyAsync(
            new[] { projectIndex, assigneeIndex });
    }
}
