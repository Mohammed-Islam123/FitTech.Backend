using Courses.Features.Coaches.CreateCoach;
using Courses.Features.Coaches.GetCoachClientProfile;
using Courses.Features.Coaches.GetCoachClients;
using Courses.Features.Coaches.GetCoachPrograms;
using Courses.Features.Programs.AcceptProgram;
using Courses.Features.Programs.CreateProgram;
using Courses.Features.Programs.GetProgramMembers;
using Courses.Features.Programs.GetProgramRequest;
using Courses.Features.Programs.ListProgramRequests;
using Courses.Features.Programs.RejectProgram;
using Courses.Features.Sessions.MarkAttendance;

namespace Courses.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddCoursesServices(this IServiceCollection services)
    {
        services.AddScoped<CreateCoachHandler>();
        services.AddScoped<GetCoachClientProfileHandler>();
        services.AddScoped<GetCoachClientsHandler>();
        services.AddScoped<GetCoachProgramsHandler>();
        services.AddScoped<AcceptProgramHandler>();
        services.AddScoped<CreateProgramHandler>();
        services.AddScoped<GetProgramMembersHandler>();
        services.AddScoped<GetProgramRequestHandler>();
        services.AddScoped<ListProgramRequestsHandler>();
        services.AddScoped<RejectProgramHandler>();
        services.AddScoped<MarkAttendanceHandler>();

        return services;
    }
}
