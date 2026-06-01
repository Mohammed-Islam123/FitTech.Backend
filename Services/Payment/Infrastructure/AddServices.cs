using Payment.Features.Payments.ConfirmPayment;
using Payment.Features.Payments.CreatePayment;
using Payment.Features.Payments.CreatePaymentIntent;
using Payment.Features.Payments.GetMemberPayments;
using Payment.Features.Payments.GetMyPayments;
using Payment.Features.Payments.ListPayments;
using Payment.Features.Payments.Webhook;
using Payment.Infrastructure.Gateways;

namespace Payment.Infrastructure;

public static class AddServices
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services)
    {
        services.AddScoped<CreatePaymentHandler>();
        services.AddScoped<CreatePaymentIntentHandler>();
        services.AddScoped<ConfirmPaymentHandler>();
        services.AddScoped<ListPaymentsHandler>();
        services.AddScoped<GetMyPaymentsHandler>();
        services.AddScoped<GetMemberPaymentsHandler>();
        services.AddScoped<WebhookHandler>();

        return services;
    }

    public static IServiceCollection AddPaymentGateway(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
        return services;
    }
}
