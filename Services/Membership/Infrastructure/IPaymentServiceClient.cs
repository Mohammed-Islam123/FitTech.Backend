using Refit;

namespace Membership.Infrastructure;

public record CreatePaymentRequest(
    Guid UserId,
    decimal Amount,
    string PaymentMethod,
    string PaymentType,
    Guid ReferenceId,
    string? Notes);

public record CreatePaymentResponse(Guid PaymentId);

public record ConfirmPaymentResponseDto(Guid PaymentId, string Status);

public record MemberPaymentDto(
    Guid PaymentId,
    decimal Amount,
    string PaymentMethod,
    string PaymentType,
    Guid ReferenceId,
    string Status,
    DateTime CreatedAt
);

public interface IPaymentServiceClient
{
    [Post("/api/payments")]
    Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);

    [Post("/api/payments/{paymentId}/confirm")]
    Task<ApiResponse<ConfirmPaymentResponseDto>> ConfirmPaymentAsync(Guid paymentId);

    [Get("/api/payments/member/me")]
    Task<ApiResponse<List<MemberPaymentDto>>> GetMyPaymentsAsync();
}
