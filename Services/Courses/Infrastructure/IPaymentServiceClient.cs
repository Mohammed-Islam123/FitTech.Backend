using Refit;

namespace Courses.Infrastructure;

public record CreatePaymentRequest(
    Guid UserId,
    decimal Amount,
    string PaymentMethod,
    string PaymentType,
    Guid ReferenceId,
    string? Notes);

public record CreatePaymentResponse(Guid PaymentId);

public record ConfirmPaymentResponse(Guid PaymentId, string Status);

public interface IPaymentServiceClient
{
    [Post("/api/payments")]
    Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);

    [Post("/api/payments/{paymentId}/confirm")]
    Task<ApiResponse<ConfirmPaymentResponse>> ConfirmPaymentAsync(Guid paymentId);
}
