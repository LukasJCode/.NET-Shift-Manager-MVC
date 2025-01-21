using System.ComponentModel.DataAnnotations;

namespace ShiftManager.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RequiredAge { get; set; }

        //Relationships
        public List<Job_Shift> Jobs_Shifts { get; set; }
    }
}
