using Microsoft.AspNetCore.Mvc;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;

namespace ShiftManager.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        //Display all employees
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return View(employees);
        }

        //Filter employees by search string
        [HttpGet]
        public async Task<IActionResult> Filter(string searchString)
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                employees = employees
                    .Where(e => e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View("Index", employees);
        }

        //Display create employee form
        public IActionResult Create()
        {
            return View();
        }

        //Create new employee
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeVM employee)
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

        //Display edit employee form
        public async Task<IActionResult> Edit(int id)
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

        //Update an employee
        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeVM employee)
        {
            if (!ModelState.IsValid)
            {
                return View(employee);
            }
            await _employeeRepository.UpdateAsync(employee);
            return RedirectToAction(nameof(Index));
        }

        //Delete an employee
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return View("NotFound");
            }

            await _employeeRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
