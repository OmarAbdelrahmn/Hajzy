using Application.Abstraction;
using Application.Contracts.Policy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Policy;

public interface IGenerallPolicyService
{
    Task<Result<IEnumerable<GeneralPolicyResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<GeneralPolicyDetailsResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<GeneralPolicyResponse>> CreateAsync(CreateGeneralPolicyRequest request, CancellationToken cancellationToken = default);

    Task<Result<GeneralPolicyResponse>> UpdateAsync(int id, UpdateGeneralPolicyRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Unit/SubUnit Management
    Task<Result<IEnumerable<GeneralPolicyResponse>>> GetPoliciesByUnitAsync(int unitId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<GeneralPolicyResponse>>> GetPoliciesBySubUnitAsync(int subUnitId, CancellationToken cancellationToken = default);

    Task<Result<GeneralPolicyResponse>> AttachPolicyToUnitAsync(AttachPolicyToUnitRequest request, CancellationToken cancellationToken = default);

    Task<Result<GeneralPolicyResponse>> AttachPolicyToSubUnitAsync(AttachPolicyToSubUnitRequest request, CancellationToken cancellationToken = default);

    Task<Result> RemovePolicyFromUnitAsync(int policyId, int unitId, CancellationToken cancellationToken = default);

    Task<Result> RemovePolicyFromSubUnitAsync(int policyId, int subUnitId, CancellationToken cancellationToken = default);

    // Custom Policies
    Task<Result<GeneralPolicyResponse>> CreateCustomPolicyForUnitAsync(CreateCustomPolicyForUnitRequest request, CancellationToken cancellationToken = default);

    // Filtering
    Task<Result<IEnumerable<GeneralPolicyResponse>>> FilterPoliciesAsync(PolicyFilterRequest filter, CancellationToken cancellationToken = default);

    // Global Policies (not attached to any unit/subunit)
    Task<Result<IEnumerable<GeneralPolicyResponse>>> GetGlobalPoliciesAsync(CancellationToken cancellationToken = default);

    // Toggle Active Status
    Task<Result> ToggleActiveAsync(int id, CancellationToken cancellationToken = default);

}
