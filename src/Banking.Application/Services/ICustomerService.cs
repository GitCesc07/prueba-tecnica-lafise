using Banking.Application.Dtos;

namespace Banking.Application.Services;

public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
}
