using Microsoft.AspNetCore.Mvc;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;

namespace ShiftManager.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeRepository employeeRepository, ILogger<EmployeeController> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _employeeRepository.GetAllEmployeesAsync();
                return View(employees);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employees.");
                return View("GenericError");
            }
        }

        //Filter employees by name
        [HttpGet]
        public async Task<IActionResult> Filter(string employeeName)
        {
            try
            {
                var employees = await _employeeRepository.GetAllEmployeesAsync();

                if (!string.IsNullOrWhiteSpace(employeeName))
                {
                    employees = employees
                        .Where(e => e.Name.Contains(employeeName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return View(nameof(Index), employees);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employees.");
                return View("GenericError");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeVM employee)
        {
            try
            {
                if (employee.DOB > DateTime.Today)
                {
                    ModelState.AddModelError("DOB", "Date of Birth cannot be in the future.");
                }

                if (!ModelState.IsValid)
                {
                    return View(employee);
                }

                await _employeeRepository.AddAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding employee.");
                return View("GenericError");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employeeDetails = await _employeeRepository.GetEmployeeByIdAsync(id);
                if (employeeDetails == null)
                {
                    return View("NotFound");
                }

                var employeeVM = new EmployeeVM()
                {
                    Name = employeeDetails.Name,
                    DOB = employeeDetails.DOB,
                };

                return View(employeeVM);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employee.");
                return View("GenericError");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeVM employee)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(employee);
                }
                await _employeeRepository.UpdateAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating employee.");
                return View("GenericError");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return View("NotFound");
                }

                await _employeeRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting employeee.");
                return View("GenericError");
            }
        }
    }
}
