using Membership.Features.Members.GetMyProfile;

namespace Membership.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddMembershipServices(this IServiceCollection services)
    {
        services.AddScoped<GetMyProfileHandler>();
        return services;
    }
}
