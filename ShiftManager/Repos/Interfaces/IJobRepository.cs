using ShiftManager.Models;
using ShiftManager.Models.ViewModels;

namespace ShiftManager.Services.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> GetJobByIdAsync(int id);
        Task<IEnumerable<Job>> GetAllJobsAsync();
        Task AddAsync(JobVM job);
        Task UpdateAsync(JobVM updatedJob);
        Task DeleteAsync(int id);
    }
}
