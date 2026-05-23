namespace Payment.Domain.Errors;

public static class PaymentErrors
{
    public static readonly string PaymentNotFound = "Payment.NotFound";
    public static readonly string PaymentFailed = "Payment.Failed";
    public static readonly string InvalidAmount = "Payment.InvalidAmount";
}
