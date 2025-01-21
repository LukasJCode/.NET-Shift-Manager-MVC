using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Controllers;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Repos;
using ShiftManager.Services.Interfaces;


namespace ShiftManager.Tests.ControllerTests
{
    public class JobControllerTests
    {
        private IJobRepository _jobRepository;
        private JobController _jobController;
        public JobControllerTests()
        {
            //Dependencies
            _jobRepository = A.Fake<IJobRepository>();

            //SUT
            _jobController = new JobController(_jobRepository);
        }

        [Fact]
        public async void JobController_Index_ReturnsViewWithJobs()
        {
            //Arrange
            var jobs = new List<Job>
            {
                new Job { Id = 1, Name = "Job1", RequiredAge = 1 },
                new Job { Id = 2, Name = "Job2", RequiredAge = 1 }
            };

            A.CallTo(() => _jobRepository.GetAllJobsAsync()).Returns(jobs);

            //Act
            var result = await _jobController.Index();

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(jobs);
        }

        [Fact]
        public async void JobController_Index_ReturnsViewWithNoJobs()
        {
            //Arrange
            var jobs = new List<Job>();

            A.CallTo(() => _jobRepository.GetAllJobsAsync()).Returns(jobs);

            //Act
            var result = await _jobController.Index();

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(jobs);
        }

        [Fact]
        public async void JobController_Filter_ReturnsViewWithFilteredJobs()
        {
            //Arrange
            var jobs = new List<Job>
            {
                new Job { Id = 1, Name = "Wait Tables", RequiredAge = 1 },
                new Job { Id = 2, Name = "Wash Dishes", RequiredAge = 1 }
            };
            var filteredList = new List<Job>
            {
                new Job { Id = 1, Name = "Wait Tables", RequiredAge = 1 },
            };

            A.CallTo(() => _jobRepository.GetAllJobsAsync()).Returns(jobs);

            //Act
            var result = await _jobController.Filter("Wait");

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(filteredList);
        }

        [Fact]
        public async void JobController_Create_ValidJob_RedirectsToIndex()
        {
            //Arrange
            var job = new JobVM
            {
                Name = "Wait tables",
                RequiredAge = 16,
            };

            //Act
            var result = await _jobController.Create(job);

            //Assert
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.Should().NotBeNull();
            redirectToActionResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async void JobController_EditGet_ValidId_ReturnsViewWithJobVM()
        {
            //Arrange
            var job = new Job
            {
                Name = "Wait tables",
                RequiredAge = 16,
            };

            A.CallTo(() => _jobRepository.GetJobByIdAsync(1)).Returns(job);

            //Act
            var result = await _jobController.Edit(1);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var model = viewResult.Model as JobVM;
            model.Should().NotBeNull();
            model.Name.Should().Be(job.Name);
            model.RequiredAge.Should().Be(job.RequiredAge);
        }

        [Fact]
        public async void JobController_EditGet_InvalidId_ReturnsNotFoundView()
        {
            //Arrange
            A.CallTo(() => _jobRepository.GetJobByIdAsync(999)).Returns((Job)null);

            //Act
            var result = await _jobController.Edit(999);

            //Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("NotFound");
        }

        [Fact]
        public async void JobController_EditPost_ValidJob_RedirectsToIndex()
        {
            //Arrange
            var job = new JobVM
            {
                Name = "Wait tables",
                RequiredAge = 16,
            };

            //Act
            var result = await _jobController.Edit(job);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_jobController.Index));
        }

        //Delete tests
        [Fact]
        public async void JobController_Delete_ValidJob_RedirectsToIndex()
        {
            //Arrange
            var job = new JobVM
            {
                Name = "Wait tables",
                RequiredAge = 16,
            };

            //Act
            var result = await _jobController.Delete(1);

            //Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be(nameof(_jobController.Index));
        }
    }
}
