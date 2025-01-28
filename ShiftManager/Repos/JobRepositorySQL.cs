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
            const string query = "INSERT INTO Job (name, required_age) VALUES (@name, @required_age)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", job.Name);
            command.Parameters.AddWithValue("@required_age", job.RequiredAge);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            const string query = "DELETE FROM Job WHERE job_id = @job_id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@job_id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            const string query = "SELECT job_id, name, required_age FROM Job";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var jobs = new List<Job>();
            while (await reader.ReadAsync())
            {
                var job = new Job
                {
                    Id = reader.GetInt32(reader.GetOrdinal("job_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    RequiredAge = reader.GetInt32(reader.GetOrdinal("required_age"))
                };
                jobs.Add(job);
            }
            return jobs;
        }

        public async Task<Job> GetJobByIdAsync(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            const string query = "SELECT job_id, name, required_age FROM Job WHERE job_id = @job_id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@job_id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Job
                {
                    Id = reader.GetInt32(reader.GetOrdinal("job_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    RequiredAge = reader.GetInt32(reader.GetOrdinal("required_age"))
                };
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateAsync(JobVM updatedJob)
        {
            const string query = "UPDATE Job SET name = @name, required_age = @required_age WHERE job_id = @job_id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@job_id", updatedJob.Id);
            command.Parameters.AddWithValue("@name", updatedJob.Name);
            command.Parameters.AddWithValue("@required_age", updatedJob.RequiredAge);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}
