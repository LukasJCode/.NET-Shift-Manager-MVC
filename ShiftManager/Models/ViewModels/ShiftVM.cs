using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShiftManager.Models.ViewModels
{
    public class ShiftVM
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Start Of The Shift")]
        public DateTime ShiftStart { get; set; }
        [Required]
        [Display(Name = "End Of The Shift")]
        public DateTime ShiftEnd { get; set; }

        //Relationships
        [Required]
        [Display(Name = "Select an Employee")]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Select a Job")]
        public List<int> JobIds { get; set; }


    }
}
