using Microsoft.Data.SqlClient;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;
using ShiftManager.Utilities;

namespace ShiftManager.Repos
{
    public class ShiftRepositorySQL : IShiftRepository
    {
        private readonly string _connectionString;
        public ShiftRepositorySQL() 
        {
            _connectionString = Handlers.GetConnectionString();
        }

        public async Task AddAsync(ShiftVM shift)
        {
            const string insertShiftQuery = @"
                INSERT INTO Shifts (ShiftStart, ShiftEnd, EmployeeId) 
                OUTPUT INSERTED.Id 
                VALUES (@ShiftStart, @ShiftEnd, @EmployeeId)";

            //const string insertJobShiftQuery = "INSERT INTO Shifts_Jobs (ShiftId, JobId) VALUES (@ShiftId, @JobId)";
            const string insertJobShiftQuery = "EXEC insertJobShiftQuery @JobId, @ShiftId";
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Add shift
            int shiftId;
            using (var command = new SqlCommand(insertShiftQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftStart", shift.ShiftStart);
                command.Parameters.AddWithValue("@ShiftEnd", shift.ShiftEnd);
                command.Parameters.AddWithValue("@EmployeeId", shift.EmployeeId);

                shiftId = (int)await command.ExecuteScalarAsync();
            }

            // Add associated jobs
            foreach (var jobId in shift.JobIds)
            {
                using var command = new SqlCommand(insertJobShiftQuery, connection);
                command.Parameters.AddWithValue("@ShiftId", shiftId);
                command.Parameters.AddWithValue("@JobId", jobId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string deleteShiftQuery = "DELETE FROM Shifts WHERE Id = @Id";
            const string deleteJobShiftsQuery = "DELETE FROM Shifts_Jobs WHERE ShiftId = @ShiftId";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Delete associated jobs
            using (var command = new SqlCommand(deleteJobShiftsQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftId", id);
                await command.ExecuteNonQueryAsync();
            }

            // Delete shift
            using (var command = new SqlCommand(deleteShiftQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Shift>> GetAllShiftsAsync()
        {
            const string shiftQuery = @"
                SELECT s.Id, s.ShiftStart, s.ShiftEnd, s.EmployeeId, e.Name AS EmployeeName
                FROM Shifts s
                INNER JOIN Employees e ON s.EmployeeId = e.Id";

            const string jobsQuery = @"
                SELECT sj.ShiftId, sj.JobId, j.Name AS JobName
                FROM Shifts_Jobs sj
                INNER JOIN Jobs j ON sj.JobId = j.Id";

            var shifts = new List<Shift>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch shifts with employee data
            using (var command = new SqlCommand(shiftQuery, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    shifts.Add(new Shift
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ShiftStart = reader.GetDateTime(reader.GetOrdinal("ShiftStart")),
                        ShiftEnd = reader.GetDateTime(reader.GetOrdinal("ShiftEnd")),
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        Employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            Name = reader.GetString(reader.GetOrdinal("EmployeeName"))
                        },
                        Jobs_Shifts = new List<Job_Shift>() // Initialize Jobs_Shifts list
                    });
                }
            }

            // Fetch jobs for each shift
            using (var command = new SqlCommand(jobsQuery, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var shiftId = reader.GetInt32(reader.GetOrdinal("ShiftId"));
                    var jobId = reader.GetInt32(reader.GetOrdinal("JobId"));
                    var jobName = reader.GetString(reader.GetOrdinal("JobName"));

                    var shift = shifts.FirstOrDefault(s => s.Id == shiftId);
                    if (shift != null)
                    {
                        shift.Jobs_Shifts.Add(new Job_Shift
                        {
                            ShiftId = shiftId,
                            JobId = jobId,
                            Job = new Job
                            {
                                Id = jobId,
                                Name = jobName
                            }
                        });
                    }
                }
            }

            return shifts;
        }

        public async Task<Shift> GetShiftByIdAsync(int? Id)
        {
            const string shiftQuery = @"
                SELECT Id, ShiftStart, ShiftEnd, EmployeeId 
                FROM Shifts 
                WHERE Id = @Id";

            const string jobShiftQuery = @"
                SELECT js.ShiftId, js.JobId, j.Name, j.RequiredAge 
                FROM Shifts_Jobs js
                INNER JOIN Jobs j ON js.JobId = j.Id
                WHERE js.ShiftId = @ShiftId";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            Shift shift = null;

            // Fetch shift details
            using (var command = new SqlCommand(shiftQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", Id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    shift = new Shift
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ShiftStart = reader.GetDateTime(reader.GetOrdinal("ShiftStart")),
                        ShiftEnd = reader.GetDateTime(reader.GetOrdinal("ShiftEnd")),
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        Jobs_Shifts = new List<Job_Shift>() // Initialize list to avoid null reference
                    };
                }
            }

            if (shift == null) return null;

            // Fetch related jobs for the shift
            using (var command = new SqlCommand(jobShiftQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftId", shift.Id);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    shift.Jobs_Shifts.Add(new Job_Shift
                    {
                        ShiftId = reader.GetInt32(reader.GetOrdinal("ShiftId")),
                        JobId = reader.GetInt32(reader.GetOrdinal("JobId")),
                        Job = new Job
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("JobId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            RequiredAge = reader.GetInt32(reader.GetOrdinal("RequiredAge"))
                        }
                    });
                }
            }

            return shift;
        }

        public async Task UpdateAsync(ShiftVM updatedShift)
        {
            const string updateShiftQuery = @"
                UPDATE Shifts 
                SET ShiftStart = @ShiftStart, 
                    ShiftEnd = @ShiftEnd, 
                    EmployeeId = @EmployeeId
                WHERE Id = @Id";

            const string deleteJobsQuery = @"
                DELETE FROM Shifts_Jobs 
                WHERE ShiftId = @ShiftId";

            const string insertJobShiftQuery = "EXEC insertJobShiftQuery @JobId, @ShiftId";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Update the shift details
            using (var command = new SqlCommand(updateShiftQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftStart", updatedShift.ShiftStart);
                command.Parameters.AddWithValue("@ShiftEnd", updatedShift.ShiftEnd);
                command.Parameters.AddWithValue("@EmployeeId", updatedShift.EmployeeId);
                command.Parameters.AddWithValue("@Id", updatedShift.Id);

                await command.ExecuteNonQueryAsync();
            }

            // Remove existing job-shift mappings
            using (var command = new SqlCommand(deleteJobsQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftId", updatedShift.Id);
                await command.ExecuteNonQueryAsync();
            }

            // Add updated job-shift mappings
            foreach (var jobId in updatedShift.JobIds)
            {
                using (var command = new SqlCommand(insertJobShiftQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftId", updatedShift.Id);
                    command.Parameters.AddWithValue("@JobId", jobId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
