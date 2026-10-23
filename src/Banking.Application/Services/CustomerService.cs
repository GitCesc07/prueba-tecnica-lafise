using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;

namespace Banking.Application.Services;

public sealed class CustomerService(ICustomerRepository customers, IUnitOfWork uow, TimeProvider clock) : ICustomerService
{
    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = Customer.Create(request.FullName, request.DateOfBirth, request.Gender,
                                       request.MonthlyIncome, clock.GetUtcNow().UtcDateTime);
        await customers.AddAsync(customer, ct);
        await uow.SaveChangesAsync(ct);
        return customer.ToResponse();
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await customers.GetByIdAsync(id, ct) ?? throw new CustomerNotFoundException(id);
        return customer.ToResponse();
    }
}
