using ShiftManager.Models;
using ShiftManager.Models.ViewModels;

namespace ShiftManager.Services.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> GetJobByIdAsync(int? Id);
        Task<IEnumerable<Job>> GetAllJobsAsync();
        Task AddAsync(JobVM job);
        Task UpdateAsync(JobVM updatedJob);
        Task DeleteAsync(int Id);
    }
}
