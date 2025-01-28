using ShiftManager.Services.Interfaces;
using FakeItEasy;
using ShiftManager.Models;
using ShiftManager.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace ShiftManager.Tests.ControllerTests
{
    public class EmployeeControllerTests
    {
        private IEmployeeRepository _employeeRepository;
        private EmployeeController _employeeController;
        private ILogger<EmployeeController> _logger;
        public EmployeeControllerTests()
        {
            //Dependencies
            _employeeRepository = A.Fake<IEmployeeRepository>();

            //SUT
            _employeeController = new EmployeeController(_employeeRepository, _logger);
        }

        [Fact]
        public async void EmployeeController_Index_ReturnsViewWithEmployees()
        {
            //Arrange
            var employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "John Doe", DOB = new DateTime(1990, 1, 1) },
                new Employee { Id = 2, Name = "Jane Doe", DOB = new DateTime(1995, 5, 15) }
            };

            A.CallTo(() => _employeeRepository.GetAllEmployeesAsync()).Returns(employees);

            //Act
            var result = await _employeeController.Index();

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(employees);
        }

        [Fact]
        public async void EmployeeController_Index_ReturnsViewWithNoEmployees()
        {
            //Arrange
            var employees = new List<Employee>();

            A.CallTo(() => _employeeRepository.GetAllEmployeesAsync()).Returns(employees);

            //Act
            var result = await _employeeController.Index();

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(employees);
        }

        [Fact]
        public async void EmployeeController_Filter_ReturnsViewWithFilteredEmployees()
        {
            //Arrange
            var employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "John Doe", DOB = new DateTime(1990, 1, 1) },
                new Employee { Id = 2, Name = "Jane Doe", DOB = new DateTime(1995, 5, 15) }
            };
            var expectedResult = new List<Employee>
            {
                new Employee { Id = 1, Name = "John Doe", DOB = new DateTime(1990, 1, 1) }
            };

            A.CallTo(() => _employeeRepository.GetAllEmployeesAsync()).Returns(employees);

            //Act
            var result = await _employeeController.Filter("John");

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async void EmployeeController_Create_ValidEmployee_RedirectsToIndex()
        {
            //Arrange
            var employee = new EmployeeVM
            {
                Name = "John Doe",
                DOB = new DateTime(1990, 1, 1)
            };

            //Act
            var result = await _employeeController.Create(employee);

            //Assert
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.Should().NotBeNull();
            redirectToActionResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async void EmployeeController_Create_EmployeeWithFutureDOB_ReturnsViewWithError()
        {
            //Arrange
            var employee = new EmployeeVM
            {
                Name = "John Doe",
                DOB = DateTime.Today.AddDays(1) // Future DOB
            };

            //Act
            var result = await _employeeController.Create(employee);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(employee);

            var modelStateError = _employeeController.ModelState["DOB"]?.Errors.FirstOrDefault()?.ErrorMessage;
            modelStateError.Should().Be("Date of Birth cannot be in the future.");
        }

        [Fact]
        public async void EmployeeController_EditGet_ValidId_ReturnsViewWithEmployeeVM()
        {
            //Arrange
            var employee = new Employee
            {
                Id = 1,
                Name = "John Doe",
                DOB = new DateTime(1990, 1, 1)
            };

            A.CallTo(() => _employeeRepository.GetEmployeeByIdAsync(1)).Returns(employee);

            //Act
            var result = await _employeeController.Edit(1);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult.Model as EmployeeVM;
            model.Should().NotBeNull();
            model.Name.Should().Be(employee.Name);
            model.DOB.Should().Be(employee.DOB);
        }

        [Fact]
        public async void EmployeeController_EditGet_InvalidId_ReturnsNotFoundView()
        {
            //Arrange
            A.CallTo(() => _employeeRepository.GetEmployeeByIdAsync(999)).Returns((Employee)null);

            //Act
            var result = await _employeeController.Edit(999);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("NotFound");
        }

        [Fact]
        public async void EmployeeController_EditPost_ValidEmployee_RedirectsToIndex()
        {
            //Arrange
            var employee = new EmployeeVM
            {
                Id = 1,
                Name = "John Doe",
                DOB = new DateTime(1990, 1, 1)
            };

            //Act
            var result = await _employeeController.Edit(employee);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_employeeController.Index));
        }

        //Delete tests
        [Fact]
        public async void EmployeeController_Delete_ValidEmployee_RedirectsToIndex()
        {
            //Arrange
            var employee = new EmployeeVM
            {
                Id = 1,
                Name = "John Doe",
                DOB = new DateTime(1990, 1, 1)
            };

            //Act
            var result = await _employeeController.Delete(1);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_employeeController.Index));
        }

    }
}
