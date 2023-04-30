using DreamBig.Repository.Abstractions;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IRepository<Activity> activityRepository;

    public ActivityController(IRepository<Activity> activityRepository)
    {
        this.activityRepository = activityRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await activityRepository.GetAllAsync();
        return Ok(results);
    }
}
