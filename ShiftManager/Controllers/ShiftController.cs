using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ILogger<ShiftController> _logger;

        public ShiftController(
            IShiftRepository shiftRepository, 
            IEmployeeRepository employeeRepository, 
            IJobRepository jobRepository,
            ILogger<ShiftController> logger
            )
        {
            _shiftRepository = shiftRepository;
            _employeeRepository = employeeRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _shiftRepository.GetAllShiftsAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching shifts.");
                return View("GenericError");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                //Populating employees and jobs in the view
                await SetDropdownsForView();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating shift.");
                return View("GenericError");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShiftVM shift)
        {
            try
            {
                //Validating employees age for jobs
                if (!await ValidateEmployeeAge(shift))
                {
                    return await RetryView(shift);
                }

                //Making sure shift doesnt start after it has ended
                if (shift.ShiftStart > shift.ShiftEnd)
                {
                    ModelState.AddModelError("Shift creation error", "Shift cannot start after shift has ended");
                }

                if (!ModelState.IsValid)
                {
                    return await RetryView(shift);
                }

                await _shiftRepository.AddAsync(shift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding shift.");
                return View("GenericError");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating shift.");
                return View("GenericError");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ShiftVM shift)
        {
            try
            {
                if (!await ValidateEmployeeAge(shift))
                {
                    return await RetryView(shift);
                }

                //Making sure shift doesnt start after it has ended
                if (shift.ShiftStart > shift.ShiftEnd)
                {
                    ModelState.AddModelError("Shift creation error", "Shift cannot start after shift has ended");
                }

                if (!ModelState.IsValid)
                {
                    return await RetryView(shift);
                }

                await _shiftRepository.UpdateAsync(shift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating shift.");
                return View("GenericError");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var shift = await _shiftRepository.GetShiftByIdAsync(id);
                if (shift == null)
                {
                    return View("NotFound");
                }

                await _shiftRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting shift.");
                return View("GenericError");
            }

            return RedirectToAction(nameof(Index));
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
        private async Task<IActionResult> RetryView(ShiftVM shift)
        {
            await SetDropdownsForView();
            return View(shift);
        }
    }
}
