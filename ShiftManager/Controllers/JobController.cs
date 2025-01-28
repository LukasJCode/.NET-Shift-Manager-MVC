using Microsoft.AspNetCore.Mvc;
using ShiftManager.Models.ViewModels;
using ShiftManager.Services.Interfaces;

namespace ShiftManager.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<JobController> _logger;

        public JobController(IJobRepository jobRepository, ILogger<JobController> logger)
        {
            _jobRepository = jobRepository;
            _logger = logger;
        }
        
        public async Task<IActionResult> Index()
        {
            try
            {
                var jobs = await _jobRepository.GetAllJobsAsync();
                return View(jobs);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching jobs.");
                return View("GenericError");
            }
        }

        //Filter jobs by name
        [HttpGet]
        public async Task<IActionResult> Filter(string jobName)
        {
            try
            {
                var jobs = await _jobRepository.GetAllJobsAsync();

                if (!string.IsNullOrWhiteSpace(jobName))
                {

                    jobs = jobs
                        .Where(j => j.Name.Contains(jobName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return View(nameof(Index), jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching jobs.");
                return View("GenericError");
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobVM job)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(job);
                }

                await _jobRepository.AddAsync(job);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding job.");
                return View("GenericError");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching job.");
                return View("GenericError");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(JobVM job)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(job);
                }
                await _jobRepository.UpdateAsync(job);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a job.");
                return View("GenericError");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var job = await _jobRepository.GetJobByIdAsync(id);
                if (job == null)
                {
                    return View("NotFound");
                }

                await _jobRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting job.");
                return View("GenericError");
            }

        }
    }
}
