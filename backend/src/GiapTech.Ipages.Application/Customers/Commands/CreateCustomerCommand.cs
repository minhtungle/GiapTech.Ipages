using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Customers.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;

namespace GiapTech.Ipages.Application.Customers.Commands;

public record CreateCustomerCommand(
    string FullName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    string? Notes) : IRequest<CustomerDto>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateCustomerCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var customer = new Customer
        {
            TenantId = _tenant.TenantId.Value,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Notes = request.Notes
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        return new CustomerDto(customer.Id, customer.FullName, customer.Email, customer.Phone, customer.AvatarUrl, customer.DateOfBirth, customer.Gender, customer.IsActive, customer.LoyaltyPoints, customer.Notes, 0, customer.CreatedAt);
    }
}

public record UpdateCustomerCommand(
    Guid Id,
    string FullName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    bool IsActive,
    string? Notes) : IRequest<CustomerDto>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateCustomerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _db.Customers.FindAsync([request.Id], ct)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        customer.FullName = request.FullName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.DateOfBirth = request.DateOfBirth;
        customer.Gender = request.Gender;
        customer.IsActive = request.IsActive;
        customer.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);

        return new CustomerDto(customer.Id, customer.FullName, customer.Email, customer.Phone, customer.AvatarUrl, customer.DateOfBirth, customer.Gender, customer.IsActive, customer.LoyaltyPoints, customer.Notes, 0, customer.CreatedAt);
    }
}
