using Microsoft.AspNetCore.Mvc;
using ShiftManager.Models;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;

namespace ShiftManager.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;

        public JobController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        
        //Display all jobs
        public async Task<IActionResult> Index()
        {
            var jobs = await _jobRepository.GetAllJobsAsync();
            return View(jobs);
        }

        //Filter jobs by search string
        [HttpGet]
        public async Task<IActionResult> Filter(string searchString)
        {
            var jobs = await _jobRepository.GetAllJobsAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {

                jobs = jobs
                    .Where(j => j.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View("Index", jobs);
        }


        //Display create job form
        public IActionResult Create()
        {
            return View();
        }

        //Create a new job
        [HttpPost]
        public async Task<IActionResult> Create(JobVM job)
        {
            if (!ModelState.IsValid)
            {
                return View(job);
            }

            await _jobRepository.AddAsync(job);
            return RedirectToAction(nameof(Index));
        }

        //Display edit job form
        public async Task<IActionResult> Edit(int id)
        {
            var jobDetails = await _jobRepository.GetJobByIdAsync(id);

            if (jobDetails == null)
            {
                return View("NotFound");
            }

            var jobVM = new JobVM()
            {
                Name = jobDetails.Name,
                RequiredAge = jobDetails.RequiredAge
            };

            return View(jobVM);
        }

        //Update a job
        [HttpPost]
        public async Task<IActionResult> Edit(JobVM job)
        {
            if (!ModelState.IsValid)
            {
                return View(job);
            }
            await _jobRepository.UpdateAsync(job);
            return RedirectToAction(nameof(Index));
        }

        //Delete a job
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _jobRepository.GetJobByIdAsync(id);
            if (job == null)
            {
                return View("NotFound");
            }

            await _jobRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
