using DreamBig.Repository.Abstractions;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[ApiController]
[Route("kids")]
public class KidController : ControllerBase
{
    private readonly IRepository<Kid> kidRepository;

    public KidController(IRepository<Kid> kidRepository)
    {
        this.kidRepository = kidRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await kidRepository.GetAllAsync();
        return Ok(results);
    }
}
