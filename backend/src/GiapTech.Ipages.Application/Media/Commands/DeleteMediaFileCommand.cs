using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Media.Commands;

public record DeleteMediaFileCommand(Guid Id) : IRequest;

public class DeleteMediaFileCommandHandler : IRequestHandler<DeleteMediaFileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DeleteMediaFileCommandHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteMediaFileCommand request, CancellationToken ct)
    {
        var file = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MediaFile), request.Id);

        await _storage.DeleteAsync(file.StoragePath);

        _db.MediaFiles.Remove(file);
        await _db.SaveChangesAsync(ct);
    }
}
