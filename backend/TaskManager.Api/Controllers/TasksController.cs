using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasks()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskEntity>> GetTask(string id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
            return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskEntity>> CreateTask(CreateTaskDto dto)
    {
        var createdTask = await _taskService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(string id, UpdateTaskDto dto)
    {
        var updatedTask = await _taskService.UpdateAsync(id, dto);
        if (updatedTask == null)
            return NotFound();
        return Ok(updatedTask);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(string id)
    {
        var result = await _taskService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("organization/{organizationId}")]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasksByOrganization(string organizationId)
    {
        var tasks = await _taskService.GetByOrganizationAsync(organizationId);
        return Ok(tasks);
    }

    [HttpGet("organization/{organizationId}/pool")]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTaskPool(string organizationId)
    {
        var tasks = await _taskService.GetTaskPoolAsync(organizationId);
        return Ok(tasks);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetUserTasks(string userId)
    {
        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpPost("{id}/assign/{userId}")]
    public async Task<ActionResult<TaskEntity>> AssignTask(string id, string userId)
    {
        var task = await _taskService.AssignTaskAsync(id, userId);
        if (task == null)
            return BadRequest("Task cannot be assigned");
        return Ok(task);
    }

    [HttpPost("{id}/pick/{userId}")]
    public async Task<ActionResult<TaskEntity>> PickTask(string id, string userId)
    {
        var task = await _taskService.PickTaskAsync(id, userId);
        if (task == null)
            return BadRequest("Task cannot be picked");
        return Ok(task);
    }

    [HttpPost("{id}/complete/{userId}")]
    public async Task<ActionResult<TaskEntity>> CompleteTask(string id, string userId)
    {
        var task = await _taskService.CompleteTaskAsync(id, userId);
        if (task == null)
            return BadRequest("Task cannot be completed");
        return Ok(task);
    }

    [HttpPost("{id}/finalize/{userId}")]
    public async Task<ActionResult<TaskEntity>> FinalizeTask(string id, string userId)
    {
        var task = await _taskService.FinalizeTaskAsync(id, userId);
        if (task == null)
            return BadRequest("Task cannot be finalized");
        return Ok(task);
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<TaskHistory>>> GetTaskHistory(string id)
    {
        var history = await _taskService.GetTaskHistoryAsync(id);
        return Ok(history);
    }
}