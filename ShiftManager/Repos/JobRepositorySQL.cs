using Microsoft.Data.SqlClient;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;
using ShiftManager.Utilities;

namespace ShiftManager.Repos
{
    public class JobRepositorySQL : IJobRepository
    {
        private readonly string _connectionString;
        public JobRepositorySQL() 
        {
            _connectionString = Handlers.GetConnectionString();
        }

        public async Task AddAsync(JobVM job)
        {
            const string query = "INSERT INTO Jobs (Name, RequiredAge) VALUES (@Name, @RequiredAge)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Name", job.Name);
            command.Parameters.AddWithValue("@RequiredAge", job.RequiredAge);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int Id)
        {
            const string query = "DELETE FROM Jobs WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            const string query = "SELECT Id, Name, RequiredAge FROM Jobs";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var jobs = new List<Job>();
            while (await reader.ReadAsync())
            {
                var job = new Job
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    RequiredAge = reader.GetInt32(reader.GetOrdinal("RequiredAge"))
                };
                jobs.Add(job);
            }
            return jobs;
        }

        public async Task<Job> GetJobByIdAsync(int? Id)
        {
            if (Id == null)
                throw new ArgumentNullException(nameof(Id));

            const string query = "SELECT Id, Name, RequiredAge FROM Jobs WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", Id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Job
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    RequiredAge = reader.GetInt32(reader.GetOrdinal("RequiredAge"))
                };
            }
            else
            {
                return null; // Or throw a custom exception indicating the job was not found
            }
        }

        public async Task UpdateAsync(JobVM updatedJob)
        {
            const string query = "UPDATE Jobs SET Name = @Name, RequiredAge = @RequiredAge WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", updatedJob.Id);
            command.Parameters.AddWithValue("@Name", updatedJob.Name);
            command.Parameters.AddWithValue("@RequiredAge", updatedJob.RequiredAge);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}
