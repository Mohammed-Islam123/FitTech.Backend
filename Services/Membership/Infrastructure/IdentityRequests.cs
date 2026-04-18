namespace Membership.Infrastructure;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
