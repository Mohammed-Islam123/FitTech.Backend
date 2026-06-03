using Courses.Features.Coaches.CreateCoach;
using Courses.Features.Coaches.GetCoachClientProfile;
using Courses.Features.Coaches.GetCoachClients;
using Courses.Features.Coaches.GetCoachPrograms;
using Courses.Features.Coaches.GetCoachProfile;
using Courses.Features.Coaches.ListCoaches;
using Courses.Features.Programs.AcceptProgram;
using Courses.Features.Programs.AcceptPurchase;
using Courses.Features.Programs.CreateProgram;
using Courses.Features.Programs.GetAvailablePrograms;
using Courses.Features.Programs.GetEnrolledPrograms;
using Courses.Features.Programs.GetProgramDetail;
using Courses.Features.Programs.GetProgramMembers;
using Courses.Features.Programs.GetProgramSessions;
using Courses.Features.Programs.GetProgramRequest;
using Courses.Features.Programs.ListProgramRequests;
using Courses.Features.Programs.ListPurchaseRequests;
using Courses.Features.Programs.PurchaseCash;
using Courses.Features.Programs.PurchaseOnline;
using Courses.Features.Programs.RejectProgram;
using Courses.Features.Programs.RejectPurchase;
using Courses.Features.Sessions.GetSessionHistory;
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
        services.AddScoped<GetCoachProfileHandler>();
        services.AddScoped<ListCoachesHandler>();
        services.AddScoped<AcceptProgramHandler>();
        services.AddScoped<AcceptPurchaseHandler>();
        services.AddScoped<CreateProgramHandler>();
        services.AddScoped<GetAvailableProgramsHandler>();
        services.AddScoped<GetEnrolledProgramsHandler>();
        services.AddScoped<GetProgramDetailHandler>();
        services.AddScoped<GetProgramMembersHandler>();
        services.AddScoped<GetProgramRequestHandler>();
        services.AddScoped<GetProgramSessionsHandler>();
        services.AddScoped<ListProgramRequestsHandler>();
        services.AddScoped<ListPurchaseRequestsHandler>();
        services.AddScoped<PurchaseCashHandler>();
        services.AddScoped<PurchaseOnlineHandler>();
        services.AddScoped<RejectProgramHandler>();
        services.AddScoped<RejectPurchaseHandler>();
        services.AddScoped<MarkAttendanceHandler>();
        services.AddScoped<GetSessionHistoryHandler>();

        return services;
    }
}
