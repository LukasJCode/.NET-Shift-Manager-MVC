using System.ComponentModel.DataAnnotations;

namespace ShiftManager.Models.ViewModels
{
    public class JobVM
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Job Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Required Age")]
        [Range(0, 150, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int RequiredAge { get; set; }
    }
}
