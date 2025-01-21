using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;
using ShiftManager.Utilities;

namespace ShiftManager.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobRepository _jobRepository;

        public ShiftController(IShiftRepository shiftRepository, IEmployeeRepository employeeRepository, IJobRepository jobRepository)
        {
            _shiftRepository = shiftRepository;
            _employeeRepository = employeeRepository;
            _jobRepository = jobRepository;
        }

        // Display all shifts
        public async Task<IActionResult> Index()
        {
            var data = await _shiftRepository.GetAllShiftsAsync();
            return View(data);
        }

        //Display create shift form
        public async Task<IActionResult> Create()
        {
            //Populating employees and jobs in the view
            await SetDropdownsForView();

            return View();
        }

        //Create a new shift
        [HttpPost]
        public async Task<IActionResult> Create(ShiftVM shift)
        {
            //Validating employees age
            if(!await ValidateEmployeeAge(shift))
            {
                return await RetryCreateView(shift);
            }

            //Making sure shift doesnt start after it has ended
            if (shift.ShiftStart > shift.ShiftEnd)
            {
                ModelState.AddModelError("Shift creation error", "Shift cannot start after shift has ended");
            }

            if (!ModelState.IsValid)
            {
                return await RetryCreateView(shift);
            }

            await _shiftRepository.AddAsync(shift);
            return RedirectToAction(nameof(Index));
        }

        //Display edit shift form
        public async Task<IActionResult> Edit(int id)
        {
            var shiftDetails = await _shiftRepository.GetShiftByIdAsync(id);
            if (shiftDetails == null)
            {
                return View("NotFound");
            }

            var shiftVM = new ShiftVM()
            {
                ShiftStart = shiftDetails.ShiftStart,
                ShiftEnd = shiftDetails.ShiftEnd,
                EmployeeId = shiftDetails.EmployeeId,
                JobIds = shiftDetails.Jobs_Shifts.Select(x => x.JobId).ToList()
            };

            //Populating employees and jobs in the view
            await SetDropdownsForView();

            return View(shiftVM);
        }

        //Update a shift
        [HttpPost]
        public async Task<IActionResult> Edit(ShiftVM shift)
        {
            if(!await ValidateEmployeeAge(shift))
            {
                return await RetryCreateView(shift);
            }

            //Making sure shift doesnt start after it has ended
            if (shift.ShiftStart > shift.ShiftEnd)
            {
                ModelState.AddModelError("Shift creation error", "Shift cannot start after shift has ended");
            }

            if (!ModelState.IsValid)
            {
                return await RetryCreateView(shift);
            }

            await _shiftRepository.UpdateAsync(shift);
            return RedirectToAction(nameof(Index));
        }

        //Delete a shift
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _shiftRepository.GetShiftByIdAsync(id);
            if (shift == null)
            {
                return View("NotFound");
            }

            await _shiftRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Helper method to set dropdowns for the view
        private async Task SetDropdownsForView()
        {
            var dropdowns = new ShiftDropDownsVM();

            dropdowns.Employees = await _employeeRepository.GetAllEmployeesAsync();
            dropdowns.Jobs = await _jobRepository.GetAllJobsAsync();

            ViewBag.Employees = new SelectList(dropdowns.Employees, "Id", "Name");
            ViewBag.Jobs = new SelectList(dropdowns.Jobs, "Id", "Name");
        }

        // Helper method to validate employee age for jobs
        private async Task<bool> ValidateEmployeeAge(ShiftVM shift)
        {
            List<string> jobsTooYoungFor = new List<string>();
            var employee = await _employeeRepository.GetEmployeeByIdAsync(shift.EmployeeId);

            foreach (var jobId in shift.JobIds)
            {
                var job = await _jobRepository.GetJobByIdAsync(jobId);
                if (AgeCalculator.CalculateEmployeeAge(employee.DOB) < job.RequiredAge)
                {
                    jobsTooYoungFor.Add(job.Name);
                }
            }

            if (jobsTooYoungFor.Any())
            {
                string message = string.Join(", ", jobsTooYoungFor);
                ModelState.AddModelError("Too Young", $"{employee.Name} is too young for task(s): {message}");
                return false;
            }

            return true;
        }

        // Helper method to retry Create view with dropdowns and errors
        private async Task<IActionResult> RetryCreateView(ShiftVM shift)
        {
            await SetDropdownsForView();
            return View(shift);
        }
    }
}
