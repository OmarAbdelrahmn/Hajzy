using Application.Contracts.Admin;
using Application.Contracts.Auth;
using Domain.Entities;
using Mapster;

namespace Application.Mapping;

public class MappingConfigration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<Registerrequest, ApplicationUser>()
            .Map(des => des.UserName, src => src.Email);

        config.NewConfig<(ApplicationUser user, string userrole), UserResponse>()
            .Map(des => des, src => src.user)
            .Map(des => des.Role, src => src.userrole);



    }
}
