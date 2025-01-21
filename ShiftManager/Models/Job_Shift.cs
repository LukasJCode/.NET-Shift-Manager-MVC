namespace ShiftManager.Models
{
    public class Job_Shift
    {
        public int JobId { get; set; }
        public Job Job { get; set; }
        public int ShiftId { get; set; }
        public Shift Shift { get; set; }
    }
}
