using MongoDB.Driver;
using ProjectFlow.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectFlow.Services
{
    public class ProjectService
    {
        private readonly IMongoCollection<Project> _projects;

        public ProjectService(MongoDbService mongoDbService)
        {
            _projects = mongoDbService.Database.GetCollection<Project>("projects");
        }

        /// <summary>
        /// Получение всех проектов
        /// </summary>
        public async Task<List<Project>> GetAllAsync()
        {
            return await _projects.Find(_ => true).ToListAsync();
        }

        public async Task<Project> GetByIdAsync(string id)
        {
            return await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Project project)
        {
            await _projects.InsertOneAsync(project);
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.Id))
                return false;

            var result = await _projects.ReplaceOneAsync(p => p.Id == project.Id, project);

            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _projects.DeleteOneAsync(p => p.Id == id);

            return result.DeletedCount > 0;
        }
        
    }
}
