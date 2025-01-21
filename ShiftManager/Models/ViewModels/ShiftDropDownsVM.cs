using ShiftManager.Models;

namespace ShiftManager.Models.ViewModels
{
    public class ShiftDropDownsVM
    {
        public IEnumerable<Employee> Employees { get; set; }
        public IEnumerable<Job> Jobs { get; set; }
        public ShiftDropDownsVM()
        {
            Employees = new List<Employee>();
            Jobs = new List<Job>();
        }
    }
}
