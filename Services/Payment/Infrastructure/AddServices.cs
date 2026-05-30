using Payment.Features.Payments.CreatePayment;
using Payment.Features.Payments.ListPayments;
using Payment.Features.Requests.AcceptCoursePurchase;
using Payment.Features.Requests.AcceptMembershipRenewal;
using Payment.Features.Requests.ListCoursePurchaseRequests;
using Payment.Features.Requests.ListMembershipRenewalRequests;
using Payment.Features.Requests.RejectCoursePurchase;
using Payment.Features.Requests.RejectMembershipRenewal;
using Payment.Features.Requests.RequestCoursePurchase;
using Payment.Features.Requests.RequestMembershipRenewal;

namespace Payment.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services)
    {
        services.AddScoped<CreatePaymentHandler>();
        services.AddScoped<ListPaymentsHandler>();
        services.AddScoped<RequestMembershipRenewalHandler>();
        services.AddScoped<ListMembershipRenewalRequestsHandler>();
        services.AddScoped<AcceptMembershipRenewalHandler>();
        services.AddScoped<RejectMembershipRenewalHandler>();
        services.AddScoped<RequestCoursePurchaseHandler>();
        services.AddScoped<ListCoursePurchaseRequestsHandler>();
        services.AddScoped<AcceptCoursePurchaseHandler>();
        services.AddScoped<RejectCoursePurchaseHandler>();

        return services;
    }
}
