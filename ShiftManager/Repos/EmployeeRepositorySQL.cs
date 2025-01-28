using ShiftManager.Models;
using ShiftManager.Services.Interfaces;
using ShiftManager.Utilities;
using Microsoft.Data.SqlClient;
using ShiftManager.Models.ViewModels;

namespace ShiftManager.Repos
{
    public class EmployeeRepositorySQL : IEmployeeRepository
    {
        private readonly string _connectionString;
        public EmployeeRepositorySQL() 
        {
            _connectionString = Handlers.GetConnectionString();
        }
        public async Task AddAsync(EmployeeVM employee)
        {
            const string query = "INSERT INTO employee (name, dob) VALUES (@name, @dob)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", employee.Name);
            command.Parameters.AddWithValue("@dob", employee.DOB);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            const string query = "DELETE FROM Employee WHERE emp_id = @id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            const string query = "SELECT emp_id, name, dob FROM Employee";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var employees = new List<Employee>();
            while (await reader.ReadAsync())
            {
                var emp = new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("emp_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    DOB = reader.GetDateTime(reader.GetOrdinal("dob"))
                };
                employees.Add(emp);
            }
            return employees;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            const string query = "SELECT emp_id, name, dob FROM Employee WHERE emp_id = @id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("emp_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    DOB = reader.GetDateTime(reader.GetOrdinal("dob"))
                };
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateAsync(EmployeeVM updatedEmployee)
        {
            const string query = "UPDATE Employee SET name = @name, dob = @dob WHERE emp_id = @id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", updatedEmployee.Id);
            command.Parameters.AddWithValue("@name", updatedEmployee.Name);
            command.Parameters.AddWithValue("@dob", updatedEmployee.DOB);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}
