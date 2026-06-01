using Membership.Features.Members.ActivateMember;
using Membership.Features.Members.CreateMember;
using Membership.Features.Members.DeleteMember;
using Membership.Features.Members.GetActiveSubscription;
using Membership.Features.Members.GetMember;
using Membership.Features.Members.GetMemberByCard;
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
using Membership.Features.Subscriptions.AcceptRenewal;
using Membership.Features.Subscriptions.ConfirmCashPayment;
using Membership.Features.Subscriptions.CreateSubscription;
using Membership.Features.Subscriptions.ListRenewalRequests;
using Membership.Features.Subscriptions.OnlineRenewal;
using Membership.Features.Subscriptions.RejectRenewal;
using Membership.Features.Subscriptions.RequestRenewal;
using Membership.Features.Me.Payments;
using Membership.Features.Me.Sessions;
using Membership.Features.Me.Subscriptions;

namespace Membership.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddMembershipServices(this IServiceCollection services)
    {
        services.AddScoped<ActivateMemberHandler>();
        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<DeleteMemberHandler>();
        services.AddScoped<GetActiveSubscriptionHandler>();
        services.AddScoped<GetMemberHandler>();
        services.AddScoped<GetMemberByCardHandler>();
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
        services.AddScoped<RequestRenewalHandler>();
        services.AddScoped<AcceptRenewalHandler>();
        services.AddScoped<RejectRenewalHandler>();
        services.AddScoped<OnlineRenewalHandler>();
        services.AddScoped<ListRenewalRequestsHandler>();
        services.AddScoped<MePaymentsHandler>();
        services.AddScoped<MeSessionsHandler>();
        services.AddScoped<MeSubscriptionsHandler>();
        return services;
    }
}
