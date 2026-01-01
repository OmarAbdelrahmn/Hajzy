using Application.Abstraction;
using Application.Contracts.Policy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Policy;

public interface ICancelPolicyService
{
    Task<Result<IEnumerable<CancellationPolicyResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<CancellationPolicyResponse>>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Result<CancellationPolicyDetailsResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CancellationPolicyResponse>> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<Result<CancellationPolicyResponse>> CreateAsync(CreateCancellationPolicyRequest request, CancellationToken cancellationToken = default);

    Task<Result<CancellationPolicyResponse>> UpdateAsync(int id, UpdateCancellationPolicyRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> SetDefaultAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> ToggleActiveAsync(int id, CancellationToken cancellationToken = default);

}
