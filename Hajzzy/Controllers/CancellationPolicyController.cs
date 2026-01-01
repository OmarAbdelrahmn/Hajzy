using Application.Contracts.Policy;
using Application.Service.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CancellationPolicyController(ICancelPolicyService service) : ControllerBase
{

    private readonly ICancelPolicyService service = service;


    /// <summary>
    /// Get all cancellation policies
    /// </summary>
    [HttpGet("")]
 
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get only active cancellation policies
    /// </summary>
    [HttpGet("active")]
 
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await service.GetActiveAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a specific cancellation policy by ID
    /// </summary>
    [HttpGet("{id:int}")]
 
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get the default cancellation policy
    /// </summary>
    [HttpGet("default")]
 
    public async Task<IActionResult> GetDefault(CancellationToken cancellationToken)
    {
        var result = await service.GetDefaultAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new cancellation policy
    /// </summary>
    [HttpPost("")]
 
    public async Task<IActionResult> Create([FromBody] CreateCancellationPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing cancellation policy
    /// </summary>
    [HttpPut("{id:int}")]
 
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCancellationPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a cancellation policy
    /// </summary>
    [HttpDelete("{id:int}")]
 
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Cancellation policy deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set a policy as the default
    /// </summary>
    [HttpPost("{id:int}/set-default")]
 
    public async Task<IActionResult> SetDefault(int id, CancellationToken cancellationToken)
    {
        var result = await service.SetDefaultAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Policy set as default successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle the active status of a policy
    /// </summary>
    [HttpPost("{id:int}/toggle-active")]
 
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var result = await service.ToggleActiveAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Policy status toggled successfully" })
            : result.ToProblem();
    }
}
