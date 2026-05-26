using Shared.Enums;

namespace Payment.Features.Payments.ListPayments;

public record ListPaymentsResponse(
    Guid PaymentId,
    string MemberFullName,
    string Email,
    decimal Amount,
    string Subscription,
    DateTime DateAndTime,
    string PaymentType
);
