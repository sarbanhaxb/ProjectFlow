
using MongoDB.Driver;
using ProjectFlow.Models;

namespace ProjectFlow.Services;

public class EmployeeService
{
    private readonly IMongoCollection<Employee> _employees;

    public EmployeeService(MongoDbService mongoDbService)
    {
        _employees = mongoDbService.Database
            .GetCollection<Employee>("users");
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await _employees
            .Find(_ => true)
            .SortBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(string id)
    {
        return await _employees
            .Find(e => e.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        string? excludedId = null)
    {
        var filter = Builders<Employee>.Filter.Eq(
            e => e.Email, email);

        if (!string.IsNullOrWhiteSpace(excludedId))
        {
            filter &= Builders<Employee>.Filter.Ne(
                e => e.Id, excludedId);
        }

        return await _employees
            .Find(filter)
            .AnyAsync();
    }

    public async Task CreateAsync(Employee employee)
    {
        await _employees.InsertOneAsync(employee);
    }

    public async Task<bool> UpdateAsync(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.Id))
            return false;

        var result = await _employees.ReplaceOneAsync(
            e => e.Id == employee.Id,
            employee);

        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _employees.DeleteOneAsync(
            e => e.Id == id);

        return result.DeletedCount > 0;
    }
}
