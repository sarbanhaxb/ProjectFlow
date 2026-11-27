
using MongoDB.Driver;
using ProjectFlow.Models;

namespace ProjectFlow.Services;

public class TaskService
{
    private readonly IMongoCollection<TaskItem> _tasks;
    private readonly IMongoCollection<Project> _projects;
    private readonly IMongoCollection<Employee> _employees;


    public TaskService(MongoDbService mongoDbService)
    {
        _tasks = mongoDbService.Database
            .GetCollection<TaskItem>("tasks");

        _projects = mongoDbService.Database
            .GetCollection<Project>("projects");

        _employees = mongoDbService.Database
            .GetCollection<Employee>("users");
    }


    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _tasks
            .Find(_ => true)
            .SortBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetByProjectAsync(
        string projectId)
    {
        return await _tasks
            .Find(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(string id)
    {
        return await _tasks
            .Find(t => t.Id == id)
            .FirstOrDefaultAsync();
    }


    private async Task ValidateTaskAsync(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new InvalidOperationException(
                "Название задачи не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(task.ProjectId))
        {
            throw new InvalidOperationException(
                "Для задачи необходимо выбрать проект.");
        }

        // Проверяем существование проекта
        var project = await _projects
            .Find(p => p.Id == task.ProjectId)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new InvalidOperationException(
                "Выбранный проект больше не существует.");
        }

        // Задача может не иметь исполнителя
        if (string.IsNullOrWhiteSpace(task.AssigneeId))
            return;

        // Проверяем существование сотрудника
        var employeeExists = await _employees
            .Find(e => e.Id == task.AssigneeId)
            .AnyAsync();

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "Назначенный сотрудник больше не существует.");
        }

        // Проверяем участие сотрудника в проекте
        if (!project.MemberIds.Contains(task.AssigneeId))
        {
            throw new InvalidOperationException(
                "Выбранный сотрудник не участвует " +
                "в указанном проекте.\n" +
                "Обновите данные и выберите другого исполнителя.");
        }
    }


    public async Task CreateAsync(TaskItem task)
    {
        await ValidateTaskAsync(task);

        await _tasks.InsertOneAsync(task);
    }



    public async Task<bool> UpdateAsync(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Id))
            return false;

        await ValidateTaskAsync(task);

        var result = await _tasks.ReplaceOneAsync(
            t => t.Id == task.Id,
            task);

        return result.MatchedCount > 0;
    }


    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _tasks.DeleteOneAsync(
            t => t.Id == id);

        return result.DeletedCount > 0;
    }

    public async Task<bool> HasTasksForProjectAsync(
        string projectId)
    {
        return await _tasks
            .Find(t => t.ProjectId == projectId)
            .AnyAsync();
    }

    public async Task<bool> HasTasksForEmployeeAsync(
        string employeeId)
    {
        return await _tasks
            .Find(t => t.AssigneeId == employeeId)
            .AnyAsync();
    }
}
