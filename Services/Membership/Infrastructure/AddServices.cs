using Membership.Features.Courses.GetAvailableCourses;
using Membership.Features.Courses.GetCourseDetail;
using Membership.Features.Courses.GetEnrolledCourses;
using Membership.Features.Members.ActivateMember;
using Membership.Features.Members.CreateMember;
using Membership.Features.Members.DeleteMember;
using Membership.Features.Members.GetActiveSubscription;
using Membership.Features.Members.GetMember;
using Membership.Features.Members.GetMyProfile;
using Membership.Features.Members.GetSubscriptionHistory;
using Membership.Features.Members.ListMembers;
using Membership.Features.Members.SuspendMember;
using Membership.Features.Members.UpdateMember;
using Membership.Features.Members.UpdateMyProfile;
using Membership.Features.Plans.CreatePlan;
using Membership.Features.Plans.DeletePlan;
using Membership.Features.Plans.ListPlans;
using Membership.Features.Plans.UpdatePlan;
using Membership.Features.Subscriptions.ConfirmCashPayment;
using Membership.Features.Subscriptions.CreateSubscription;

namespace Membership.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddMembershipServices(this IServiceCollection services)
    {
        services.AddScoped<GetAvailableCoursesHandler>();
        services.AddScoped<GetCourseDetailHandler>();
        services.AddScoped<GetEnrolledCoursesHandler>();
        services.AddScoped<ActivateMemberHandler>();
        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<DeleteMemberHandler>();
        services.AddScoped<GetActiveSubscriptionHandler>();
        services.AddScoped<GetMemberHandler>();
        services.AddScoped<GetMyProfileHandler>();
        services.AddScoped<GetSubscriptionHistoryHandler>();
        services.AddScoped<ListMembersHandler>();
        services.AddScoped<SuspendMemberHandler>();
        services.AddScoped<UpdateMemberHandler>();
        services.AddScoped<UpdateMyProfileHandler>();
        services.AddScoped<CreatePlanHandler>();
        services.AddScoped<DeletePlanHandler>();
        services.AddScoped<ListPlansHandler>();
        services.AddScoped<UpdatePlanHandler>();
        services.AddScoped<ConfirmCashPaymentHandler>();
        services.AddScoped<CreateSubscriptionHandler>();
        return services;
    }
}
