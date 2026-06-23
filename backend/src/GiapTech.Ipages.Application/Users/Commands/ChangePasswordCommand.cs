using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Users.Commands;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IPasswordHasher hasher)
    {
        _db = db;
        _currentUser = currentUser;
        _hasher = hasher;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId == null)
            throw new UnauthorizedException();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId);

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("CurrentPassword", "Mật khẩu hiện tại không đúng.") });

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);
    }
}
