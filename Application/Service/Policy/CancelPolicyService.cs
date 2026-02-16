using Application.Abstraction;
using Application.Contracts.Policy;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Policy;

public class CancelPolicyService(ApplicationDbcontext dbcontext) : ICancelPolicyService
{
    private readonly ApplicationDbcontext dbcontext = dbcontext;

    public async Task<Result<IEnumerable<CancellationPolicyResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await dbcontext.CancellationPolicies
                .Include(p => p.Units)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<CancellationPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<CancellationPolicyResponse>>(
                new Error("ServerError", $"Error retrieving cancellation policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<IEnumerable<CancellationPolicyResponse>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await dbcontext.CancellationPolicies
                .Where(p => p.IsActive)
                .Include(p => p.Units)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = policies.Select(p => MapToResponse(p)).ToList();

            return Result.Success<IEnumerable<CancellationPolicyResponse>>(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<CancellationPolicyResponse>>(
                new Error("ServerError", $"Error retrieving active policies: {ex.Message}", 500));
        }
    }

    public async Task<Result<CancellationPolicyDetailsResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.CancellationPolicies
                .Include(p => p.Units)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure<CancellationPolicyDetailsResponse>(
                    new Error("NotFound", "Cancellation policy not found", 404));

            var response = MapToDetailsResponse(policy);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<CancellationPolicyDetailsResponse>(
                new Error("ServerError", $"Error retrieving policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<CancellationPolicyResponse>> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.CancellationPolicies
                .Include(p => p.Units)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);

            if (policy == null)
                return Result.Failure<CancellationPolicyResponse>(
                    new Error("NotFound", "No default cancellation policy found", 404));

            var response = MapToResponse(policy);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<CancellationPolicyResponse>(
                new Error("ServerError", $"Error retrieving default policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<CancellationPolicyResponse>> CreateAsync(CreateCancellationPolicyRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Check if name already exists
            var nameExists = await dbcontext.CancellationPolicies
                .AnyAsync(p => p.Name == request.Name, cancellationToken);

            if (nameExists)
                return Result.Failure<CancellationPolicyResponse>(
                    new Error("AlreadyExists", "A policy with this name already exists", 400));

            // If setting as default, remove default from others
            if (request.IsDefault)
            {
                var currentDefault = await dbcontext.CancellationPolicies
                    .Where(p => p.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var policy in currentDefault)
                {
                    policy.IsDefault = false;
                }
            }

            var newPolicy = new CancellationPolicy
            {
                Name = request.Name,
                Description = request.Description,
                FullRefundDays = request.FullRefundDays,
                PartialRefundDays = request.PartialRefundDays,
                PartialRefundPercentage = request.PartialRefundPercentage,
                IsActive = request.IsActive,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            await dbcontext.CancellationPolicies.AddAsync(newPolicy, cancellationToken);
            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(newPolicy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CancellationPolicyResponse>(
                new Error("ServerError", $"Error creating policy: {ex.Message}", 500));
        }
    }

    public async Task<Result<CancellationPolicyResponse>> UpdateAsync(int id, UpdateCancellationPolicyRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.CancellationPolicies
                .Include(p => p.Units)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure<CancellationPolicyResponse>(
                    new Error("NotFound", "Cancellation policy not found", 404));

            // Check name uniqueness if changing name
            if (request.Name != null && request.Name != policy.Name)
            {
                var nameExists = await dbcontext.CancellationPolicies
                    .AnyAsync(p => p.Name == request.Name && p.Id != id, cancellationToken);

                if (nameExists)
                    return Result.Failure<CancellationPolicyResponse>(
                        new Error("AlreadyExists", "A policy with this name already exists", 400));

                policy.Name = request.Name;
            }

            if (request.Description != null)
                policy.Description = request.Description;

            if (request.FullRefundDays.HasValue)
                policy.FullRefundDays = request.FullRefundDays.Value;

            if (request.PartialRefundDays.HasValue)
                policy.PartialRefundDays = request.PartialRefundDays.Value;

            if (request.PartialRefundPercentage.HasValue)
                policy.PartialRefundPercentage = request.PartialRefundPercentage.Value;

            if (request.IsActive.HasValue)
                policy.IsActive = request.IsActive.Value;

            // Handle setting as default
            if (request.IsDefault.HasValue && request.IsDefault.Value && !policy.IsDefault)
            {
                var currentDefault = await dbcontext.CancellationPolicies
                    .Where(p => p.IsDefault && p.Id != id)
                    .ToListAsync(cancellationToken);

                foreach (var defaultPolicy in currentDefault)
                {
                    defaultPolicy.IsDefault = false;
                }

                policy.IsDefault = true;
            }
            else if (request.IsDefault.HasValue && !request.IsDefault.Value)
            {
                policy.IsDefault = false;
            }

            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(MapToResponse(policy));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CancellationPolicyResponse>(
                new Error("ServerError", $"Error updating policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.CancellationPolicies
                .Include(p => p.Units)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Cancellation policy not found", 404));

            // Check if policy is in use
            if (policy.Units.Any())
                return Result.Failure(new Error("InUse",
                    $"Cannot delete policy. It is currently assigned to {policy.Units.Count} unit(s)", 400));

            dbcontext.CancellationPolicies.Remove(policy);
            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(new Error("ServerError", $"Error deleting policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> SetDefaultAsync(int id, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var policy = await dbcontext.CancellationPolicies
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Cancellation policy not found", 404));

            if (!policy.IsActive)
                return Result.Failure(new Error("NotActive", "Cannot set an inactive policy as default", 400));

            // Remove default from all others
            var currentDefaults = await dbcontext.CancellationPolicies
                .Where(p => p.IsDefault && p.Id != id)
                .ToListAsync(cancellationToken);

            foreach (var defaultPolicy in currentDefaults)
            {
                defaultPolicy.IsDefault = false;
            }

            policy.IsDefault = true;

            await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(new Error("ServerError", $"Error setting default policy: {ex.Message}", 500));
        }
    }

    public async Task<Result> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await dbcontext.CancellationPolicies
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Cancellation policy not found", 404));

            // Don't allow deactivating the default policy
            if (policy.IsDefault && policy.IsActive)
                return Result.Failure(new Error("IsDefault",
                    "Cannot deactivate the default policy. Set another policy as default first", 400));

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
    private static CancellationPolicyResponse MapToResponse(CancellationPolicy policy)
    {
        return new CancellationPolicyResponse(
            policy.Id,
            policy.Name,
            policy.Description,
            policy.FullRefundDays,
            policy.PartialRefundDays,
            policy.PartialRefundPercentage,
            policy.IsActive,
            policy.IsDefault,
            policy.CreatedAt,
            policy.Units?.Count ?? 0
        );
    }

    private static CancellationPolicyDetailsResponse MapToDetailsResponse(CancellationPolicy policy)
    {
        var assignedUnits = policy.Units?.Select(u => new UnitBasicInfo(
            u.Id,
            u.Name,
            u.Address
        )).ToList() ?? new List<UnitBasicInfo>();

        return new CancellationPolicyDetailsResponse(
            policy.Id,
            policy.Name,
            policy.Description,
            policy.FullRefundDays,
            policy.PartialRefundDays,
            policy.PartialRefundPercentage,
            policy.IsActive,
            policy.IsDefault,
            policy.CreatedAt,
            assignedUnits
        );
    }
}
