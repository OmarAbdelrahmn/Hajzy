using Application.Abstraction;
using Application.Contracts.Aminety;
using Application.Service.Amenity;
using Application.Service.UnitAmenity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AmenityController(IAmenityService service) : ControllerBase
{
    private readonly IAmenityService _service = service;

    #region CRUD Operations

    /// <summary>
    /// Get all amenities
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAmenitiesAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get amenities with filtering
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] AmenityFilter filter)
    {
        var result = await _service.FilterAmenitiesAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get amenity by ID with details
    /// </summary>
    [HttpGet("{amenityId}")]
    public async Task<IActionResult> GetById(int amenityId)
    {
        var result = await _service.GetByIdAsync(amenityId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get amenities by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var result = await _service.GetByCategoryAsync(category);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new amenity
    /// </summary>
    [HttpPost("")]
 
    public async Task<IActionResult> Create([FromBody] CreateAmenityRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { amenityId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing amenity
    /// </summary>
    [HttpPut("{amenityId}")]
 
    public async Task<IActionResult> Update(int amenityId, [FromBody] UpdateAmenityRequest request)
    {
        var result = await _service.UpdateAsync(amenityId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete an amenity
    /// </summary>
    [HttpDelete("{amenityId}")]
 
    public async Task<IActionResult> Delete(int amenityId)
    {
        var result = await _service.DeleteAsync(amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity deleted successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Statistics

    ///// <summary>
    ///// Get amenity usage statistics
    ///// </summary>
    //[HttpGet("{amenityId}/statistics")]
    //[Authorize(Roles = "SuperAdmin,CityAdmin")]
    //public async Task<IActionResult> GetStatistics(int amenityId)
    //{
    //    var result = await _service.GetAmenityStatisticsAsync(amenityId);
    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}

    /// <summary>
    /// Get all amenity categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _service.GetCategoriesAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion
}