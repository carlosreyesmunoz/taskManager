using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
    {
        var organizations = await _organizationService.GetAllAsync();
        return Ok(organizations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Organization>> GetOrganization(string id)
    {
        var organization = await _organizationService.GetByIdAsync(id);
        if (organization == null)
            return NotFound();
        return Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<Organization>> CreateOrganization(CreateOrganizationDto dto)
    {
        var createdOrganization = await _organizationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetOrganization), new { id = createdOrganization.Id }, createdOrganization);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(string id, UpdateOrganizationDto dto)
    {
        var updatedOrganization = await _organizationService.UpdateAsync(id, dto);
        if (updatedOrganization == null)
            return NotFound();
        return Ok(updatedOrganization);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(string id)
    {
        var result = await _organizationService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/users")]
    public async Task<ActionResult<IEnumerable<User>>> GetOrganizationUsers(string id)
    {
        var users = await _organizationService.GetUsersAsync(id);
        return Ok(users);
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetOrganizationTasks(string id)
    {
        var tasks = await _organizationService.GetTasksAsync(id);
        return Ok(tasks);
    }
}