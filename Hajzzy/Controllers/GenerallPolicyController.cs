using Application.Contracts.Policy;
using Application.Service.Policy;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenerallPolicyController(IGenerallPolicyService service) : ControllerBase
{
    private readonly IGenerallPolicyService service = service;
    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a specific policy by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all global policies (not attached to any unit/subunit)
    /// </summary>
    [HttpGet("global")]
    public async Task<IActionResult> GetGlobal(CancellationToken cancellationToken)
    {
        var result = await service.GetGlobalPoliciesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all policies for a specific unit
    /// </summary>
    [HttpGet("unit/{unitId:int}")]
    public async Task<IActionResult> GetByUnit(int unitId, CancellationToken cancellationToken)
    {
        var result = await service.GetPoliciesByUnitAsync(unitId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all policies for a specific subunit
    /// </summary>
    [HttpGet("subunit/{subUnitId:int}")]
    public async Task<IActionResult> GetBySubUnit(int subUnitId, CancellationToken cancellationToken)
    {
        var result = await service.GetPoliciesBySubUnitAsync(subUnitId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Filter policies by various criteria
    /// </summary>
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] PolicyFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await service.FilterPoliciesAsync(filter, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new general policy
    /// </summary>
    [HttpPost("")]

    public async Task<IActionResult> Create([FromBody] CreateGeneralPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Create a custom policy for a specific unit
    /// </summary>
    [HttpPost("custom-for-unit")]

    public async Task<IActionResult> CreateCustomForUnit([FromBody] CreateCustomPolicyForUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateCustomPolicyForUnitAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing policy
    /// </summary>
    [HttpPut("{id:int}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateGeneralPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a policy
    /// </summary>
    [HttpDelete("{id:int}")]

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Policy deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Attach a global policy to a unit
    /// </summary>
    [HttpPost("attach-to-unit")]

    public async Task<IActionResult> AttachToUnit([FromBody] AttachPolicyToUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AttachPolicyToUnitAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Attach a global policy to a subunit
    /// </summary>
    [HttpPost("attach-to-subunit")]

    public async Task<IActionResult> AttachToSubUnit([FromBody] AttachPolicyToSubUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AttachPolicyToSubUnitAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Remove a policy from a unit
    /// </summary>
    [HttpDelete("unit/{unitId:int}/policy/{policyId:int}")]

    public async Task<IActionResult> RemoveFromUnit(int policyId, int unitId, CancellationToken cancellationToken)
    {
        var result = await service.RemovePolicyFromUnitAsync(policyId, unitId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Policy removed from unit successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove a policy from a subunit
    /// </summary>
    [HttpDelete("subunit/{subUnitId:int}/policy/{policyId:int}")]

    public async Task<IActionResult> RemoveFromSubUnit(int policyId, int subUnitId, CancellationToken cancellationToken)
    {
        var result = await service.RemovePolicyFromSubUnitAsync(policyId, subUnitId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { Message = "Policy removed from subunit successfully" })
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