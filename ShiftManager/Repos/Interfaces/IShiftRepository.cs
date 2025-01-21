using ShiftManager.Models;
using ShiftManager.Models.ViewModels;

namespace ShiftManager.Services.Interfaces
{
    public interface IShiftRepository
    {
        Task<Shift> GetShiftByIdAsync(int? Id);
        Task<IEnumerable<Shift>> GetAllShiftsAsync();
        Task AddAsync(ShiftVM shift);
        Task UpdateAsync(ShiftVM updatedShift);
        Task DeleteAsync(int Id);
    }
}
