using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Repositories.MaterialTransactions;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.Services.ServiceOrders;

public class ServiceOrderStockDebit
{
    private readonly IServiceOrderMaterialReadOnlyRepository _serviceOrderMaterialReadOnlyRepository;
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialWriteOnlyRepository _materialWriteOnlyRepository;
    private readonly IMaterialTransactionWriteOnlyRepository _materialTransactionWriteOnlyRepository;

    public ServiceOrderStockDebit(
        IServiceOrderMaterialReadOnlyRepository serviceOrderMaterialReadOnlyRepository,
        IMaterialReadOnlyRepository materialReadOnlyRepository,
        IMaterialWriteOnlyRepository materialWriteOnlyRepository,
        IMaterialTransactionWriteOnlyRepository materialTransactionWriteOnlyRepository)
    {
        _serviceOrderMaterialReadOnlyRepository = serviceOrderMaterialReadOnlyRepository;
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialWriteOnlyRepository = materialWriteOnlyRepository;
        _materialTransactionWriteOnlyRepository = materialTransactionWriteOnlyRepository;
    }

    // Só lê: não altera saldo nem cria transação. Usado para decidir entre IN_EXECUTION e AWAITING_MATERIALS.
    public async Task<bool> HasSufficientStockAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        var rows = (await _serviceOrderMaterialReadOnlyRepository.GetAllByServiceOrder(order.Id, cancellationToken))
            .Where(r => r.Status == EntityStatus.ACTIVE)
            .ToList();

        foreach (var row in rows)
        {
            var material = await _materialReadOnlyRepository.GetById(row.MaterialId, cancellationToken)
                ?? throw new MaterialNotFoundException(row.MaterialId);

            if (material.StockQuantity < row.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    // Não confirma nada: quem chama faz o Commit, junto com a mudança de status.
    public async Task DebitActiveMaterialsAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        var rows = (await _serviceOrderMaterialReadOnlyRepository.GetAllByServiceOrder(order.Id, cancellationToken))
            .Where(r => r.Status == EntityStatus.ACTIVE)
            .ToList();

        var pairs = new List<(ServiceOrderMaterial Row, Material Material)>();

        foreach (var row in rows)
        {
            var material = await _materialReadOnlyRepository.GetById(row.MaterialId, cancellationToken)
                ?? throw new MaterialNotFoundException(row.MaterialId);

            pairs.Add((row, material));
        }

        // Checa todos os saldos antes de mudar qualquer material.
        foreach (var (row, material) in pairs)
        {
            if (material.StockQuantity < row.Quantity)
            {
                throw new InsufficientStockException(material.Id);
            }
        }

        foreach (var (row, material) in pairs)
        {
            material.DecreaseStock(row.Quantity);

            await _materialWriteOnlyRepository.Update(material, cancellationToken);

            var transaction = MaterialTransaction.Create(
                material.Id,
                TransactionType.OUT,
                row.Quantity,
                material.StockQuantity,
                order.Id);

            await _materialTransactionWriteOnlyRepository.Add(transaction, cancellationToken);
        }
    }
}
