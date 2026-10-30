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
    public void Create_ZeroPriceAndMinimum_ShouldBeAccepted()
    {
        var material = Material.Create("PARAFUSO", null, 0m, 0);

        material.Price.ShouldBe(0m);
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
}
