﻿using Microsoft.Data.SqlClient;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;
using ShiftManager.Utilities;
using System.Data;

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
                INSERT INTO Shift (shift_start, shift_end, emp_id) 
                OUTPUT INSERTED.sft_id 
                VALUES (@shift_start, @shift_end, @emp_id)";

            const string insertJobShiftQuery = @"
                INSERT INTO Shift_Job (sft_id, job_id)
                VALUES (@sft_id, @job_id)";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Using transaction to execute both queries at once
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert the Shift and get the generated Shift ID
                int shiftId;
                using (var shiftCommand = new SqlCommand(insertShiftQuery, connection, (SqlTransaction)transaction))
                {
                    shiftCommand.Parameters.AddWithValue("@shift_start", shift.ShiftStart);
                    shiftCommand.Parameters.AddWithValue("@shift_end", shift.ShiftEnd);
                    shiftCommand.Parameters.AddWithValue("@emp_id", shift.EmployeeId);

                    shiftId = (int)await shiftCommand.ExecuteScalarAsync();
                }

                // Assign jobs for the shift
                using (var jobShiftCommand = new SqlCommand(insertJobShiftQuery, connection, (SqlTransaction)transaction))
                {

                    jobShiftCommand.Parameters.Add(new SqlParameter("@job_id", SqlDbType.Int));
                    jobShiftCommand.Parameters.Add(new SqlParameter("@sft_id", SqlDbType.Int));

                    foreach (var jobId in shift.JobIds)
                    {
                        jobShiftCommand.Parameters["@job_id"].Value = jobId;
                        jobShiftCommand.Parameters["@sft_id"].Value = shiftId;
                        await jobShiftCommand.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string deleteShiftQuery = "DELETE FROM Shift WHERE sft_id = @sft_id";


            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using (var deleteShiftCommand = new SqlCommand(deleteShiftQuery, connection))
            {
                deleteShiftCommand.Parameters.AddWithValue("@sft_id", id);
                await deleteShiftCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Shift>> GetAllShiftsAsync()
        {
            const string shiftQuery = @"
                SELECT s.sft_id, s.shift_start, s.shift_end, s.emp_id, e.name AS employee_name
                FROM Shift s
                INNER JOIN Employee e ON s.emp_id = e.emp_id";

            const string jobsQuery = @"
                SELECT sj.sft_id, sj.job_id, j.name AS job_name
                FROM Shift_Job sj
                INNER JOIN Job j ON sj.job_id = j.job_id";

            var shifts = new List<Shift>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Fetch shifts with employee data
                using (var shiftCommand = new SqlCommand(shiftQuery, connection, (SqlTransaction)transaction))
                using (var reader = await shiftCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        shifts.Add(new Shift
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("sft_id")),
                            ShiftStart = reader.GetDateTime(reader.GetOrdinal("shift_start")),
                            ShiftEnd = reader.GetDateTime(reader.GetOrdinal("shift_end")),
                            EmployeeId = reader.GetInt32(reader.GetOrdinal("emp_id")),
                            Employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("emp_id")),
                                Name = reader.GetString(reader.GetOrdinal("employee_name"))
                            },
                            Jobs_Shifts = new List<Job_Shift>() // Initialize Jobs_Shifts list
                        });
                    }
                }

                // Fetch jobs for each shift
                using (var jobShiftCommand = new SqlCommand(jobsQuery, connection, (SqlTransaction)transaction))
                using (var reader = await jobShiftCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var shiftId = reader.GetInt32(reader.GetOrdinal("sft_id"));
                        var jobId = reader.GetInt32(reader.GetOrdinal("job_id"));
                        var jobName = reader.GetString(reader.GetOrdinal("job_name"));

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

                await transaction.CommitAsync();

                return shifts;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Shift> GetShiftByIdAsync(int id)
        {
            const string shiftQuery = @"
                SELECT sft_id, shift_start, shift_end, emp_id 
                FROM Shift 
                WHERE sft_id = @sft_id";

            const string jobShiftQuery = @"
                SELECT js.sft_id, js.job_id, j.name, j.required_age 
                FROM Shift_Job js
                INNER JOIN Job j ON js.job_id = j.job_id
                WHERE js.sft_id = @sft_id";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            Shift shift = null;

            try
            {
                // Fetch shift details
                using (var shiftCommand = new SqlCommand(shiftQuery, connection, (SqlTransaction)transaction))
                {
                    shiftCommand.Parameters.AddWithValue("@sft_id", id);
                    using var reader = await shiftCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        shift = new Shift
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("sft_id")),
                            ShiftStart = reader.GetDateTime(reader.GetOrdinal("shift_start")),
                            ShiftEnd = reader.GetDateTime(reader.GetOrdinal("shift_end")),
                            EmployeeId = reader.GetInt32(reader.GetOrdinal("emp_id")),
                            Jobs_Shifts = new List<Job_Shift>() // Initialize list to avoid null reference
                        };
                    }
                }

                if (shift == null)
                    throw new KeyNotFoundException($"Shift with ID {id} not found.");

                // Fetch related jobs for the shift
                using (var jobShiftCommand = new SqlCommand(jobShiftQuery, connection, (SqlTransaction)transaction))
                {
                    jobShiftCommand.Parameters.AddWithValue("@sft_id", shift.Id);
                    using var reader = await jobShiftCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        shift.Jobs_Shifts.Add(new Job_Shift
                        {
                            ShiftId = reader.GetInt32(reader.GetOrdinal("sft_id")),
                            JobId = reader.GetInt32(reader.GetOrdinal("job_id")),
                            Job = new Job
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("job_id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                RequiredAge = reader.GetInt32(reader.GetOrdinal("required_age"))
                            }
                        });
                    }
                }

                await transaction.CommitAsync();

                return shift;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(ShiftVM updatedShift)
        {
            const string updateShiftQuery = @"
                UPDATE Shift 
                SET shift_start = @shift_start, 
                    shift_end = @shift_end, 
                    emp_id = @emp_id
                WHERE sft_id = @sft_id";

            const string deleteJobsQuery = @"
                DELETE FROM Shift_Job 
                WHERE sft_id = @sft_id";

            const string insertJobShiftQuery = @"
                INSERT INTO Shift_Job (sft_id, job_id)
                VALUES (@sft_id, @job_id)";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Update the shift details
                using (var updateShiftCommand = new SqlCommand(updateShiftQuery, connection, (SqlTransaction)transaction))
                {
                    updateShiftCommand.Parameters.AddWithValue("@shift_start", updatedShift.ShiftStart);
                    updateShiftCommand.Parameters.AddWithValue("@shift_end", updatedShift.ShiftEnd);
                    updateShiftCommand.Parameters.AddWithValue("@emp_id", updatedShift.EmployeeId);
                    updateShiftCommand.Parameters.AddWithValue("@sft_id", updatedShift.Id);

                    await updateShiftCommand.ExecuteNonQueryAsync();
                }

                // Remove existing job-shift mappings
                using (var deleteJobsCommand = new SqlCommand(deleteJobsQuery, connection, (SqlTransaction)transaction))
                {
                    deleteJobsCommand.Parameters.AddWithValue("@sft_id", updatedShift.Id);
                    await deleteJobsCommand.ExecuteNonQueryAsync();
                }

                // Add updated job-shift mappings
                using (var updateJobShifts = new SqlCommand(insertJobShiftQuery, connection, (SqlTransaction)transaction))
                {
                    updateJobShifts.Parameters.Add(new SqlParameter("@job_id", SqlDbType.Int));
                    updateJobShifts.Parameters.Add(new SqlParameter("@sft_id", SqlDbType.Int));

                    foreach (var jobId in updatedShift.JobIds)
                    {
                        updateJobShifts.Parameters["@sft_id"].Value = updatedShift.Id;
                        updateJobShifts.Parameters["@job_id"].Value = jobId;

                        await updateJobShifts.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
