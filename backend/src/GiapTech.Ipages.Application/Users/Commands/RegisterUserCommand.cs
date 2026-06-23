using FluentValidation;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Users.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Users.Commands;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string? FullName,
    string? PhoneNumber) : IRequest<UserDto>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50).Matches("^[a-zA-Z0-9_]+$").WithMessage("Username chỉ chứa chữ cái, số và dấu gạch dưới.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _hasher;

    public RegisterUserCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, IPasswordHasher hasher)
    {
        _db = db;
        _tenant = tenant;
        _hasher = hasher;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new Common.Exceptions.ForbiddenException();

        var exists = await _db.Users.AnyAsync(u => u.TenantId == _tenant.TenantId && (u.Username == request.Username || u.Email == request.Email), ct);
        if (exists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Username", "Username hoặc Email đã tồn tại.") });

        var user = new User
        {
            TenantId = _tenant.TenantId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.AvatarUrl, user.IsActive, user.LastLoginAt, user.CreatedAt);
    }
}
