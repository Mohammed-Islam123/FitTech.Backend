using Activity.Features.EntryExit.ManualEnter;
using Activity.Features.EntryExit.ManualExit;
using Activity.Features.EntryExit.ScanEntryExit;
using Activity.Features.Members.GetMemberActivity;
using Activity.Features.Sessions.GetSessionsToday;

namespace Activity.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddActivityServices(this IServiceCollection services)
    {
        services.AddScoped<ManualEnterHandler>();
        services.AddScoped<ManualExitHandler>();
        services.AddScoped<ScanEntryExitHandler>();
        services.AddScoped<GetMemberActivityHandler>();
        services.AddScoped<GetSessionsTodayHandler>();

        return services;
    }
}
