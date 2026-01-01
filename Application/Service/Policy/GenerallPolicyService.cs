using Application.Abstraction;
using Application.Contracts.Policy;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Policy;

public class GenerallPolicyService(ApplicationDbcontext dbcontext) : IGenerallPolicyService
{
    private readonly ApplicationDbcontext dbcontext = dbcontext;
    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<GeneralPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                new Error("ServerError", $"Error retrieving policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyDetailsResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure<GeneralPolicyDetailsResponse>(
                    new Error("NotFound", "Policy not found", 404));

            var response = MapToDetailsResponse(policy);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<GeneralPolicyDetailsResponse>(
                new Error("ServerError", $"Error retrieving policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyResponse>> CreateAsync(CreateGeneralPolicyRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate CancellationPolicy if provided
            if (request.CancellationPolicyId.HasValue)
            {
                var policyExists = await dbcontext.CancellationPolicies
                    .AnyAsync(p => p.Id == request.CancellationPolicyId.Value, cancellationToken);

                if (!policyExists)
                    return Result.Failure<GeneralPolicyResponse>(
                        new Error("NotFound", "Cancellation policy not found", 404));
            }

            // Validate Unit if provided
            if (request.UnitId.HasValue)
            {
                var unitExists = await dbcontext.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value, cancellationToken);

                if (!unitExists)
                    return Result.Failure<GeneralPolicyResponse>(
                        new Error("NotFound", "Unit not found", 404));
            }

            // Validate SubUnit if provided
            if (request.SubUnitId.HasValue)
            {
                var subUnitExists = await dbcontext.SubUnits
                    .AnyAsync(s => s.Id == request.SubUnitId.Value, cancellationToken);

                if (!subUnitExists)
                    return Result.Failure<GeneralPolicyResponse>(
                        new Error("NotFound", "SubUnit not found", 404));
            }

            var newPolicy = new GeneralPolicy
            {
                Title = request.Title,
                Description = request.Description,
                PolicyType = request.PolicyType,
                PolicyCategory = request.PolicyCategory,
                CustomPolicyName = request.CustomPolicyName,
                CancellationPolicyId = request.CancellationPolicyId,
                IsMandatory = request.IsMandatory,
                IsHighlighted = request.IsHighlighted,
                IsActive = request.IsActive,
                UnitId = request.UnitId,
                SubUnitId = request.SubUnitId
            };

            await dbcontext.GeneralPolicies.AddAsync(newPolicy, cancellationToken);
            await dbcontext.SaveChangesAsync(cancellationToken);

            // Reload to get navigation properties
            await dbcontext.Entry(newPolicy)
                .Reference(p => p.Unit)
                .LoadAsync(cancellationToken);
            await dbcontext.Entry(newPolicy)
                .Reference(p => p.SubUnit)
                .LoadAsync(cancellationToken);
            await dbcontext.Entry(newPolicy)
                .Reference(p => p.CancellationPolicy)
                .LoadAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(newPolicy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<GeneralPolicyResponse>(
                new Error("ServerError", $"Error creating policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyResponse>> UpdateAsync(int id, UpdateGeneralPolicyRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "Policy not found", 404));

            if (request.Title != null)
                policy.Title = request.Title;

            if (request.Description != null)
                policy.Description = request.Description;

            if (request.PolicyType.HasValue)
                policy.PolicyType = request.PolicyType.Value;

            if (request.PolicyCategory.HasValue)
                policy.PolicyCategory = request.PolicyCategory;

            if (request.CustomPolicyName != null)
                policy.CustomPolicyName = request.CustomPolicyName;

            if (request.CancellationPolicyId.HasValue)
            {
                var policyExists = await dbcontext.CancellationPolicies
                    .AnyAsync(p => p.Id == request.CancellationPolicyId.Value, cancellationToken);

                if (!policyExists)
                    return Result.Failure<GeneralPolicyResponse>(
                        new Error("NotFound", "Cancellation policy not found", 404));

                policy.CancellationPolicyId = request.CancellationPolicyId;
            }

            if (request.IsMandatory.HasValue)
                policy.IsMandatory = request.IsMandatory.Value;

            if (request.IsHighlighted.HasValue)
                policy.IsHighlighted = request.IsHighlighted.Value;

            if (request.IsActive.HasValue)
                policy.IsActive = request.IsActive.Value;

            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(policy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<GeneralPolicyResponse>(
                new Error("ServerError", $"Error updating policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.GeneralPolicies
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found", 404));

            dbcontext.GeneralPolicies.Remove(policy);
            await dbcontext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ServerError", $"Error deleting policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> GetPoliciesByUnitAsync(int unitId, CancellationToken cancellationToken = default)
    {
        try
        {
            var unitExists = await dbcontext.Units.AnyAsync(u => u.Id == unitId, cancellationToken);
            if (!unitExists)
                return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                    new Error("NotFound", "Unit not found", 404));

            var policies = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<GeneralPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                new Error("ServerError", $"Error retrieving unit policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> GetPoliciesBySubUnitAsync(int subUnitId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subUnitExists = await dbcontext.SubUnits.AnyAsync(s => s.Id == subUnitId, cancellationToken);
            if (!subUnitExists)
                return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var policies = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .Where(p => p.SubUnitId == subUnitId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<GeneralPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                new Error("ServerError", $"Error retrieving subunit policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyResponse>> AttachPolicyToUnitAsync(AttachPolicyToUnitRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId, cancellationToken);

            if (policy == null)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "Policy not found", 404));

            var unitExists = await dbcontext.Units.AnyAsync(u => u.Id == request.UnitId, cancellationToken);
            if (!unitExists)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "Unit not found", 404));

            // Check if policy is already attached to another unit/subunit
            if (policy.UnitId.HasValue || policy.SubUnitId.HasValue)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("AlreadyAttached", "Policy is already attached to a unit or subunit", 400));

            policy.UnitId = request.UnitId;
            policy.SubUnitId = null;

            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(policy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<GeneralPolicyResponse>(
                new Error("ServerError", $"Error attaching policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyResponse>> AttachPolicyToSubUnitAsync(AttachPolicyToSubUnitRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId, cancellationToken);

            if (policy == null)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "Policy not found", 404));

            var subUnitExists = await dbcontext.SubUnits.AnyAsync(s => s.Id == request.SubUnitId, cancellationToken);
            if (!subUnitExists)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "SubUnit not found", 404));

            // Check if policy is already attached to another unit/subunit
            if (policy.UnitId.HasValue || policy.SubUnitId.HasValue)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("AlreadyAttached", "Policy is already attached to a unit or subunit", 400));

            policy.SubUnitId = request.SubUnitId;
            policy.UnitId = null;

            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(policy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<GeneralPolicyResponse>(
                new Error("ServerError", $"Error attaching policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> RemovePolicyFromUnitAsync(int policyId, int unitId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.GeneralPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && p.UnitId == unitId, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found or not attached to this unit", 404));

            policy.UnitId = null;

            await dbcontext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ServerError", $"Error removing policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> RemovePolicyFromSubUnitAsync(int policyId, int subUnitId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.GeneralPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && p.SubUnitId == subUnitId, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found or not attached to this subunit", 404));

            policy.SubUnitId = null;

            await dbcontext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ServerError", $"Error removing policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<GeneralPolicyResponse>> CreateCustomPolicyForUnitAsync(CreateCustomPolicyForUnitRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var unitExists = await dbcontext.Units.AnyAsync(u => u.Id == request.UnitId, cancellationToken);
            if (!unitExists)
                return Result.Failure<GeneralPolicyResponse>(
                    new Error("NotFound", "Unit not found", 404));

            if (request.CancellationPolicyId.HasValue)
            {
                var policyExists = await dbcontext.CancellationPolicies
                    .AnyAsync(p => p.Id == request.CancellationPolicyId.Value, cancellationToken);

                if (!policyExists)
                    return Result.Failure<GeneralPolicyResponse>(
                        new Error("NotFound", "Cancellation policy not found", 404));
            }

            var newPolicy = new GeneralPolicy
            {
                Title = request.Title,
                Description = request.Description,
                PolicyType = request.PolicyType,
                PolicyCategory = request.PolicyCategory,
                CustomPolicyName = request.CustomPolicyName,
                CancellationPolicyId = request.CancellationPolicyId,
                IsMandatory = request.IsMandatory,
                IsHighlighted = request.IsHighlighted,
                IsActive = request.IsActive,
                UnitId = request.UnitId,
                SubUnitId = null
            };

            await dbcontext.GeneralPolicies.AddAsync(newPolicy, cancellationToken);
            await dbcontext.SaveChangesAsync(cancellationToken);

            // Reload to get navigation properties
            await dbcontext.Entry(newPolicy)
                .Reference(p => p.Unit)
                .LoadAsync(cancellationToken);
            await dbcontext.Entry(newPolicy)
                .Reference(p => p.CancellationPolicy)
                .LoadAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(newPolicy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<GeneralPolicyResponse>(
                new Error("ServerError", $"Error creating custom policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> FilterPoliciesAsync(PolicyFilterRequest filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = dbcontext.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.SubUnit)
                .Include(p => p.CancellationPolicy)
                .AsQueryable();

            if (filter.PolicyType.HasValue)
                query = query.Where(p => p.PolicyType == filter.PolicyType.Value);

            if (filter.PolicyCategory.HasValue)
                query = query.Where(p => p.PolicyCategory == filter.PolicyCategory.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (filter.IsMandatory.HasValue)
                query = query.Where(p => p.IsMandatory == filter.IsMandatory.Value);

            if (filter.UnitId.HasValue)
                query = query.Where(p => p.UnitId == filter.UnitId.Value);

            if (filter.SubUnitId.HasValue)
                query = query.Where(p => p.SubUnitId == filter.SubUnitId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Scope))
            {
                switch (filter.Scope.ToLower())
                {
                    case "global":
                        query = query.Where(p => p.UnitId == null && p.SubUnitId == null);
                        break;
                    case "unit":
                        query = query.Where(p => p.UnitId != null);
                        break;
                    case "subunit":
                        query = query.Where(p => p.SubUnitId != null);
                        break;
                }
            }

            var policies = await query.AsNoTracking().ToListAsync(cancellationToken);
            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<GeneralPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                new Error("ServerError", $"Error filtering policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> GetGlobalPoliciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await dbcontext.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == null && p.SubUnitId == null)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<GeneralPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<GeneralPolicyResponse>>(
                new Error("ServerError", $"Error retrieving global policies: {ex.Message}", 500));
        }
    }

    public async Task<Result> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.GeneralPolicies
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found", 404));

            policy.IsActive = !policy.IsActive;

            await dbcontext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ServerError", $"Error toggling policy status: {ex.Message}", 500));
        }
    }

    // Helper Methods
    private static GeneralPolicyResponse MapToResponse(GeneralPolicy policy)
    {
        var scope = "Global";
        if (policy.UnitId.HasValue)
            scope = "Unit";
        else if (policy.SubUnitId.HasValue)
            scope = "SubUnit";

        return new GeneralPolicyResponse(
            policy.Id,
            policy.Title,
            policy.Description,
            policy.PolicyType,
            policy.PolicyCategory,
            policy.CustomPolicyName,
            policy.CancellationPolicyId,
            policy.CancellationPolicy?.Name,
            policy.IsMandatory,
            policy.IsHighlighted,
            policy.IsActive,
            policy.UnitId,
            policy.Unit?.Name,
            policy.SubUnitId,
            policy.SubUnit?.RoomNumber,
            scope
        );
    }

    private static GeneralPolicyDetailsResponse MapToDetailsResponse(GeneralPolicy policy)
    {
        var scope = "Global";
        if (policy.UnitId.HasValue)
            scope = "Unit";
        else if (policy.SubUnitId.HasValue)
            scope = "SubUnit";

        CancellationPolicyBasicInfo? cancellationPolicyInfo = null;
        if (policy.CancellationPolicy != null)
        {
            cancellationPolicyInfo = new CancellationPolicyBasicInfo(
                policy.CancellationPolicy.Id,
                policy.CancellationPolicy.Name,
                policy.CancellationPolicy.FullRefundDays,
                policy.CancellationPolicy.PartialRefundDays,
                policy.CancellationPolicy.PartialRefundPercentage
            );
        }

        UnitBasicInfo? unitInfo = null;
        if (policy.Unit != null)
        {
            unitInfo = new UnitBasicInfo(
                policy.Unit.Id,
                policy.Unit.Name,
                policy.Unit.Address
            );
        }

        SubUnitBasicInfo? subUnitInfo = null;
        if (policy.SubUnit != null)
        {
            subUnitInfo = new SubUnitBasicInfo(
                policy.SubUnit.Id,
                policy.SubUnit.RoomNumber,
                policy.SubUnit.Type.ToString()
            );
        }

        return new GeneralPolicyDetailsResponse(
            policy.Id,
            policy.Title,
            policy.Description,
            policy.PolicyType,
            policy.PolicyCategory,
            policy.CustomPolicyName,
            cancellationPolicyInfo,
            policy.IsMandatory,
            policy.IsHighlighted,
            policy.IsActive,
            unitInfo,
            subUnitInfo,
            scope
        );
    }
}