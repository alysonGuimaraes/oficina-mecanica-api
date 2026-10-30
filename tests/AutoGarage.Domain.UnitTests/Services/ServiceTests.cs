using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Services;

public class ServiceTests
{
    [Fact]
    public void Create_ValidValues_ShouldBeActiveWithSameValues()
    {
        var service = Service.Create("TROCA DE ÓLEO", "Troca com filtro", 150.50m);

        service.Name.ShouldBe("TROCA DE ÓLEO");
        service.Description.ShouldBe("Troca com filtro");
        service.Price.ShouldBe(150.50m);
        service.Status.ShouldBe(EntityStatus.ACTIVE);
        service.Id.ShouldNotBe(Guid.Empty);
        service.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_NullDescription_ShouldKeepNull()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);

        service.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldSetCreatedAtInUtc()
    {
        var before = DateTime.UtcNow;

        var service = Service.Create("ALINHAMENTO", null, 80m);

        service.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        service.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        service.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Activate_InactiveService_ShouldBeActiveAndSetUpdatedAt()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();

        service.Activate();

        service.Status.ShouldBe(EntityStatus.ACTIVE);
        service.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Activate_AlreadyActive_ShouldThrowServiceAlreadyActiveException()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);

        Should.Throw<ServiceAlreadyActiveException>(() => service.Activate());

        service.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Deactivate_ActiveService_ShouldBeInactiveAndSetUpdatedAt()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);

        service.Deactivate();

        service.Status.ShouldBe(EntityStatus.INACTIVE);
        service.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ShouldThrowServiceAlreadyInactiveException()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();

        Should.Throw<ServiceAlreadyInactiveException>(() => service.Deactivate());
    }

    [Fact]
    public void UpdateProfile_ValidValues_ShouldChangeNameDescriptionPriceAndSetUpdatedAt()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);

        service.UpdateProfile("TROCA DE ÓLEO", "Com filtro", 150.50m);

        service.Name.ShouldBe("TROCA DE ÓLEO");
        service.Description.ShouldBe("Com filtro");
        service.Price.ShouldBe(150.50m);
        service.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfile_NullDescription_ShouldClearDescription()
    {
        var service = Service.Create("ALINHAMENTO", "Antiga", 80m);

        service.UpdateProfile("ALINHAMENTO", null, 80m);

        service.Description.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfile_InactiveService_ShouldKeepInactiveStatus()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();

        service.UpdateProfile("ALINHAMENTO", null, 90m);

        service.Status.ShouldBe(EntityStatus.INACTIVE);
        service.Price.ShouldBe(90m);
        service.UpdatedAt.ShouldNotBeNull();
    }
}
