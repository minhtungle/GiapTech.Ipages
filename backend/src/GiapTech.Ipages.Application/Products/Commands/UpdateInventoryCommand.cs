using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Products.Commands;

public record UpdateInventoryCommand(Guid ProductId, int Quantity, string? Note = null) : IRequest;

public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
{
    public UpdateInventoryCommandValidator()
    {
        RuleFor(x => x.Quantity).NotEqual(0).WithMessage("Số lượng không được bằng 0.");
    }
}

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpdateInventoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task Handle(UpdateInventoryCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        product.StockQuantity += request.Quantity;
        if (product.StockQuantity < 0)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Quantity", "Tồn kho không đủ.") });

        var entry = new Inventory
        {
            TenantId = product.TenantId,
            ProductId = product.Id,
            Quantity = Math.Abs(request.Quantity),
            Type = request.Quantity > 0 ? InventoryType.Import : InventoryType.Export,
            Notes = request.Note,
            QuantityBefore = product.StockQuantity - request.Quantity,
            QuantityAfter = product.StockQuantity,
            CreatedBy = Guid.Empty
        };

        _db.Inventories.Add(entry);
        await _db.SaveChangesAsync(ct);
    }
}
