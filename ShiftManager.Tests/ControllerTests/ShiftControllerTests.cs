using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Controllers;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftManager.Tests.ControllerTests
{
    public class ShiftControllerTests
    {
        private IShiftRepository _shiftRepository;
        private IEmployeeRepository _employeeRepository;
        private IJobRepository _jobRepository;
        private ShiftController _shiftController;

        public ShiftControllerTests()
        {
            //Dependencies
            _shiftRepository = A.Fake<IShiftRepository>();
            _employeeRepository = A.Fake<IEmployeeRepository>();
            _jobRepository = A.Fake<IJobRepository>();

            //SUT
            _shiftController = new ShiftController(_shiftRepository, _employeeRepository, _jobRepository);
        }

        [Fact]
        public async void ShiftController_Index_ReturnsViewWithShifts()
        {
            //Arrange
            var shifts = new List<Shift>
            {
                new Shift { Id = 1, ShiftStart = DateTime.Now, ShiftEnd = DateTime.Now.AddHours(1)},
                new Shift { Id = 2, ShiftStart = DateTime.Now, ShiftEnd = DateTime.Now.AddHours(1)},
            };

            A.CallTo(() => _shiftRepository.GetAllShiftsAsync()).Returns(shifts);

            //Act
            var result = await _shiftController.Index();

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(shifts);
        }

        [Fact]
        public async void ShiftController_Create_ValidShift_RedirectsToIndex()
        {
            //Arrange
            var shift = new ShiftVM
            {
                Id = 1,
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(1),
                EmployeeId = 1,
                JobIds = new List<int> { 1 }
            };

            // Act
            var result = await _shiftController.Create(shift);

            // Assert
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.Should().NotBeNull();
            redirectToActionResult.ActionName.Should().Be("Index");

        }

        [Fact]
        public async void ShiftController_Create_ShiftEndsBeforeItStarts_ReturnsViewWithError()
        {
            //Arrange
            var shift = new ShiftVM
            {
                Id = 1,
                ShiftStart = DateTime.Now.AddHours(1),
                ShiftEnd = DateTime.Now,
                EmployeeId = 1,
                JobIds = new List<int> { 1 }
            };

            //Act
            var result = await _shiftController.Create(shift);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(shift);

            var modelStateError = _shiftController.ModelState["Shift creation error"]?.Errors.FirstOrDefault()?.ErrorMessage;
            modelStateError.Should().Be("Shift cannot start after shift has ended");


        }

        [Fact]
        public async void ShiftController_EditGet_ValidId_ReturnsViewWithShiftVM()
        {
            //Arrange
            var shift = new Shift
            {
                Id = 1,
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(1),
                EmployeeId = 1,
                Employee = new Employee(),
                Jobs_Shifts = new List<Job_Shift>()
            };

            A.CallTo(() => _shiftRepository.GetShiftByIdAsync(1)).Returns(shift);

            //Act
            var result = await _shiftController.Edit(1);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult.Model as ShiftVM;
            model.Should().NotBeNull();
            model.ShiftStart.Should().Be(shift.ShiftStart);
            model.ShiftEnd.Should().Be(shift.ShiftEnd);
        }

        [Fact]
        public async void ShiftController_EditGet_InvalidId_ReturnsNotFoundView()
        {
            //Arrange
            A.CallTo(() => _shiftRepository.GetShiftByIdAsync(999)).Returns((Shift)null);

            //Act
            var result = await _shiftController.Edit(999);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("NotFound");
        }

        [Fact]
        public async void ShiftController_EditPost_ValidShift_RedirectsToIndex()
        {
            //Arrange
            var shift = new ShiftVM
            {
                Id = 1,
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(1),
                EmployeeId = 1,
                JobIds = new List<int> { 1 }
            };

            //Act
            var result = await _shiftController.Edit(shift);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_shiftController.Index));
        }

        [Fact]
        public async void ShiftController_Delete_ValidShift_RedirectsToIndex()
        {
            //Arrange
            var shift = new ShiftVM
            {
                Id = 1,
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(1),
                EmployeeId = 1,
                JobIds = new List<int> { 1 }
            };


            //Act
            var result = await _shiftController.Delete(1);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_shiftController.Index));
        }
    }
}
