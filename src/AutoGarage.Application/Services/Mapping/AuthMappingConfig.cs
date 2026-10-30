using AutoGarage.Application.Commands.Auth;
using AutoGarage.Communication.Requests.Auth;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginRequest, LoginCommand>();
    }
}
