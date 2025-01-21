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
            const string query = "INSERT INTO Employees (Name, DOB) VALUES (@Name, @DOB)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Name", employee.Name);
            command.Parameters.AddWithValue("@DOB", new DateTime(employee.DOB.Year, employee.DOB.Month, employee.DOB.Day));

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int? Id)
        {
            if (Id == null)
                throw new ArgumentNullException(nameof(Id));

            const string query = "DELETE FROM Employees WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            const string query = "SELECT Id, Name, DOB FROM Employees";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var employees = new List<Employee>();
            while (await reader.ReadAsync())
            {
                var emp = new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    DOB = reader.GetDateTime(reader.GetOrdinal("DOB"))
                };
                employees.Add(emp);
            }
            return employees;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int? Id)
        {
            if (Id == null)
                throw new ArgumentNullException(nameof(Id));

            const string query = "SELECT Id, Name, DOB FROM Employees WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", Id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    DOB = reader.GetDateTime(reader.GetOrdinal("DOB"))
                };
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateAsync(EmployeeVM updatedEmployee)
        {
            const string query = "UPDATE Employees SET Name = @Name, DOB = @DOB WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", updatedEmployee.Id);
            command.Parameters.AddWithValue("@Name", updatedEmployee.Name);
            command.Parameters.AddWithValue("@DOB", updatedEmployee.DOB);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}
