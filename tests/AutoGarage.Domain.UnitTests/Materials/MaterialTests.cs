using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Materials;

public class MaterialTests
{
    [Fact]
    public void Create_ValidValues_ShouldBeActiveWithZeroStock()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);

        material.Name.ShouldBe("PARAFUSO");
        material.Description.ShouldBe("Sextavado");
        material.Price.ShouldBe(2.50m);
        material.StockQuantity.ShouldBe(0);
        material.StockMinimum.ShouldBe(3);
        material.Status.ShouldBe(EntityStatus.ACTIVE);
        material.Id.ShouldNotBe(Guid.Empty);
        material.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_NullDescription_ShouldKeepNull()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        material.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_ZeroStockMinimum_ShouldBeAccepted()
    {
        var material = Material.Create("PARAFUSO", null, 1m, 0);

        material.Price.ShouldBe(1m);
        material.StockMinimum.ShouldBe(0);
        material.StockQuantity.ShouldBe(0);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtInUtc()
    {
        var before = DateTime.UtcNow;

        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        material.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        material.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        material.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Activate_InactiveMaterial_ShouldBeActiveAndSetUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();

        material.Activate();

        material.Status.ShouldBe(EntityStatus.ACTIVE);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Activate_AlreadyActive_ShouldThrowMaterialAlreadyActiveException()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        Should.Throw<MaterialAlreadyActiveException>(() => material.Activate());

        material.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Deactivate_ActiveMaterial_ShouldBeInactiveAndSetUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        material.Deactivate();

        material.Status.ShouldBe(EntityStatus.INACTIVE);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ShouldThrowMaterialAlreadyInactiveException()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();

        Should.Throw<MaterialAlreadyInactiveException>(() => material.Deactivate());
    }

    [Fact]
    public void UpdateProfile_ValidValues_ShouldChangeFieldsKeepStockAndSetUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        material.UpdateProfile("PORCA SEXTAVADA", "Aço inox", 4.00m, 5);

        material.Name.ShouldBe("PORCA SEXTAVADA");
        material.Description.ShouldBe("Aço inox");
        material.Price.ShouldBe(4.00m);
        material.StockMinimum.ShouldBe(5);
        material.StockQuantity.ShouldBe(0);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfile_NullDescription_ShouldClearDescription()
    {
        var material = Material.Create("PARAFUSO", "Antiga", 2.50m, 3);

        material.UpdateProfile("PARAFUSO", null, 2.50m, 3);

        material.Description.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfile_InactiveMaterial_ShouldKeepInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();

        material.UpdateProfile("PARAFUSO", null, 3.00m, 3);

        material.Status.ShouldBe(EntityStatus.INACTIVE);
        material.Price.ShouldBe(3.00m);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void IncreaseStock_PositiveQuantity_ShouldAddToStockAndSetUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);

        material.IncreaseStock(5);

        material.StockQuantity.ShouldBe(5);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void IncreaseStock_InactiveMaterial_ShouldAddToStockAndKeepInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();

        material.IncreaseStock(4);

        material.StockQuantity.ShouldBe(4);
        material.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public void DecreaseStock_SufficientStock_ShouldDecreaseAndSetUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.IncreaseStock(10);

        material.DecreaseStock(4);

        material.StockQuantity.ShouldBe(6);
        material.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void DecreaseStock_ExactBalance_ShouldLeaveZeroStock()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.IncreaseStock(6);

        material.DecreaseStock(6);

        material.StockQuantity.ShouldBe(0);
    }

    [Fact]
    public void DecreaseStock_InsufficientStock_ShouldThrowInsufficientStockExceptionAndKeepStock()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.IncreaseStock(6);
        var updatedAtBefore = material.UpdatedAt;

        var exception = Should.Throw<InsufficientStockException>(() => material.DecreaseStock(7));

        exception.ErrorType.ShouldBe("insufficient-stock");
        material.StockQuantity.ShouldBe(6);
        material.UpdatedAt.ShouldBe(updatedAtBefore);
    }

    [Fact]
    public void DecreaseStock_InactiveMaterial_ShouldDecreaseAndKeepInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.IncreaseStock(6);
        material.Deactivate();

        material.DecreaseStock(2);

        material.StockQuantity.ShouldBe(4);
        material.Status.ShouldBe(EntityStatus.INACTIVE);
    }
}
