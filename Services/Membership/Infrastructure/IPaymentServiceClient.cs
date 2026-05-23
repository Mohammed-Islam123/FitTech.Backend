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

public interface IPaymentServiceClient
{
    [Post("/api/payments")]
    Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);
}
