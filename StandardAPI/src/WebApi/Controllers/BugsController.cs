using Microsoft.AspNetCore.Mvc;
using BugService.Models;
using BugService.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/bugs")]
public class BugsController : ControllerBase
{
    private readonly IBugService _service;

    public BugsController(IBugService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<BugResponse>> Post([FromBody] BugDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpGet]
    public async Task<ActionResult<List<BugResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());
}